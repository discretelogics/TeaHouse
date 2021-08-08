using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaTime.Data;

namespace TeaTime.Data
{    
    class Import
    {
        // always creates the target folder if it does not exist, witout asking. add parameter to control this if needed.
        public static void ImportCSV(string csvSourceFile, string targetTeaFile, CSVImportParameters p, Timescale timescale, ITextReporter textReporter)
        {
            if (csvSourceFile == null) throw new ArgumentNullException("csvSourceFile");
            if (targetTeaFile == null) throw new ArgumentNullException("targetTeaFile");
            if (p == null) throw new ArgumentNullException("p");
            if (!p.TeaFileFields.Any()) throw new ArgumentException("CSV import parameters do not specify any assigned fields");
            textReporter = textReporter ?? new NullTextReporter();

            textReporter.WriteLine("Importing {0} into {1}".Formatted(csvSourceFile, targetTeaFile));

            FieldTypeDescriptionManager.Instance.DateTimeFormat = p.DateTimeFormat;
            FieldTypeDescriptionManager.Instance.NumberFormat = p.GetNumberFormat();

            // ensure targetdir
            string targetDirectory = Path.GetDirectoryName(targetTeaFile);
            try
            {
                Directory.CreateDirectory(targetDirectory);
            }
            catch (Exception)
            {
                throw new Exception("Failed to create target Directory {0}".Formatted(targetDirectory));
            }

            using (var stream = new FileStream(targetTeaFile, FileMode.Create))
            {
                var fields = p.TeaFileFields.Select(f => new Field { FieldType = f.FieldTypeDesc.Type, Name = f.Name, IsTime = f.FieldType == "time" });
                ItemDescription id = ItemDescription.CreateUntyped(fields.ToArray(), p.TargetTypeName);
                TeaFileDescription desc = new TeaFileDescription();
                desc.ItemDescription = id;
                desc.Timescale = Timescale.Java;
                var w = TeaFile.Create(stream, desc, false);
                // write values
                IEnumerable<string> lines = File.ReadLines(csvSourceFile);
                var delimeters = p.GetDelimiters();
                int linecount = 0;
                foreach (var line in lines.Skip(p.GetLinesToSkip()))
                {
                    string[] arr = line.Split(delimeters);
                    object[] values = arr.Take(p.TeaFileFields.Count) //if less fields are specified than elements in the line, restrict. this is safe code execution. to avoid unexpected behavior check arguments elsewhere
                                         .Select((text, i) => p.TeaFileFields[i].Parse(text)).ToArray();
                    w.Write(values);
                    linecount++;
                    if (linecount % 50000 == 0)
                    {
                        textReporter.WriteLine("Imported {0} lines ...".Formatted(linecount));
                    }
                }
                textReporter.WriteLine("Import completed. Imported {0} lines into {1}".Formatted(linecount, targetTeaFile));
            }
        }
    }
}
