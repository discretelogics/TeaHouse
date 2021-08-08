// copyright discretelogics 2013.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NLog;
using TeaTime.Data;
using TeaTime.VSX;

namespace TeaTime.Yahoo
{
    class DownloadTask
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Path { get; set; }
    }

    public class Downloader
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        #region scheduling

        public static void ScheduleSymbolUpdate(string symbol, string name)
        {
            var d = YahooPackage.Instance.Options.GetDownloadDirectoryEnsured();
            if (!Directory.Exists(d))
            {
                try
                {
                    Directory.CreateDirectory(d);
                    YahooPackage.Instance.WriteMessage("Created download directory {0}".Formatted(d));
                }
                catch (Exception ex)
                {
                    YahooPackage.Instance.WriteMessage(ex.Message);
                    return;
                }
            }

            var path = GetTeaFileName(symbol, d);
            YahooPackage.Instance.WriteMessage(string.Format("Schedule download of '{0}' '{1}' to '{2}'", name, symbol, path));
            Task.Factory.StartNew(SymbolUpdateAsync, new DownloadTask { Symbol = symbol, Path = path, Name = name });
        }

        static void SymbolUpdateAsync(object task)
        {
            try
            {
                var t = (DownloadTask)task;
                CreateOrUpdate(t.Path, t.Symbol, t.Name);
            }
            catch (Exception ex)
            {
                YahooPackage.Instance.WriteMessage(ex.Message);
            }

        }

        public static List<Event<OHLCV>> DownloadYahooData(string symbol, DateTime? startDate, DateTime? endDate)
        {
            string url = null;
            try
            {
                DateTime start = startDate.HasValue ? startDate.Value : DateTime.Now.AddYears(-5);
                DateTime end = endDate.HasValue ? endDate.Value : DateTime.Now;

                symbol = AliasedSymbol(symbol);

                url = @"http://ichart.finance.yahoo.com/table.csv?s=" + symbol +
                      "&a=" + (start.Month - 1) +
                      "&b=" + start.Day +
                      "&c=" + start.Year +
                      "&d=" + (end.Month - 1) +
                      "&e=" + end.Day +
                      "&f=" + end.Year +
                      "&g=d&ignore=.csv";

                // GetTeaHousePackage.InstanceWindow().WriteLine(url);

                // download
                string values;
                using (var wc = new WebClient())
                {
                    values = wc.DownloadString(url);
                    if (!values.IsSet()) return null;
                }

                // convert string to bars
                var bars = GetBarsFromString(values);

                YahooPackage.Instance.WriteMessage("Received {0} values from {1}".Formatted(bars.Count, url));
                if (bars.Any())
                {
                    YahooPackage.Instance.WriteMessage(" Last value: " + bars.First());
                }

                return bars;
            }
            catch (Exception ex)
            {
                YahooPackage.Instance.WriteMessage("Data download from yahoo failed: {0}. url:'{1}'".Formatted(ex.Message, url));
                logger.Error(ex);
                throw;
            }
        }

        static string AliasedSymbol(string symbol)
        {
            string[] aliases = YahooPackage.Instance.Options.SymbolAliases.Split(';');
            foreach (var aa in aliases.Where(a => a.StartsWith(symbol, StringComparison.InvariantCultureIgnoreCase)))
            {
                var parts = aa.Split('=');
                if (parts.Length == 2)
                {
                    if (parts[0].EqualsIgnoreCase(symbol)) return parts[1];
                }
                else
                {
                    YahooPackage.Instance.WriteError("Yahoo Aliases configuration is illformed: {0}".Formatted(aa));
                }

            }
            return symbol; // no change
        }

        #endregion

        #region file

        /// <summary>
        ///     Opens the TeaFile, searches for the Yahoo symbol in its description. If available, downloads
        ///     new data from Yahoo and appends it to the file.
        /// </summary>
        /// <param name = "fullName">The full name.</param>
        public static void UpdateTeaFile(string fullName)
        {
            try
            {
                YahooPackage.Instance.WriteMessage("Updating existing file " + fullName);

                List<Event<OHLCV>> bars;
                var lastTime = DateTime.MinValue;
                using (var ts = TeaFile<Event<OHLCV>>.OpenRead(fullName))
                {
                    if (!ts.Description.NameValues.SafeAny(nv => nv.Name == "YahooSymbol"))
                    {
                        YahooPackage.Instance.WriteMessage(fullName + ": File has no Yahoo Symbol and is ignored.");
                        return; // this file has no yahoo symbol
                    }

                    var symbol = ts.Description.NameValues.GetValue<string>("YahooSymbol");
                    if (ts.Count == 0)
                    {
                        YahooPackage.Instance.WriteMessage("File has not data, downloading all values");
                        bars = DownloadYahooData(symbol, null, null);
                    }
                    else
                    {
                        YahooPackage.Instance.WriteMessage("File already holds data, downloading additional values");
                        lastTime = ts.Items[ts.Count - 1].Time;
                        bars = DownloadYahooData(symbol, lastTime, null);
                    }
                }
                int count = 0;
                var file = TeaFile<Event<OHLCV>>.Append(fullName);
                bars.ForEachReverse(ohlcv =>
                    {
                        if (ohlcv.Time > (Time)lastTime)
                        {
                            file.Write(ohlcv);
                            count++;
                        }
                    });
                YahooPackage.Instance.WriteMessage("Added " + count + " items");
            }
            catch (Exception ex)
            {
                YahooPackage.Instance.WriteMessage("Error downloading data from Yahoo: " + ex.Message);
            }
        }

        public static void UpdateTeaFolder(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.EnumerateFiles(folder).ForEach(UpdateTeaFile);
                Directory.EnumerateDirectories(folder).ForEach(UpdateTeaFolder);
            }
        }

        public static void CreateFile(string path, string symbol, string name)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (symbol == null) throw new ArgumentNullException("symbol");
            if (name == null) throw new ArgumentNullException("name");

            YahooPackage.Instance.WriteMessage("creating new file '{0}'".Formatted(path));

            var bars = DownloadYahooData(symbol, null, null);
            using (var file = TeaFile<Event<OHLCV>>.Create(path, name, new NameValueCollection().Add("YahooSymbol", symbol)))
            {
                bars.ForEachReverse(file.Write);
            }
        }

        public static void CreateOrUpdate(string fullFileName, string symbol, string name)
        {
            if (File.Exists(fullFileName))
            {
                UpdateTeaFile(fullFileName);
            }
            else
            {
                CreateFile(fullFileName, symbol, name);
            }
        }

        // remove invalid chars + alias illegal filenames
        static string GetTeaFileName(string symbol, string folder)
        {
            string filename = symbol.Replace("^", "");
            filename = IOUtils.LegalizeFilename(filename);
            return Path.Combine(folder, filename) + YahooConstants.TeaFileExtension;
        }

        #endregion

        #region data

        static List<Event<OHLCV>> GetBarsFromString(string values)
        {
            using (TextReader tr = new StringReader(values)) // actually TextReader.Dispose() is a noop, but this way we please CA
            {
                var format = new CultureInfo("en-US");
                string fields = tr.ReadLine(); // reads header
                if (fields == null)
                {
                    throw new Exception("Received no data from Yahoo.");
                }
                if (!fields.StartsWith("Date,Open,High,Low,Close,Volume"))
                {
                    throw new Exception("Unexpected order of fields.");
                }
                var bars = new List<Event<OHLCV>>();
                while (true)
                {
                    string s = tr.ReadLine();
                    if (s == null)
                    {
                        break;
                    }
                    string[] arr = s.Split(',');

                    var value = new OHLCV();
                    value.Open = double.Parse(arr[1], format);
                    value.High = double.Parse(arr[2], format);
                    value.Low = double.Parse(arr[3], format);
                    value.Close = double.Parse(arr[4], format);
                    value.Volume = long.Parse(arr[5], format);

                    bars.Add(new Event<OHLCV>(DateTime.Parse(arr[0]), value));
                }
                return bars;
            }
        }

        #endregion
    }
}
