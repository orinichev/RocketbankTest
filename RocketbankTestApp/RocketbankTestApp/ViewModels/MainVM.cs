using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Popups;

namespace RocketbankTestApp.ViewModels
{
    public class MainVM: BaseVM
    {
        public event EventHandler<bool> GeolocationStateChanged;

        private bool isGeolocationEnabled = false;

        public bool IsGeolocationEnabled
        {
            get { return isGeolocationEnabled; }
            set { isGeolocationEnabled = value;
            riseGeolocationChanged(isGeolocationEnabled);
            }
        }

        private Geolocator geolocator;


        private Geopoint myLocation;

        public Geopoint MyLocation
        {
            get { return myLocation; }
            set
            {
                myLocation = value;
                risePropertyChanged();
            }
        }

        private ObservableCollection<Models.Atm> atmData;

        public ObservableCollection<Models.Atm> AtmData
        {
            get { return atmData; }
            set { atmData = value;
            risePropertyChanged();
            }
        }

        public async Task initGeolocation()
        {
             try
            {
                // Get a geolocator object 
                geolocator = new Geolocator();
                var meGeoPoint = await geolocator.GetGeopositionAsync();
                BasicGeoposition geoPosition = new BasicGeoposition()
                {
                    Latitude = meGeoPoint.Coordinate.Latitude,
                    Longitude = meGeoPoint.Coordinate.Longitude,
                };
                MyLocation = new Geopoint(geoPosition);
                IsGeolocationEnabled = true;
            }
            catch
             {
                 IsGeolocationEnabled = false;
             }
        }

       public int GetDistanceTo(Models.Atm atm)
       {
           int result;
           if (isGeolocationEnabled)
           {

               const double degreesToRadians = (Math.PI / 180.0);
               const double earthRadius = 6371; // kilometers

               // convert latitude and longitude values to radians
               var prevRadLat = atm.Location.Position.Latitude * degreesToRadians;
               var prevRadLong = atm.Location.Position.Longitude * degreesToRadians;
               var currRadLat = myLocation.Position.Latitude * degreesToRadians;
               var currRadLong = myLocation.Position.Longitude * degreesToRadians;

               // calculate radian delta between each position.
               var radDeltaLat = currRadLat - prevRadLat;
               var radDeltaLong = currRadLong - prevRadLong;

               // calculate distance
               var expr1 = (Math.Sin(radDeltaLat / 2.0) *
                            Math.Sin(radDeltaLat / 2.0)) +

                           (Math.Cos(prevRadLat) *
                            Math.Cos(currRadLat) *
                            Math.Sin(radDeltaLong / 2.0) *
                            Math.Sin(radDeltaLong / 2.0));

               var expr2 = 2.0 * Math.Atan2(Math.Sqrt(expr1),
                                            Math.Sqrt(1 - expr1));

               var distance = Math.Abs(earthRadius * expr2);
               result = (int) (distance * 1000);  // return results as meters
           }
           else
           {
               result = 0;
           }
           return result;
       }
        
        public async Task LoadAtmData()
        {
            DataAccess.AtmDataSource atmDataSource = new DataAccess.AtmDataSource();
            var dialog = new MessageDialog("Не удалось получить информацию о банкоматах");
            List<Models.Atm> atmData = null;
            try
            {
                atmData = await atmDataSource.GetATMs();
                var typesQuery = from a in atmData
                                 select a.Type;


                AtmData = new ObservableCollection<Models.Atm>(atmData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception during getting ATM " + ex.Message);

            }
            if (atmData == null)
            {
                await dialog.ShowAsync();
            }
        }

        private void riseGeolocationChanged(bool isEnabled)
        {
            var handler = GeolocationStateChanged;
            if (handler != null)
            {
                handler(this, isEnabled);
            }
        }
    }
}
