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
        public HashSet<IGeoItem> Items { get; set; }

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
            Items = new HashSet<IGeoItem>();
        }

        public void Merge(IGeoItem item)
        {
            Count = item is Cluster ? Count + (item as Cluster).Count : Count + 1;
            Items.Add(item);
            Position = Count == 1 ? item.Position : calculateCenter(item);
            Radius = Count == 1 ? 0 : calculateRadius();
        }

        private double calculateRadius()
        {
            var distances = from item in Items
                            let d = item.Position.GetDistance(Position)
                            orderby d
                            select d;
            return distances.First();
        }

        private Windows.Devices.Geolocation.Geopoint calculateCenter(IGeoItem newItem)
        {
            return new Geopoint(new BasicGeoposition()
            {
                Longitude = (this.Position.Position.Longitude + newItem.Position.Position.Longitude) / 2,
                Latitude = (this.Position.Position.Latitude + newItem.Position.Position.Latitude) / 2
            });
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
