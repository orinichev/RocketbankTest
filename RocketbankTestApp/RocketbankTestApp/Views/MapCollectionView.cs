using RocketbankTestApp.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;

namespace RocketbankTestApp.Views
{
    public class MapCollectionView : DependencyObject, IList, INotifyCollectionChanged
    {
        private IList<IGeoItem> forView = new List<IGeoItem>();


        public static readonly DependencyProperty CollectionSourceProperty = DependencyProperty.Register
            ("CollectionSource"
            , typeof(IList<IGeoItem>)
            , typeof(MapCollectionView)
            , new PropertyMetadata(null, (d, e) =>
                {
                    MapCollectionView _this = d as MapCollectionView;
                    if (_this != null)
                    {
                        if (_this.MapControl!=null)
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

        int waitCounter = 0;
        int Delay = 50;
        bool running = false;

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
            var toAdd = CollectionSource.Except(forView);
            var rect = getVisibleRect(); 
            foreach (var point in toAdd)
            {
                if (canBeViewed(point.Position, rect))
                {
                    forView.Add(point);
                    riseCollectionChanged(NotifyCollectionChangedAction.Add, point, forView.Count-1);
                }
            }
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
