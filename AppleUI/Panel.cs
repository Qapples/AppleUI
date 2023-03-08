using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using AppleSerialization;
using AppleUI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using IDrawable = AppleUI.Interfaces.IDrawable;
using IUpdateable = AppleUI.Interfaces.IUpdateable;

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
        public Vector2 Position { get; set; }

        /// <summary>
        /// Represents the size of the panel, with X representing the width and Y representing the height
        /// </summary>
        public Vector2 Size { get; set; }

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
            (Drawables, Updateables, Elements, Position, Size, GraphicsDevice) = (new List<IDrawable>(),
                new List<IUpdateable>(), new List<IUserInterfaceElement>(),
                Vector2.Zero, new Vector2(100), graphicsDevice);

            //casting to an int right here might lose to accuracy when we do (width * height). 
            var (width, height) = ((int) Size.X, (int) Size.Y);

            BackgroundTexture = TextureHelper.CreateTextureFromColor(graphicsDevice, Color.White, width, height);
            Border = new Border
            {
                Thickness = 5,
                Texture = TextureHelper.CreateTextureFromColor(graphicsDevice, Color.Black, width, height)
            };
        }

        /// <summary>
        /// Constructs a panel object given all the necessary fields.
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice used to draw the panel and it's elements</param>
        /// <param name="position">The position of the panel in relation to 0, 0 of the game window (usually the top
        /// left)</param>
        /// <param name="size">The width (X) and height (Y) of the panel</param>
        /// <param name="backgroundTexture">The background texture of the panel. All elements are displayed on top
        /// of this texture. If null, it will be transparent.</param>
        /// <param name="border">The border of this panel. The border struct represents the texture that is displayed
        /// surrounding the panel as well as the width. If null, a border will not be drawn</param>
        public Panel(GraphicsDevice graphicsDevice, in Vector2 position, in Vector2 size, Texture2D? backgroundTexture,
            in Border? border)
        {
            var (width, height) = ((int) size.X, (int) size.Y);
            Texture2D transparentTexture =
                TextureHelper.CreateTextureFromColor(graphicsDevice, Color.Transparent, width, height);

            //if backgroundTexture is null, set it to the transparent texture created above
            (Drawables, Updateables, Elements, GraphicsDevice, Position, Size, BackgroundTexture, Border) = (
                new List<IDrawable>(), new List<IUpdateable>(), new List<IUserInterfaceElement>(), graphicsDevice,
                position, size, backgroundTexture ?? transparentTexture, border);
        }

        /// <summary>
        /// Constructs a panel object given a background color rather than a background texture
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice used to draw the panel and it's elements</param>
        /// <param name="position">The position of the panel in relation to 0, 0 of the game window (usually the top
        /// left)</param>
        /// <param name="size">The width (X) and height (Y) of the panel</param>
        /// <param name="backgroundColor">The background color of this panel. All elements are displayed on top
        /// of this color</param>
        /// <param name="border">The border of this panel. The border struct represents the texture that is displayed
        /// surrounding the panel as well as the width. If null, a border will not be drawn</param>
        public Panel(GraphicsDevice graphicsDevice, in Vector2 position, in Vector2 size, in Color backgroundColor,
            in Border? border)
        {
            (Drawables, Updateables, Elements, GraphicsDevice, Position, Size, Border) = (new List<IDrawable>(),
                new List<IUpdateable>(), new List<IUserInterfaceElement>(), graphicsDevice, position, size, border);

            var (width, height) = ((int) size.X, (int) size.Y);

            BackgroundTexture =
                TextureHelper.CreateTextureFromColor(graphicsDevice, in backgroundColor, width, height);
        }

        //-------------------
        // Json Constructors
        //-------------------

        /// <summary>
        /// Constructor that is used for Json serialization provided a path to a texture. Not intended to be used in
        /// code. After calling, ensure that the newly created panel instance has a GraphicsDevice property and that
        /// SetDrawnBorderTextureField is called. </summary>
        /// <param name="elements">Elements that the panel will contain. If element does not implement either IDrawable
        /// or IUpdatable, it will not be included</param>
        /// <param name="position">Position of the Panel (from the top left)</param>
        /// <param name="size">Size of the panel</param>
        /// <param name="backgroundTexture">Represents the texture that is displayed in the background</param>
        /// <param name="border">The border that will be drawn surrounding the panel</param>
        //we separated the border parameter into thickness and texture because right now customs objects
        //(not custom types!) cannot serialized yet (although that will change)
        [JsonConstructor]
        public Panel(object[] elements, Vector2 position, Vector2 size, Texture2D backgroundTexture, Border? border)
        {
            (Drawables, Updateables, Elements) = 
                (new List<IDrawable>(), new List<IUpdateable>(), new List<IUserInterfaceElement>());
            (Position, Size, BackgroundTexture, Border) = (position, size, backgroundTexture, border);

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
        /// Draws all the instances in Drawables 
        /// </summary>
        /// <param name="gameTime">The GameTime object provided by the currently active Game object</param>
        /// <param name="batch">SpriteBatch that will be used to draw the instances in Drawables</param>
        public void Draw(GameTime gameTime, SpriteBatch batch)
        {
            if (GraphicsDevice is null)
            {
                //NoNullAllowed refers to something else but it will work in this context
                Debug.WriteLine(
                    "The GraphicsDevice property is null! Did you assign it a value after deserializing it? " +
                    "The Panel will not be drawn");
                
                return;
            }

            //Set up the scissor rect so that anything drawn off-panel will not be shown drawn on the screen 
            Rectangle batchScissorRect = batch.GraphicsDevice.ScissorRectangle;
            batch.GraphicsDevice.ScissorRectangle = new Rectangle(Position.ToPoint(), Size.ToPoint());

            //draw the background and everything that is drawable
            batch.Draw(BackgroundTexture, Position, Color.White);
            foreach (var drawable in Drawables)
            {
                drawable.Draw(this, gameTime, batch);
            }

            batch.GraphicsDevice.ScissorRectangle = batchScissorRect;

            Border?.DrawBorder(batch, new Rectangle(Position.ToPoint(), Size.ToPoint()));
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