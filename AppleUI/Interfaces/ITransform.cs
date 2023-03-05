using Microsoft.Xna.Framework;

namespace AppleUI.Interfaces
{
    /// <summary>
    /// Represents a UI element that has a position, scale, and rotation
    /// </summary>
    public interface ITransform : IUserInterfaceElement
    {
        /// <summary>
        /// Represents a position in 2d space. Depending on the context, it can either be in reference to the game
        /// window or a panel
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Represents a SCALE (not the width/height of the element!) in either the x-axis (width) or the
        /// y-axis (height)
        /// </summary>
        public Vector2 Scale { get; set; }
        
        /// <summary>
        /// A float value that represents a rotation, with the origin usually being around the center.
        /// </summary>
        public float Rotation { get; set; }
    }
}