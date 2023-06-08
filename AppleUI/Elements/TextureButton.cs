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
        
        public ButtonEvents ButtonEvents => _baseButton.ButtonEvents;
        
        private StaticTexture _texture;
        private BaseButton _baseButton;

        private string? _scriptName;

        public TextureButton(Panel? parentPanel, Measurement position, Vector2 scale,
            Measurement buttonSize, float rotation, Texture2D texture, string? scriptName = null)
        {
            _baseButton = new BaseButton(null, position, buttonSize, rotation);
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
                ButtonEvents.LoadBehaviorScript(callingPanel.Manager, _scriptName);
                
                _scriptName = null;
            }

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