using System;
using System.Linq;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    /// <summary>
    /// A UI element that represents a static image that has minimal or no update behavior
    /// </summary>
    public sealed class StaticTexture : UserInterfaceElement, IScriptableElement, ITextureElement
    {
        public override Vector2 RawSize => TextureSize * Transform.Scale;

        /// <summary>
        /// The size of the texture itself in pixels with no scales applied
        /// </summary>
        public Vector2 TextureSize { get; set; }

        /// <summary>
        /// Represents the Texture that will be drawn
        /// </summary>
        public Texture2D Texture { get; set; }
        
        /// <summary>
        /// Determines if the texture will be stretched to fit its container.
        /// </summary>
        public bool FitContainer { get; set; }

        /// <summary>
        /// User-defined scripts that will be executed every frame.
        /// </summary>
        public IElementBehaviorScript[] Scripts { get; set; }

        public StaticTexture TextureObject => this;

        private ElementScriptInfo[] _scriptInfos;
        private Texture2D? _placeholderTexture;
        
        /// <summary>
        /// Constructor for StaticTexture provided a parent panel. The Transform property will have a default value a
        /// position and rotation rotation of zero with a size of 100 100, and the Texture object will have
        /// a 100x100 purple texture
        /// </summary>
        /// <param name="id">Id of the element.</param>
        /// <param name="owner">The element container that owns this element.</param>
        /// <param name="graphicsDevice">The graphics device that will be used to create the placeholder texture.
        /// </param>
        public StaticTexture(string id, IElementContainer? owner, GraphicsDevice graphicsDevice)
        {
            Id = new ElementId(id);
            Owner = owner;
            TextureSize = new Vector2(100f, 100f);
            Transform = new ElementTransform(); 

            _placeholderTexture = new Texture2D(graphicsDevice, (int) TextureSize.X, (int) TextureSize.Y);
            Texture = _placeholderTexture;

            //Set the texture to a green 100x100 texture
            Texture.SetData(new Color[(int) (TextureSize.X * TextureSize.Y)].Select(e => e = Color.Green).ToArray());

            Scripts = Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        /// <summary>
        /// Constructs a StaticTexture object given all the necessary fields
        /// </summary>
        /// <param name="id">Id of the element.</param>
        /// <param name="owner">The element container that owns this element.</param>
        /// <param name="transform">The transform of the texture, representing its position, scale, and rotation.</param>
        /// <param name="fitContainer">Determines if the texture will be stretched to fit its container.</param>
        /// <param name="texture">The texture that will be drawn (in this case it would be the name of the texture)</param>
        /// <param name="border">The border that will surround this element when drawn. If null, no border will be drawn.</param>
        /// <param name="scripts">User-defined scripts that will be executed every frame.</param>
        public StaticTexture(string id, IElementContainer? owner, ElementTransform transform, Texture2D texture, 
            bool fitContainer, Border? border, IElementBehaviorScript[]? scripts = null)
        {
            (Id, Owner, Texture, Transform, FitContainer, Border) =
                (new ElementId(id), owner, texture, transform, fitContainer, border);
            
            TextureSize = new Vector2(texture.Width, texture.Height);

            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        /// <summary>
        /// Constructor that Json files can call to create instances of StaticTextures <br/>
        /// Warning: The Owner property is not set to when using this constructor, and must be set to externally
        /// </summary>
        /// <param name="id">Id of the element.</param>
        /// <param name="position">The position of the texture in relation to the parent panel</param>
        /// <param name="positionType">The type of position the <see cref="position"/> parameter is.</param>
        /// <param name="positionBasePoint">The local base point of this object that position is relative to.</param>
        /// <param name="scale">The scale of the texture on the x-axis(width) and on the y-axis(height)</param>
        /// <param name="rotation">The rotation of the texture</param>
        /// <param name="texture">The texture that will be drawn (in this case it would be the name of the texture</param>
        /// <param name="fitContainer">Determines if the texture will be stretched to fit its container.</param>
        /// <param name="border">The border that will surround this element when drawn. If null, no border will be drawn.</param>
        /// <param name="scripts">Information needed to load a user-defined script. This
        /// <see cref="ElementScriptInfo"/> array is converted into instances of <see cref="IElementBehaviorScript"/>
        /// instances after this UI element is created.</param>
        [JsonConstructor]
        public StaticTexture(string id, Vector2 position, MeasurementType positionType,
            PositionBasePoint positionBasePoint, Vector2 scale, float rotation,
            Texture2D texture, bool fitContainer, Border? border, object[]? scripts)
            : this(id, null, new ElementTransform(new Measurement(position, positionType),
                positionBasePoint, scale, rotation), texture, fitContainer, border)
        {
            _scriptInfos = scripts?.Cast<ElementScriptInfo>().ToArray() ?? _scriptInfos;
        }

        public void LoadScripts(UserInterfaceManager manager) =>
            Scripts = manager.LoadElementBehaviorScripts(this, _scriptInfos);

        public override void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Draws the texture.
        /// </summary>
        /// <param name="gameTime">The object that represents the current time for the active Game</param>
        /// <param name="spriteBatch">The sprite batch that is used for drawing</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Border?.DrawBorder(spriteBatch, new RotatableRectangle(RawPosition, RawSize, Transform.Rotation));

            Rectangle? srcRect =
                FitContainer ? new Rectangle(OwnerRawPosition.ToPoint(), OwnerRawSize.ToPoint()) : null;
            spriteBatch.Draw(Texture, RawPosition, srcRect, Color.White, Transform.Rotation, Vector2.Zero, Transform.Scale,
                SpriteEffects.None, 0f);
        }

        /// <summary>
        /// If a texture was not provided and a place holder texture was created, this method will dispose of the place
        /// holder texture.
        /// </summary>
        public override void Dispose()
        {
            _placeholderTexture?.Dispose();
            base.Dispose();
        }

        public override object Clone()
        {
            StaticTexture clone = new(Id.Name, Owner, Transform, Texture, FitContainer, Border)
                { _scriptInfos = _scriptInfos };

            UserInterfaceManager? buttonManager = GetParentPanel()?.Manager;
            if (buttonManager is not null) clone.LoadScripts(buttonManager);

            return clone;
        }
    }
}