// copyright discretelogics 2012.

using System;
using Newtonsoft.Json;

namespace TeaTime
{
    public class MruItem
    {
        public MruItem(string fullname)
        {
            Guard.ArgumentNotNullOrWhiteSpace(fullname, "fullname");

            this.fullname = fullname;
        }

        readonly string fullname;
        public string FullName { get { return this.fullname; } }
        
        [JsonIgnore]
        public string DisplayName { get { return IOUtils.GetCompactPath(this.FullName, 48); } }

        public override bool Equals(object obj)
        {
            var other = obj as MruItem;
            return (other != null) && this.fullname.Equals(other.fullname, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.fullname.GetHashCode();
        }
    }
}
