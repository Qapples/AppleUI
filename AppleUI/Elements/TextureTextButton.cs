using System;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    public sealed class TextureTextButton : UserInterfaceElement, IButtonElement, ITextElement, ITextureElement,
        IScriptableElement
    {
        public override Vector2 RawPosition => Transform.GetDrawPosition(Owner);
        public override Vector2 RawSize => ButtonObject.Size.GetRawPixelValue(Owner) * Transform.Scale;

        public BaseButton ButtonObject { get; private set; }
        public StaticTexture TextureObject { get; private set; }

        public Label TextObject { get; private set; }

        public IElementBehaviorScript[] Scripts { get; private set; }
        private ElementScriptInfo[] _scriptInfos;

        public TextureTextButton(string id, IElementContainer? owner, ElementTransform transform,
            StaticTexture textureObject, BaseButton buttonObject, Label textObject, Border? border,
            IElementBehaviorScript[]? scripts = null)
        {
            Id = new ElementId(id);
            TextureObject = textureObject;
            ButtonObject = buttonObject;
            TextObject = textObject;

            TextureObject.Owner = null;
            TextObject.Owner = null;
            ButtonObject.Parent = this;

            (Owner, Transform, Border) = (owner, transform, border);

            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        public TextureTextButton(string id, IElementContainer? owner, ElementTransform transform,
            Measurement buttonSize, Texture2D texture, string text, int fontSize, Color textColor,
            FontSystem fontSystem, Border? border, IElementBehaviorScript[]? scripts = null)
            : this(id, owner, transform, new StaticTexture($"{id}_texture", null, transform, texture, null),
                new BaseButton(null!, buttonSize),
                new Label($"{id}_text", null, transform, text, fontSize, textColor, fontSystem, null), border, scripts)
        {
        }

        public void LoadScripts(UserInterfaceManager manager)
        {
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
            //There's some positioning code repeating here from TextureButton and TextButton. Can't immediately think
            //of a good way to reuse it without some significant refactoring, so it is how it is. I don't think this is
            //going to be much of a problem in the long-term anyway.
            
            Border?.DrawBorder(spriteBatch, new RotatableRectangle(RawPosition, RawSize, Transform.Rotation));
            
            Vector2 textureScaleFactor =
                RawSize / new Vector2(TextureObject.Texture.Width, TextureObject.Texture.Height);
            Measurement texturePosition = new(RawPosition, MeasurementType.Pixel);

            TextureObject.Transform = new ElementTransform(texturePosition, textureScaleFactor, Transform.Rotation);
            TextureObject.Draw(gameTime, spriteBatch);
            
            Vector2 ownerSize = Owner?.RawSize ?? Vector2.One;
            Vector2 textBoundsRotated = Vector2.Transform(TextObject.Bounds / 2f,
                Quaternion.CreateFromYawPitchRoll(0f, 0f, Transform.Rotation));
            Vector2 buttonCenter = ButtonObject.GetCenterPositionPixels(ownerSize).Value;

            TextObject.Transform = Transform with
            {
                Position = new Measurement(buttonCenter - textBoundsRotated, MeasurementType.Pixel)
            };
            
            TextObject.Draw(gameTime, spriteBatch);
        }

        public override object Clone()
        {
            TextureTextButton clone = new(Id.Name, Owner, Transform, ButtonObject.Size,
                TextureObject.Texture, TextObject.Text.ToString(), TextObject.FontSize, TextObject.TextColor,
                TextObject.FontSystem, Border)
            {
                _scriptInfos = _scriptInfos
            };

            UserInterfaceManager? buttonManager = GetParentPanel()?.Manager;
            if (buttonManager is not null) clone.LoadScripts(buttonManager);
            
            return clone;
        }
    }
}