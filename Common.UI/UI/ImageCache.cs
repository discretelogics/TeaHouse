using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

namespace TeaTime.UI
{
    public class ImageCache
    {
        #region properties
        public static ImageCache Instance
        {
            get
            {
                return Singleton.Instance;
            }
        }
        #endregion

        #region ctor
        private ImageCache()
        {
            cache = new Dictionary<Uri, BitmapSource>();
        }
        #endregion

        #region public methods
        public BitmapSource Get(Uri uri)
        {
            if (!cache.ContainsKey(uri))
            {
                BitmapImage img = new BitmapImage();
                using (Stream stream = Application.GetResourceStream(uri).Stream)
                {
                    img.BeginInit();
                    img.StreamSource = stream;
                    img.EndInit();
                }
                cache.Add(uri, img);
            }

            return cache[uri];
        }
        #endregion

        #region fields
        private Dictionary<Uri, BitmapSource> cache;
        #endregion

        #region Singleton
        private class Singleton
        {
            static Singleton()
            {
            }
            internal static readonly ImageCache Instance = new ImageCache();
        }
        #endregion
    }
}
