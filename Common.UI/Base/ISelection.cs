using System;
using TeaTime.Data;
using TeaTime.Tree;

namespace TeaTime
{
    interface ISelection<T>
    {
        // path can be file or folder. not necessary filesystem based, could be http protol path for instance to. the implementation decides what it can handle tough.
        T Current { get; set; }
        event EventHandler<T> OnChanged;
    }
}