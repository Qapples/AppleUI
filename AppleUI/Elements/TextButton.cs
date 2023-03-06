using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
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
        

        public (Vector2 Value, PositionType Type) Position { get; set; }
        public Vector2 Scale { get; set; }
        public Vector2 ButtonSize { get; set; }
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

        public TextButton(Panel? parentPanel, Vector2 position, PositionType positionType, Vector2 scale,
            Vector2 buttonSize, float rotation, string text, int fontSize, Color textColor, FontSystem fontSystem)
            : this(position, positionType, scale, buttonSize, rotation, text, fontSize, textColor, fontSystem)
        {
            ParentPanel = parentPanel;
        }

        [JsonConstructor]
        public TextButton(Vector2 position, PositionType positionType, Vector2 scale,
            Vector2 buttonSize, float rotation, string text, int fontSize, Color textColor, FontSystem fontSystem)
        {
            _baseButton = new BaseButton(null, position, positionType, buttonSize, rotation);
            _text = new ImmutableText(null, position, positionType, scale, rotation,
                text, fontSize, textColor, fontSystem);

            // //for testing purpose 
            // OnHover += (_, _) => Debug.WriteLine("OnHover");
            // OnMouseLeave += (_, _) => Debug.WriteLine("OnMouseLeave");
            // OnPress += (_, _) => Debug.WriteLine("OnPress");
            // OnRelease += (_, _) => Debug.WriteLine("OnRelease");

            (Position, Scale, ButtonSize, Rotation) =
                ((position, positionType), scale, buttonSize, rotation);
        }

        public void Update(Panel callingPanel, GameTime gameTime)
        {
            this.CopyTransformTo(_baseButton);
            _baseButton.Scale = ButtonSize;
            
            _baseButton.Update(callingPanel, gameTime);
        }

        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch batch)
        {
            this.CopyTransformTo(_text);
            _text.Draw(callingPanel, gameTime, batch);
        }

        public object Clone() => new TextButton(ParentPanel, Position.Value, Position.Type, Scale, ButtonSize, Rotation,
            Text, FontSize, _text.Color, FontSystem);
    }
}