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
        /// Creates an array of Colors that is representative of the Border struct
        /// </summary>
        /// <param name="width">Width of the texture to make a border of</param>
        /// <param name="height">Height of the texture to make a border of</param>
        /// <returns>An array of Colors that represents the border</returns>
        public Color[] CreateBorderColorArray(int width, int height)
        {
            Color[] textureColor = new Color[Texture.Width * Texture.Height];
            Texture.GetData(textureColor);
            
            var (outWidth, outHeight) = (width + Thickness, height + Thickness);
            
            Color[] outArray = new Color[outWidth * outHeight];
            
            int texIndex = 0;
            for (int r = 0; r < outHeight; r++)
            {
                for (int c = 0; c < outWidth; c++)
                {
                    //if both row and column are past the border, then make it transparent to allow for something else  
                    //to be displayed.otherwise, then set it to a pixel from the border texture
                    if (r >= Thickness && c >= Thickness && r < height && c < width)
                    {
                        outArray[(r * outHeight) + c] = Color.Transparent;
                    }
                    else
                    {
                        outArray[(r * outHeight) + c] = textureColor[texIndex++ % textureColor.Length];
                    }
                }
            }

            return outArray;
        }
    }
}