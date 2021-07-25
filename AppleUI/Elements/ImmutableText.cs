using System.Text.Json.Serialization;
using AppleUI;
using FontStashSharp;
using GrappleFightNET5.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleSerialization.Elements
{
    /// <summary>
    /// A UI element that represents text whose string value and font cannot be changed
    /// </summary>
    public partial class ImmutableText : Serializer<ImmutableText>
    {
        /// <summary>
        /// The position of the text in relation to the parent panel. Represents the CENTER of the text!
        /// </summary>
        public Vector2 Position { get; set; }

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
                    Bounds = _spriteFontBase.MeasureString(Value);
                }

                _fontSize = value;
            }
        }

        /// <summary>
        /// The string that the text represents. CANNOT be changed.
        /// </summary>
        public string Value { get; init; }

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
        [JsonIgnore]
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
        /// <param name="fontSystem">The FontSystem that will generate SpriteFonts of a specific font.</param>
        /// <param name="position">Position of the text in relation to the parent panel.</param>
        /// <param name="scale">The scale of the text along the x-axis (width) and y-axis (height). (Warning:
        /// Manipulating this value may result in a loss of resolution!)</param>
        /// <param name="color">The color of the text when drawn.</param>
        /// <param name="rotation">Rotation of the text along it's origin.</param>
        /// <param name="fontSize">The size of the font when rendered.</param>
        /// <param name="text">The string value that will be displayed when this object is drawn.</param>
        public ImmutableText(Panel parentPanel, FontSystem fontSystem, in Vector2? position = null, in Vector2?
            scale = null, in Color? color = null, float rotation = 0f, int fontSize = 24, string text = "Sample Text")
        {
            (ParentPanel, FontSystem, Position, Scale, Color, Rotation, _fontSize, Value) =
                (parentPanel, fontSystem, position ?? Vector2.Zero, scale ?? Vector2.One, color ?? Color.Black,
                    rotation, fontSize, text);

            _spriteFontBase = FontSystem.GetFont(_fontSize);
            Bounds = _spriteFontBase.MeasureString(Value);
        }

        /// <summary>
        /// Constructor that Json files can call to create ImmutableText instances.<br/>
        /// Warning: The ParentPanel property is not set to when using this constructor, and must be set to externally
        /// </summary>
        /// <param name="position">Position of the text in relation to the parent panel</param>
        /// <param name="scale">The scale of the text along the x-axis (width) and y-axis (height). (Warning:
        /// Manipulating this value may result in a loss of resolution!)</param>
        /// <param name="rotation">Rotation of the text</param>
        /// <param name="fontSize">The size of the font when rendered</param>
        /// <param name="text">The string value that will be displayed when this object is drawn.</param>
        /// <param name="fontSystem">The FontSystem that will generate SpriteFonts of a specific font.</param>
        /// <param name="color">The color of the text when drawn</param>
        [JsonConstructor]
        public ImmutableText(Vector2 position, Vector2 scale, float rotation, int fontSize, string text,
            FontSystem fontSystem, Color color)
        {
            (Position, Scale, Rotation, _fontSize, Value, FontSystem, Color) =
                (position, scale, rotation, fontSize, text, fontSystem, color);

            _spriteFontBase = FontSystem.GetFont(_fontSize);
            Bounds = _spriteFontBase.MeasureString(Value);
        }

        /// <summary>
        /// Draws this ImmutableText instance
        /// </summary>
        /// <param name="callingPanel">The panel that is calling this method</param>
        /// <param name="gameTime">The current time within the Game</param>
        /// <param name="batch">SpriteBatch objected used to render the text</param>
        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch batch)
        {
            batch.DrawString(_spriteFontBase, Value, Position, Color, Scale, Rotation, Origin);
        }

        /// <summary>
        /// Disposes all disposable resources being used by this ImmutableText instance (other than ParentPanel)
        /// </summary>
        public void Dispose()
        {
            FontSystem.Dispose();
        }
    }
}