using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace RocketbankTestApp.Views
{
    public class ATMNameToImageConverter: IValueConverter
    {
        private Dictionary<string, string> atmTypeToPath;

        public ATMNameToImageConverter()
        {
            atmTypeToPath = new Dictionary<string, string>()
            {
                {Models.Atm.IC,  "Assets/pin_ic.png"},
                {Models.Atm.ICB,  "Assets/pin_icb.png"},
                {Models.Atm.MKB,  "Assets/pin_mkb.png"},
                {Models.Atm.ORS,  "Assets/pin_opc.png"},
            };
        }


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new BitmapImage(new Uri("ms-appx:///"+atmTypeToPath[(string)value]));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class NegativeConverter: IValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (double)value * -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
