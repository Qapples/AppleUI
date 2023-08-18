using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    /// <summary>
    /// A UI element that represents text whose string value and font cannot be changed
    /// </summary>
    public sealed class ImmutableText : UserInterfaceElement, IScriptableElement
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
        /// Constructs an <see cref="ImmutableText"/> object.
        /// </summary>
        /// <param name="id">Id of the element.</param>
        /// <param name="owner">The element container that owns this element.</param>
        /// <param name="transform">The position, scale, and rotation of this element.</param>
        /// <param name="text">The string value that will be displayed when this object is drawn.</param>
        /// <param name="fontSize">The size of the font when rendered</param>
        /// <param name="textColor">The color of the text when drawn</param>
        /// <param name="fontSystem">The FontSystem that will generate SpriteFonts of a specific font.</param>
        /// <param name="scripts">User-defined scripts that will be executed every frame.</param>
        public ImmutableText(string id, IElementContainer? owner, ElementTransform transform, string text,
            int fontSize, Color textColor, FontSystem fontSystem, IElementBehaviorScript[]? scripts = null)
        {
            (Id, Owner, Transform, Text, _fontSize, TextColor, FontSystem) =
                (id, owner, transform, text, fontSize, textColor, fontSystem);

            _spriteFontBase = FontSystem.GetFont(_fontSize);
            Bounds = _spriteFontBase.MeasureString(Text);
            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        /// <summary>
        /// Constructor that Json files can call to create ImmutableText instances. It is not recommended to use this
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
        /// <param name="scripts">Information needed to load a user-defined script. This
        /// <see cref="ElementScriptInfo"/> array is converted into instances of <see cref="IElementBehaviorScript"/>
        /// instances after this UI element is created.</param>
        [JsonConstructor]
        public ImmutableText(string id, Vector2 position, MeasurementType positionType, Vector2 scale, float rotation,
            string text, int fontSize, Color textColor, FontSystem fontSystem, object[]? scripts)
            : this(id, null, new ElementTransform(new Measurement(position, positionType), scale, rotation), text,
                fontSize, textColor, fontSystem)
        {
            _scriptInfos = scripts?.Cast<ElementScriptInfo>().ToArray() ?? _scriptInfos;
        }

        public void LoadScripts(UserInterfaceManager manager) =>
            Scripts = manager.LoadElementBehaviorScripts(this, _scriptInfos);

        public override void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Draws this ImmutableText instance. It will be drawn in the center.
        /// </summary>
        /// <param name="gameTime">The current time within the Game</param>
        /// <param name="batch">SpriteBatch objected used to render the text</param>
        public override void Draw(GameTime gameTime, SpriteBatch batch)
        {
            batch.DrawString(_spriteFontBase, Text, Transform.GetDrawPosition(Owner), TextColor, Transform.Scale,
                Transform.Rotation);
        }

        public override object Clone()
        {
            ImmutableText clone = new(GenerateCloneId(Id), Owner, Transform, Text, FontSize, TextColor, FontSystem)
            {
                _scriptInfos = _scriptInfos
            };
            
            UserInterfaceManager? buttonManager = GetParentPanel()?.Manager;
            if (buttonManager is not null) clone.LoadScripts(buttonManager);

            return clone;
        }
    }
}