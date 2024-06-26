using System;
using System.Linq;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    public sealed class TextButton : UserInterfaceElement, IButtonElement, ITextElement, IScriptableElement
    {
        public override Vector2 RawSize => ButtonObject.Size.GetRawPixelValue(OwnerRawSize) * Transform.Scale;
        
        public Label TextObject { get; private set; }
        public BaseButton ButtonObject { get; set; }
        
        public TextAlignment TextAlignment { get; set; }

        public IElementBehaviorScript[] Scripts { get; private set; }

        private ElementScriptInfo[] _scriptInfos;

#if DEBUG
        private const bool DrawButtonBorder = false;
        private Border? _buttonBorder;
#endif

        public TextButton(string id, IElementContainer? owner, ElementTransform transform, Label textObject,
            TextAlignment textAlignment, BaseButton buttonObject, Border? border,
            IElementBehaviorScript[]? scripts = null)
        {
            Id = new ElementId(id);
            TextObject = textObject;
            ButtonObject = buttonObject;

            TextObject.Owner = null;
            ButtonObject.Parent = this;

            (Owner, Transform, TextAlignment, Border) = (owner, transform, textAlignment, border);

            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        public TextButton(string id, IElementContainer? owner, ElementTransform transform, Measurement buttonSize,
            string text, TextAlignment textAlignment, int fontSize, Color textColor, FontSystem fontSystem, 
            Border? border, IElementBehaviorScript[]? scripts = null)
            : this(id, owner, transform,
                new Label($"{id}_text", null, transform, text, fontSize, textColor, fontSystem, null),
                textAlignment, new BaseButton(null!, buttonSize), border, scripts)
        {
        }

        [JsonConstructor]
        public TextButton(string id, Vector2 position, MeasurementType positionType,
            PositionBasePoint positionBasePoint, Vector2 scale, Vector2 buttonSize,
            MeasurementType sizeType, float rotation, string text, TextAlignment textAlignment, int fontSize,
            Color textColor, FontSystem fontSystem, Border? border, object[]? scripts)
            : this(id, null,
                new ElementTransform(new Measurement(position, positionType), positionBasePoint, scale, rotation),
                new Measurement(buttonSize, sizeType), text, textAlignment, fontSize, textColor, fontSystem, border)
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

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Border?.DrawBorder(spriteBatch, new RotatableRectangle(RawPosition, RawSize, Transform.Rotation));

            Vector2 textBoundsRotated = Vector2.Transform(TextObject.Bounds / 2f,
                Quaternion.CreateFromYawPitchRoll(0f, 0f, Transform.Rotation));
            Vector2 textCenterPosition = TextAlignment switch
            {
                TextAlignment.Left => RawPosition,
                TextAlignment.Center => ButtonObject.GetCenterPositionPixels(OwnerRawSize).Value - textBoundsRotated,
                _ => Vector2.Zero
            };

            TextObject.Transform = Transform with
            {
                Position = new Measurement(textCenterPosition, MeasurementType.Pixel)
            };

            TextObject.Draw(gameTime, spriteBatch);

            //draw the bounds of the button in debug mode.
#if DEBUG
            if (!DrawButtonBorder) return;

            if (_buttonBorder is null)
            {
                Texture2D borderTexture = new(spriteBatch.GraphicsDevice, 1, 1);
                borderTexture.SetData(new[] { Color.Red });

                _buttonBorder = new Border(1, borderTexture);
            }

            Point positionPixels = (OwnerRawPosition + Transform.Position.GetRawPixelValue(OwnerRawSize)).ToPoint();
            Point sizePixels = ButtonObject.Size.GetRawPixelValue(OwnerRawSize).ToPoint();

            _buttonBorder?.DrawBorder(spriteBatch, new RotatableRectangle(positionPixels, sizePixels, Transform.Rotation));
#endif
        }

        public override object Clone()
        {
            TextButton clone = new(Id.Name, Owner, Transform, ButtonObject.Size, TextObject.Text.ToString(),
                TextAlignment, TextObject.FontSize, TextObject.TextColor, TextObject.FontSystem, Border)
            {
                _scriptInfos = _scriptInfos
            };

            UserInterfaceManager? manager = GetParentPanel()?.Manager;
            if (manager is not null) clone.LoadScripts(manager);
            
            return clone;
        }
    }
}