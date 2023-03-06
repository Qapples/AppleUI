using Microsoft.Xna.Framework;

namespace AppleUI.Interfaces
{
    /// <summary>
    /// Represents a UI element that can update
    /// </summary>
    public interface IUpdateable : IUserInterfaceElement
    {
        /// <summary>
        /// Updates the IUpdateable instance
        /// </summary>
        /// <param name="callingPanel">The panel that is calling this method, usually the ParentPanel of this element.
        /// </param>
        /// <param name="gameTime">GameTime that represents the time the current Game object is running on</param>
        void Update(Panel callingPanel, GameTime gameTime);
    }
}