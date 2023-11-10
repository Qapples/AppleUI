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
    public sealed class InputTextBox : UserInterfaceElement, IButtonElement, ITextElement, IScriptableElement, 
        IDisposable
    {
        public static readonly TimeSpan KeyHeldDelay = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan KeyRepeatOnHeldInterval = TimeSpan.FromSeconds(0.05);
        
        public override Vector2 RawPosition => Transform.GetDrawPosition(Owner);
        public override Vector2 RawSize => ButtonObject.Size.GetRawPixelValue(Owner) * Transform.Scale;
        
        public TextButton TextButton { get; set; }

        public BaseButton ButtonObject => TextButton.ButtonObject;
        public Label TextObject => TextButton.TextObject;

        public IElementBehaviorScript[] Scripts { get; private set; }
        private ElementScriptInfo[] _scriptInfos;
        
        public string DefaultText { get; private set; }

        private Cursor? _textInputCursor;
        private TextInput? _textInput;

        public InputTextBox(string id, IElementContainer? owner, ElementTransform transform,
            TextButton textButton, Border? border, IElementBehaviorScript[]? scripts)
        {
            Id = new ElementId(id);

            TextButton = textButton;
            textButton.Owner = null;
            
            (Owner, Transform, Border) = (owner, transform, border);

            Scripts = scripts ?? Array.Empty<IElementBehaviorScript>();
            _scriptInfos = Array.Empty<ElementScriptInfo>();

            DefaultText = textButton.TextObject.Text.ToString();
            
            ButtonObject.ButtonEvents.OnPress += (_, _) =>
            {
                UserInterfaceManager? manager = GetParentPanel()?.Manager;
                if (manager is null) return;
                
                manager.FocusedElement = this;
            };
        }

        public InputTextBox(string id, IElementContainer? owner, ElementTransform transform, Measurement boxSize,
            string defaultText, int fontSize, Color textColor, FontSystem fontSystem, Border? border,
            IElementBehaviorScript[]? scripts = null) :
            this(id, owner, transform, new TextButton($"{id}_text_button", null, transform, boxSize, defaultText,
                fontSize, textColor, fontSystem, null, null), border, scripts)
        {
        }

        [JsonConstructor]
        public InputTextBox(string id, Vector2 position, MeasurementType positionType, Vector2 scale, Vector2 boxSize,
            MeasurementType sizeType, float rotation, string defaultText, int fontSize, Color textColor,
            FontSystem fontSystem, Border? border, object[]? scripts)
            : this(id, null, new ElementTransform(new Measurement(position, positionType), scale, rotation),
                new Measurement(boxSize, sizeType), defaultText, fontSize, textColor, fontSystem, border)
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

            _textInput?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _textInputCursor ??= new Cursor(spriteBatch.GraphicsDevice, TextObject.TextColor, 3);
            _textInputCursor.Color = TextObject.TextColor;

            if (_textInput is null)
            {
                GameWindow? window = GetParentPanel()?.Manager?.Window;

                if (window is not null)
                {
                    _textInput = new TextInput(window);
                }
            }
            else
            {
                _textInput.AcceptingInput = GetParentPanel()?.Manager?.FocusedElement == this;
                TextObject.Text = _textInput.Text;
            }
            
            Border?.DrawBorder(spriteBatch, new Rectangle(RawPosition.ToPoint(), RawSize.ToPoint()));

            TextButton.Transform = Transform;
            TextButton.Draw(gameTime, spriteBatch);

            Vector2 cursorPosition = Transform.GetDrawPosition(Owner) + TextObject.RawSize;
            _textInputCursor.Draw(spriteBatch, cursorPosition, (int)TextObject.RawSize.Y, Transform.Rotation);
        }

        public override object Clone()
        {
            InputTextBox clone = new(Id.Name, Owner, Transform, ButtonObject.Size, DefaultText, TextObject.FontSize,
                TextObject.TextColor, TextObject.FontSystem, Border)
            {
                _scriptInfos = _scriptInfos
            };
            
            UserInterfaceManager? manager = GetParentPanel()?.Manager;
            if (manager is not null) clone.LoadScripts(manager);
            
            return clone;
        }

        public void Dispose()
        {
            _textInputCursor?.Dispose();
        }

        private class Cursor : IDisposable
        {
            public int Width { get; set; }

            private Color _color;

            public Color Color
            {
                get => _color;
                set
                {
                    if (_color == value) return;
                    
                    _cursorTextureColor[0] = value;
                    _texture.SetData(_cursorTextureColor);
                    
                    _color = value;
                }
            }
            
            private Color[] _cursorTextureColor;
            private Texture2D _texture;

            public Cursor(GraphicsDevice graphicsDevice, Color color, int width)
            {
                _cursorTextureColor = new Color[1];
                _texture = new Texture2D(graphicsDevice, 1, 1);

                Width = width;
                Color = color;
            }

            public void Draw(SpriteBatch spriteBatch, Vector2 position, int height, float rotation)
            {
                Rectangle destRect = new((int)position.X, (int)position.Y, Width, height);
                spriteBatch.Draw(_texture, destRect, null, Color, rotation, Vector2.Zero, SpriteEffects.None, 0);
            }

            public void Dispose()
            {
                _texture.Dispose();
            }
        }
    }
}