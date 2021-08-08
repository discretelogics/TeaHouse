using System;
using System.Collections.Generic;
using System.Linq;

namespace TeaTime.Data
{
    public class SafeTeaFileAccessor : ITeaFile
    {
        #region ctor

        public SafeTeaFileAccessor(ITeaFile teaFile)
        {
            Guard.ArgumentNotNull(teaFile, "teafile");

            this.teaFile = teaFile;
        }

        #endregion

        #region events

        public event EventHandler<Exception> DataAccessFailed;

        void OnDataAccessFailed(Exception ex)
        {
            if (DataAccessFailed != null)
            {
                DataAccessFailed(this, ex);
            }
        }

        #endregion

        #region ITeaFile wrapper

        public long Count
        {
            get
            {
                try
                {
                    return this.teaFile.Count;
                }
                catch (Exception ex)
                {
                    OnDataAccessFailed(ex);
                    return 0;
                }
            }
        }

        public DateTime TimeAt(long index)
        {
            try
            {
                return teaFile.TimeAt(index);
            }
            catch (Exception ex)
            {
                this.OnDataAccessFailed(ex);
                return DateTime.MinValue;
            }
        }

        public TeaFileDescription Description
        {
            get
            {
                try
                {
                    return teaFile.Description;
                }
                catch (Exception ex)
                {
                    this.OnDataAccessFailed(ex);
                    return null;
                }
            }
        }

        public string Name
        {
            get
            {
                try
                {
                    return teaFile.Name;
                }
                catch (Exception ex)
                {
                    this.OnDataAccessFailed(ex);
                    return String.Empty;
                }
            }
            set { teaFile.Name = value; }
        }

        public Type ItemType
        {
            get
            {
                try
                {
                    return teaFile.ItemType;
                }
                catch (Exception ex)
                {
                    this.OnDataAccessFailed(ex);
                    return typeof (double);
                }
            }
        }

        public IItemCollection<T> GetItemsConverted<T>(IEnumerable<IFieldMapping> fieldMappings) where T : struct
        {
            return new SafeItemCollection<T>(this, fieldMappings);
        }

        public string GetFieldValueText(Field f, long index)
        {
            try
            {
                return teaFile.GetFieldValueText(f, index);
            }
            catch (Exception ex)
            {
                this.OnDataAccessFailed(ex);
                return String.Empty;
            }
        }

        public string YScaleName
        {
            get
            {
                try
                {
                    return teaFile.YScaleName;
                }
                catch (Exception ex)
                {
                    this.OnDataAccessFailed(ex);
                    return String.Empty;
                }
            }
        }

        public void Dispose()
        {
            teaFile.Dispose();
        }

        public IItemLines ItemLines
        {
            get
            {
                return new SafeItemLines(this);
            }
        }

        #endregion

        #region fields

        readonly ITeaFile teaFile;

        #endregion

        #region embedded types

        class SafeItemCollection<T> : IItemCollection<T> where T : struct
        {
            public SafeItemCollection(SafeTeaFileAccessor parent, IEnumerable<IFieldMapping> fieldMappings)
            {
                this.parent = parent;
                this.itemCollection = parent.teaFile.GetItemsConverted<T>(fieldMappings);
            }

            readonly SafeTeaFileAccessor parent;
            readonly IItemCollection<T> itemCollection;

            public T this[long index]
            {
                get
                {
                    try
                    {
                        return itemCollection[index];
                    }
                    catch (Exception ex)
                    {
                        parent.OnDataAccessFailed(ex);
                        return default(T);
                    }
                }
            }
        }

        class SafeItemLines : IItemLines
        {
            public SafeItemLines(SafeTeaFileAccessor parent)
            {
                this.parent = parent;
                this.itemLines = parent.teaFile.ItemLines;
            }

            readonly SafeTeaFileAccessor parent;
            readonly IItemLines itemLines;

            public string GetLineText(long index)
            {
                try
                {
                    return itemLines.GetLineText(index);
                }
                catch (Exception ex)
                {
                    parent.OnDataAccessFailed(ex);
                    return String.Empty;
                }
            }

            public string GetHeader()
            {
                try
                {
                    return itemLines.GetHeader();
                }
                catch (Exception ex)
                {
                    parent.OnDataAccessFailed(ex);
                    return String.Empty;
                }
            }
        }

        #endregion
    }
}
