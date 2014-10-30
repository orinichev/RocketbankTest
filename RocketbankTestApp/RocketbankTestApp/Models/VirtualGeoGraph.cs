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

        public VirtualGeoGraph(IEnumerable<IGeoItem> items)
        {
            nodes = new List<IGeoItem>(items);
        }

        public double this[IGeoItem node1, IGeoItem node2]
        {
            get
            {
                return node1.Position.GetDistance(node2.Position);
            }
            set
            {

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

    }
}
