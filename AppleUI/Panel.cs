using System;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace AppleUI
{
    /// <summary>
    /// A UI Panel that can contain UI elements.
    /// </summary>
    public sealed class Panel : IElementContainer, IDisposable, ICloneable
    {
        //------------
        // Properties
        //------------
        
        /// <summary>
        /// Graphics device used to draw the elements. If null, nothing will be drawn.
        /// </summary>
        [JsonIgnore]
        public GraphicsDevice? GraphicsDevice { get; set; }

        /// <summary>
        /// <see cref="UserInterfaceManager"/> that has ownership and control over this Panel.
        /// </summary>
        public UserInterfaceManager? Manager { get; internal set; }

        /// <summary>
        /// The elements that are contained within this panel.
        /// </summary>
        public ElementContainer ElementContainer { get; private set; }

        /// <summary>
        /// Position of the panel in relation to the origin (0, 0) (in most cases it's the top left) of the game window
        /// </summary>
        public Measurement Position { get; set; }

        /// <summary>
        /// Represents the size of the panel, with X representing the width and Y representing the height
        /// </summary>
        public Measurement Size { get; set; }

        /// <summary>
        /// The position of the panel in pixels, relative to the top-left corner of the screen.
        /// </summary>
        public Vector2 RawPosition => Position.GetRawPixelValue(GraphicsDevice?.Viewport ?? new Viewport(0, 0, 1, 1));

        /// <summary>
        /// The size of the panel in pixels, representing the width and height on the screen.
        /// </summary>
        public Vector2 RawSize => Size.GetRawPixelValue(GraphicsDevice?.Viewport ?? new Viewport(0, 0, 1, 1));

        //We don't have a rotation property here because it would unnecessarily complicate things. Rotating every
        //element when the panel rotates would be a nightmare to handle

        /// <summary>
        /// Background texture of the panel. If null, then there is no texture and the background will be transparent.
        /// </summary>
        [JsonIgnore]
        public Texture2D? BackgroundTexture { get; set; }
        
        /// <summary>
        /// Represents the border of this panel. If null, then no border will be drawn.
        /// </summary>
        [JsonIgnore]
        public Border? Border { get; set; }

        //----------------
        // Constructors
        //----------------

        /// <summary>
        /// Constructs a panel given only a GraphicsDevice. The panel will have a default size of 100 100 and will be
        /// positioned at 0 0. The background will be white with a 5 pixel back border. 
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice that will be used for drawing</param>
        public Panel(GraphicsDevice graphicsDevice)
        {
            ElementContainer = new ElementContainer(this);

            Position = new Measurement(Vector2.Zero, MeasurementType.Pixel);
            Size = new Measurement(new Vector2(100), MeasurementType.Pixel);

            GraphicsDevice = graphicsDevice;

            var (width, height) = Size.GetRawPixelValue(graphicsDevice.Viewport).ToPoint();

            BackgroundTexture = TextureHelper.CreateTextureFromColor(graphicsDevice, Color.White, width, height);
            Border = new Border
            {
                Thickness = 5,
                Texture = TextureHelper.CreateTextureFromColor(graphicsDevice, Color.Black, width, height)
            };
        }

        /// <summary>
        /// Constructs a new Panel object with the specified position and size, background texture, and border.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to use for drawing the Panel</param>
        /// <param name="position">The position of the Panel in pixels or as a ratio of the screen</param>
        /// <param name="size">The size of the Panel in pixels or as a ratio of the screen</param>
        /// <param name="backgroundTexture">The texture to use as the background for the Panel. If null, the Panel will
        /// be transparent.</param>
        /// <param name="border">The border to draw around the Panel. If null, no border will be drawn.</param>
        public Panel(GraphicsDevice graphicsDevice, in Measurement position, in Measurement size,
            Texture2D? backgroundTexture, Border? border)
        {
            var (width, height) = size.GetRawPixelValue(graphicsDevice.Viewport).ToPoint();

            Texture2D transparentTexture =
                TextureHelper.CreateTextureFromColor(graphicsDevice, Color.Transparent, width, height);

            //if backgroundTexture is null, set it to the transparent texture created above
            ElementContainer = new ElementContainer(this);
            (GraphicsDevice, Position, Size, BackgroundTexture, Border) = 
                (graphicsDevice, position, size, backgroundTexture ?? transparentTexture, border);
        }

        /// <summary>
        /// Constructs a new Panel object with the specified position and size, background color, and border.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to use for drawing the Panel</param>
        /// <param name="position">The position of the Panel in pixels or as a ratio of the screen</param>
        /// <param name="size">The size of the Panel in pixels or as a ratio of the screen</param>
        /// <param name="backgroundColor">The color to use as the background for the Panel.</param>
        /// <param name="border">The border to draw around the Panel. If null, no border will be drawn.</param>
        public Panel(GraphicsDevice graphicsDevice, in Measurement position, in Measurement size,
            in Color backgroundColor, in Border? border)
        {
            ElementContainer = new ElementContainer(this);
            (GraphicsDevice, Position, Size, Border) = (graphicsDevice, position, size, border);

            var (width, height) = size.GetRawPixelValue(graphicsDevice.Viewport).ToPoint();

            BackgroundTexture =
                TextureHelper.CreateTextureFromColor(graphicsDevice, in backgroundColor, width, height);
        }

        //-------------------
        // Json Constructors
        //-------------------

        /// <summary>
        /// Used for deserialization from JSON data. Creates a new Panel object with specified position, 
        /// size, background texture, and border by deserializing JSON data. Not intended to be used in code. 
        /// After calling, ensure the newly created panel instance has GraphicsDevice property and
        /// SetDrawnBorderTextureField called. </summary>
        /// <param name="elements">The elements that the Panel will contain. If an element does not implement either
        /// IDrawable or IUpdatable, it will not be included.</param>
        /// <param name="position">The position of the Panel in pixels or as a ratio of the screen.</param>
        /// <param name="positionType">The type of measurement used for the Panel's position.</param>
        /// <param name="size">The size of the Panel in pixels or as a ratio of the screen.</param>
        /// <param name="sizeType">The type of measurement used for the Panel's size.</param>
        /// <param name="backgroundTexture">The texture to use as the background for the Panel. If null, the Panel will
        /// be transparent.</param>
        /// <param name="border">The border to draw around the Panel. If null, no border will be drawn.</param>
        [JsonConstructor]
        public Panel(object[] elements, Vector2 position, MeasurementType positionType, Vector2 size,
            MeasurementType sizeType, Texture2D backgroundTexture, Border? border)
        {
            ElementContainer = new ElementContainer(this);
            
            Position = new Measurement(position, positionType);
            Size = new Measurement(size, sizeType);
            
            (BackgroundTexture, Border) = (backgroundTexture, border);

            foreach (object elementObj in elements)
            {
                if (elementObj is not UserInterfaceElement element) continue;

                element.Owner = this;
            }
        }

        //----------
        // Methods
        //----------

        internal void LoadAllScripts()
        {
            if (Manager is null) return;
            
            ElementContainer.LoadAllElementScripts(Manager);
        }

        /// <summary>
        /// Update all UI elements that implement <see cref="IUpdateable"/> and all loaded scripts attached to UI
        /// elements.
        /// </summary>
        /// <param name="gameTime">The GameTime object provided by the currently active Game object.</param>
        public void Update(GameTime gameTime)
        {
            ElementContainer.UpdateElements(gameTime);
        }

        /// <summary>
        /// Draws all elements in the Drawables list as well as the Panel's background and border. 
        /// </summary>
        /// <param name="gameTime">A GameTime object containing information about the time that has elapsed since the last call to Update.</param>
        /// <param name="spriteBatch">The SpriteBatch used to draw the panel.</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;

            Vector2 position = Position.GetRawPixelValue(graphicsDevice.Viewport);
            Vector2 size = Size.GetRawPixelValue(graphicsDevice.Viewport);
            Point positionPoint = position.ToPoint();
            Point sizePoint = size.ToPoint();

            // Set up the scissor rect so that anything drawn off-panel will not be shown on the screen
            Rectangle oldScissorRect = graphicsDevice.ScissorRectangle;
            Rectangle panelRect = new(positionPoint, sizePoint);
            graphicsDevice.ScissorRectangle = panelRect;

            spriteBatch.Draw(BackgroundTexture, position, panelRect, Color.White);
            
            foreach (UserInterfaceElement element in ElementContainer.Values)
            {
                element.Draw(gameTime, spriteBatch);

                ResetSpriteBatch(spriteBatch);
                graphicsDevice.ScissorRectangle = panelRect;
            }

            ResetSpriteBatch(spriteBatch);
            graphicsDevice.ScissorRectangle = oldScissorRect;

            Border?.DrawBorder(spriteBatch, panelRect);
        }

        private static readonly RasterizerState ScissorTestEnabled = new() { ScissorTestEnable = true };
        
        private static void ResetSpriteBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(rasterizerState: ScissorTestEnabled);
        }

        /// <summary>
        /// Disposes all associated disposable resources.
        /// </summary>
        public void Dispose()
        {
            BackgroundTexture?.Dispose();
            Border?.Texture.Dispose();
            ElementContainer.Dispose();
        }

        public object Clone()
        {
            Panel panelClone = (Panel) MemberwiseClone();
            panelClone.ElementContainer = new ElementContainer(panelClone);
            
            ElementContainer.CloneElementsTo(panelClone.ElementContainer);

            return panelClone;
        }
    }
}