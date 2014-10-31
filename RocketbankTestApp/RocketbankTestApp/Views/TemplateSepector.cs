using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RocketbankTestApp.Views
{
    public abstract class TemplateSepector : ContentControl
    {

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            ContentTemplate = SelectTemplate(newContent, this);
        }

        protected abstract DataTemplate SelectTemplate(object newContent, TemplateSepector clusterTemplateSepector);
    }
}
