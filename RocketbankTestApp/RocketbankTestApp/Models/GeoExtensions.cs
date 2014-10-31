using System;
using Windows.Devices.Geolocation;

namespace RocketbankTestApp.Models
{
    public static class GeoExtensions
    {
        public static bool Contains(this GeoboundingBox _this, Geopoint point)
        {
            bool longInside = point.Position.Longitude > _this.NorthwestCorner.Longitude && point.Position.Longitude < _this.SoutheastCorner.Longitude;
            bool latInside = point.Position.Latitude > _this.SoutheastCorner.Latitude && point.Position.Latitude < _this.NorthwestCorner.Latitude;
            return longInside && latInside;
        }

        /// <summary>
        /// return distance in meters
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double GetDistance(this Geopoint _this, Geopoint point)
        {
            var lat1 = _this.Position.Latitude;
            var lat2 = point.Position.Latitude;
            var lon1 = _this.Position.Longitude;
            var lon2 = point.Position.Longitude;
            double R = 6371;
            var dlat = toRad(lat1 - lat2);
            var dlon = toRad(lon1 - lon2);
            var l1rad = toRad(lat1);
            var l2rad = toRad(lat2);
            var a = Math.Sin(dlat / 2) * Math.Sin(dlat / 2) +
        Math.Sin(dlon / 2) * Math.Sin(dlon / 2) * Math.Cos(l1rad) * Math.Cos(l2rad);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double toRad(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
