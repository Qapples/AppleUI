using System;
using System.Linq;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    public class TextureButton : UserInterfaceElement, IButtonElement, ITextureElement, IScriptableElement
    {
        public StaticTexture TextureObject { get; private set; }
        public BaseButton ButtonObject { get; private set; }
        
        public IElementBehaviorScript[] Scripts { get; private set; }
        private ElementScriptInfo[] _scriptInfos;
        
        public TextureButton(IElementContainer? owner, ElementTransform transform, StaticTexture textureObject,
            BaseButton buttonObject, IElementBehaviorScript[]? scripts = null)
        {
            TextureObject = textureObject;
            ButtonObject = buttonObject;

            TextureObject.Owner = null;
            ButtonObject.Parent = this;

            (Owner, Transform) = (owner, transform);

            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        public TextureButton(IElementContainer? owner, ElementTransform transform, Measurement buttonSize,
            Texture2D texture, IElementBehaviorScript[]? scripts = null) : this(owner, transform,
            new StaticTexture(null, transform, texture), new BaseButton(null!, buttonSize), scripts)
        {
        }

        [JsonConstructor]
        public TextureButton(Vector2 position, MeasurementType positionType, Vector2 scale, Vector2 buttonSize,
            MeasurementType sizeType, float rotation, Texture2D texture, object[]? scripts) : this(null,
            new ElementTransform(new Measurement(position, positionType), scale, rotation),
            new Measurement(buttonSize, sizeType), texture)
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
            TextureObject.Transform = Transform;
            TextureObject.Draw(gameTime, spriteBatch);
        }

        public override object Clone()
        {
            TextureButton clone = new(Owner, Transform, ButtonObject.Size, TextureObject.Texture)
            {
                _scriptInfos = _scriptInfos
            };

            UserInterfaceManager? buttonManager = GetParentPanel()?.Manager;
            if (buttonManager is not null) clone.LoadScripts(buttonManager);
            
            return clone;
        }
    }
}