// copyright discretelogics 2013.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TeaTime.Base;

namespace TeaTime.Data
{
    public class CSVImportParameters : NotifyPropertyChanged
    {
        #region life

        public CSVImportParameters()
        {
            this.targetTypeName = String.Empty;
            this.firstLineHoldsFieldNames = true;
            this.fieldDelimeters = CsvFormats.FieldDelimiter;
            this.decimalSeparator = CsvFormats.DecimalSeparator;
            this.dateTimeFormat = CsvFormats.DateTimeFormat;
            this.teaFileFields = new ObservableCollection<CSVFieldMapping>();
        }

        #endregion

        #region state

        // source
        bool firstLineHoldsFieldNames;
        string fieldDelimeters;
        string decimalSeparator;
        string dateTimeFormat;
        // source -> target
        ObservableCollection<CSVFieldMapping> teaFileFields;
        // target
        string targetTypeName;
        bool overwriteExistingFile;
        string csvFileName;
        string targetFileName;

        #endregion

        #region core

        public bool FirstLineHoldsFieldNames { get { return this.firstLineHoldsFieldNames; } set { this.SetProperty(ref this.firstLineHoldsFieldNames, value); } }
        public string FieldDelimeters { get { return this.fieldDelimeters; } set { this.SetProperty(ref this.fieldDelimeters, value); } }
        public string DecimalSeparator { get { return this.decimalSeparator; } set { this.SetProperty(ref this.decimalSeparator, value); } }
        public string DateTimeFormat { get { return this.dateTimeFormat; } set { this.SetProperty(ref this.dateTimeFormat, value); } }
        public string TargetTypeName { get { return this.targetTypeName; } set { this.SetProperty(ref this.targetTypeName, value); } }
        public bool OverwriteExistingFile { get { return this.overwriteExistingFile; } set { this.SetProperty(ref this.overwriteExistingFile, value); } }        
        public ObservableCollection<CSVFieldMapping> TeaFileFields { get { return this.teaFileFields; } }
        public string CSVFileName { get { return this.csvFileName; } set { this.SetProperty(ref this.csvFileName, value); } }
        public string TargetFileName { get { return this.targetFileName; } set { this.SetProperty(ref this.targetFileName, value); } }

        #endregion

        #region int

        public int GetLinesToSkip()
        {
            return this.firstLineHoldsFieldNames ? 1 : 0;
        }

        public NumberFormatInfo GetNumberFormat()
        {
            var nfi = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
            if (this.decimalSeparator.IsSet()) // empty separator is illegal, in this case we take the current culture. might be subtle but is same as whitespace is used for field delimeter - seems to be a nice behavior
            {
                nfi.NumberDecimalSeparator = this.decimalSeparator;
            }
            return nfi;
        }

        // considers tabs
        public char[] GetDelimiters()
        {
            return Util.ParseDelimeter(this.FieldDelimeters).ToCharArray();
        }

        public CSVFieldMapping GetAssignedField(int i)
        {
            while (i > this.TeaFileFields.Count - 1)
            {
                this.TeaFileFields.Add(new CSVFieldMapping(FieldTypeDescriptionManager.Instance.All.First(), GetDefaultFieldName(this.TeaFileFields.Count)));
            }
            return this.TeaFileFields[i];
        }

        public static string GetDefaultFieldName(int index)
        {
            int n = index + 1;
            return "Field" + n;
        }

        #endregion
    }
}
