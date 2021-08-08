using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaTime.API
{
    public class FieldMapping : NotifyPropertyChanged, IFieldMapping
    {
        readonly string accessorItemFieldName;
        public string AccessorItemFieldName { get { return accessorItemFieldName; } }

        string fileItemFieldName;

        public string FileItemFieldName
        {
            get { return fileItemFieldName; }
            set { SetProperty(ref fileItemFieldName, value); }
        }

        public FieldMapping(string accessorItemFieldName, string fileItemFieldName = null)
        {
            Guard.ArgumentNotNullOrWhiteSpace(accessorItemFieldName, "accessorItemFieldName");

            this.accessorItemFieldName = accessorItemFieldName;
            this.fileItemFieldName = fileItemFieldName;
        }
    }
}
