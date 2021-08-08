namespace TeaTime.Chart.Core
{
    /// <summary>
    /// RFC - something selectable. ok.
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// RFC but why must the sender be iteself selectable? Yes: why ??
        /// </summary>
        /// <param name="sender"></param>
        void Select(object sender);
        void Deselect(object sender);
    }
}
