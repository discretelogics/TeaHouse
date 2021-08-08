namespace TeaTime.Data
{
    public class PreviewCell : NotifyPropertyChanged
    {
        bool parsed;
        public string Value { get; set; }

        /// <summary>
        /// Indicates whether the string value could be parsed successfully into the type of the field it shall be assigned to.
        /// If this property is false, cells are marked red.
        /// </summary>
        public bool Parsed { get { return this.parsed; } set { this.SetProperty(ref this.parsed, value); } }

        public override string ToString()
        {
            return this.Value;
        }
    }
}