using System;
namespace RocketbankTestApp.Models
{
    interface IVirtualGeoGraph
    {
        System.Collections.Generic.IEnumerable<Tuple<IGeoItem, double>> GetEdgesFor(IGeoItem node);
        System.Collections.Generic.IEnumerable<IGeoItem> GetNeiboursInRadius(IGeoItem node, double radius);
        void Merge(double distance, params IGeoItem[] nodes);
        System.Collections.Generic.List<IGeoItem> Nodes { get; }
        double this[IGeoItem node1, IGeoItem node2] { get; }
        void DecomposeClusters(global::Windows.Devices.Geolocation.Geopoint center, double radius, double distance);
    }
}
