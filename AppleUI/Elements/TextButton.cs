using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IDrawable = AppleUI.Interfaces.IDrawable;
using IUpdateable = AppleUI.Interfaces.IUpdateable;
using Quaternion = System.Numerics.Quaternion;

namespace AppleUI.Elements
{
    public class TextButton : IButton, ITransform, IUpdateable, IDrawable, IScriptableElement
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

        public IElementBehaviorScript[] Scripts { get; set; }

        public Measurement Position { get; set; }
        public Vector2 Scale { get; set; }
        public Measurement ButtonSize { get; set; }
        public float Rotation { get; set; }

        public string Text => _text.Text;
        public FontSystem FontSystem => _text.FontSystem;

        public Color TextColor
        {
            get => _text.TextColor;
            set => _text.TextColor = value;
        }

        public int FontSize
        {
            get => _text.FontSize;
            set => _text.FontSize = value;
        }
        
        public ButtonEvents ButtonEvents => _baseButton.ButtonEvents;

        private ImmutableText _text;
        private BaseButton _baseButton;

        private ElementScriptInfo[] _scriptInfos;

#if DEBUG
        private const bool DrawButtonBorder = false;
        private Border? _buttonBorder;
#endif

        public TextButton(Panel? parentPanel, Measurement position, Vector2 scale,
            Measurement buttonSize, float rotation, string text, int fontSize, Color textColor,
            FontSystem fontSystem, IElementBehaviorScript[]? scripts = null)
        {
            _baseButton = new BaseButton(null, position, buttonSize, rotation);
            _text = new ImmutableText(null, position, scale, rotation, text, fontSize, textColor, fontSystem);

            (ParentPanel, Position, Scale, ButtonSize, Rotation) =
                (parentPanel, position, scale, buttonSize, rotation);
            
            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        [JsonConstructor]
        public TextButton(Vector2 position, MeasurementType positionType, Vector2 scale, Vector2 buttonSize,
            MeasurementType sizeType, float rotation, string text, int fontSize, Color textColor, FontSystem fontSystem,
            object[]? scripts) : this(null, new Measurement(position, positionType), scale,
            new Measurement(buttonSize, sizeType), rotation, text, fontSize, textColor, fontSystem)
        {
            _scriptInfos = scripts?.Cast<ElementScriptInfo>().ToArray() ?? _scriptInfos;
        }

        public void LoadScripts(UserInterfaceManager manager)
        {
            //ButtonEvents only loads scripts that implement IButtonBehavior.
            ButtonEvents.LoadBehaviorScripts(this, manager, _scriptInfos);
            Scripts = manager.LoadElementBehaviorScripts(this, _scriptInfos);
        }

        public void Update(Panel callingPanel, GameTime gameTime)
        {
            this.CopyTransformTo(_baseButton);
            _baseButton.Update(callingPanel, gameTime);
        }

        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch batch)
        {
            this.CopyTransformTo(_text);
            
            Vector2 boundsRotated =
                Vector2.Transform(_text.Bounds / 2f, Quaternion.CreateFromYawPitchRoll(0f, 0f, Rotation));
            Vector2 buttonCenter = _baseButton.GetCenterPositionPixels(callingPanel.RawSize).Value;
            _text.Position = new Measurement(buttonCenter - boundsRotated, MeasurementType.Pixel);

            _text.Draw(callingPanel, gameTime, batch);

            //draw the bounds of the button in debug mode.
#if DEBUG
            if (!DrawButtonBorder) return;
            
            if (_buttonBorder is null)
            {
                Texture2D borderTexture = new(batch.GraphicsDevice, 1, 1);
                borderTexture.SetData(new[] { Color.White });

                _buttonBorder = new Border(1, borderTexture);
            }

            Point positionPixels = _baseButton.GetDrawPosition(callingPanel).ToPoint();
            Point sizePixels = ButtonSize.GetRawPixelValue(callingPanel.RawSize).ToPoint();

            _buttonBorder?.DrawBorder(batch, new RotatableRectangle(positionPixels, sizePixels, Rotation));
#endif
        }

        public object Clone() => new TextButton(ParentPanel, Position, Scale, ButtonSize, Rotation, Text, FontSize,
            _text.TextColor, FontSystem, Scripts);
    }
}