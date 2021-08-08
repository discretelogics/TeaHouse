using System;
using System.Windows.Input;

namespace TeaTime.VSX
{
    public abstract class CommandBase<T> : ICommand
    {
        #region ICommand Members

        public virtual bool CanExecute(object parameter)
        {
            return this.package != null;
        }

        public event EventHandler CanExecuteChanged;

        public abstract void Execute(object parameter);

        #endregion

        #region public methods
        public void Initialize(T package)
        {
            this.package = package;

            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }
        #endregion

        #region fields
        protected T package;
        #endregion
    }
}
