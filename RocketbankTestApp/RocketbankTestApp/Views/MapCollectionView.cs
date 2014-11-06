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
using Windows.ApplicationModel.Core;
using System.Threading;

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

        const double MIN_VISUAL_DISTANCE = 60;
        const int DELAY = 50;

        VirtualGeoGraph pointGraph;
        private ClusterizedList forView = new ClusterizedList();

        int waitCounter = 0;

        bool running = false;


        public static readonly DependencyProperty CollectionSourceProperty = DependencyProperty.Register
            ("CollectionSource"
            , typeof(IList<IGeoItem>)
            , typeof(MapCollectionView)
            , new PropertyMetadata(null, async (d, e) =>
                {
                    MapCollectionView _this = d as MapCollectionView;
                    if (_this != null)
                    {
                        _this.generateGraph(e.NewValue as IList<IGeoItem>);
                        if (_this.MapControl != null)
                        {
                            _this.Cancel();
                            if (_this.source!=null)
                                _this.source.Dispose();
                            _this.source = new CancellationTokenSource();
                                await _this.addPointsToMap(_this.source.Token);
                        }

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
            waitCounter = DELAY;
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
            source.Cancel();
            if (source!=null)
                source.Dispose();
            source = new CancellationTokenSource();
                await addPointsToMap(source.Token);
        }

        CancellationTokenSource source;

        public void Cancel()
        {
            try
            {

                if (source != null)
                    source.Cancel();
            }
            catch (Exception)
            {

            }
        }

        private async Task addPointsToMap(CancellationToken token)
        {
            if (pointGraph.Nodes.Count == 0)
                return;
            if (token.IsCancellationRequested)
                return;
            GeoboundingBox visibleArea = null;
            Geopoint testGeoPoint = null;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //get visible area
                visibleArea = getVisibleRect();
                //get distance for cluster
                MapControl.GetLocationFromOffset
                    (new Point(0, MIN_VISUAL_DISTANCE)
                    , out testGeoPoint);
            });
            if (token.IsCancellationRequested)
                return;
            double distance = new Geopoint(new BasicGeoposition()
            {
                Latitude = visibleArea.NorthwestCorner.Latitude,
                Longitude = visibleArea.NorthwestCorner.Longitude
            }).GetDistance(testGeoPoint);

            var graphView = pointGraph.GetView(visibleArea, distance);
            if (token.IsCancellationRequested)
                return;
            graphView.Decompose();
            if (token.IsCancellationRequested)
                return;
            graphView.Clusterize();
            if (token.IsCancellationRequested)
                return;
            forView = new ClusterizedList(graphView.Nodes);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        {
            riseCollectionChanged(NotifyCollectionChangedAction.Reset);
        });




        }


        private bool canBeViewed(Geopoint point, GeoboundingBox rect)
        {

            bool longInside = point.Position.Longitude > rect.NorthwestCorner.Longitude && point.Position.Longitude < rect.SoutheastCorner.Longitude;
            bool latInside = point.Position.Latitude > rect.SoutheastCorner.Latitude && point.Position.Latitude < rect.NorthwestCorner.Latitude;
            return longInside && latInside;

        }

        private GeoboundingBox getVisibleRect()
        {
            Geopoint nordwestConor;
            Geopoint eastSourthConor;
            try
            {
                MapControl.GetLocationFromOffset(new Point(0, 0), out nordwestConor);
                MapControl.GetLocationFromOffset(new Point(MapControl.ActualWidth, MapControl.ActualHeight), out eastSourthConor);
            }
            catch
            {
                nordwestConor = new Geopoint(new BasicGeoposition()
                    {
                        Latitude = 77.6087,
                        Longitude = -6.7633
                    });
                eastSourthConor = new Geopoint(new BasicGeoposition()
                    {
                        Latitude = 27.9378,
                        Longitude = -167.0589
                    });
            }



            return new GeoboundingBox(nordwestConor.Position, eastSourthConor.Position);
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
