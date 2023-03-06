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

        private (Vector2 Value, PositionType Type) _position;
        private Vector2 _scale;
        private Vector2 _buttonSize;
        private float _rotation;

        public (Vector2 Value, PositionType Type) Position
        {
            get => _position;
            set
            {
                _texture.Position = value;
                _baseButton.Position = value;
                _position = value;
            }
        }

        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _texture.Scale = value;
                _scale = value;
            }
        }

        public Vector2 ButtonSize
        {
            get => _buttonSize;
            set
            {
                _baseButton.Scale = value;
                _buttonSize = value;
            }
        }

        public float Rotation
        {
            get => _rotation;
            set
            {
                _texture.Rotation = value;
                _baseButton.Rotation = value;
                _rotation = value;
            }
        }
        
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
        {
            _baseButton = new BaseButton(parentPanel, position, positionType, buttonSize, rotation);
            _texture = new StaticTexture(parentPanel, texture, position, positionType, scale, rotation);
            
            (ParentPanel, Position, Scale, ButtonSize, Rotation) = 
                (parentPanel, (position, positionType), scale, buttonSize, rotation);
        }

        [JsonConstructor]
        public TextureButton(Vector2 position, PositionType positionType, Vector2 scale, Vector2 buttonSize,
            float rotation, Texture2D texture)
        {
            _baseButton = new BaseButton(null, position, positionType, buttonSize, rotation);
            _texture = new StaticTexture(null, texture, position, positionType, scale, rotation);
            
            (Position, Scale, ButtonSize, Rotation) = 
                ((position, positionType), scale, buttonSize, rotation);
        }

        public void Update(Panel callingPanel, GameTime gameTime)
        {
            _baseButton.Update(callingPanel, gameTime);
        }

        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch spriteBatch)
        {
            _texture.Draw(callingPanel, gameTime, spriteBatch);
        }

        public object Clone() => new TextureButton(ParentPanel, Position.Value, Position.Type, Scale, ButtonSize,
            Rotation, _texture.Texture);
    }
}