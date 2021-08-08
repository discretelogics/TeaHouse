// copyright discretelogics 2012.

using System;
using System.Globalization;
using System.IO;
using TeaTime.Base;

namespace TeaTime.Data
{
    public class CSVExportParameters : NotifyPropertyChanged
    {
        string dateTimeFormat;
        string decimalSeparator;
        string delimeter;
        bool recursive;
        string sourceFileOrFolder;
        string targetFolder;
        bool writeFieldNames;

        public CSVExportParameters()
        {
            this.recursive = false;
            this.writeFieldNames = true;
            this.delimeter = CsvFormats.FieldDelimiter;
            this.decimalSeparator = CsvFormats.DecimalSeparator;
            this.dateTimeFormat = CsvFormats.DateTimeFormat;
        }

        public bool Recursive { get { return this.recursive; } set { this.SetProperty(ref this.recursive, value); } }
        
        public string DateTimeFormat { get { return this.dateTimeFormat; } set { this.SetProperty(ref this.dateTimeFormat, value); } }

        public string DecimalSeparator { get { return this.decimalSeparator; } set { this.SetProperty(ref this.decimalSeparator, value); } }

        public string SourceFileOrFolder
        {
            get { return this.sourceFileOrFolder; }
            set
            {
                this.SourceIsFolder = Directory.Exists(value);
                this.SetProperty(ref this.sourceFileOrFolder, value);
            }
        }

        public bool SourceIsFolder { get; private set; }

        public string TargetFolder { get { return this.targetFolder; } set { this.SetProperty(ref this.targetFolder, value); } }

        public string Delimeter { get { return this.delimeter; } set { this.SetProperty(ref this.delimeter, value); } }

        public bool WriteFieldNames { get { return this.writeFieldNames; } set { this.SetProperty(ref this.writeFieldNames, value); } }

        public bool IsConfigured
        {
            get
            {
                return this.TargetFolder.IsSet() && (File.Exists(this.SourceFileOrFolder) || Directory.Exists(this.SourceFileOrFolder)) &&
                       !String.IsNullOrEmpty(this.Delimeter);
            }
        }

        public NumberFormatInfo GetNumberFormat()
        {
            return new NumberFormatInfo {NumberDecimalSeparator = this.decimalSeparator};
        }
    }
}
