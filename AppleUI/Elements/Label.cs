using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    /// <summary>
    /// A UI element that represents text.
    /// </summary>
    public sealed class Label : UserInterfaceElement, IScriptableElement
    {
        public override Vector2 RawPosition => Transform.GetDrawPosition(Owner);
        public override Vector2 RawSize => Bounds * Transform.Scale;

        /// <summary>
        /// Represents the dimensions of a box that would fully contain the rendered out text.
        /// </summary>
        public Vector2 Bounds { get; private set; }

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

        // We use a StringBuilder here because we want to change the contents of the text efficiently without creating
        // as much garbage and the APIs for Monogame/FontStashSarp accept StringBuilders. 
        private StringBuilder _text;

        /// <summary>
        /// Represents the text of the label.
        /// </summary>
        public StringBuilder Text
        {
            get => _text;
            set
            {
                if (value == _text) return;
                
                Bounds = _spriteFontBase.MeasureString(value);
                _text = value;
            }
        }

        private FontSystem _fontSystem;

        /// <summary>
        /// The FontSystem that will be to provide renderable fonts.
        /// </summary>
        public FontSystem FontSystem
        {
            get => _fontSystem;
            set
            {
                _spriteFontBase = value.GetFont(_fontSize);
                Bounds = _spriteFontBase.MeasureString(_text);

                _fontSystem = value;
            }
        }
        
        /// <summary>
        /// Color of the text when displayed.
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// User-defined scripts that will be executed every frame.
        /// </summary>
        public IElementBehaviorScript[] Scripts { get; set; }

        private ElementScriptInfo[] _scriptInfos;
        
        /// <summary>
        /// The object responsible for rendering fonts
        /// </summary>
        private SpriteFontBase _spriteFontBase;

        /// <summary>
        /// Constructs an <see cref="Label"/> object.
        /// </summary>
        /// <param name="id">Id of the element.</param>
        /// <param name="owner">The element container that owns this element.</param>
        /// <param name="transform">The position, scale, and rotation of this element.</param>
        /// <param name="text">The string value that will be displayed when this object is drawn.</param>
        /// <param name="fontSize">The size of the font when rendered</param>
        /// <param name="textColor">The color of the text when drawn</param>
        /// <param name="fontSystem">The FontSystem that will generate SpriteFonts of a specific font.</param>
        /// <param name="border">The border that will surround this element when drawn. If null, no border will be drawn.</param>
        /// <param name="scripts">User-defined scripts that will be executed every frame.</param>
        public Label(string id, IElementContainer? owner, ElementTransform transform, string text,
            int fontSize, Color textColor, FontSystem fontSystem, Border? border,
            IElementBehaviorScript[]? scripts = null)
        {
            (Id, Owner, Transform, _text, _fontSize, _fontSystem, TextColor, Border) =
                (new ElementId(id), owner, transform, new StringBuilder(text), fontSize, fontSystem, textColor, border);

            _spriteFontBase = FontSystem.GetFont(_fontSize);
            Bounds = _spriteFontBase.MeasureString(Text);
            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        /// <summary>
        /// Constructor that Json files can call to create Label instances. It is not recommended to use this
        /// constructor in written code.<br/>
        /// Warning: The Owner property is not set to when using this constructor, and must be set to externally
        /// </summary>
        /// <param name="id">Id of the element.</param>
        /// <param name="position">Position of the text in relation to the parent panel</param>
        /// <param name="positionType">The type of position the <see cref="position"/> parameter is.</param>
        /// <param name="scale">The scale of the text along the x-axis (width) and y-axis (height). (Warning:
        /// Manipulating this value may result in a loss of resolution!)</param>
        /// <param name="rotation">Rotation of the text</param>
        /// <param name="text">The string value that will be displayed when this object is drawn.</param>
        /// <param name="fontSize">The size of the font when rendered</param>
        /// <param name="textColor">The color of the text when drawn</param>
        /// <param name="fontSystem">The FontSystem that will generate SpriteFonts of a specific font.</param>
        /// <param name="border">The border that will surround this element when drawn. If null, no border will be drawn.</param>
        /// <param name="scripts">Information needed to load a user-defined script. This
        /// <see cref="ElementScriptInfo"/> array is converted into instances of <see cref="IElementBehaviorScript"/>
        /// instances after this UI element is created.</param>
        [JsonConstructor]
        public Label(string id, Vector2 position, MeasurementType positionType, Vector2 scale, float rotation,
            string text, int fontSize, Color textColor, FontSystem fontSystem, Border? border, object[]? scripts)
            : this(id, null, new ElementTransform(new Measurement(position, positionType), scale, rotation), text,
                fontSize, textColor, fontSystem, border)
        {
            _scriptInfos = scripts?.Cast<ElementScriptInfo>().ToArray() ?? _scriptInfos;
        }

        public void LoadScripts(UserInterfaceManager manager) =>
            Scripts = manager.LoadElementBehaviorScripts(this, _scriptInfos);

        public override void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Draws this Label instance. It will be drawn in the center.
        /// </summary>
        /// <param name="gameTime">The current time within the Game</param>
        /// <param name="spriteBatch">SpriteBatch objected used to render the text</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Border?.DrawBorder(spriteBatch, new RotatableRectangle(RawPosition, RawSize, Transform.Rotation));
            
            spriteBatch.DrawString(_spriteFontBase, Text, Transform.GetDrawPosition(Owner), TextColor, Transform.Scale,
                Transform.Rotation);
        }

        public override object Clone()
        {
            Label clone = new(Id.Name, Owner, Transform, Text.ToString(), FontSize, TextColor, FontSystem, Border)
            {
                _scriptInfos = _scriptInfos
            };
            
            UserInterfaceManager? buttonManager = GetParentPanel()?.Manager;
            if (buttonManager is not null) clone.LoadScripts(buttonManager);

            return clone;
        }
    }
}