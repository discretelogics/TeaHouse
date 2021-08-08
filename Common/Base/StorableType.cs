using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaTime.Base
{
    public class StorableType
    {
        public string FullName { get; set; }
        public string AssemblyName { get; set; }

        public StorableType() { }
        public StorableType(Type t)
        {
            FullName = t.FullName;
            AssemblyName = t.Assembly.GetName().Name;
        }

        public bool Equals(Type t)
        {
            return t.FullName == FullName && t.Assembly.GetName().Name == AssemblyName; // we could be more flexible here, but for now it's enough
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            var t = obj as Type;
            if (!ReferenceEquals(t, null))
                return Equals(t);

            var st = obj as StorableType;
            if (ReferenceEquals(st, null))
                return false;

            return FullName == st.FullName && AssemblyName == st.AssemblyName;
        }
        public override int GetHashCode()
        {
            return (FullName + AssemblyName).GetHashCode();
        }

        public override string ToString()
        {
 	         return FullName + " " + AssemblyName;
        }
    }
}
