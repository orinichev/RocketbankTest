using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace RocketbankTestApp.Models
{
    public class VirtualGraphView
    {
        private VirtualGeoGraph source;
        private double distance;


        private VirtualGeoGraph viewArea;
        private VirtualGraphView(VirtualGeoGraph graph)
        {
            this.source = graph;
        }

        public static VirtualGraphView Create
            (VirtualGeoGraph graph
            , Geopoint center
            , double radius
            , double distance)
        {
            return new VirtualGraphView(graph)
                {
                    distance = distance,
                    viewArea = new VirtualGeoGraph(
                        from item in graph.Nodes
                        let d = item.Position.GetDistance(center)
                        where d < radius
                        orderby d
                        select item)
                };
        }

        public static VirtualGraphView Create
            (VirtualGeoGraph graph
            , GeoboundingBox box
            , double distance)
        {
            return new VirtualGraphView(graph)
            {
                distance = distance,
                viewArea = new VirtualGeoGraph(
                        from item in graph.Nodes
                        where box.Contains(item.Position) || ((item is Cluster) && box.intersectWithArea(item as Cluster))
                        select item)
            };
        }




        public IEnumerable<IGeoItem> GetNeiboursInRadius(IGeoItem node, double radius)
        {
            return viewArea.GetNeiboursInRadius(node, radius);
        }



        public List<IGeoItem> Nodes
        {
            get
            {
                return viewArea.Nodes;
            }
        }


        public void Decompose()
        {
            bool decomposed;
            do
            {
                decomposed = false;
                var clusters = from item in Nodes
                               where item is Cluster
                               let cluster = (item as Cluster)
                               where cluster.Radius >= distance
                               select cluster;

                foreach (var cluster in clusters.ToList())
                {
                    var decomposedSet = cluster.DecomposeForDistance(distance);
                    if (decomposedSet.Count() > 0)
                    {
                        decomposed = true;
                        Nodes.AddRange(decomposedSet);
                        source.Nodes.AddRange(decomposedSet);
                        Nodes.Remove(cluster);
                        source.Nodes.Remove(cluster);
                        if (cluster.Count != 0)
                        {
                            Nodes.Add(cluster);
                            source.Nodes.Add(cluster);
                        }

                    }


                }
            } while (decomposed);


        }

        public void Clusterize()
        {
            bool clusterCreated;
            do
            {
                clusterCreated = false;
                for (int i = 0; i < Nodes.Count; i++)
                {
                    var curentItem = Nodes[i];
                    if (curentItem.CanBeClusterized)
                    {
                        var neibours = from n in GetNeiboursInRadius(curentItem, distance)
                                       where n.CanBeClusterized
                                       select n;
                        var neiboudsList = neibours.ToList();

                        if (neibours.Count() > 0)
                        {
                            neiboudsList.Add(curentItem);
                            Merge(neiboudsList.ToArray());
                            clusterCreated = true;
                        }
                    }

                }
            }
            while (clusterCreated);
#if DEBUG
            int total = 0;
            foreach (var item in source.Nodes)
            {
                total = item is Cluster ? (item as Cluster).Count + total : total + 1;
            }
            System.Diagnostics.Debug.WriteLine(total);
#endif
        }

        public void Merge(params IGeoItem[] nodes)
        {
            var cluster = viewArea.Merge(distance, nodes);
            foreach (var item in nodes)
            {
                source.Nodes.Remove(item);
            }
            source.Nodes.Add(cluster);
        }


    }
}
