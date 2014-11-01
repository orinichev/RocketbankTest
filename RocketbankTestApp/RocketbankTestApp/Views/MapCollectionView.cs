using RocketbankTestApp.Models;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;

namespace RocketbankTestApp.Views
{
    public class MapCollectionView : DependencyObject, IList, INotifyCollectionChanged
    {

        #region Nested classes

        private class simpleGeoItem : IGeoItem
        {

            public Geopoint Position
            {
                get;
                set;
            }

            public bool CanBeClusterized
            {
                get { return false; }
            }
        }

        #endregion

        const double MIN_VISUAL_DISTANCE = 30;

        VirtualGeoGraph pointGraph;
        private ClusterizedList forView = new ClusterizedList();

        int waitCounter = 0;
        const int Delay = 30;
        bool running = false;


        public static readonly DependencyProperty CollectionSourceProperty = DependencyProperty.Register
            ("CollectionSource"
            , typeof(IList<IGeoItem>)
            , typeof(MapCollectionView)
            , new PropertyMetadata(null, (d, e) =>
                {
                    MapCollectionView _this = d as MapCollectionView;
                    if (_this != null)
                    {
                        _this.generateGraph(e.NewValue as IList<IGeoItem>);
                        if (_this.MapControl != null)
                            _this.addNew();
                    }
                })
            );


        public static readonly DependencyProperty MapControlProperty = DependencyProperty.Register
            ("MapControl"
            , typeof(MapControl)
            , typeof(MapCollectionView)
            , new PropertyMetadata(null, (d, e) =>
                {
                    var _this = d as MapCollectionView;
                    if (_this != null)
                    {
                        if (e.OldValue != null)
                        {
                            var oldMap = e.OldValue as MapControl;
                            oldMap.CenterChanged -= _this.mapControl_Changed;
                            oldMap.ZoomLevelChanged -= _this.mapControl_Changed;
                        }
                        var newMap = e.NewValue as MapControl;
                        newMap.CenterChanged += _this.mapControl_Changed;
                        newMap.ZoomLevelChanged += _this.mapControl_Changed;

                        _this.addNew();
                    }
                })
            );
        public MapControl MapControl
        {
            get
            {
                return GetValue(MapControlProperty) as MapControl;
            }
            set
            {
                SetValue(MapControlProperty, value);
            }
        }

        public IList<IGeoItem> CollectionSource
        {
            get
            {
                return GetValue(CollectionSourceProperty) as IList<IGeoItem>;
            }
            set
            {
                SetValue(CollectionSourceProperty, value);
            }
        }



        private void generateGraph(IList<IGeoItem> sourcePoints)
        {
            System.Diagnostics.Debug.WriteLine("start graph generation");
            pointGraph = new VirtualGeoGraph(sourcePoints);
            System.Diagnostics.Debug.WriteLine("Graph generated for " + sourcePoints.Count.ToString() + " points");
        }


        async void mapControl_Changed(MapControl sender, object args)
        {
            waitCounter = Delay;
            if (running)
            {
                return;
            }
            running = true;
            do
            {
                await Task.Delay(1);
                waitCounter--;
            }
            while (waitCounter != 0);
            running = false;
            removeUnvisible();
            addNew();
        }


        private void removeUnvisible()
        {
            HashSet<IGeoItem> toRemove = new HashSet<IGeoItem>();
            var rect = getVisibleRect();
            foreach (var point in forView)
            {
                if (!canBeViewed(point.Position, rect))
                {
                    toRemove.Add(point);
                }
            }
            foreach (var item in toRemove)
            {
                int index = forView.IndexOf(item);
                forView.Remove(item);
                riseCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            }
        }

        private void addNew()
        {
            if (CollectionSource == null) return;
            var watch = Stopwatch.StartNew();

            var visibleArea = getVisibleRect();
            IGeoItem first = new simpleGeoItem()
            {
                Position = new Geopoint(visibleArea.Center)
            };


            double radius = first.Position.GetDistance(new Geopoint(visibleArea.NorthwestCorner));

            var candidates = from item in pointGraph.GetNeiboursInRadius(first, radius)
                             where visibleArea.Contains(item.Position)
                             select item;

            var withClusters = clusterize(candidates, visibleArea);
            forView.Clear();
            forView = new ClusterizedList(withClusters);
            riseCollectionChanged(NotifyCollectionChangedAction.Reset);         

            watch.Stop();
            System.Diagnostics.Debug.WriteLine("Execution time " + watch.ElapsedMilliseconds);
        }

        private IEnumerable<IGeoItem> clusterize(IEnumerable<IGeoItem> candidates, GeoboundingBox curentView)
        {
            Geopoint testGeoPoint;
            MapControl.GetLocationFromOffset
                ( new Point(0, MIN_VISUAL_DISTANCE)
                , out testGeoPoint);

            double distance = new Geopoint(new BasicGeoposition()
            {
                Latitude = curentView.NorthwestCorner.Latitude,
                Longitude = curentView.NorthwestCorner.Longitude
            }).GetDistance(testGeoPoint);

            VirtualGeoGraph geoGraph = new VirtualGeoGraph(candidates);

            bool clusterCreated;
            do
            {
                clusterCreated = false;
                for (int i = 0; i < geoGraph.Nodes.Count; i++)
                {
                    var curentItem = geoGraph.Nodes[i];
                    var neibours = from n in geoGraph.GetNeiboursInRadius(curentItem, distance)
                                   where n.CanBeClusterized
                                   select n;
                    var neiboudsList = neibours.ToList();
                    neiboudsList.Add(curentItem);
                    if (neibours.Count() != 0)
                    {
                        geoGraph.Merge(neiboudsList.ToArray());
                        clusterCreated = true;
                    }
                }
            }
            while (clusterCreated);

            return geoGraph.Nodes;
        }


        private bool canBeViewed(Geopoint point, GeoboundingBox rect)
        {

            bool longInside = point.Position.Longitude > rect.NorthwestCorner.Longitude && point.Position.Longitude < rect.SoutheastCorner.Longitude;
            bool latInside = point.Position.Latitude > rect.SoutheastCorner.Latitude && point.Position.Latitude < rect.NorthwestCorner.Latitude;
            return longInside && latInside;

        }

        private GeoboundingBox getVisibleRect()
        {
            Geopoint leftTop;

            Geopoint rightBottom;
            MapControl.GetLocationFromOffset(new Point(0, 0), out leftTop);

            MapControl.GetLocationFromOffset(new Point(MapControl.ActualWidth, MapControl.ActualHeight), out rightBottom);

            return new GeoboundingBox(leftTop.Position, rightBottom.Position);
        }



        public int Add(object value)
        {

            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public object this[int index]
        {
            get
            {
                return forView[index];
            }
            set
            {
                forView[index] = value as IGeoItem;
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get
            {
                return forView != null ? forView.Count : 0;
            }
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private void riseCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, new NotifyCollectionChangedEventArgs(action, item, index));
            }
        }

        private void riseCollectionChanged(NotifyCollectionChangedAction action)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, new NotifyCollectionChangedEventArgs(action));
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
