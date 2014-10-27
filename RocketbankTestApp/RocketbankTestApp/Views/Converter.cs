using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace RocketbankTestApp.Views
{
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
