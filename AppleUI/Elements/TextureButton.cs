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

        public (Vector2 Value, PositionType Type) Position { get; set; }
        public Vector2 Scale { get; set; }
        public Vector2 ButtonSize { get; set; }
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

        public TextureButton(Panel? parentPanel, Vector2 position, PositionType positionType, Vector2 scale,
            Vector2 buttonSize, float rotation, Texture2D texture)
            : this(position, positionType, scale, buttonSize, rotation, texture)
        {
            ParentPanel = parentPanel;
        }

        [JsonConstructor]
        public TextureButton(Vector2 position, PositionType positionType, Vector2 scale, Vector2 buttonSize,
            float rotation, Texture2D texture)
        {
            _baseButton = new BaseButton(null, position, positionType, buttonSize, rotation);
            _texture = new StaticTexture(null, position, positionType, scale, rotation, texture);

            (Position, Scale, ButtonSize, Rotation) = 
                ((position, positionType), scale, buttonSize, rotation);
        }

        public void Update(Panel callingPanel, GameTime gameTime)
        {
            this.CopyTransformTo(_baseButton);
            _baseButton.Scale = ButtonSize;
            
            _baseButton.Update(callingPanel, gameTime);
        }

        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch spriteBatch)
        {
            this.CopyTransformTo(_texture);

            _texture.Draw(callingPanel, gameTime, spriteBatch);
        }

        public object Clone() => new TextureButton(ParentPanel, Position.Value, Position.Type, Scale, ButtonSize,
            Rotation, _texture.Texture);
    }
}