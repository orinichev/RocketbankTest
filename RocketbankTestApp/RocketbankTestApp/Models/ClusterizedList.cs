using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketbankTestApp.Models
{
    public class ClusterizedList: IList<IGeoItem>
    {
        private List<IGeoItem> wrappedList;

        public ClusterizedList()
        {
            wrappedList = new List<IGeoItem>();
        }

        public ClusterizedList(IEnumerable<IGeoItem> source)
        {
            wrappedList = source.ToList();
        }
        public int IndexOf(IGeoItem item)
        {
            return wrappedList.IndexOf(item);
        }

        public void Insert(int index, IGeoItem item)
        {
            wrappedList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            wrappedList.RemoveAt(index);
        }

        public IGeoItem this[int index]
        {
            get
            {
                return wrappedList[index];
            }
            set
            {
                wrappedList[index] = value;
            }
        }

        public void Add(IGeoItem item)
        {
            wrappedList.Add(item);
        }

        public void Clear()
        {
            wrappedList.Clear();
        }

        public bool Contains(IGeoItem item)
        {
            var simpleContains = wrappedList.Contains(item);
            var clusterList = from geoItem in wrappedList
                              where geoItem is Cluster && (geoItem as Cluster).Contains(item)
                              select geoItem;
            var clusterContains = clusterList.Count() != 0;
            return clusterContains || simpleContains;
        }

        public void CopyTo(IGeoItem[] array, int arrayIndex)
        {
            wrappedList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return wrappedList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IGeoItem item)
        {
            return wrappedList.Remove(item);
        }

        public IEnumerator<IGeoItem> GetEnumerator()
        {
            return wrappedList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return wrappedList.GetEnumerator();
        }
    }
}
