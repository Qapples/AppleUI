using System;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IDrawable = AppleUI.Interfaces.IDrawable;
using IUpdateable = AppleUI.Interfaces.IUpdateable;

namespace AppleUI.Elements
{
    public class TextureButton : IButton, ITransform, IUpdateable, IDrawable
    {
        private Panel? _parentPanel;

        public Panel? ParentPanel
        {
            get => _parentPanel;
            set
            {
                _texture.ParentPanel = value;
                _baseButton.ParentPanel = value;
                _parentPanel = value;
            }
        }

        public Measurement Position { get; set; }
        public Vector2 Scale { get; set; }
        public Measurement ButtonSize { get; set; }
        public float Rotation { get; set; }
        
        private StaticTexture _texture;
        private BaseButton _baseButton;

        public event IButton.ButtonEventDelegate? OnHover
        {
            add => _baseButton.OnHover += value;
            remove => _baseButton.OnHover -= value;
        }
        
        public event IButton.ButtonEventDelegate? OnMouseLeave
        {
            add => _baseButton.OnMouseLeave += value;
            remove => _baseButton.OnMouseLeave -= value;
        }
        
        public event IButton.ButtonEventDelegate? OnPress
        {
            add => _baseButton.OnPress += value;
            remove => _baseButton.OnPress -= value;
        }

        public event IButton.ButtonEventDelegate? OnRelease
        {
            add => _baseButton.OnRelease += value;
            remove => _baseButton.OnRelease -= value;
        }

        private string? _scriptName;

        public TextureButton(Panel? parentPanel, Measurement position, Vector2 scale,
            Measurement buttonSize, float rotation, Texture2D texture, string? scriptName = null)
        {
            //The size of BaseButton represents the absolute bounds of the button in pixels. We change this in the
            //update method when we actually have a panel to reference to get the absolute pixel value. 
            Vector2 buttonSizePixels = buttonSize.GetRawPixelValue(parentPanel?.RawPosition ?? Vector2.One);

            _baseButton = new BaseButton(null, position, buttonSizePixels, rotation);
            _texture = new StaticTexture(null, position, scale, rotation, texture);

            (ParentPanel, Position, Scale, ButtonSize, Rotation, _scriptName) =
                (parentPanel, position, scale, buttonSize, rotation, scriptName);
        }

        [JsonConstructor]
        public TextureButton(Vector2 position, MeasurementType positionType, Vector2 scale, Vector2 buttonSize,
            MeasurementType sizeType, float rotation, Texture2D texture, string? scriptName = null) :
            this(null, new Measurement(position, positionType), scale, new Measurement(buttonSize, sizeType), rotation,
                texture, scriptName)
        {
        }

        public void Update(Panel callingPanel, GameTime gameTime)
        {
            if (_scriptName is not null && callingPanel.Manager is not null)
            {
                this.LoadBehaviorScript(callingPanel.Manager, _scriptName);
                
                _scriptName = null;
            }

            _baseButton.UpdatePositionAndSize(callingPanel.RawSize, Position, ButtonSize);
            _baseButton.Update(callingPanel, gameTime);
        }

        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch spriteBatch)
        {
            this.CopyTransformTo(_texture);

            _texture.Draw(callingPanel, gameTime, spriteBatch);
        }

        public object Clone() =>
            new TextureButton(ParentPanel, Position, Scale, ButtonSize, Rotation, _texture.Texture);
    }
}