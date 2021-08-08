// copyright discretelogics 2012.
namespace TeaTime.Tree
{
    interface INode
    {
        string Name { get; }
        string FullPath { get; }

        bool IsFolder { get; }
        bool IsExpanded { get; set; }
        
        void UpdatePath(string newpath, Folder parent);        
    }
}
