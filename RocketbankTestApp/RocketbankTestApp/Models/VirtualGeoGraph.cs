using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketbankTestApp.Models
{
    public class VirtualGeoGraph
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

        public void Merge(params IGeoItem[] nodes)
        {
            Cluster cluster = new Cluster();
            foreach (var node in nodes)
            {
                cluster.Merge(node);
                this.nodes.Remove(node);
            }
            this.nodes.Add(cluster);
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
                   let t = new Tuple<IGeoItem, double>(n, n.Position.GetDistance(node.Position))
                   where (n != node) && t.Item2 <= radius
                   orderby t.Item2
                   select t.Item1;
        }

    }
}
