using System;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TeaTime
{
    public static class IOUtils
    {
        public static bool IsFile(string fullPath)
        {
            return !IsFolder(fullPath);
        }
        public static bool IsFolder(string fullPath)
        {
            return File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory);
        }

        public static string GetDirectoryName(string directory)
        {
            return new DirectoryInfo(directory).Name;
        }

        [DebuggerStepThrough]
        public static bool IsFileInUse(string file)
        {
            if (!File.Exists(file)) return false;
            try
            {
                using (File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }

        public static string GetComparablePath(string path)
        {
            Guard.ArgumentNotNull(path, "path");

            return Path.GetFullPath(path).TrimEnd('\\', '/').ToLower();
        }

        public static bool AreEqualPaths(string path1, string path2)
        {
            return GetComparablePath(path1) == GetComparablePath(path2);
        }

        public static string GetUniqueDirectory(string parentDirectory, string directoryName)
        {
            string targetFullname = Path.Combine(parentDirectory, directoryName);
            int i = 1;
            while (Directory.Exists(targetFullname))
            {
                targetFullname = Path.Combine(parentDirectory, String.Format("{0} ({1})", directoryName, i++));
            }
            return targetFullname;
        }

        public static string GetUniqueFile(string parentDirectory, string fileName, string extension)
        {
            string targetFullname = Path.Combine(parentDirectory, fileName + extension);
            int i = 1;
            while (File.Exists(targetFullname))
            {
                targetFullname = Path.Combine(parentDirectory, String.Format("{0} ({1})", fileName, i++) + extension);
            }
            return targetFullname;
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathCompactPathEx(
            StringBuilder pszOut,
            string pszPath,
            int cchMax,
            int reserved);

        static readonly Range compactPathRange = new Range(15, 255);
        public static string GetCompactPath(string fullname, int length)
        {
            length = compactPathRange.EnsureContained(length);
            StringBuilder pszOut = new StringBuilder(length * 3);
            if (PathCompactPathEx(pszOut, fullname, length, 0))
            {
                return pszOut.ToString();
            }
            else
            {
                return fullname;
            }
        }

        public static string PathCombine(this string directory, params string[] parts)
        {
            string combined = directory;
            parts.ForEach(part => combined = Path.Combine(combined, part));
            return combined;
        }

        public static FileInfo CombineToFile(this DirectoryInfo directory, string fileName)
        {
            return new FileInfo(directory.FullName.PathCombine(fileName));
        }

        public static void Clear(this DirectoryInfo directoryInfo)
        {
            directoryInfo.EnumerateFiles().ForEach(f => f.Delete());
            directoryInfo.EnumerateDirectories().ForEach(d => d.Delete(true));
        }

        public static void EnsureExists(this FileInfo fi)
        {
            if (!fi.Exists) throw new FileNotFoundException(fi.FullName);
        }

        public static string ConvertToRelativePath(this string root, string absolutePath)
        {
            if (String.IsNullOrWhiteSpace(root) || String.IsNullOrWhiteSpace(absolutePath))
            {
                return absolutePath;
            }

            root = root.TrimEnd(new[] { '\\', '/' }) + "\\";

            Uri rootUri;
            Uri absoluteUri;

            if (!Uri.TryCreate(root, UriKind.Absolute, out rootUri) ||
                !Uri.TryCreate(absolutePath, UriKind.Absolute, out absoluteUri))
            {
                return absolutePath;
            }

            if (!rootUri.IsBaseOf(absoluteUri))
            {
                return absolutePath;
            }

            Uri relativeUri = rootUri.MakeRelativeUri(absoluteUri);
            return relativeUri.ToString();
        }

        public static string ConvertToAbsolutePath(this string root, string relativePath)
        {
            if (String.IsNullOrWhiteSpace(root) || String.IsNullOrWhiteSpace(relativePath))
            {
                return relativePath;
            }
            return Path.Combine(root, relativePath);
        }

        static readonly string[] invalidnames = new string[] { "AUX", "CLOCK$", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "CON", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "NUL", "PRN" };

        public static string LegalizeFilename(string filename)
        {            
            foreach (var invalidname in invalidnames)
            {
                if (string.Compare(filename, invalidname, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return "_" + filename;
                }
                if (filename.StartsWith(invalidname + ".", StringComparison.InvariantCultureIgnoreCase))
                {
                    return "_" + filename;
                }
            }
            return filename;
        }

        public static string LegalizePath(string path)
        {
            return new string(path.Where(c => !Path.GetInvalidPathChars().Contains(c)).ToArray());
        }

        static char[] directorySeperators;

        public static char[] DirectorySeparators
        {
            get
            {
                if (directorySeperators == null)
                {
                    directorySeperators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
                }
                return directorySeperators;
            }
        }

        #region using wraps

        public static T ReadFile<T>(string filename, Func<Stream, T> f)
        {
            using (var stream = File.OpenRead(filename))
            {
                return f(stream);
            }
        }

        public static void WriteNewFile(string filename, Action<Stream> a, FileMode fm = FileMode.CreateNew)
        {
            using (var stream = new FileStream(filename, fm))
            {
                a(stream);
            }
        }

        #endregion

        static readonly string[] sizeStrings = { "B", "KB", "MB", "GB" };
        public static string GetFileSizeString(string fullname)
        {
            double len = new FileInfo(fullname).Length;
            int order = 0;
            while (len >= 1024 && order + 1 < sizeStrings.Length)
            {
                order++;
                len = len / 1024;
            }
            return String.Format("{0:0.##} {1}", len, sizeStrings[order]);
        }

        public static bool CanDiscover(string directory)
        {
            try
            {
                var accessControlList = Directory.GetAccessControl(directory);
                var accessRules = accessControlList.GetAccessRules(true, true, typeof (System.Security.Principal.SecurityIdentifier));

                var listAllow = false;

                foreach (FileSystemAccessRule rule in accessRules)
                {
                    if ((FileSystemRights.ListDirectory & rule.FileSystemRights) != FileSystemRights.ListDirectory) continue;

                    if (rule.AccessControlType == AccessControlType.Allow)
                        listAllow = true;
                    else if (rule.AccessControlType == AccessControlType.Deny)
                        return false;
                }

                return listAllow;
            }
            catch
            {
                return false;
            }
        }
    }
}
