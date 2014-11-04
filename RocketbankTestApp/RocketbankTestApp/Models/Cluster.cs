using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace RocketbankTestApp.Models
{
    public class Cluster : IGeoItem, INotifyPropertyChanged
    {
        private Dictionary<IGeoItem, double> Items { get; set; }



        private Windows.Devices.Geolocation.Geopoint position;

        public Windows.Devices.Geolocation.Geopoint Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                risePropertyChanged();
            }
        }

        private double radius;

        public double Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                risePropertyChanged();
            }
        }

        private int count = 0;

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                risePropertyChanged();
            }
        }

        public Cluster()
        {
            Items = new Dictionary<IGeoItem, double>();
        }

        public void Merge(IGeoItem item, double distance)
        {
            Items.Add(item, distance);
            Position = Items.Count == 1 ? item.Position : calculateCenter(item);
            Radius = Items.Count == 1 ? 0 : calculateRadius();
            Count = item is Cluster ? Count + (item as Cluster).Count : Count + 1;
        }

        private double calculateRadius()
        {
            var distances = from item in Items
                            let d = item.Key.Position.GetDistance(Position)
                            orderby d
                            select d;
            return distances.Count() == 0 ? 0 : distances.First();
        }

        private Windows.Devices.Geolocation.Geopoint calculateCenter(IGeoItem newItem)
        {
            return new Geopoint(new BasicGeoposition()
            {
                Longitude = (this.Position.Position.Longitude + newItem.Position.Position.Longitude) / 2,
                Latitude = (this.Position.Position.Latitude + newItem.Position.Position.Latitude) / 2
            });
        }

        public bool Contains(IGeoItem itemToFind)
        {
            bool result = false;
            foreach (var item in Items.Keys)
            {
                if (item is Cluster)
                {
                    result = (item as Cluster).Contains(itemToFind);
                }
                else
                {
                    result = item == itemToFind;
                }
                if (result) break;

            }
            return result;
        }

        private void regrateCenter(IGeoItem item)
        {
            this.Position = new Geopoint(new BasicGeoposition()
            {
                Latitude = 2 * Position.Position.Latitude - item.Position.Position.Latitude,
                Longitude = 2 * Position.Position.Longitude - item.Position.Position.Longitude,
            });
        }

        public IEnumerable<IGeoItem> DecomposeForDistance(double distance)
        {
            List<IGeoItem> result = new List<IGeoItem>();

            var query = from pair in Items
                        where pair.Value > distance
                        select pair.Key;
            foreach (var item in query.ToList())
            {
                regrateCenter(item);
                Items.Remove(item);
                Count = item is Cluster ? Count - (item as Cluster).count:Count-1;
                result.Add(item);
            }
            if (result.Count>0)
            {
                calculateRadius();   
            }
                 

            return result;
        }



        private void risePropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
            {

                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanBeClusterized
        {
            get { return true; }
        }
    }
}
