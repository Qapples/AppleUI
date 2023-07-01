using System;
using AppleUI.Interfaces;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    public sealed class ScrollBar
    {
        public UserInterfaceElement Owner { get; set; }

        public Location AttachedLocation { get; set; }
        
        public Orientation Orientation => AttachedLocation is Location.Left or Location.Right
            ? Orientation.Vertical
            : Orientation.Horizontal;
        
        public TextButton UpButton { get; set; }
        public TextButton DownButton { get; set; }
        public TextureButton Bar { get; set; }
        
        public float ScrollAmount { get; set; } //between 0 and 1
        public int MaxScrollAmountPixels { get; set; }

        public Color Color { get; set; }
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
        
        internal readonly FontSystem ButtonFontSystem;

        public ScrollBar(UserInterfaceElement owner, Location attachedLocation, Color color,
            (float Value, MeasurementType Type) size, FontSystem fontSystem)
        {
            (Owner, AttachedLocation, Color, Size, ButtonFontSystem) = (owner, attachedLocation, color, size, fontSystem);

            UpButton = new TextButton(null, default, default, "^", 32, Color.White, fontSystem);
            DownButton = new TextButton(null, default, default, "▼", 32, Color.White, fontSystem);
            Bar = new TextureButton(null, default, default, TextureHelper.BlankTexture);

            UpdateElementTransformAndSize();
        }

        public void Update(GameTime gameTime)
        {
            UpdateElementTransformAndSize();
            
            UpButton.Update(gameTime);
            DownButton.Update(gameTime);
            Bar.Update(gameTime);
        }

        private Texture2D? _barTexture;

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_barTexture is null)
            {
                _barTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _barTexture.SetData(new[] { Color });
                
                Bar.TextureObject.Texture = _barTexture;
            }
            
            UpButton.Draw(gameTime, spriteBatch);
            DownButton.Draw(gameTime, spriteBatch);
            Bar.Draw(gameTime, spriteBatch);
        }

        private void UpdateElementTransformAndSize()
        {
            //This method is pretty long and hard to read, but I'm not sure if I can improve it much and I don't want to
            //spend too much time on something that is quite trivial in the grand scheme of things.
            
            bool isVertical = Orientation is Orientation.Vertical;
            Vector2 buttonSize = isVertical ? new Vector2(DrawSize.X) : new Vector2(DrawSize.Y);

            Vector2 upButtonPosition = new(
                AttachedLocation is Location.Left or Location.Top ? 0f : Owner.RawSize.X - buttonSize.X,
                AttachedLocation is Location.Left or Location.Right or Location.Top
                    ? 0f
                    : Owner.RawSize.Y - buttonSize.Y);

            Vector2 downButtonPosition = new(
                AttachedLocation is Location.Left ? 0f : Owner.RawSize.X - buttonSize.X,
                AttachedLocation is Location.Top ? 0f : Owner.RawSize.Y - buttonSize.Y);

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

            float barSize = isVertical
                ? (MaxScrollAmountPixels / Owner.RawSize.Y) * (Owner.RawSize.Y - buttonSize.Y * 2f)
                : (MaxScrollAmountPixels / Owner.RawSize.X) * (Owner.RawSize.X - buttonSize.X * 2f);

            float maximumOffset = isVertical ? Owner.RawSize.Y - buttonSize.Y : Owner.RawSize.X - buttonSize.X;
            float minimumOffset = isVertical ? buttonSize.Y : buttonSize.X;

            float currentOffset =
                minimumOffset + (maximumOffset - minimumOffset) * MathHelper.Clamp(ScrollAmount, 0f, 1f);
            Vector2 barPosition = isVertical ? new Vector2(0f, currentOffset) : new Vector2(currentOffset, 0f);

            Bar.Transform = new ElementTransform(new Measurement(barPosition, MeasurementType.Pixel), Vector2.One, 0f);
            Bar.ButtonObject.Size = new Measurement(
                isVertical ? new Vector2(buttonSize.X, barSize) : new Vector2(barSize, buttonSize.Y),
                MeasurementType.Pixel);
        }
    }
}