using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace RocketbankTestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Models.Atm> viewModel;
        DataAccess.BancomatDataSource atmDataSource;
        Geolocator geolocator;
            
        public MainPage()
        {
            this.InitializeComponent();
            atmDataSource = new DataAccess.BancomatDataSource();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {                
                await loadAtmData();
                
            }

            // TODO: Prepare page for display here.
            
            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
           
        }

        private async Task initGeolocation(bool isNew)
        {
            try
            {
                // Get a geolocator object 
                geolocator = new Geolocator();            
                var myGeoposition = await geolocator.GetGeopositionAsync();
            }
            catch (UnauthorizedAccessException)
            {
                

            }
            catch (Exception)
            {               
                if (geolocator.LocationStatus == PositionStatus.Disabled)
                {
                    var notificationDialog = new MessageDialog("Геолокаця отключена, хотите включить?");
                    notificationDialog.Commands.Add(new UICommand()
                    {
                        Label = "Не сейчас",
                        Invoked = (c) =>
                        {

                        }
                    });
                    notificationDialog.Commands.Add(new UICommand()
                    {
                        Label = "Да",
                        Invoked = async (c) =>
                        {
                            await Launcher.LaunchUriAsync(new Uri("ms-settings-proximity:///"));
                        }
                    });
                }

            }
        }

        private async Task goToMe()
        {
            try
            {
                 var meGeoPoint = await geolocator.GetGeopositionAsync();
                 BasicGeoposition geoPosition = new BasicGeoposition()
                 {
                     Latitude = meGeoPoint.Coordinate.Latitude,
                     Longitude = meGeoPoint.Coordinate.Longitude,
                 };
                 Map.Center = new Geopoint(geoPosition);
            }    
            catch
            {

            }
        }

        private async System.Threading.Tasks.Task loadAtmData()
        {
            var dialog = new MessageDialog("Не удалось получить информацию о банкоматах");
            List<Models.Atm> atmData = null;
            try
            {
                atmData = await atmDataSource.GetATMs();
                viewModel = new ObservableCollection<Models.Atm>(atmData);
                this.DataContext = viewModel;
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

        private void showAlert()
        {
            VisualStateManager.GoToState(this, "Opened", true);
        }

        private void hideAlert()
        {
            VisualStateManager.GoToState(this, "Closed", true);
        }

        private void MeBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hideAlert();
        }

        private void ZoomInBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void ZoomOutBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
