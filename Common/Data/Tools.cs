// copyright discretelogics 2013.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace TeaTime.Data
{
    public static class Tools
    {
        public static void ExportToCSV(string sourceFileName, string targetFileName, CSVExportParameters parameters,
                                       ITextReporter textReporter,
                                       IProgressReporter progressReporter)
        {
            using (var tf = TeaFile.OpenRead(sourceFileName))
            {
                uint total = (uint)tf.Items.Count.Limit(uint.MaxValue);
                if (textReporter != null)
                {
                    textReporter.WriteLine("Exporting {0}: {1} items of type {2}".Formatted(
                        sourceFileName,
                        tf.Items.Count,
                        tf.Description.ItemDescription.ItemTypeName));
                }

                var previous = SetCultureInfo(parameters);
                try
                {
                    using (var csv = File.CreateText(targetFileName))
                    {
                        var delimeter = ParseDelimeter(parameters.Delimeter);
                        var id = tf.Description.ItemDescription;
                        if (parameters.WriteFieldNames)
                        {
                            csv.WriteLine(id.Fields.Select(f => f.Name).Joined(delimeter));
                        }
                        uint progress = 0;
                        if (progressReporter != null) progressReporter.ReportProgress("Exporting", progress, total, LengthyOperation.Start);
                        foreach (var item in tf.Items)
                        {
                            csv.WriteLine(id.GetValueString(item, delimeter));
                            (++progress).OnEvery(100, () =>
                                {
                                    if (progressReporter != null)
                                    {
                                        progressReporter.ReportProgress("Exporting", progress, total, LengthyOperation.Proceeding);
                                    }
                                });
                        }
                        if (progressReporter != null) progressReporter.ReportProgress("Exporting", progress, total, LengthyOperation.Done);
                    }
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = previous;
                }
            }
        }

        static CultureInfo SetCultureInfo(CSVExportParameters parameters)
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            CultureInfo previous = (CultureInfo)ci.Clone();
            ci.NumberFormat.NumberDecimalSeparator = parameters.DecimalSeparator;
            ci.DateTimeFormat.ShortDatePattern = parameters.DateFormat;
            ci.DateTimeFormat.LongTimePattern = parameters.TimeFormat;
            Thread.CurrentThread.CurrentCulture = ci;
            return previous;
        }

        public static string GetCSVPreview(string filename, CSVExportParameters parameters)
        {
            var previous = SetCultureInfo(parameters);
            try
            {
                using (var tf = TeaFile.OpenRead(filename))
                using (var sw = new StringWriter())
                {
                    var delimeter = ParseDelimeter(parameters.Delimeter);
                    var id = tf.Description.ItemDescription;
                    if (parameters.WriteFieldNames)
                    {
                        sw.WriteLine(id.Fields.Select(f => f.Name).Joined(delimeter));
                    }
                    tf.Items.Take(10).ForEach(item => sw.WriteLine(id.GetValueString(item, delimeter)));
                    return sw.ToString();
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }

        public static string ParseDelimeter(string delimeter)
        {
            if (delimeter == null)
                return null;
            return delimeter.Replace(@"\t", "\t");
        }
    }
}
