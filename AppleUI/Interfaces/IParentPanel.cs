namespace AppleUI.Interfaces
{
    /// <summary>
    /// Represents a UI element that has a parent panel (which should be every UI element)
    /// </summary>
    public interface IParentPanel
    {
        /// <summary>
        /// The parent panel this object is apart of
        /// </summary>
        Panel ParentPanel { get; set; }
    }
}