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

        private ImmutableText _text;
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

#if DEBUG
        private Border? _buttonBorder;
#endif

        public TextButton(Panel? parentPanel, Measurement position, Vector2 scale,
            Measurement buttonSize, float rotation, string text, int fontSize, Color textColor,
            FontSystem fontSystem, string? scriptName = null)
        {
            //The size of BaseButton represents the absolute bounds of the button in pixels. We change this in the
            //update method when we actually have a panel to reference to get the absolute pixel value. 
            Vector2 buttonSizePixels = buttonSize.GetRawPixelValue(parentPanel?.RawPosition ?? Vector2.One);

            _baseButton = new BaseButton(null, position, buttonSizePixels, rotation);
            _text = new ImmutableText(null, position, scale, rotation, text, fontSize, textColor, fontSystem);

            //for testing purpose 
            OnHover += (_, _) => Debug.WriteLine("OnHover");
            OnMouseLeave += (_, _) => Debug.WriteLine("OnMouseLeave");
            OnPress += (_, _) => Debug.WriteLine("OnPress");
            OnRelease += (_, _) => Debug.WriteLine("OnRelease");

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
                this.LoadBehaviorScript(callingPanel.Manager, _scriptName);
                
                _scriptName = null;
            }

            _baseButton.UpdatePositionAndSize(callingPanel.RawSize, Position, ButtonSize);
            _baseButton.Update(callingPanel, gameTime);
        }

        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch batch)
        {
            this.CopyTransformTo(_text);
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

            _buttonBorder?.DrawBorder(batch, new Rectangle(positionPixels, sizePixels));
#endif
        }

        public object Clone() =>
            new TextButton(ParentPanel, Position, Scale, ButtonSize, Rotation, Text, FontSize, _text.Color, FontSystem)
                { _scriptName = this._scriptName };
    }
}