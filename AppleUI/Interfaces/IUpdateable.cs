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
        /// <param name="parentPanel">The panel that the IUpdateable instance belongs to</param>
        /// <param name="gameTime">GameTime that represents the time the current Game object is running on</param>
        void Update(Panel parentPanel, GameTime gameTime);
    }
}