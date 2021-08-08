using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaTime
{
    class Util
    {
        public static string ParseDelimeter(string delimeter)
        {
            if (delimeter == null) return null;
            return delimeter.Replace(@"\t", "\t");
        }
    }
}
