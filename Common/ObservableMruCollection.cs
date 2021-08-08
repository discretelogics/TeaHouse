using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaTime
{
    public class ObservableMruCollection<T> : ObservableCollection<T>
    {
        public new void Add(T item)
        {
            Guard.ArgumentNotNull(item, "item");

            if (base.Contains(item))
            {
                if (base.IndexOf(item) != 0)
                {
                    base.Remove(item);
                    base.Insert(0, item);
                }
            }
            else
            {
                base.Insert(0, item);
                while (base.Count > maxCount)
                {
                    base.RemoveAt(base.Count - 1);
                }
            }
        }

        private int maxCount = 25;
        public int MaxCount
        {
            get { return maxCount; }
            set { maxCount = value; }
        }
    }
}
