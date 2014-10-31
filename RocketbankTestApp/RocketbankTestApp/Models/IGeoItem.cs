using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace RocketbankTestApp.Models
{
    public interface IGeoItem
    {
        bool CanBeClusterized { get; }

        Geopoint Position { get; set; }
    }
}
