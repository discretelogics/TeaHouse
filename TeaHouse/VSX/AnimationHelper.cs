// copyright discretelogics 2012.

using System;
using System.Drawing;
using System.Reflection;
using NLog;

namespace TeaTime.VSX
{
    static class AnimationHelper
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        public static IntPtr GetAnimation(string fileName)
        {
            var asm = Assembly.GetExecutingAssembly();
            var resourceName = "TeaTime.Resources." + fileName;
            using (var rs = asm.GetManifestResourceStream(resourceName))
            {
                if (rs == null)
                {
                    logger.Error("Resource not found: {0}".Formatted(resourceName));
                }
                var animationBitmap = new Bitmap(rs);
                return animationBitmap.GetHbitmap();
            }
        }
    }
}
