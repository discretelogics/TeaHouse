using System;
using System.Windows.Media;
using TeaTime.UI;

namespace TeaTime
{
    internal static class ResourcesUtility
    {
        public static ImageSource GetImage(string name)
        {
            Uri uri = GetUri(name);
            return ImageCache.Instance.Get(uri);
        }

        #region private methods
        private static Uri GetUri(string resourceName)
        {
            Guard.ArgumentNotNullOrWhiteSpace(resourceName, "resourceName");

            return new Uri("/DiscreteLogics.TeaTime.Common.UI;component/resources/" + resourceName.ToLower(), UriKind.Relative);
        }
        #endregion
    }
}
