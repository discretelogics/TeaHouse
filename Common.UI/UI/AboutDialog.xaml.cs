// copyright discretelogics 2012.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using LogicNP.CryptoLicensing;

namespace TeaTime.Special
{
    public partial class AboutDialog : Window
    {
        License License { get { return (License)DataContext; }}

        #region life

        public AboutDialog()
        {
            this.InitializeComponent();
        }

        public static void ShowModal()
        {
            var dlg = new AboutDialog();
            dlg.ShowModalInternal();
        }        

        #endregion

        #region buttons

        void PurchaseClick(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.discretelogics.com/shop");
            this.Close();
        }
        void RemoveClick(object sender, RoutedEventArgs e)
        {
            License.LicenseKey = null;
        }

        void OkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        #endregion

        #region int

        void ShowModalInternal()
        {
            var asm = Assembly.GetExecutingAssembly();
            version.Text = String.Format("Version {0}", asm.GetName().Version);
            var copyrightAttributes = asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (copyrightAttributes.Any())
            {
                copyright.Text = ((AssemblyCopyrightAttribute)copyrightAttributes[0]).Copyright;
            }
            var companyAttributes = asm.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            var productAttributes = asm.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (companyAttributes.Any() && productAttributes.Any())
            {
                product.Text = String.Format("{0} {1}", ((AssemblyCompanyAttribute)companyAttributes[0]).Company, ((AssemblyProductAttribute)productAttributes[0]).Product);
            }

            using (var cw = new CryptoLicenseWrapper(new CryptoLicense(LicenseStorageMode.ToRegistry,
                                                         "AMAAMACWGtSDBakQcYnfM72e5WJnKto54Zld/krvMlc6PzKAhHC9bctdL8s3Y47+SQmojd8DAAEAAQ==")))
            {
                this.DataContext = new License(cw.CryptoLicense);

                if (this.ShowDialog() == true)
                {
                    if (String.IsNullOrWhiteSpace(License.LicenseKey))
                    {
                        cw.CryptoLicense.Remove();
                    }
                    else
                    {
                        cw.CryptoLicense.LicenseCode = License.LicenseKey;
                        cw.CryptoLicense.Save();
                    }
                }
            }
        }

        #endregion
    }
}
