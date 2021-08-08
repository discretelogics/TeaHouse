// copyright discretelogics 2012.

using System;
using System.IO;
using System.Linq;

namespace TeaTime.Data
{
    class Export
    {
        public static string GetExportedCSVPreview(string teaFileName, CSVExportParameters parameters)
        {
            using (var tf = TeaFile.OpenRead(teaFileName))
            using (var sw = new StringWriter())
            {
                var delimeter = Util.ParseDelimeter(parameters.Delimeter);
                var id = tf.Description.ItemDescription;
                if (parameters.WriteFieldNames)
                {
                    sw.WriteLine(id.Fields.Select(f => f.Name).Joined(delimeter));
                }
                tf.Items.Take(10).ForEach(item => sw.WriteLine(id.GetValueString(item, delimeter, parameters.DateTimeFormat, parameters.GetNumberFormat())));
                return sw.ToString();
            }
        }

        public static void ExportToCSV(string sourceFileName,
                                       string targetFileName,
                                       CSVExportParameters parameters,
                                       ITextReporter textReporter,
                                       IProgressReporter progressReporter)
        {
            using (var tf = TeaFile.OpenRead(sourceFileName))
            {
                uint total = (uint)tf.Items.Count.Limit(uint.MaxValue);
                if (textReporter != null)
                {
                    textReporter.WriteLine("Exporting {0}: {1} items of type {2} to file {3}".Formatted(
                        sourceFileName,
                        tf.Items.Count,
                        tf.Description.ItemDescription.ItemTypeName,
                        targetFileName));
                }
                using (var csv = File.CreateText(targetFileName))
                {
                    var delimeter = Util.ParseDelimeter(parameters.Delimeter);
                    var id = tf.Description.ItemDescription;
                    if (parameters.WriteFieldNames)
                    {
                        csv.WriteLine(id.Fields.Select(f => f.Name).Joined(delimeter));
                    }
                    uint progress = 0;
                    if (progressReporter != null) progressReporter.ReportProgress("Exporting", progress, total);
                    foreach (var item in tf.Items)
                    {
                        csv.WriteLine(id.GetValueString(item, delimeter, parameters.DateTimeFormat, parameters.GetNumberFormat()));
                        (++progress).OnEvery(100, () =>
                            {
                                if (progressReporter != null)
                                {
                                    progressReporter.ReportProgress("Exporting", progress, total);
                                }
                            });
                    }
                    if (progressReporter != null) progressReporter.ReportProgress("Export complete", progress, total);
                }
            }
        }
    }
}
