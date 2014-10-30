using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Popups;
using RocketbankTestApp.Models;
using Windows.ApplicationModel.Core;

namespace RocketbankTestApp.ViewModels
{
    public class MainVM : BaseVM
    {
        public event EventHandler<bool> GeolocationStateChanged;

        private bool isGeolocationEnabled = false;

        public bool IsGeolocationEnabled
        {
            get { return isGeolocationEnabled; }
            set
            {
                isGeolocationEnabled = value;
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

        private ObservableCollection<Models.IGeoItem> atmData;

        public ObservableCollection<Models.IGeoItem> AtmData
        {
            get { return atmData; }
            set
            {
                atmData = value;
                risePropertyChanged();
            }
        }

        public async Task initGeolocation()
        {
            try
            {
                // Get a geolocator object 
                geolocator = new Geolocator();
                geolocator.MovementThreshold = 5;
                geolocator.StatusChanged += geolocator_StatusChanged;
                geolocator.PositionChanged += geolocator_PositionChanged;
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

        async void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    MyLocation = new Geopoint(new BasicGeoposition()
                        {
                            Latitude = args.Position.Coordinate.Latitude,
                            Longitude = args.Position.Coordinate.Longitude,
                        });
                });


        }

        async void geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync( Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    IsGeolocationEnabled = args.Status == PositionStatus.Ready;
                });
         
        }

        public int GetDistanceTo(Models.Atm atm)
        {
            return isGeolocationEnabled ? (int)atm.Position.GetDistance(MyLocation) : 0;
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


                AtmData = new ObservableCollection<Models.IGeoItem>(atmData);
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
