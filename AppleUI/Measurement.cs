using System;
using AppleSerialization;
using AppleUI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI
{
    /// <summary>
    /// Represents a measurement value with a specified <see cref="MeasurementType"/> describing what that value is
    /// measuring.
    /// </summary>
    public readonly struct Measurement
    {
        /// <summary>
        /// The type of the measurement value, which defines how to interpret the underlying value. In other words,
        /// this field represents the units. of <see cref="Value"/>.
        /// </summary>
        public readonly MeasurementType Type;

        /// <summary>
        /// The measurement value. The meaning of this value depends on the <see cref="Type"/> property.
        /// </summary>
        public readonly Vector2 Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Measurement"/> struct with the specified value and type.
        /// </summary>
        /// <param name="value">The measurement value.</param>
        /// <param name="type">The measurement type, which defines how to interpret the underlying value.</param>
        public Measurement(Vector2 value, MeasurementType type)
        {
            Value = value;
            Type = type;
        }
        
        /// <summary>
        /// Tries to parse a string representation of a <see cref="Measurement"/> value.
        /// </summary>
        /// <param name="value">The string representation of the measurement value.</param>
        /// <param name="type">The string representation of the measurement type.</param>
        /// <param name="measurement">When this method returns, contains the parsed <see cref="Measurement"/> value if
        /// the parsing succeeded, or the default value if the parsing failed.</param>
        /// <returns><c>true</c> if the parsing succeeded and <paramref name="measurement"/> contains the parsed value;
        /// otherwise, <c>false</c>.</returns>
        public static bool TryParse(string? value, string? type, out Measurement measurement)
        {
            if (value is not null && type is not null && ParseHelper.TryParseVector2(value, out Vector2 vector2) &&
                Enum.TryParse(type, out MeasurementType measurementType))
            {
                measurement = new Measurement(vector2, measurementType);
                return true;
            }

            measurement = default;
            return false;
        }
        
        /// <summary>
        /// Calculates a <see cref="Vector2"/> that represents <see cref="Value"/> in pixels. This value can be used
        /// directly in drawing to a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="width">The width value used to calculate the computed value in pixels.</param>
        /// <param name="height">The height value used to calculate the computed value in pixels.</param>
        /// <returns>A <see cref="Vector2"/> that represents the computed pixel value, which depends on the
        /// <see cref="Type"/> property.</returns>
        public Vector2 GetRawPixelValue(int width, int height) => Type switch
        {
            MeasurementType.Ratio => new Vector2(width * Value.X, height * Value.Y),
            _ => Value
        };

        /// <summary>
        /// Gets the calculated value based on the specified <see cref="Rectangle"/>'s width and height.
        /// </summary>
        /// <param name="rect">The <see cref="Rectangle"/> object used to calculate the computed value using its width
        /// and height in pixels.</param>
        /// <returns>A <see cref="Vector2"/> that represents the computed pixel value, which depends on the
        /// <see cref="Type"/> property.</returns>
        public Vector2 GetRawPixelValue(Rectangle rect) => GetRawPixelValue(rect.Width, rect.Height);

        /// <summary>
        /// Gets the calculated value based on the specified <see cref="Viewport"/> object's width and height.
        /// </summary>
        /// <param name="viewport">The <see cref="Viewport"/> object used to calculate the computed pixel value.</param>
        /// <returns>A <see cref="Vector2"/> that represents the computed pixel value, which depends on the
        /// <see cref="Type"/> property.</returns>
        public Vector2 GetRawPixelValue(Viewport viewport) => GetRawPixelValue(viewport.Width, viewport.Height);


        /// <summary>
        /// Gets the calculated value based on the specified size. 
        /// </summary>
        /// <param name="size">The <see cref="Vector2"/> object representing the size used to calculate the computed
        /// pixel value, where X is width and Y is height in pixels.</param>
        /// <returns>A <see cref="Vector2"/> that represents the computed pixel value, which depends on the
        /// <see cref="Type"/> property.</returns>
        public Vector2 GetRawPixelValue(Vector2 size) => GetRawPixelValue((int) size.X, (int) size.Y);
        
        public Vector2 GetRawPixelValue(IElementContainer? owner) => GetRawPixelValue(owner?.RawSize ?? Vector2.One);
    }
}
