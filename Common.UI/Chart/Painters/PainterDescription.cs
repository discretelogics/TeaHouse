using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeaTime.Chart.Painters
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PainterDescriptionAttribute : Attribute
    {
        public PainterDescriptionAttribute(string text, string imageResourceName, int order)
        {
            this.Text = text;
            this.ImageResourceName = imageResourceName;
            this.Order = order;
        }

        public string Text { get; private set; }
        public string ImageResourceName { get; private set; }
        public int Order { get; private set; }
    }
}
