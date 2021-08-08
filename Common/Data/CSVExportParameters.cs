// copyright discretelogics 2013.

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;

namespace TeaTime.Data
{
    public class CSVExportParameters : NotifyPropertyChanged
    {
        string dateFormat;
        string decimalSeparator;
        string delimeter;        
        bool recursive;
        string sourceFileOrFolder;
        string targetFolder;
        string timeFormat;
        bool writeFieldNames;

        public CSVExportParameters()
        {
            this.recursive = false;
            this.writeFieldNames = true;
            this.delimeter = ";";
            var ci = new CultureInfo("en-US"); // financial ASCII data is traditionally formatted in english culture
            this.decimalSeparator = ci.NumberFormat.NumberDecimalSeparator;
            this.dateFormat = ci.DateTimeFormat.ShortDatePattern;
            this.timeFormat = ci.DateTimeFormat.ShortTimePattern;
        }

        public bool Recursive { get { return this.recursive; } set { this.SetProperty(ref this.recursive, value); } }

        public string DateFormat { get { return this.dateFormat; } set { this.SetProperty(ref this.dateFormat, value); } }

        public string TimeFormat { get { return this.timeFormat; } set { this.SetProperty(ref this.timeFormat, value); } }

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
                return Directory.Exists(this.TargetFolder) && (File.Exists(this.SourceFileOrFolder) || Directory.Exists(this.SourceFileOrFolder)) &&
                       !String.IsNullOrEmpty(this.Delimeter);
            }
        }
    }
}
