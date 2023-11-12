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

        private Vector2 _cursorPosition;

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _textInputCursor ??= new Cursor(spriteBatch.GraphicsDevice, TextObject.TextColor, 5);
            _textInputCursor.Color = TextObject.TextColor;

            if (_textInput is null)
            {
                GameWindow? window = GetParentPanel()?.Manager?.Window;

                if (window is not null)
                {
                    _textInput = new TextInput(window);
                    _textInput.OnTextChanged += (_, _) =>  TextObject.UpdateBounds();
                    _textInput.OnCursorPositionChanged += (_, _) => _cursorPosition = GetCursorDrawPosition();
                }
            }
            else
            {
                _textInput.AcceptingInput = GetParentPanel()?.Manager?.FocusedElement == this;
                TextObject.Text = _textInput.Text;
            }
            
            Border?.DrawBorder(spriteBatch, new Rectangle(RawPosition.ToPoint(), RawSize.ToPoint()));

            TextButton.Transform = Transform with
            {
                Position = new Measurement(Transform.GetDrawPosition(Owner) + TextObject.Bounds / 2,
                    MeasurementType.Pixel)
            };
            TextButton.Draw(gameTime, spriteBatch);
            
            _textInputCursor.Draw(spriteBatch, _cursorPosition, (int) TextObject.Bounds.Y, Transform.Rotation);
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

        private const int CharsAfterCursorCacheSize = 2048;
        
        private char[]? _charsAfterCursorArrCache;
        private int _arrCacheSize = CharsAfterCursorCacheSize;
        
        private Vector2 GetCursorDrawPosition()
        {
            if (_textInput is null) return Vector2.Zero;
            
            // We need to measure a section of the Text in order to get the position of the cursor. Instead of creating
            // new StringBuilder or string objects that represent a section of the text, which results in constant heap
            // allocations everytime the cursor position changes, we remove the characters beyond the cursor, store
            // them temporarily, measure the string, and then append the characters back to the text. 

            int numCharsAfterCursor = _textInput.Text.Length - _textInput.CursorPosition;

            // If the number of characters after the cursor is less than or equal to the const cache size, then
            // store the chars after the cursor in a stackalloc as to prevent unnecessary heap allocations. Otherwise,
            // use an array whose capacity expands twice everytime the length is exceeded to store the characters.
            Span<char> charsAfterCursorCache = numCharsAfterCursor <= CharsAfterCursorCacheSize
                ? stackalloc char[CharsAfterCursorCacheSize]
                : numCharsAfterCursor > _arrCacheSize
                    ? _charsAfterCursorArrCache = new char[_arrCacheSize *= 2]
                    : _charsAfterCursorArrCache;
            
            int charsAfterIndex = 0;
                        
            for (int i = _textInput.CursorPosition; i < _textInput.Text.Length; i++)
            {
                charsAfterCursorCache[charsAfterIndex++] = _textInput.Text[i];
            }
            
            _textInput.Text.Remove(_textInput.CursorPosition, numCharsAfterCursor);

            Vector2 cursorDrawPosition =
                Transform.GetDrawPosition(Owner) + TextObject.SpriteFontBase.MeasureString(_textInput.Text);
            
            for (int i = 0; i < charsAfterIndex; i++)
            {
                _textInput.Text.Append(charsAfterCursorCache[i]);
            }

            return cursorDrawPosition;
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