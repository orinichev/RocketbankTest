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
using Windows.Phone.UI.Input;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace RocketbankTestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const double MOSCOW_LATITUDE = 55.7431;
        private const double MOSCOW_LONGITUDE = 37.6129;
        private const int MOSCOW_ZOOM_LEVEL = 11;

        Image MePin;
        private bool isAdditionalInfoOpened = false;
            
        private ViewModels.MainVM viewModel
        {
            get
            {
                return Resources["MainVM"] as ViewModels.MainVM;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();          
            this.NavigationCacheMode = NavigationCacheMode.Required;
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            createMePin();
        }

        void createMePin()
        {
            MePin = new Image();
            MePin.Source = new BitmapImage(new Uri("ms-appx:///Assets/pin_me.png"));
            Binding locationBinding = new Binding();
            locationBinding.Path = new PropertyPath("MyLocation");
            locationBinding.Source = viewModel;
            BindingOperations.SetBinding(MePin, MapControl.LocationProperty, locationBinding);
            Map.Children.Add(MePin);
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame == null)
            {
                return;
            }
            if (isAdditionalInfoOpened)
                {
                    closeAdditionalInfo();
                    e.Handled = true;
                }
            else if (frame.CanGoBack)
            {
                
         
                    frame.GoBack();
                
                
                e.Handled = true;
            }
        }



        private string getMeasure(int meters)
        {
            string result = "м";
            if (meters / 1000 > 1)
                result = "км";
            return result;
        }

        private int getDistance(int originalDistance)
        {
            if (originalDistance / 1000 > 1)
            {
                originalDistance = originalDistance / 1000;
            }
            return originalDistance;
        }

        private void openAdditionalInfo(Models.Atm atm)
        {
            VisualStateManager.GoToState(this, "Opened", true);
            isAdditionalInfoOpened = true;
            if (viewModel.IsGeolocationEnabled)
            {
                var distance = viewModel.GetDistanceTo(atm);
                DistanceBlock.Text = getDistance(distance).ToString();
                DistanceMeterBlock.Text = getMeasure(distance).ToString() ;
            }
        }

        private void closeAdditionalInfo()
        {
            VisualStateManager.GoToState(this, "Closed", true);
            isAdditionalInfoOpened = false;
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

                await viewModel.LoadAtmData();
                await viewModel.initGeolocation();
                if (viewModel.IsGeolocationEnabled)
                {
                    System.Diagnostics.Debug.WriteLine("Geolocation is enabled");
                    goToMe();
                    setUIToLocatedMode();
                   
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Geolocation not enabled");
                    goToMoscow();
                    setUIToUnlocatedMode();
                    await showNavigationAlert();
                }
            }
            viewModel.GeolocationStateChanged += viewModel_GeolocationStateChanged;
            ProgressPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            // TODO: Prepare page for display here.
            
            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
           
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            viewModel.GeolocationStateChanged -= viewModel_GeolocationStateChanged;
            base.OnNavigatedFrom(e);
        }

        void viewModel_GeolocationStateChanged(object sender, bool e)
        {
            if (e)
            {
                setUIToLocatedMode();
            }
            else
            {
                setUIToUnlocatedMode();
            }
        }

        private async Task showNavigationAlert()
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
                    await Launcher.LaunchUriAsync(new Uri("ms-settings-location:///"));
                }
            });
            await notificationDialog.ShowAsync();            
        }     

        private void setUIToUnlocatedMode()
        {
            MePin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            MeBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
         
        }

        private void setUIToLocatedMode()
        {
            
            MePin.Visibility = Windows.UI.Xaml.Visibility.Visible;
            MeBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
               

        private void goToMe()
        {
            try
            {
                 
                 Map.ZoomLevel = 15;
                 Map.Center = viewModel.MyLocation;
            }    
            catch
            {

            }
        }

        private void goToMoscow()
        {
            BasicGeoposition geoPosition = new BasicGeoposition()
            {

                Latitude = MOSCOW_LATITUDE,
                Longitude = MOSCOW_LONGITUDE,
            };
            Map.ZoomLevel = MOSCOW_ZOOM_LEVEL;
            Map.Center = new Geopoint(geoPosition);
        }



        private void MeBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            goToMe();
            e.Handled = true;
        }

        private void ZoomInBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Map.ZoomLevel!=20)
            {
               
                Map.ZoomLevel++;
            }
            e.Handled = true;
        }

        private void ZoomOutBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Map.ZoomLevel!=1)
            {              
                Map.ZoomLevel--;
            }
            e.Handled = true;
        }

        private void ATM_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var context = (sender as FrameworkElement).DataContext as Models.Atm;
            Map.Center = context.Location;
            AdditionalData.DataContext = context;
            openAdditionalInfo(context);
            e.Handled = true;
        }

        private void Map_Tapped(object sender, TappedRoutedEventArgs e)
        {
            closeAdditionalInfo();
        }
    }
}
