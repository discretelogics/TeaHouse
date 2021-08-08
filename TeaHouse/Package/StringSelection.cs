using System;
using TeaTime.CommonUI;
using TeaTime.Tree;

namespace TeaTime
{
    class StringSelection : GoodBase
    {        
        string currentSelection;
        
        public event EventHandler<string> OnChanged;        

        public string Current
        {
            get { return this.currentSelection; }
            set
            {
                if (this.currentSelection != value)
                {
                    this.currentSelection = value;
                    if (this.OnChanged != null) this.OnChanged(this, value);
                }
            }
        }
    }
}