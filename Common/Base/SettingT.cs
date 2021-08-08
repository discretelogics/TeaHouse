using System;
using System.Linq;

namespace TeaTime.Base
{
    public class Setting<T>
    {
        public Setting(T value)
        {
            this.Value = value;
        }
        public T Value { get; set; }
    }
}
