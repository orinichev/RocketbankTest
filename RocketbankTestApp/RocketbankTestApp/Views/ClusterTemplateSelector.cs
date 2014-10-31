using RocketbankTestApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace RocketbankTestApp.Views
{
    public class ClusterTemplateSelector:TemplateSepector
    {
        public DataTemplate ItemTemplate { get; set; }

        public DataTemplate ClusterTemplate { get; set; }

        protected override Windows.UI.Xaml.DataTemplate SelectTemplate(object newContent, TemplateSepector clusterTemplateSepector)
        {
            return newContent is Cluster ? ClusterTemplate : ItemTemplate;
            
        }
    }
}
