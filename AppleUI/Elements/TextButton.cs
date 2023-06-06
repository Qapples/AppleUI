using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IDrawable = AppleUI.Interfaces.IDrawable;
using IUpdateable = AppleUI.Interfaces.IUpdateable;

namespace AppleUI.Elements
{
    public class TextButton : IButton, ITransform, IUpdateable, IDrawable
    {
        private Panel? _parentPanel;

        public Panel? ParentPanel
        {
            get => _parentPanel;
            set
            {
                _text.ParentPanel = value;
                _baseButton.ParentPanel = value;
                _parentPanel = value;
            }
        }
        
        public Measurement Position { get; set; }
        public Vector2 Scale { get; set; }
        public Measurement ButtonSize { get; set; }
        public float Rotation { get; set; }

        public string Text => _text.Text;
        public FontSystem FontSystem => _text.FontSystem;

        public Color Color
        {
            get => _text.Color;
            set => _text.Color = value;
        }

        public int FontSize
        {
            get => _text.FontSize;
            set => _text.FontSize = value;
        }
        
        public ButtonEvents ButtonEvents => _baseButton.ButtonEvents;

        private ImmutableText _text;
        private BaseButton _baseButton;

        private string? _scriptName;

#if DEBUG
        private Border? _buttonBorder;
#endif

        public TextButton(Panel? parentPanel, Measurement position, Vector2 scale,
            Measurement buttonSize, float rotation, string text, int fontSize, Color textColor,
            FontSystem fontSystem, string? scriptName = null)
        {
            _baseButton = new BaseButton(null, position, buttonSize, rotation);
            _text = new ImmutableText(null, position, scale, rotation, text, fontSize, textColor, fontSystem);

            (ParentPanel, Position, Scale, ButtonSize, Rotation, _scriptName) =
                (parentPanel, position, scale, buttonSize, rotation, scriptName);
        }

        [JsonConstructor]
        public TextButton(Vector2 position, MeasurementType positionType, Vector2 scale, Vector2 buttonSize,
            MeasurementType sizeType, float rotation, string text, int fontSize, Color textColor, FontSystem fontSystem,
            string? scriptName = null) : this(null, new Measurement(position, positionType), scale,
            new Measurement(buttonSize, sizeType), rotation, text, fontSize, textColor, fontSystem, scriptName)
        {
        }

        public void Update(Panel callingPanel, GameTime gameTime)
        {
            if (_scriptName is not null && callingPanel.Manager is not null)
            {
                ButtonEvents.LoadBehaviorScript(callingPanel.Manager, _scriptName);
                
                _scriptName = null;
            }
            
            this.CopyTransformTo(_baseButton);
            _baseButton.Update(callingPanel, gameTime);
        }

        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch batch)
        {
            this.CopyTransformTo(_text);
            _text.Position = _baseButton.GetCenterPositionPixels(callingPanel.RawSize);
            
            _text.Draw(callingPanel, gameTime, batch);

            //draw the bounds of the button in debug mode.
#if DEBUG
            if (_buttonBorder is null)
            {
                Texture2D borderTexture = new(batch.GraphicsDevice, 1, 1);
                borderTexture.SetData(new[] { Color.White });

                _buttonBorder = new Border(3, borderTexture);
            }

            Point positionPixels = _baseButton.GetDrawPosition(callingPanel).ToPoint();
            Point sizePixels = ButtonSize.GetRawPixelValue(callingPanel.RawSize).ToPoint();
            
            _buttonBorder?.DrawBorder(batch, new RotatableRectangle(positionPixels, sizePixels, Rotation));
#endif
        }

        public object Clone() =>
            new TextButton(ParentPanel, Position, Scale, ButtonSize, Rotation, Text, FontSize, _text.Color, FontSystem)
                { _scriptName = this._scriptName };
    }
}