﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace RocketbankTestApp.Models
{
    public class VirtualGeoGraph : RocketbankTestApp.Models.IVirtualGeoGraph
    {
        private List<IGeoItem> nodes;

        public List<IGeoItem> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }

        public VirtualGeoGraph(IEnumerable<IGeoItem> items)
        {
            nodes = new List<IGeoItem>(items);
        }

        public VirtualGraphView GetView
            ( Geopoint center
            , double radius
            , double distance)
        {
            return VirtualGraphView.Create(this, center, radius, distance);
        }

        public VirtualGraphView GetView
            ( GeoboundingBox box
            , double distance)
        {
            return VirtualGraphView.Create(this, box, distance);
        }

        public void Merge(double distance, params IGeoItem[] nodes)
        {            
            Cluster cluster = new Cluster();
            foreach (var node in nodes)
            {
                cluster.Merge(node, distance);
                this.nodes.Remove(node);
            }
            this.nodes.Add(cluster);            
        }

        public void DecomposeClusters(Geopoint center, double radius, double distance)
        {
            //var localClustersTodecompose = from node in nodes
            //                               let d = center.GetDistance(node.Position)
            //                               where node is Cluster 
            //                               && d < radius
            //                               && (node as Cluster).Radius * 2 < distance
            //                               select node as Cluster;
            //foreach (var item in localClustersTodecompose)
            //{
            //    nodes.Remove(item);
            //}
            //foreach (var item in localClustersTodecompose)
            //{
            //    nodes.AddRange(item.Items.Keys);
            //}
        }
       

        public double this[IGeoItem node1, IGeoItem node2]
        {
            get
            {
                return node1.Position.GetDistance(node2.Position);
            }          
        }

        public IEnumerable<Tuple<IGeoItem, double>> GetEdgesFor(IGeoItem node)
        {
            return from n in nodes
                   where (n != node)
                   let t = new Tuple<IGeoItem, double>(n, n.Position.GetDistance(node.Position))
                   orderby t.Item2
                   select t;
        }

        public IEnumerable<IGeoItem> GetNeiboursInRadius(IGeoItem node, double radius)
        {
            return from n in nodes
                   let t = new Tuple<IGeoItem, double>(n, node.Position.GetDistance(n.Position))
                   where (t.Item1 != node) && t.Item2 <= radius
                   orderby t.Item2
                   select t.Item1;
        }


        internal void Prepare()
        {
          
        }
    }
}
