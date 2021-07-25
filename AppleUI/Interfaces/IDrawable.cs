using AppleUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrappleFightNET5.UI.Interfaces
{
    /// <summary>
    /// Represents a UI element that can be drawn
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// Draws the IDrawable instance to a specified SpriteBatch object
        /// </summary>
        /// <param name="callingPanel">The panel that is calling this method</param>
        /// <param name="gameTime">GameTime that represents the time the current Game object is running on</param>
        /// <param name="batch">The SpriteBatch object to draw to</param>
        void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch batch);
    }
}