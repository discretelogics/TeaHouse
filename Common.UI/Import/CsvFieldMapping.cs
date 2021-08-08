using System;
using System.Linq;
using Newtonsoft.Json;

namespace TeaTime.Data
{
    public class CSVFieldMapping : NotifyPropertyChanged
    {
        #region state

        string fieldType;
        string name;

        #endregion

        #region life

        public CSVFieldMapping(FieldTypeDescription fieldTypeDesc, string name)
        {
            Guard.ArgumentNotNull(name, "name");

            this.FieldTypeDesc = fieldTypeDesc;
            this.name = name;
        }

        #endregion

        #region core
        
        [JsonIgnore]
        public FieldTypeDescription FieldTypeDesc
        {
            get { return FieldTypeDescriptionManager.Instance.All.FirstOrDefault(fd => fd.Name == FieldType); }
            set { this.FieldType = value != null ? value.Name : null; }
        }
        public string FieldType
        {
            get { return this.fieldType; }
            set
            {
                this.SetProperty(ref this.fieldType, value);
                Changed("FieldTypeDesc");
            }
        }
        public string Name
        {
            get { return this.name; } 
            set{this.SetProperty(ref this.name, value);}
        }

        public bool CanAssign(string s, string decimalSeparator, string dateTimeFormat)
        {
            return this.FieldTypeDesc.CanAssign(s);           
        }

        public object Parse(string text)
        {
            return this.FieldTypeDesc.Parse(text);
        }

        #endregion
        
        public override string ToString()
        {
            return "{0} {1}".Formatted(this.name, this.FieldTypeDesc);
        }
    }
}
