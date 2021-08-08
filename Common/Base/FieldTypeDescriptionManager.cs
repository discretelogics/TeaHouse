// copyright discretelogics 2013.

using System;
using System.Globalization;
using System.Linq;

namespace TeaTime
{    
    public class FieldTypeDescription
    {
        public FieldTypeDescription(string name, FieldType type, Func<string, bool> canAssign, Func<string, object> parse)
        {            
            this.Name = name;
            this.Type = type;
            this.CanAssign = canAssign;
            this.Parse = parse;
        }

        public string Name { get; private set; }
        public FieldType Type { get; private set; }
        
        public Func<string, bool> CanAssign { get; private set; }
        public Func<string, object> Parse { get; private set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class FieldTypeDescriptionManager
    {
        public FieldTypeDescription[] All { get; private set; }

        public FieldTypeDescriptionManager()
        {
            byte dummyuint8;
            UInt16 dummyuint16;
            UInt32 dummyuint32;
            UInt64 dummyuint64;
            sbyte dummyint8;
            Int16 dummyint16;
            Int32 dummyint32;
            Int64 dummyint64;
            double dummydouble;
            float dummyfloat;
            decimal dummyNetDecimal;
            this.All = new[]
                {
                    new FieldTypeDescription("time", FieldType.Int64, text => Time.CanParse(text, this.DateTimeFormat), text => Time.Parse(text, this.DateTimeFormat)),
                    new FieldTypeDescription("int8", FieldType.Int8, text => sbyte.TryParse(text, out dummyint8), text => sbyte.Parse(text)),
                    new FieldTypeDescription("int16", FieldType.Int16, text => Int16.TryParse(text, out dummyint16), text => Int16.Parse(text)),
                    new FieldTypeDescription("int32", FieldType.Int32, text => Int32.TryParse(text, out dummyint32), text => Int32.Parse(text)),
                    new FieldTypeDescription("int64", FieldType.Int64, text => Int64.TryParse(text, out dummyint64), text => Int64.Parse(text)),
                    new FieldTypeDescription("uint8", FieldType.UInt8, text => byte.TryParse(text, out dummyuint8), text => byte.Parse(text)),
                    new FieldTypeDescription("uint16", FieldType.UInt16, text => UInt16.TryParse(text, out dummyuint16), text => UInt16.Parse(text)),
                    new FieldTypeDescription("uint32", FieldType.UInt32, text => UInt32.TryParse(text, out dummyuint32), text => UInt32.Parse(text)),
                    new FieldTypeDescription("uint64", FieldType.UInt64, text => UInt64.TryParse(text, out dummyuint64), text => UInt64.Parse(text)),
                    new FieldTypeDescription("double", FieldType.Double, text => double.TryParse(text, NumberStyles.Float, this.NumberFormat, out dummydouble), this.ParseDouble),
                    new FieldTypeDescription("float", FieldType.Float, text => float.TryParse(text, NumberStyles.Float, this.NumberFormat, out dummyfloat), text => float.Parse(text, this.NumberFormat)),
                    new FieldTypeDescription("decimal", FieldType.NetDecimal, text => decimal.TryParse(text, NumberStyles.Float, this.NumberFormat, out dummyNetDecimal), text => decimal.Parse(text, this.NumberFormat))
                };
        }

        object ParseDouble(string text)
        {
            try
            {
                return double.Parse(text, this.NumberFormat);
            }
            catch (FormatException)
            {
                throw new FormatException("text: '{0}' decimalseparator:'{1}'".Formatted(text, (NumberFormat != null) ? NumberFormat.NumberDecimalSeparator.ToString() : "null"));
            }
        }        

        public FieldTypeDescription Get(string name)
        {
            return this.All.FirstOrDefault(ft => ft.Name == name);
        }

        // not "thread safe" but there are not several exports / imports with different formats on the way. ever.
        public string DateTimeFormat { get; set; }
        public NumberFormatInfo NumberFormat { get; set; }

        #region singleton

        public static FieldTypeDescriptionManager Instance { get { return Singleton.instance; } }

        class Singleton
        {
            static Singleton()
            {
            }

            internal static readonly FieldTypeDescriptionManager instance = new FieldTypeDescriptionManager();
        }

        #endregion
    }
}
