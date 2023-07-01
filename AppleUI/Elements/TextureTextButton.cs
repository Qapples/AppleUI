using System;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    public sealed class TextureTextButton : UserInterfaceElement, IButtonElement, ITextElement, ITextureElement, IScriptableElement
    {
        public override Vector2 RawPosition => Transform.GetDrawPosition(Owner);
        public override Vector2 RawSize => ButtonObject.Size.GetRawPixelValue(Owner) * Transform.Scale;
        
        public BaseButton ButtonObject { get; private set; }
        public StaticTexture TextureObject { get; private set; }

        public ImmutableText TextObject { get; private set; }
        
        public IElementBehaviorScript[] Scripts { get; private set; }
        private ElementScriptInfo[] _scriptInfos;
        
        public TextureTextButton(IElementContainer? owner, ElementTransform transform, StaticTexture textureObject,
            BaseButton buttonObject, ImmutableText textObject, IElementBehaviorScript[]? scripts = null)
        {
            TextureObject = textureObject;
            ButtonObject = buttonObject;
            TextObject = textObject;
 
            TextureObject.Owner = null;
            TextObject.Owner = null;
            ButtonObject.Parent = this;

            (Owner, Transform) = (owner, transform);

            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        public TextureTextButton(IElementContainer? owner, ElementTransform transform, Measurement buttonSize,
            Texture2D texture, string text, int fontSize, Color textColor, FontSystem fontSystem,
            IElementBehaviorScript[]? scripts = null) : this(
            owner,
            transform,
            new StaticTexture(owner, transform, texture),
            new BaseButton(null!, buttonSize),
            new ImmutableText(owner, transform, text, fontSize, textColor, fontSystem), scripts)
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
            TextureObject.Transform = Transform;
            TextureObject.Draw(gameTime, spriteBatch);
            
            Vector2 ownerSize = Owner?.RawSize ?? Vector2.One;

            Vector2 boundsRotated = Vector2.Transform(TextObject.Bounds / 2f,
                Quaternion.CreateFromYawPitchRoll(0f, 0f, Transform.Rotation));
            Vector2 buttonCenter = ButtonObject.GetCenterPositionPixels(ownerSize).Value;

            TextObject.Transform = Transform with
            {
                Position = new Measurement(buttonCenter - boundsRotated, MeasurementType.Pixel)
            };
            
            TextObject.Draw(gameTime, spriteBatch);
        }

        public override object Clone()
        {
            TextureTextButton clone = new(Owner, Transform, ButtonObject.Size, TextureObject.Texture,
                TextObject.Text, TextObject.FontSize, TextObject.TextColor, TextObject.FontSystem)
            {
                _scriptInfos = _scriptInfos
            };

            UserInterfaceManager? buttonManager = GetParentPanel()?.Manager;
            if (buttonManager is not null) clone.LoadScripts(buttonManager);
            
            return clone;
        }
    }
}