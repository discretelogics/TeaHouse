// copyright discretelogics 2012.

using System;
using System.IO;
using System.Linq;
using LogicNP.CryptoLicensing;

namespace TeaTime.Special
{
    public class License : NotifyPropertyChanged
    {
        public License(CryptoLicense license)
        {
            if (license.Load())
            {
                this.licenseKey = license.LicenseCode;
                this.Assign(license);
            }
            else
            {
                this.Message = "Missing license.";
            }
        }

        void Assign(CryptoLicense license)
        {
            this.IsTrial = license.IsEvaluationLicense();
            this.IsValid = license.ValidateSignature();
            if (!this.IsValid)
            {
                this.Message = String.IsNullOrWhiteSpace(this.LicenseKey) ? "Missing license." : "Invalid license key.";
            }
            else if (this.IsTrial)
            {
                this.TrialDaysLeft = license.RemainingUsageDays;
                if (license.Status == LicenseStatus.Valid)
                {
                    this.Message = "Trial license expires in {0} days.".Formatted(license.RemainingUsageDays);
                }
                else
                {
                    this.Message = GetInvalidMessage(license.Status);
                    this.IsValid = false;
                }
                this.Licensee = "Trial License";
            }
            else
            {
                if (license.Status == LicenseStatus.Valid)
                {
                    this.Message = "License valid.";
                }
                else
                {
                    this.Message = GetInvalidMessage(license.Status);
                    this.IsValid = false;
                }
                this.Licensee = license.UserData;
            }
            this.CanStore = this.IsValid || String.IsNullOrWhiteSpace(this.LicenseKey);
        }
        string GetInvalidMessage(LicenseStatus status)
        {
            if (status == LicenseStatus.Expired || 
                status == LicenseStatus.EvaluationExpired ||
                status == LicenseStatus.UsageDaysExceeded ||
                status == LicenseStatus.EvaluationlTampered ||
                status == LicenseStatus.ExecutionsExceeded ||
                status == LicenseStatus.UniqueUsageDaysExceeded ||
                status == LicenseStatus.RunTimeExceeded)
                return "License expired.";
            return "License invalid.";
        }

        int trialDaysLeft;
        public int TrialDaysLeft { get { return this.trialDaysLeft; } set { this.SetProperty(ref this.trialDaysLeft, value); } }

        bool isTrial;
        public bool IsTrial { get { return this.isTrial; } set { this.SetProperty(ref this.isTrial, value); } }

        bool isValid;
        public bool IsValid { get { return this.isValid; } set { this.SetProperty(ref this.isValid, value); } }

        bool canStore;
        public bool CanStore { get { return this.canStore; } set { this.SetProperty(ref this.canStore, value); } }

        string licensee;
        public string Licensee { get { return this.licensee; } set { this.SetProperty(ref this.licensee, value); } }

        string message;
        public string Message { get { return this.message; } set { this.SetProperty(ref this.message, value); } }

        string licenseKey;
        public string LicenseKey
        {
            get
            {
                return this.licenseKey;
            }
            set
            {
                if (this.SetProperty(ref this.licenseKey, value))
                {
                    using (var cw = new CryptoLicenseWrapper(new CryptoLicense(LicenseStorageMode.ToRegistry,
                                                                               "AMAAMACWGtSDBakQcYnfM72e5WJnKto54Zld/krvMlc6PzKAhHC9bctdL8s3Y47+SQmojd8DAAEAAQ==")))
                    {
                        cw.CryptoLicense.LicenseCode = value;
                        this.Assign(cw.CryptoLicense);
                    }
                }
            }
        }
    }
}
