using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace RocketbankTestApp.Models
{
   

    public class Atm: IGeoItem
    {
        public const string ORS = "ors";
        public const string MKB = "mkb";
        public const string IC = "intercommerz_office";
        public const string ICB = "intercommerz_atm";

        public int Id { get; set; }


        public string Name { get; set; }


        public bool Rur { get; set; }


        public bool Usd { get; set; }


        public bool Eur { get; set; }


        public string Hours { get; set; }


        public string Address { get; set; }


        public double Lat { get; set; }


        public double Lon { get; set; }


        public bool Hidden { get; set; }


        public string Type { get; set; }

        public Geopoint Position
        {
            get
            {
                return new Geopoint(new BasicGeoposition()
                    {
                        Longitude = Lon,
                        Latitude = Lat
                    });
            }
            set
            {

            }
        }
    }
}
