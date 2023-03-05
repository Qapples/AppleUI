using System;

namespace AppleUI.Interfaces
{
    /// <summary>
    /// Represents a UI element. All UI elements should implement this interface.
    /// </summary>
    public interface IUserInterfaceElement : ICloneable
    {
        /// <summary>
        /// The parent panel this UI element is apart of
        /// </summary>
        Panel ParentPanel { get; set; }
    }
}