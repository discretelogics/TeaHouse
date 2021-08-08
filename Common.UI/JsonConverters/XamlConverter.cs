using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace TeaTime.JsonConverters
{
    public class XamlConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string xaml = reader.Value as string;
            if (String.IsNullOrEmpty(xaml))
            {
                return null;
            }
            return XamlReader.Parse(xaml);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                string xaml = XamlWriter.Save(value);
                writer.WriteValue(xaml);
            }
        }
    }
}
