using System;
using System.Reflection;
using System.Windows;
using LogicNP.CryptoLicensing;
using TeaTime.CommonUI;
using TeaTime.Special;

namespace TeaTime
{
    // completely senseless base class. its static ctor triggers license code
    public class GoodBase : IEquatable<GoodBase>
    {
        // license
        static GoodBase ()
        {
            var cw = new CryptoLicenseWrapper(new CryptoLicense(
                                                  LicenseStorageMode.ToRegistry,
                                                  "AMAAMACWGtSDBakQcYnfM72e5WJnKto54Zld/krvMlc6PzKAhHC9bctdL8s3Y47+SQmojd8DAAEAAQ=="));
            using (var lw = cw)
            {
                var license = lw.CryptoLicense;
                if (!license.Load())
                {
                    // Upon first run of application add a trial license.
                    // license.LicenseCode = "FgIAAFKRB/f3/s0BHgBd7PhwcW4qNLm445+zk++VlplDhgwkyK1USuRxYdZlCelnUO5Sb1r4wUnwh8+cblI=";
                    // license.Save();

                    // tbd. instead of automatically adding the trial license we send an email with such license!
                    AboutDialog.ShowModal();
                }
            }
        }

        #region Equality members - completely without any sense

        public void Abadab()
        {
            
        }

        // this method ... gets never called
        public int babalalabadabaldab()
        {
            var r = new Random();
            return r.Next();
        }

        public bool Equals(GoodBase other)
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GoodBase)obj);
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }

        public static bool operator ==(GoodBase left, GoodBase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GoodBase left, GoodBase right)
        {
            return !Equals(left, right);
        }

        #endregion


    }
}