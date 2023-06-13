using System;
using System.Collections.Generic;
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
    public sealed class StaticTexture : Interfaces.IDrawable, ITransform, IScriptableElement, IDisposable
    {
        /// <summary>
        /// Position of the texture in relation to the parent panel
        /// </summary>
        public Measurement Position { get; set; }
        
        /// <summary>
        /// The SCALE (not the width/height) that is applied to the texture when being drawn
        /// </summary>
        public Vector2 Scale { get; set; }
        
        /// <summary>
        /// The size of the texture itself in pixels with no scales applied
        /// </summary>
        public Vector2 TextureSize { get; set; }
        
        /// <summary>
        /// The rotation of the texture around the origin of the texture (usually the center of the texture)
        /// </summary>
        public float Rotation { get; set; }
        
        /// <summary>
        /// Represents the Texture that will be drawn
        /// </summary>
        public Texture2D Texture { get; set; }
        
        /// <summary>
        /// The panel this object is a part of 
        /// </summary>
        public Panel? ParentPanel { get; set; }
        
        /// <summary>
        /// User-defined scripts that will be executed every frame.
        /// </summary>
        public IElementBehaviorScript[] Scripts { get; set; }

        private ElementScriptInfo[] _scriptInfos;
        private Texture2D? _placeholderTexture;
        
        /// <summary>
        /// Constructor for StaticTexture provided a parent panel. The Transform property will have a default value a
        /// position and rotation rotation of zero with a size of 100 100, and the Texture object will have
        /// a 100x100 purple texture
        /// </summary>
        /// <param name="parentPanel">The panel this element is a part of</param>
        public StaticTexture(Panel parentPanel)
        {
            Vector2 size = new(100, 100);
            (ParentPanel, Position, Scale, TextureSize, Rotation) =
                (parentPanel, new Measurement(Vector2.Zero, MeasurementType.Pixel), Vector2.One, size, 0f);
            
            _placeholderTexture = new Texture2D(parentPanel.GraphicsDevice, (int)size.X, (int)size.Y);
            Texture = _placeholderTexture;

            //Set the texture to a green 100x100 texture
            Texture.SetData(new Color[(int) (size.X * size.Y)].Select(e => e = Color.Green).ToArray());
            
            Scripts = Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        /// <summary>
        /// Constructs a StaticTexture object given all the necessary fields
        /// </summary>
        /// <param name="parentPanel">The panel this texture is associated with</param>
        /// <param name="position">The position of the texture in relation to the parent panel</param>
        /// <param name="scale">The scale of the texture on the x-axis(width) and on the y-axis(height)</param>
        /// <param name="rotation">The rotation of the texture</param>
        /// <param name="texture">The texture that will be drawn (in this case it would be the name of the texture)</param>
        /// <param name="scripts">User-defined scripts that will be executed every frame.</param>
        public StaticTexture(Panel? parentPanel, Measurement position, Vector2 scale, float rotation, Texture2D texture,
            IElementBehaviorScript[]? scripts = null)
        {
            (ParentPanel, Texture, Position, Scale, Rotation) =
                (parentPanel, texture, position, scale, rotation);

            TextureSize = new Vector2(texture.Width, texture.Height);

            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        /// <summary>
        /// Constructor that Json files can call to create instances of StaticTextures <br/>
        /// Warning: The ParentPanel property is not set to when using this constructor, and must be set to externally
        /// </summary>
        /// <param name="position">The position of the texture in relation to the parent panel</param>
        /// <param name="positionType">The type of position the <see cref="position"/> parameter is.</param>
        /// <param name="scale">The scale of the texture on the x-axis(width) and on the y-axis(height)</param>
        /// <param name="rotation">The rotation of the texture</param>
        /// <param name="texture">The texture that will be drawn (in this case it would be the name of the texture</param>
        /// <param name="scripts">Information needed to load a user-defined script. This
        /// <see cref="ElementScriptInfo"/> array is converted into instances of <see cref="IElementBehaviorScript"/>
        /// instances after this UI element is created.</param>
        [JsonConstructor]
        public StaticTexture(Vector2 position, MeasurementType positionType, Vector2 scale, float rotation,
            Texture2D texture, object[]? scripts) : this(null, new Measurement(position, positionType), scale, rotation,
            texture)
        {
            _scriptInfos = scripts?.Cast<ElementScriptInfo>().ToArray() ?? _scriptInfos;
        }

        public void LoadScripts(UserInterfaceManager manager) =>
            Scripts = manager.LoadElementBehaviorScripts(_scriptInfos);

        /// <summary>
        /// Draws the texture
        /// </summary>
        /// <param name="callingPanel">This parameter that represents the panel that has called this function.</param>
        /// <param name="gameTime">The object that represents the current time for the active Game</param>
        /// <param name="batch">The sprite batch that is used for drawing</param>
        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch batch)
        {
            batch.Draw(Texture, this.GetDrawPosition(callingPanel), null, Color.White, Rotation, Vector2.Zero, Scale,
                SpriteEffects.None, 1f);
        }

        /// <summary>
        /// If a texture was not provided and a place holder texture was created, this method will dispose of the place
        /// holder texture.
        /// </summary>
        public void Dispose()
        {
            _placeholderTexture?.Dispose();
        }

        public object Clone() => MemberwiseClone();
    }
}