using System.Windows.Controls;

namespace TeaTime.UI
{
    public partial class MruComboBox : ComboBox
    {
        #region ctor
        public MruComboBox()
        {
            InitializeComponent();
        }
        #endregion

        #region eventhandler
        protected override void OnLostFocus(System.Windows.RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (SelectedItem != null)
            {
                var collection = (ObservableMruCollection<MruItem>)ItemsSource;
                var item = (MruItem)SelectedItem;
                this.Text = item.FullName;
                if (collection.IndexOf(item) != 0)
                {
                    collection.Add(item);
                }
            }
        }
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }
        #endregion
    }
}
