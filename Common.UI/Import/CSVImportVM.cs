// copyright discretelogics 2012.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using NLog;
using TeaTime.Data;

namespace TeaTime.Data
{
    /// <summary>
    /// This class provides an import of CSV data files.<br>
    ///                                                 </br>
    /// The imported data will be saved as one of the types currently provided, which are OHLCV and Tick. In the future, arbitrary
    /// types will be available for import.<br></br>
    /// Steps required for import are
    /// <list type = "bullet">
    ///     <item>break each line into its fields. this requires the delimeter parameter.</item>
    ///     <item>assign each field to a field of the imported type's field</item>
    /// </list>
    /// The way the csv data is splitted into fields (line by line) is determined by the parameter <b>delimeter</b>. In addition, a number
    /// of files can be skipped.
    /// The assignment of the resulting fields per line to the data type that shall be imported is described by the Fields collection, whose
    /// Field items hold the name of the target type's field as well as the format used to parse the text of the field.
    /// </summary>
    public class CSVImportVM : NotifyPropertyChanged
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        #region state

        CSVImportParameters parameters;
        bool parsedOk;
        string warehouseRoot;

        #endregion

        #region life

        public CSVImportVM()
        {
            this.Preview = new ObservableCollection<PreviewCell[]>();
        }

        #endregion

        #region view

        public CSVImportParameters Parameters
        {
            get { return this.parameters; }
            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.SetProperty(ref this.parameters, value);
                this.parameters.PropertyChanged += this.OnParametersChange;

                this.UpdatePreview();
            }
        }

        public ObservableCollection<PreviewCell[]> Preview { get; private set; }

        public ObservableMruCollection<MruItem> MruFileNames { get; set; }
        public ObservableMruCollection<MruItem> MruTargetFileNames { get; set; }

        public bool IsConfigured
        {
            get
            {
                // check for parameters != null, otherwise dev app crashes
                return this.parameters.IsSet() && File.Exists(this.parameters.CSVFileName) && this.parsedOk;
            }
        }

        public string WarehouseRoot { get { return this.warehouseRoot; } set { this.warehouseRoot = value; } }

        public FieldTypeDescription[] AllFieldTypeDescs { get { return FieldTypeDescriptionManager.Instance.All; } }
        
        public void Import(ITextReporter textReporter)
        {
            TeaTime.Data.Import.ImportCSV(this.parameters.CSVFileName, this.Parameters.TargetFileName, this.parameters, this.Timescale, textReporter);
        }

        #endregion

        #region int

        public Timescale Timescale { get; set; }

        /// <summary>
        ///     Analyzes the file in order to suggests parameters.
        /// </summary>
        public static bool Analyze(string filename)
        {
            // find number of lines to skip
            using (StreamReader f = File.OpenText(filename))
            {
                // count characters and digits of first and second line
                string line1 = f.ReadLine();
                int digits1 = line1.ToCharArray().Count(Char.IsDigit);
                int characters1 = line1.ToCharArray().Count(Char.IsLetter);
                string line2 = f.ReadLine();
                int digits2 = line2.ToCharArray().Count(Char.IsDigit);
                int characters2 = line2.ToCharArray().Count(Char.IsLetter);

                //	if the numbers diverge for both, assume the first line is a header line
                return characters1 > characters2 && digits2 > digits1;
            }

            // find separator
            //using (var f = File.OpenText(fileName))
            //{
            //    var lines = 3.Times(f.ReadLine).ToArray();
            //    foreach (var line in lines)
            //    {
            //        var g = line.ToCharArray().Where(Char.IsSymbol).GroupBy(c => c);
            //        ... find symbol that have constant appearance in each line, including the first line, even if it should be skipped!
            //    }
            //}
        }

        /// <summary>
        ///     Called upon each change of Parameters, to update the Preview Grid.
        /// </summary>
        public void UpdatePreview()
        {
            if (!this.parameters.CSVFileName.IsSet()) return; // in unit tests, the filename might not be set
            if (!File.Exists(this.parameters.CSVFileName))
            {
                this.Preview.Clear();
                return;
            }

            int columns = 0;
            this.parsedOk = true;
            using (StreamReader f = File.OpenText(this.parameters.CSVFileName))
            {
                this.Preview.Clear();
                f.ReadLines(10)
                    .Skip(this.Parameters.GetLinesToSkip())
                    .ForEach(line =>
                        {
                            var cells = this.ParseLine(line);
                            this.Preview.Add(cells);
                            columns = Math.Max(columns, cells.Length);
                        });
            }

            if (this.parameters.FirstLineHoldsFieldNames)
            {
                var firstline = File.ReadLines(this.parameters.CSVFileName).FirstOrDefault();
                if (firstline.IsSet())
                {
                    var fieldnames = firstline.Split(this.parameters.GetDelimiters());
                    columns = Math.Max(columns, fieldnames.Length);
                    for (int i = 0; i < columns; i++)
                    {
                        var field = this.Parameters.GetAssignedField(i);
                        field.Name = fieldnames.Length > i ? fieldnames[i] : CSVImportParameters.GetDefaultFieldName(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.parameters.TeaFileFields.Count; i++)
                {
                    this.Parameters.GetAssignedField(i).Name = CSVImportParameters.GetDefaultFieldName(i);
                }
            }

            if (this.Preview.Any())
            {
                while (this.Parameters.TeaFileFields.Count > columns)
                {
                    this.Parameters.TeaFileFields.RemoveAt(this.Parameters.TeaFileFields.Count - 1);
                }
            }
        }

        internal string GetTeaFileNameFromTextFileName(string textFileFullName)
        {
            if (Directory.Exists(this.warehouseRoot))
            {
                return Path.Combine(this.warehouseRoot, Path.GetFileNameWithoutExtension(textFileFullName) + CommonUI.Constants.TeaFileExtension);
            }
            else
            {
                // if the warehouse does not exist, place the new tea-file beside the imported csv-file
                return Path.ChangeExtension(textFileFullName, CommonUI.Constants.TeaFileExtension);
            }
        }

        internal PreviewCell[] ParseLine(string line)
        {
            this.SetNumberAndDateFormats();
            var cells = new List<PreviewCell>();
            string[] values = line.Split(this.Parameters.GetDelimiters());
            for (int i = 0; i < values.Length; i++)
            {
                var cell = new PreviewCell();
                cell.Value = values[i];
                cell.Parsed = this.Parameters.GetAssignedField(i).CanAssign(cell.Value, this.Parameters.DecimalSeparator, this.parameters.DateTimeFormat);
                cells.Add(cell);

                this.parsedOk &= cell.Parsed;
            }
            return cells.ToArray();
        }

        public void UpdateParsing()
        {
            this.SetNumberAndDateFormats();
            this.parsedOk = true;
            foreach (var previewLine in this.Preview)
            {
                for (int i = 0; i < previewLine.Length; i++)
                {
                    PreviewCell cell = previewLine[i];
                    cell.Parsed = this.parameters.GetAssignedField(i).CanAssign(cell.Value, this.Parameters.DecimalSeparator, this.parameters.DateTimeFormat);
                    if (!cell.Parsed) this.parsedOk = false;
                }
            }
        }

        void SetNumberAndDateFormats()
        {
            FieldTypeDescriptionManager.Instance.DateTimeFormat = this.parameters.DateTimeFormat;
            FieldTypeDescriptionManager.Instance.NumberFormat = this.parameters.GetNumberFormat();
        }

        void OnParametersChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FirstLineHoldsFieldNames")
            {
                this.UpdatePreview();
            }
            else if (e.PropertyName == "FieldDelimeters")
            {
                this.UpdatePreview();
            }
            else if (e.PropertyName.IsOneOf("DecimalSeparator", "DateTimeFormat"))
            {
                this.UpdateParsing();
            }
            else if (e.PropertyName == "CSVFileName")
            {
                // we *could* be more sensible and leave a maybe set folder in tact and change only the filename. lets see how usage turns out
                this.Parameters.TargetFileName = this.GetTeaFileNameFromTextFileName(this.parameters.CSVFileName);

                this.parameters.FirstLineHoldsFieldNames = Analyze(this.parameters.CSVFileName);
                this.UpdatePreview();
            }
        }

        #endregion
    }
}
