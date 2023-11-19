using System;
using System.Linq;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    public sealed class TextureButton : UserInterfaceElement, IButtonElement, ITextureElement, IScriptableElement
    {
        public override Vector2 RawSize => ButtonObject.Size.GetRawPixelValue(OwnerRawSize) * Transform.Scale;
        
        public StaticTexture TextureObject { get; private set; }
        public BaseButton ButtonObject { get; private set; }

        public IElementBehaviorScript[] Scripts { get; private set; }
        private ElementScriptInfo[] _scriptInfos;

        public TextureButton(string id, IElementContainer? owner, ElementTransform transform,
            StaticTexture textureObject, BaseButton buttonObject, Border? border,
            IElementBehaviorScript[]? scripts = null)
        {
            Id = new ElementId(id);
            TextureObject = textureObject;
            ButtonObject = buttonObject;

            TextureObject.Owner = null;
            ButtonObject.Parent = this;

            (Owner, Transform, Border) = (owner, transform, border);

            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        public TextureButton(string id, IElementContainer? owner, ElementTransform transform, Measurement buttonSize,
            Texture2D texture, Border? border, IElementBehaviorScript[]? scripts = null)
            : this(id, owner, transform, new StaticTexture($"{id}_texture", null, transform, texture, null),
                new BaseButton(null!, buttonSize), border, scripts)
        {
        }

        [JsonConstructor]
        public TextureButton(string id, Vector2 position, MeasurementType positionType, Vector2 scale,
            Vector2 buttonSize, MeasurementType sizeType, float rotation, Texture2D texture, Border? border,
            object[]? scripts)
            : this(id, null, new ElementTransform(new Measurement(position, positionType), scale, rotation),
                new Measurement(buttonSize, sizeType), texture, border)
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
            ButtonObject.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Border?.DrawBorder(spriteBatch, new RotatableRectangle(RawPosition, RawSize, Transform.Rotation));
            
            Vector2 scaleFactor = RawSize / new Vector2(TextureObject.Texture.Width, TextureObject.Texture.Height);
            Measurement texturePosition = new(RawPosition, MeasurementType.Pixel);
            TextureObject.Transform = new ElementTransform(texturePosition, scaleFactor, Transform.Rotation);

            TextureObject.Draw(gameTime, spriteBatch);
        }

        public override object Clone()
        {
            TextureButton clone = new(Id.Name, Owner, Transform, ButtonObject.Size, TextureObject.Texture, Border)
            {
                _scriptInfos = _scriptInfos
            };

            UserInterfaceManager? buttonManager = GetParentPanel()?.Manager;
            if (buttonManager is not null) clone.LoadScripts(buttonManager);
            
            return clone;
        }
    }
}