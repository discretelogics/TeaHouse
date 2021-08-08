using System;

namespace TeaTime.Data
{
    public interface ITeaFileEditor
    {
        void SetTeaFileIndex(object sender, long tsi);
        void Update(IChange change);
    }
}