using System;
using System.Linq;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IDrawable = AppleUI.Interfaces.IDrawable;
using IUpdateable = AppleUI.Interfaces.IUpdateable;

namespace AppleUI.Elements
{
    public class TextureButton : IButton, ITransform, IUpdateable, IDrawable, IScriptableElement
    {
        private Panel? _parentPanel;

        public Panel? ParentPanel
        {
            get => _parentPanel;
            set
            {
                _texture.ParentPanel = value;
                _baseButton.ParentPanel = value;
                _parentPanel = value;
            }
        }

        public IElementBehaviorScript[] Scripts { get; set; }

        public Measurement Position { get; set; }
        public Vector2 Scale { get; set; }
        public Measurement ButtonSize { get; set; }
        public float Rotation { get; set; }
        
        public ButtonEvents ButtonEvents => _baseButton.ButtonEvents;
        
        private StaticTexture _texture;
        private BaseButton _baseButton;

        private ElementScriptInfo[] _scriptInfos;

        public TextureButton(Panel? parentPanel, Measurement position, Vector2 scale,
            Measurement buttonSize, float rotation, Texture2D texture, IElementBehaviorScript[]? scripts = null)
        {
            _baseButton = new BaseButton(null, position, buttonSize, rotation);
            _texture = new StaticTexture(null, position, scale, rotation, texture);

            (ParentPanel, Position, Scale, ButtonSize, Rotation) =
                (parentPanel, position, scale, buttonSize, rotation);
            
            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        [JsonConstructor]
        public TextureButton(Vector2 position, MeasurementType positionType, Vector2 scale, Vector2 buttonSize,
            MeasurementType sizeType, float rotation, Texture2D texture, object[]? scripts) : this(null,
            new Measurement(position, positionType), scale, new Measurement(buttonSize, sizeType), rotation, texture)
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
            _baseButton.Update(callingPanel, gameTime);
        }

        public void Draw(Panel callingPanel, GameTime gameTime, SpriteBatch spriteBatch)
        {
            this.CopyTransformTo(_texture);

            _texture.Draw(callingPanel, gameTime, spriteBatch);
        }

        public object Clone() =>
            new TextureButton(ParentPanel, Position, Scale, ButtonSize, Rotation, _texture.Texture, Scripts);
    }
}