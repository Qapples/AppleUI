using System;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI
{
    /// <summary>
    /// Represents a border that a UI element or UI panel can have. This struct is not supposed to be used on it's own.
    /// </summary>
    public readonly struct Border
    {
        /// <summary>
        /// Thickness of the border in pixels
        /// </summary>
        public int Thickness { get; init; }
        
        /// <summary>
        /// A texture that represents the pattern that will be drawn along the border
        /// </summary>
        public Texture2D Texture { get; init; }

        [JsonConstructor]
        public Border(int thickness, Texture2D texture) => (Thickness, Texture) = (thickness, texture);

        /// <summary>
        /// Draws the border using a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to draw to.</param>
        /// <param name="bounds">The <see cref="Rectangle"/> to draw the bounds of. </param>
        public void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds)
        {
            //Draw order: Left, Right, Top, Bottom
            spriteBatch.Draw(Texture, new Rectangle(bounds.Left, bounds.Top, Thickness, bounds.Height), Color.White);
            spriteBatch.Draw(Texture, new Rectangle(bounds.Right, bounds.Top, Thickness, bounds.Height),
                Color.White);
            spriteBatch.Draw(Texture, new Rectangle(bounds.Left, bounds.Top, bounds.Width, Thickness), Color.White);
            spriteBatch.Draw(Texture, new Rectangle(bounds.Left, bounds.Bottom, bounds.Width + Thickness, Thickness),
                Color.White);
        }
        
        /// <summary>
        /// Draws the border using a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to draw to.</param>
        /// <param name="bounds">The <see cref="RotatableRectangle"/> to draw the bounds of. </param>
        public void DrawBorder(SpriteBatch spriteBatch, RotatableRectangle bounds)
        {
            Span<Vector2> corners = bounds.GetCorners(stackalloc Vector2[4]);
            var (topLeft, topRight, bottomRight, bottomLeft) = (corners[0], corners[1], corners[2], corners[3]);

            //For consistency reasons, all rects will be horizontal when no rotation is applied.

            RotatableRectangle leftRect = new(topLeft.X, topLeft.Y, Vector2.Distance(topLeft, bottomLeft) + Thickness,
                Thickness, bounds.RotationRadians + MathF.PI / 2f);

            RotatableRectangle rightRect = new(topRight.X, topRight.Y, Vector2.Distance(topRight, bottomRight),
                Thickness, bounds.RotationRadians + MathF.PI / 2f);

            RotatableRectangle topRect = new(topLeft.X, topLeft.Y, Vector2.Distance(topLeft, topRight), Thickness,
                bounds.RotationRadians);

            RotatableRectangle bottomRect = new(bottomLeft.X, bottomLeft.Y, Vector2.Distance(bottomLeft, bottomRight),
                Thickness, bounds.RotationRadians);
            
            leftRect.Draw(spriteBatch, Texture, Color.White);
            rightRect.Draw(spriteBatch, Texture, Color.White);
            topRect.Draw(spriteBatch, Texture, Color.White);
            bottomRect.Draw(spriteBatch, Texture, Color.White);
        }
    }
}