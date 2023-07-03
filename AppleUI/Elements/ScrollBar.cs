using System;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    public sealed class ScrollBar : ICloneable
    {
        public UserInterfaceElement Owner { get; internal set; }

        public Location AttachedLocation { get; set; }
        
        public Orientation Orientation => AttachedLocation is Location.Left or Location.Right
            ? Orientation.Vertical
            : Orientation.Horizontal;
        
        public TextureButton UpButton { get; set; }
        public TextureButton DownButton { get; set; }
        public TextureButton Bar { get; set; }
        
        public Texture2D BackgroundTexture { get; set; }
        
        public float ScrollAmount { get; set; } //between 0 and 1
        public int MaxScrollAmountPixels { get; private set; }

        public (float Value, MeasurementType Type) Size { get; set; }
        
        private Vector2 DrawPosition
        {
            get
            {
                Vector2 drawSize = DrawSize;

                return AttachedLocation switch
                {
                    Location.Left or Location.Top => new Vector2(0f, 0f),
                    Location.Right => new Vector2(Owner.RawSize.X - drawSize.X, 0f),
                    Location.Bottom => new Vector2(0f, Owner.RawSize.Y - drawSize.Y),
                    _ => new Vector2(0f, 0f)
                };
            }
        }

        private Vector2 DrawSize
        {
            get
            {
                bool sizeIsRatio = Size.Type == MeasurementType.Ratio;
                
                return Orientation is Orientation.Vertical ?
                    new Vector2(Size.Value * (sizeIsRatio ? Owner.RawSize.X : 1f), Owner.RawSize.Y) : 
                    new Vector2(Owner.RawSize.X, Size.Value * (sizeIsRatio ? Owner.RawSize.Y : 1f));
            }
        }
        
        public ScrollBar(UserInterfaceElement owner, Location attachedLocation, Texture2D scrollButtonTexture,
            Texture2D barTexture, Texture2D backgroundTexture, (float Value, MeasurementType Type) size)
        {
            (Owner, AttachedLocation, BackgroundTexture, Size) =
                (owner, attachedLocation, backgroundTexture, size);

            UpButton = new TextureButton(null, default, default, scrollButtonTexture);
            DownButton = new TextureButton(null, default, default, scrollButtonTexture);
            Bar = new TextureButton(null, default, default, barTexture);

            UpdateElementTransformAndSize();
        }

        [JsonConstructor]
        public ScrollBar(Location attachedLocation, Texture2D scrollButtonTexture, Texture2D barTexture,
            Texture2D backgroundTexture, float size, MeasurementType sizeType) :
            this(null!, attachedLocation, scrollButtonTexture, barTexture, backgroundTexture, (size, sizeType))
        {
            //the Owner is set by the serialization system, which is why it's null here
        }

        public void Update(GameTime gameTime)
        {
            UpdateElementTransformAndSize();
            
            UpButton.Update(gameTime);
            DownButton.Update(gameTime);
            Bar.Update(gameTime);
        }
        
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawElement(UpButton, gameTime, spriteBatch);
            DrawElement(DownButton, gameTime, spriteBatch);
            DrawElement(Bar, gameTime, spriteBatch);
        }

        private void DrawElement(UserInterfaceElement element, GameTime gameTime, SpriteBatch spriteBatch)
        {
            ElementTransform prevTransform = element.Transform;
            Vector2 elementDrawPosition = element.Transform.GetDrawPosition(Owner.RawPosition, Owner.RawSize);

            element.Transform = element.Transform with
            {
                Position = new Measurement(elementDrawPosition, MeasurementType.Pixel)
            };
            
            element.Draw(gameTime, spriteBatch);
            
            element.Transform = prevTransform;
        }

        private void UpdateElementTransformAndSize()
        {
            //This method is pretty long and hard to read, but I'm not sure if I can improve it much and I don't want to
            //spend too much time on something that is quite trivial in the grand scheme of things.
            
            bool isVertical = Orientation is Orientation.Vertical;
            Vector2 buttonSizePixels = isVertical ? new Vector2(DrawSize.X) : new Vector2(DrawSize.Y);

            Vector2 upButtonPosition = new(
                AttachedLocation is Location.Left or Location.Top ? 0f : Owner.RawSize.X - buttonSizePixels.X,
                AttachedLocation is Location.Left or Location.Right or Location.Top
                    ? 0f
                    : Owner.RawSize.Y - buttonSizePixels.Y);

            Vector2 downButtonPosition = new(
                AttachedLocation is Location.Left ? 0f : Owner.RawSize.X - buttonSizePixels.X,
                AttachedLocation is Location.Top ? 0f : Owner.RawSize.Y - buttonSizePixels.Y);

            float upButtonRotation = AttachedLocation switch
            {
                Location.Left or Location.Right => 0f,
                Location.Top => MathHelper.PiOver2,
                Location.Bottom => -MathHelper.PiOver2,
                _ => 0f
            };

            float downButtonRotation = -upButtonRotation;
            
            UpButton.Transform = new ElementTransform(new Measurement(upButtonPosition, MeasurementType.Pixel),
                Vector2.One, upButtonRotation);
            DownButton.Transform = new ElementTransform(new Measurement(downButtonPosition, MeasurementType.Pixel),
                Vector2.One, downButtonRotation);
            UpButton.ButtonObject.Size = new Measurement(buttonSizePixels, MeasurementType.Pixel);
            DownButton.ButtonObject.Size = new Measurement(buttonSizePixels, MeasurementType.Pixel);

            float barSize = isVertical
                ? (MaxScrollAmountPixels / Owner.RawSize.Y) * (Owner.RawSize.Y - buttonSizePixels.Y * 2f)
                : (MaxScrollAmountPixels / Owner.RawSize.X) * (Owner.RawSize.X - buttonSizePixels.X * 2f);

            float maximumOffset = isVertical ? Owner.RawSize.Y - buttonSizePixels.Y : Owner.RawSize.X - buttonSizePixels.X;
            float minimumOffset = isVertical ? buttonSizePixels.Y : buttonSizePixels.X;

            float currentOffset =
                minimumOffset + (maximumOffset - minimumOffset) * MathHelper.Clamp(ScrollAmount, 0f, 1f);
            Vector2 barPosition = isVertical ? new Vector2(0f, currentOffset) : new Vector2(currentOffset, 0f);

            Bar.Transform = new ElementTransform(new Measurement(barPosition, MeasurementType.Pixel), Vector2.One, 0f);
            Bar.ButtonObject.Size = new Measurement(
                isVertical ? new Vector2(buttonSizePixels.X, barSize) : new Vector2(barSize, buttonSizePixels.Y),
                MeasurementType.Pixel);
        }

        public object Clone() => MemberwiseClone();
    }
}