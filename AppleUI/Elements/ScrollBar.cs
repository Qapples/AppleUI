using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AppleUI.Elements
{
    public sealed class ScrollBar : ICloneable
    {
        public UserInterfaceElement? Owner { get; internal set; }

        public Location AttachedLocation { get; set; }
        
        public Orientation Orientation => AttachedLocation is Location.Left or Location.Right
            ? Orientation.Vertical
            : Orientation.Horizontal;
        
        public TextureButton UpButton { get; set; }
        public TextureButton DownButton { get; set; }
        public TextureButton Bar { get; set; }
        
        public Texture2D BackgroundTexture { get; set; }

        private float _scrollAmountPercent;

        public float ScrollAmountPercent //between 0 and 1
        {
            get
            {
                _scrollAmountPercent = MathHelper.Clamp(_scrollAmountPercent, 0f, 1f);
                return _scrollAmountPercent;
            }
            set => _scrollAmountPercent = MathHelper.Clamp(value, 0f, 1f);
        }
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
                    Location.Right => new Vector2(Owner?.RawSize.X - drawSize.X ?? 1, 0f),
                    Location.Bottom => new Vector2(0f, Owner?.RawSize.Y - drawSize.Y ?? 1),
                    _ => new Vector2(0f, 0f)
                };
            }
        }

        private Vector2 DrawSize
        {
            get
            {
                Vector2 size = Size.Type == MeasurementType.Ratio && Owner is not null
                    ? Owner.RawSize
                    : Vector2.One;

                return Orientation is Orientation.Vertical
                    ? new Vector2(Size.Value * size.X, size.Y)
                    : new Vector2(size.X, Size.Value * size.Y);
            }
        }
        
        public ScrollBar(UserInterfaceElement? owner, Location attachedLocation, Texture2D scrollButtonTexture,
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
            this(null, attachedLocation, scrollButtonTexture, barTexture, backgroundTexture, (size, sizeType))
        {
            //the Owner is set by the serialization system, which is why it's null here
        }
        
        public void Update(GameTime gameTime)
        {
            UpdateElementTransformAndSize();
            UpdateScrollAmountFromScrollWheel();
            
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
        
        //we're checking outside the sums to avoid having to do a closure allocation
        public int UpdateMaxScrollAmount(IEnumerable<UserInterfaceElement> elements) => MaxScrollAmountPixels =
            (int) (Orientation is Orientation.Vertical
                ? elements.Sum(element => element.RawSize.Y)
                : elements.Sum(element => element.RawSize.X));

        private void DrawElement(UserInterfaceElement element, GameTime gameTime, SpriteBatch spriteBatch)
        {
            ElementTransform prevTransform = element.Transform;
            Vector2 elementDrawPosition =
                element.Transform.GetDrawPosition(Owner?.RawPosition ?? Vector2.Zero, Owner?.RawSize ?? Vector2.One);

            element.Transform = element.Transform with
            {
                Position = new Measurement(elementDrawPosition, MeasurementType.Pixel)
            };
            
            element.Draw(gameTime, spriteBatch);
            
            element.Transform = prevTransform;
        }

        private int _previousScrollWheelValue;
        private const float ScrollSpeed = 2f;
        
        private void UpdateScrollAmountFromScrollWheel()
        {
            int currentScrollWheelValue = -Mouse.GetState().ScrollWheelValue;
            float scrollAmountPixels = (currentScrollWheelValue - _previousScrollWheelValue) / 10f * ScrollSpeed;
            ScrollAmountPercent += scrollAmountPixels / MaxScrollAmountPixels;

            _previousScrollWheelValue = currentScrollWheelValue;
        }

        private void UpdateElementTransformAndSize()
        {
            //This method is pretty long and hard to read, but I'm not sure if I can improve it much and I don't want to
            //spend too much time on something that is quite trivial in the grand scheme of things.
            
            bool isVertical = Orientation is Orientation.Vertical;
            Vector2 buttonSizePixels = isVertical ? new Vector2(DrawSize.X) : new Vector2(DrawSize.Y);
            Vector2 ownerSizePixels = Owner?.RawSize ?? Vector2.Zero;
            
            Vector2 upButtonPosition = new(
                AttachedLocation is Location.Left or Location.Top ? 0f : ownerSizePixels.X - buttonSizePixels.X,
                AttachedLocation is Location.Left or Location.Right or Location.Top
                    ? 0f
                    : ownerSizePixels.Y - buttonSizePixels.Y);

            Vector2 downButtonPosition = new(
                AttachedLocation is Location.Left ? 0f : ownerSizePixels.X - buttonSizePixels.X,
                AttachedLocation is Location.Top ? 0f : ownerSizePixels.Y - buttonSizePixels.Y);

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
                ? (ownerSizePixels.Y / MaxScrollAmountPixels) * (ownerSizePixels.Y - buttonSizePixels.Y * 2f)
                : (ownerSizePixels.X / MaxScrollAmountPixels) * (ownerSizePixels.X - buttonSizePixels.X * 2f);

            float maximumOffset =
                isVertical ? ownerSizePixels.Y - buttonSizePixels.Y : ownerSizePixels.X - buttonSizePixels.X;
            float minimumOffset = isVertical ? buttonSizePixels.Y : buttonSizePixels.X;

            float currentOffset = minimumOffset + (maximumOffset - minimumOffset - barSize) * ScrollAmountPercent;
            Vector2 barPosition = isVertical
                ? new Vector2(upButtonPosition.X, currentOffset)
                : new Vector2(currentOffset, upButtonPosition.Y);

            Bar.Transform = new ElementTransform(new Measurement(barPosition, MeasurementType.Pixel), Vector2.One, 0f);
            Bar.ButtonObject.Size = new Measurement(
                isVertical ? new Vector2(buttonSizePixels.X, barSize) : new Vector2(barSize, buttonSizePixels.Y),
                MeasurementType.Pixel);
        }

        public object Clone() => MemberwiseClone();
    }
}