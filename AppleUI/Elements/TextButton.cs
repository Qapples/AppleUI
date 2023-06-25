using System;
using System.Linq;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaternion = System.Numerics.Quaternion;

namespace AppleUI.Elements
{
    public sealed class TextButton : UserInterfaceElement, IButtonElement, ITextElement, IScriptableElement
    {
        public override Vector2 RawPosition => Transform.GetDrawPosition(Owner);
        public override Vector2 RawSize => ButtonObject.Size.GetRawPixelValue(Owner) * Transform.Scale;
        
        public ImmutableText TextObject { get; private set; }
        
        private BaseButton _buttonObject;
        
        public BaseButton ButtonObject 
        {
            get
            {
                _buttonObject.Parent = this;
                return _buttonObject;
            }
            private init
            {
                _buttonObject = value;
                _buttonObject.Parent = this;
            }
        }
        
        public IElementBehaviorScript[] Scripts { get; private set; }

        private ElementScriptInfo[] _scriptInfos;

#if DEBUG
        private const bool DrawButtonBorder = false;
        private Border? _buttonBorder;
#endif

        public TextButton(IElementContainer? owner, ElementTransform transform, ImmutableText textObject,
            BaseButton buttonObject, IElementBehaviorScript[]? scripts = null)
        {
            TextObject = textObject;
            _buttonObject = buttonObject;

            TextObject.Owner = null;
            _buttonObject.Parent = this;
            
            (Owner, Transform) = (owner, transform);

            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        public TextButton(IElementContainer? owner, ElementTransform transform, Measurement buttonSize, string text,
            int fontSize, Color textColor, FontSystem fontSystem, IElementBehaviorScript[]? scripts = null) :
            this(owner, transform, new ImmutableText(null, transform, text, fontSize, textColor, fontSystem),
                new BaseButton(null!, buttonSize), scripts)
        {
        }

        [JsonConstructor]
        public TextButton(Vector2 position, MeasurementType positionType, Vector2 scale, Vector2 buttonSize,
            MeasurementType sizeType, float rotation, string text, int fontSize, Color textColor, FontSystem fontSystem,
            object[]? scripts) : this(null, new ElementTransform(new Measurement(position, positionType), scale, rotation),
            new Measurement(buttonSize, sizeType), text, fontSize, textColor, fontSystem)
        {
            _scriptInfos = scripts?.Cast<ElementScriptInfo>().ToArray() ?? _scriptInfos;
        }

        public void LoadScripts(UserInterfaceManager manager)
        {
            //ButtonEvents only loads scripts that implement IButtonBehavior.
            Scripts = manager.LoadElementBehaviorScripts(this, _scriptInfos);
            ButtonObject.ButtonEvents.AddEventsFromScripts(Scripts);
        }

        public override void Update(GameTime gameTime)
        {
            ButtonObject.Parent = this;
            ButtonObject.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch batch)
        {
            Vector2 ownerPosition = Owner?.RawPosition ?? Vector2.Zero;
            Vector2 ownerSize = Owner?.RawSize ?? Vector2.One;

            Vector2 boundsRotated = Vector2.Transform(TextObject.Bounds / 2f,
                Quaternion.CreateFromYawPitchRoll(0f, 0f, Transform.Rotation));
            Vector2 buttonCenter = ButtonObject.GetCenterPositionPixels(ownerSize).Value;

            TextObject.Transform = Transform with
            {
                Position = new Measurement(buttonCenter - boundsRotated, MeasurementType.Pixel)
            };

            TextObject.Draw(gameTime, batch);

            //draw the bounds of the button in debug mode.
#if DEBUG
            if (!DrawButtonBorder) return;

            if (_buttonBorder is null)
            {
                Texture2D borderTexture = new(batch.GraphicsDevice, 1, 1);
                borderTexture.SetData(new[] { Color.White });

                _buttonBorder = new Border(1, borderTexture);
            }

            Point positionPixels = (ownerPosition + Transform.Position.GetRawPixelValue(ownerSize)).ToPoint();
            Point sizePixels = ButtonObject.Size.GetRawPixelValue(ownerSize).ToPoint();

            _buttonBorder?.DrawBorder(batch, new RotatableRectangle(positionPixels, sizePixels, Transform.Rotation));
#endif
        }

        public override object Clone()
        {
            TextButton clone = new(Owner, Transform, ButtonObject.Size, TextObject.Text, TextObject.FontSize,
                TextObject.TextColor, TextObject.FontSystem)
            {
                _scriptInfos = _scriptInfos
            };

            UserInterfaceManager? manager = GetParentPanel()?.Manager;
            if (manager is not null) clone.LoadScripts(manager);
            
            return clone;
        }
    }
}