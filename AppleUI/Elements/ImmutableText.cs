using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using AppleSerialization;
using AppleUI.Interfaces;
using FastDeepCloner;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    /// <summary>
    /// A UI element that represents text whose string value and font cannot be changed
    /// </summary>
    public sealed class ImmutableText : Interfaces.IDrawable, ITransform, IDisposable
    {
        /// <summary>
        /// The position of the text in relation to the parent panel. Represents the CENTER of the text!
        /// </summary>
        public Measurement Position { get; set; }

        /// <summary>
        /// Represents how stretched or compress the text should be on the x-axis(width) and on the y-axis (height)
        /// WARNING: Stretching could potentially cause a reduction in resolution of the text displayed! Try to avoid
        /// direct manipulation of this value.
        /// </summary>
        public Vector2 Scale { get; set; }

        /// <summary>
        /// Represents the dimensions of a box that would fully contain the rendered out text.
        /// </summary>
        public Vector2 Bounds { get; private set; }

        /// <summary>
        /// The rotation of the text around the center.
        /// </summary>
        public float Rotation { get; set; }

        private int _fontSize;

        /// <summary>
        /// Represents how large the text will be rendered.
        /// </summary>
        public int FontSize
        {
            get => _fontSize;
            set
            {
                if (value != _fontSize)
                {
                    _spriteFontBase = FontSystem.GetFont(value);
                    Bounds = _spriteFontBase.MeasureString(Text);
                }

                _fontSize = value;
            }
        }

        /// <summary>
        /// The string that the text represents. CANNOT be changed.
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// The FontSystem that will be to provide renderable fonts.
        /// </summary>
        public FontSystem FontSystem { get; init; }
        
        /// <summary>
        /// Color of the text when displayed.
        /// </summary>
        public Color Color { get; set; }
        
        /// <summary>
        /// The panel this element is associated with.
        /// </summary>
        public Panel? ParentPanel { get; set; }

        private Vector2 Origin => Bounds / 2;

        /// <summary>
        /// The object responsible for rendering fonts
        /// </summary>
        private SpriteFontBase _spriteFontBase;

        /// <summary>
        /// Constructs an <see cref="ImmutableText"/> object.
        /// </summary>
        /// <param name="parentPanel">The panel this text element is a part of.</param>
        /// <param name="position">Position of the text in relation to the parent panel</param>
        /// <param name="scale">The scale of the text along the x-axis (width) and y-axis (height). (Warning:
        /// Manipulating this value may result in a loss of resolution!)</param>
        /// <param name="rotation">Rotation of the text</param>
        /// <param name="text">The string value that will be displayed when this object is drawn.</param>
        /// <param name="fontSize">The size of the font when rendered</param>
        /// <param name="color">The color of the text when drawn</param>
        /// <param name="fontSystem">The FontSystem that will generate SpriteFonts of a specific font.</param>
        public ImmutableText(Panel? parentPanel, Measurement position, Vector2 scale, float rotation, string text,
            int fontSize, Color color, FontSystem fontSystem)
        {
            (ParentPanel, Position, Scale, Rotation, Text, _fontSize, Color, FontSystem) =
                (parentPanel, position, scale, rotation, text, fontSize, color, fontSystem);

            _spriteFontBase = FontSystem.GetFont(_fontSize);
            Bounds = _spriteFontBase.MeasureString(Text);
        }

        /// <summary>
        /// Constructor that Json files can call to create ImmutableText instances. It is not recommended to use this
        /// constructor in written code.<br/>
        /// Warning: The ParentPanel property is not set to when using this constructor, and must be set to externally
        /// </summary>
        /// <param name="position">Position of the text in relation to the parent panel</param>
        /// <param name="positionType">The type of position the <see cref="position"/> parameter is.</param>
        /// <param name="scale">The scale of the text along the x-axis (width) and y-axis (height). (Warning:
        /// Manipulating this value may result in a loss of resolution!)</param>
        /// <param name="rotation">Rotation of the text</param>
        /// <param name="text">The string value that will be displayed when this object is drawn.</param>
        /// <param name="fontSize">The size of the font when rendered</param>
        /// <param name="color">The color of the text when drawn</param>
        /// <param name="fontSystem">The FontSystem that will generate SpriteFonts of a specific font.</param>
        [JsonConstructor]
        public ImmutableText(Vector2 position, MeasurementType positionType, Vector2 scale, float rotation,
            string text, int fontSize, Color color, FontSystem fontSystem) : this(null,
            new Measurement(position, positionType), scale, rotation, text, fontSize, color, fontSystem)
        {
        }


        /// <summary>
        /// Draws this ImmutableText instance
        /// </summary>
        /// <param name="callingPanel">The panel that is calling this method</param>
        /// <param name="gameTime">The current time within the Game</param>
        /// <param name="batch">SpriteBatch objected used to render the text</param>
        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch batch)
        {
            batch.DrawString(_spriteFontBase, Text, this.GetDrawPosition(callingPanel), Color, Scale, Rotation, Origin);
        }

        /// <summary>
        /// Disposes all disposable resources being used by this ImmutableText instance (other than ParentPanel)
        /// </summary>
        public void Dispose()
        {
            FontSystem.Dispose();
        }
        
        public object Clone() => MemberwiseClone();
    }
}