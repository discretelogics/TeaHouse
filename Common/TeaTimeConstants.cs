using System;
using System.IO;
using System.Linq;

namespace TeaTime
{
    public class TeaTimeConstants
    {
        public const string WarehouseEnvironmentVariable = "TeaTimeWarehousePath";
        public static string GetDefaultWarehousePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DiscreteLogics", "Data");
        }
    }
}
