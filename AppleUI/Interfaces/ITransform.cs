using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        public (Vector2 Value, PositionType Type) Position { get; set; }

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
    
    internal static class TransformExtensions
    {
        //I'm too lazy to create a new exception type for a case that should never happen, heh.

        /// <summary>
        /// Gets a <see cref="Vector2"/> representing the absolute pixel position that will be sent to the
        /// <see cref="SpriteBatch"/> to draw a UI element that implements <see cref="ITransform"/>
        /// </summary>
        /// <param name="transform">This UI element that implements <see cref="ITransform"/>.</param>
        /// <param name="drawingPanel"><see cref="Panel"/> object that called for this UI element to be drawn.</param>
        /// <returns>A <see cref="Vector2"/> representing an absolute pixel position in the game window.</returns>
        /// <exception cref="Exception">This exception is thrown when the <see cref="PositionType"/> is invalid.
        /// If this exception is hit, then it is the fault of this method failing to account for a value.</exception>
        public static Vector2 GetDrawPosition(this ITransform transform, Panel drawingPanel) =>
            transform.Position.Type switch
            {
                PositionType.Pixel => transform.Position.Value + drawingPanel.Position,
                PositionType.Ratio => (drawingPanel.Size * transform.Position.Value) + drawingPanel.Position,
                _ => throw new Exception($"Position of type {transform.Position.Type} is invalid")
            };

        /// <summary>
        /// Copies the <see cref="ITransform.Position"/>, <see cref="ITransform.Rotation"/>, and
        /// <see cref="ITransform.Scale"/> of one <see cref="ITransform"/> to another.
        /// </summary>
        /// <param name="fromTransform">The <see cref="ITransform"/> to copy from.</param>
        /// <param name="toTransform">The <see cref="ITransform"/> to copy to.</param>
        public static void CopyTransformTo(this ITransform fromTransform, ITransform toTransform)
        {
            toTransform.Position = fromTransform.Position;
            toTransform.Rotation = fromTransform.Rotation;
            toTransform.Scale = fromTransform.Scale;
        }
    }
}