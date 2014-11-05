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
using Windows.UI.Xaml.Media.Animation;
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
            initUI();
        }

        private void initUI()
        {
            Map.Height = Window.Current.Bounds.Height;
            this.Height = Map.Height + 500;
        }

        private void open()
        {
            Storyboard openStoryBoard = new Storyboard();

            DoubleAnimation timeline = new DoubleAnimation()
            {
                To = AdditionalData.RenderSize.Height > 500 ? -500 : -AdditionalData.RenderSize.Height,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase()
            };

            openStoryBoard.Children.Add(timeline);

            Storyboard.SetTarget(timeline, Root);
            Storyboard.SetTargetProperty(timeline, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

            openStoryBoard.Begin();

        }

        private void close()
        {
            Storyboard openStoryBoard = new Storyboard();

            DoubleAnimation timeline = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase()
            };

            openStoryBoard.Children.Add(timeline);

            Storyboard.SetTarget(timeline, Root);
            Storyboard.SetTargetProperty(timeline, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

            openStoryBoard.Begin();
        }

        void createMePin()
        {
            MePin = new Image()
                {
                    Source = new BitmapImage(new Uri("ms-appx:///Assets/pin_me.png"))
                };

            Binding locationBinding = new Binding()
                {
                    Path = new PropertyPath("MyLocation"),
                    Source = viewModel
                };

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
            
        }



        private string getMeasure(int meters)
        {
            return meters / 1000 > 1 ? "км" : "м";  
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
            //close old data
            IcImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            IcText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            RocketPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            DataScroller.VerticalScrollMode = ScrollMode.Enabled;
            ORCPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ICBImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            isAdditionalInfoOpened = true;
            //open new
            if (viewModel.IsGeolocationEnabled)
            {
                var distance = viewModel.GetDistanceTo(atm);
                DistanceBlock.Text = getDistance(distance).ToString();
                DistanceMeterBlock.Text = getMeasure(distance).ToString();
            }

            if (atm.Type == Models.Atm.IC)
            {
                DataScroller.VerticalScrollMode = ScrollMode.Disabled;
                IcImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                IcText.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            if (atm.Type == Models.Atm.MKB)
            {
                RocketPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            if (atm.Type == Models.Atm.ORS)
            {
                ORCPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            if (atm.Type == Models.Atm.ICB)
            {
                ICBImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            Root.UpdateLayout();
            AdditionalData.UpdateLayout();
            open();
        }

        private void closeAdditionalInfo()
        {
            close();
            isAdditionalInfoOpened = false;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            closeAdditionalInfo();
            viewModel.GeolocationStateChanged += viewModel_GeolocationStateChanged;
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
            ProgressPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;       
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
                goToMe();
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

        private async void ZoomInBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Map.ZoomLevel != 20)
            {
                await Map.TrySetViewAsync(Map.Center, Map.ZoomLevel+1);
            }
            e.Handled = true;
        }

        private async void ZoomOutBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Map.ZoomLevel != 1)
            {
                await Map.TrySetViewAsync(Map.Center, Map.ZoomLevel-1);
            }
            e.Handled = true;
        }

        private void ATM_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var context = (sender as FrameworkElement).DataContext as Models.Atm;
          //  Map.Center = context.Position;
            AdditionalData.DataContext = context;
            openAdditionalInfo(context);
            e.Handled = true;
        }


        private void Map_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            closeAdditionalInfo();
        }
    }
}
