using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using LogicNP.CryptoLicensing;
using TeaTime.UI;

namespace TeaTime.Special
{
    // license code
    public class GoodFactory : IEquatable<GoodFactory>
    {
        // returns either T() or a license expired control
        public static UserControl GetContent<T>(out T control) where T : UserControl, new()
        {            
            var le = new Run("License expired.");
            
            bool ok = false;
            var cw = new CryptoLicenseWrapper(new CryptoLicense(
                                                  LicenseStorageMode.ToRegistry,
                                                  "AMAAMACWGtSDBakQcYnfM72e5WJnKto54Zld/krvMlc6PzKAhHC9bctdL8s3Y47+SQmojd8DAAEAAQ=="));
            using (var lw = cw)
            {
                var license = lw.CryptoLicense;
                if (license.Load())
                {
                    ok = license.Status == LicenseStatus.Valid;
                }
            }

            var link = new Hyperlink(le);
            link.Click += (sender, args) => AboutDialog.ShowModal();

            var tb = new TextBlock(link);
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.VerticalAlignment = VerticalAlignment.Center;

            if (ok)
            {
                return control = new T();
            }
            control = null;

            var u = new UserControl();
            u.Content = tb;

            return u;
        }

        #region garbage code for obfuscation

        string trash;

        public override string ToString()
        {
            return string.Format("Trash: {0}", this.trash);
        }

        #region Equality members

        public bool Equals(GoodFactory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(this.trash, other.trash);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GoodFactory)obj);
        }

        public override int GetHashCode()
        {
            return (this.trash != null ? this.trash.GetHashCode() : 0);
        }

        public static bool operator ==(GoodFactory left, GoodFactory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GoodFactory left, GoodFactory right)
        {
            return !Equals(left, right);
        }

        #endregion

        #endregion
    }
}
