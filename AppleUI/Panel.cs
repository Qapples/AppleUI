using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using IDrawable = AppleUI.Interfaces.IDrawable;
using IUpdateable = AppleUI.Interfaces.IUpdateable;
using TextureHelper = AppleUI.TextureHelper;

namespace AppleUI
{
    /// <summary>
    /// A UI Panel that can contain UI elements.
    /// </summary>
    public sealed class Panel : IDisposable, ICloneable
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
        /// A list of all elements that can be drawn
        /// </summary>
        [JsonIgnore]
        public List<IDrawable> Drawables { get; set; }

        /// <summary>
        /// A list of all elements that can be updated.
        /// </summary>
        [JsonIgnore]
        public List<IUpdateable> Updateables { get; set; }

        /// <summary>
        /// Represents all elements that are a part of this panel. Includes all elements in both Drawables and Updateables
        /// </summary>
        public List<IUserInterfaceElement> Elements { get; set; }

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
            (Drawables, Updateables, Elements) = (new List<IDrawable>(),
                new List<IUpdateable>(), new List<IUserInterfaceElement>());

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
            (Drawables, Updateables, Elements, GraphicsDevice, Position, Size, BackgroundTexture, Border) = (
                new List<IDrawable>(), new List<IUpdateable>(), new List<IUserInterfaceElement>(), graphicsDevice,
                position, size, backgroundTexture ?? transparentTexture, border);
        }

        /// <summary>
        /// Constructs a new Panel object with the specified position and size, background color, and border.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to use for drawing the Panel</param>
        /// <param name="position">The position of the Panel in pixels or as a ratio of the screen</param>
        /// <param name="size">The size of the Panel in pixels or as a ratio of the screen</param>
        /// <param name="backgroundColor">The color to use as the background for the Panel.</param>
        /// <param name="border">The border to draw around the Panel. If null, no border will be drawn.</param>
        public Panel(GraphicsDevice graphicsDevice, in Measurement position, in Measurement size, in Color backgroundColor,
            in Border? border)
        {
            (Drawables, Updateables, Elements, GraphicsDevice, Position, Size, Border) = (new List<IDrawable>(),
                new List<IUpdateable>(), new List<IUserInterfaceElement>(), graphicsDevice, position, size, border);

            var (width, height) = size.GetRawPixelValue(graphicsDevice.Viewport).ToPoint();

            BackgroundTexture =
                TextureHelper.CreateTextureFromColor(graphicsDevice, in backgroundColor, width, height);
        }

        private static (int width, int height) GetSizeWithMeasurementType(GraphicsDevice graphicsDevice, Vector2 size,
            MeasurementType type) => type switch
        {
            MeasurementType.Ratio => ((int) (graphicsDevice.Viewport.X * size.X),
                (int) (graphicsDevice.Viewport.Y * size.Y)),
            _ => ((int) size.X, (int) size.Y),
        };

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
            (Drawables, Updateables, Elements) =
                (new List<IDrawable>(), new List<IUpdateable>(), new List<IUserInterfaceElement>());
            
            Position = new Measurement(position, positionType);
            Size = new Measurement(size, sizeType);
            
            (BackgroundTexture, Border) = (backgroundTexture, border);

            foreach (object elementObj in elements)
            {
                if (elementObj is IUserInterfaceElement element)
                {
                    element.ParentPanel = this;
                    Insert(element);
                }
            }
        }

        //----------
        // Methods
        //----------

        /// <summary>
        /// Update all the instances in Updatables property
        /// </summary>
        /// <param name="gameTime">The GameTime object provided by the currently active Game object</param>
        public void Update(GameTime gameTime)
        {
            foreach (var updatable in Updateables)
            {
                updatable.Update(this, gameTime);
            }
        }

        /// <summary>
        /// Draws all elements in the Drawables list as well as the Panel's background and border. 
        /// </summary>
        /// <param name="gameTime">A GameTime object containing information about the time that has elapsed since the last call to Update.</param>
        /// <param name="batch">The SpriteBatch used to draw the panel.</param>
        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            if (GraphicsDevice is null)
            {
                Debug.WriteLine(
                    "The GraphicsDevice property is null! Did you assign it a value after deserializing it? The Panel will not be drawn.");
                return;
            }

            Vector2 position = Position.GetRawPixelValue(GraphicsDevice.Viewport);
            Vector2 size = Size.GetRawPixelValue(GraphicsDevice.Viewport);
            Point positionPoint = position.ToPoint();
            Point sizePoint = size.ToPoint();

            // Set up the scissor rect so that anything drawn off-panel will not be shown on the screen
            Rectangle batchScissorRect = batch.GraphicsDevice.ScissorRectangle;
            batch.GraphicsDevice.ScissorRectangle = new Rectangle(positionPoint, sizePoint);

            // Draw the background and everything that is drawable
            batch.Draw(BackgroundTexture, position, new Rectangle(0, 0, sizePoint.X, sizePoint.Y),
                Color.White);
            foreach (var drawable in Drawables)
            {
                drawable.Draw(this, gameTime, batch);
            }

            batch.GraphicsDevice.ScissorRectangle = batchScissorRect;

            Border?.DrawBorder(batch, new Rectangle(positionPoint, sizePoint));
        }


        /// <summary>
        /// Inserts an object into the panel into the appropriate list. If the object implements IDrawable, it will be
        /// put into the Drawables list, and so on.
        /// </summary>
        /// <param name="obj">Object to insert</param>
        public void Insert<T>(T obj) where T : IUserInterfaceElement
        {
            bool added = false;

            if (obj is IDrawable drawable)
            {
                Drawables.Add(drawable);
                added = true;
            }

            if (obj is IUpdateable updateable)
            {
                Updateables.Add(updateable);
                added = true;
            }

            if (added)
            {
                Elements.Add(obj);
            }
        }

        /// <summary>
        /// Disposes all associated disposable resources.
        /// </summary>
        public void Dispose()
        {
            BackgroundTexture?.Dispose();
            Border?.Texture.Dispose();
        }

        public object Clone()
        {
            //Do a shallow clone of each object in each collection.
            List<T> CloneList<T>(List<T> listToClone) where T : IUserInterfaceElement
            {
                List<T> clonedList = new();
                
                foreach (T element in listToClone)
                {
                    T elementClone = (T) element.Clone();

                    clonedList.Add(elementClone);
                }

                return clonedList;
            }

            List<IUserInterfaceElement> clonedElements = CloneList(Elements);
            List<IDrawable> clonedDrawables = new();
            List<IUpdateable> clonedUpdateables = new();

            foreach (IUserInterfaceElement element in clonedElements)
            {
                //We're not using a switch because an element can be an IDrawable, IUpdateable, etc. at the same time
                //and a switch statement wont add the element to all the lists it should be a part of.
                if (element is IDrawable drawable) clonedDrawables.Add(drawable);
                if (element is IUpdateable updateable) clonedUpdateables.Add(updateable);
            }
            
            Panel panelClone = (Panel) MemberwiseClone();
            panelClone.Drawables = clonedDrawables;
            panelClone.Updateables = clonedUpdateables;
            panelClone.Elements = clonedElements;

            return panelClone;
        }
    }
}