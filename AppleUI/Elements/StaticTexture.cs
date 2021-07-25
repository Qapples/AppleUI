using System;
using System.Linq;
using System.Text.Json.Serialization;
using AppleSerialization;
using AppleUI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    /// <summary>
    /// A UI element that represents a static image that has minimal or no update behavior
    /// </summary>
    public sealed class StaticTexture : Serializer<StaticTexture>, Interfaces.IDrawable, ITransform, IParentPanel,
        IDisposable
    {
        /// <summary>
        /// Position of the texture in relation to the parent panel
        /// </summary>
        public Vector2 Position { get; set; }
        
        /// <summary>
        /// The SCALE (not the width/height) that is applied to the texture when being drawn
        /// </summary>
        public Vector2 Scale { get; set; }
        
        /// <summary>
        /// The size of the texture itself with no scales applied
        /// </summary>
        public Vector2 Size { get; set; }
        
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
        [JsonIgnore]
        public Panel? ParentPanel { get; set; }
        
        private Vector2 Center => Size / 2f;
        
        /// <summary>
        /// Constructor for StaticTexture provided a parent panel. The Transform property will have a default value a
        /// position and rotation rotation of zero with a size of 100 100, and the Texture object will have
        /// a 100x100 purple texture
        /// </summary>
        /// <param name="parentPanel">The panel this element is a part of</param>
        public StaticTexture(Panel parentPanel)
        {
            var size = new Vector2(100, 100);
            (ParentPanel, Position, Scale, Size, Rotation, Texture) = (parentPanel, Vector2.Zero, Vector2.One, size, 0f,
                new Texture2D(parentPanel.GraphicsDevice, (int) size.X, (int) size.Y));

            //Set the texture to a green 100x100 texture
            Texture.SetData(new Color[(int) (size.X * size.Y)].Select(e => e = Color.Green).ToArray());
        }

        /// <summary>
        /// Constructs a StaticTexture object given all the necessary fields
        /// </summary>
        /// <param name="parentPanel">The panel this texture is associated with</param>
        /// <param name="texture">The texture that will be drawn</param>
        /// <param name="position">The position of the texture in relation to the parent panel</param>
        /// <param name="scale">The scale of the texture on the x-axis(width) and on the y-axis(height)</param>
        /// <param name="rotation">The rotation of the texture</param>
        public StaticTexture(Panel parentPanel, Texture2D texture, in Vector2 position, in Vector2 scale,
            float rotation)
        {
            (ParentPanel, Texture, Position, Scale, Rotation, Size) = (parentPanel, texture, position, scale, rotation,
                new Vector2(texture.Width, texture.Height));
        }

        /// <summary>
        /// Constructor that Json files can call to create instances of StaticTextures <br/>
        /// Warning: The ParentPanel property is not set to when using this constructor, and must be set to externally
        /// </summary>
        /// <param name="texture">The texture that will be drawn (in this case it would be the name of the texture)
        /// </param>
        /// <param name="position">The position of the texture in relation to the parent panel</param>
        /// <param name="scale">The scale of the texture on the x-axis(width) and on the y-axis(height)</param>
        /// <param name="rotation">The rotation of the texture</param>
        [JsonConstructor]
        public StaticTexture(Texture2D texture, Vector2 position, Vector2 scale, float rotation)
        {
            (Texture, Position, Scale, Rotation, Size) = (texture, position, scale, rotation,
                new Vector2(texture.Width, texture.Height));
        }

        /// <summary>
        /// Draws the texture
        /// </summary>
        /// <param name="callingPanel">This parameter that represents the panel that has called this function.</param>
        /// <param name="gameTime">The object that represents the current time for the active Game</param>
        /// <param name="batch">The sprite batch that is used for drawing</param>
        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch batch)
        {
            batch.Draw(Texture, Position + callingPanel.Position, null, Color.White, Rotation, Center, Scale,
                SpriteEffects.None, 1f);
        }

        /// <summary>
        /// Disposes all disposable resources being used by this StaticTexture instance (other than ParentPanel)
        /// </summary>
        public void Dispose()
        {
            Texture.Dispose();
        }
    }
}