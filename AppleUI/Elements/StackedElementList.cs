using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AppleUI.Elements
{
    public sealed class StackedElementList : UserInterfaceElement, IElementContainer, IScriptableElement, IDisposable
    {
        public override Vector2 RawPosition => Transform.GetDrawPosition(Owner);
        public override Vector2 RawSize => Size.GetRawPixelValue(Owner) * Transform.Scale;

        public ElementContainer ElementContainer { get; }

        public Measurement Size { get; private set; }

        
        private ScrollBar _scrollBar;
        
        public IElementBehaviorScript[] Scripts { get; private set; }

        private ElementScriptInfo[] _scriptInfos;

        public StackedElementList(IElementContainer? owner, ElementTransform transform, Measurement size,
            FontSystem scrollBarFont, IEnumerable<UserInterfaceElement> elements, IElementBehaviorScript[] scripts)
        {
            (Owner, Transform, Size) = (owner, transform, size);
            
            _scrollBar = new ScrollBar(this, Location.Right, Color.White, (0.05f, MeasurementType.Ratio),
                scrollBarFont);

            ElementContainer = new ElementContainer(this, elements);
            
            Scripts = scripts;
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        [JsonConstructor]
        public StackedElementList(Vector2 position, MeasurementType positionType, Vector2 scale, Vector2 size,
            MeasurementType sizeType, float rotation, FontSystem scrollBarFont, object[]? elements, object[]? scripts)
            : this(null,
                new ElementTransform(new Measurement(position, positionType), scale, rotation),
                new Measurement(size, sizeType), scrollBarFont,
                elements?.Cast<UserInterfaceElement>() ?? Array.Empty<UserInterfaceElement>(),
                Array.Empty<IElementBehaviorScript>())
        {
            _scriptInfos = scripts?.Cast<ElementScriptInfo>().ToArray() ?? _scriptInfos;
        }

        public void LoadScripts(UserInterfaceManager manager)
        {
            Scripts = manager.LoadElementBehaviorScripts(this, _scriptInfos);

            foreach (UserInterfaceElement element in ElementContainer)
            {
                if (element is IScriptableElement scriptableElement)
                {
                    scriptableElement.LoadScripts(manager);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (UserInterfaceElement element in ElementContainer)
            {
                element.Update(gameTime);
            }
            
            _scrollBar.Update(gameTime);
        }

        private float _scrollOffset;
        private int _previousScrollWheelValue;
        
        private const float ScrollSpeed = 2f;
        
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _scrollBar.Draw(gameTime, spriteBatch);
            
            int currentScrollWheelValue = Mouse.GetState().ScrollWheelValue;
            _scrollOffset += (currentScrollWheelValue - _previousScrollWheelValue) / 10f * ScrollSpeed;
            
            float totalHeight = ElementContainer.Sum(e => e.RawSize.Y);
            float maxScrollOffset = MathF.Max(totalHeight - RawSize.Y, 0f);

            _scrollOffset = MathHelper.Clamp(_scrollOffset, -maxScrollOffset, 0f);

            Vector2 elementPosition = new Vector2(0f, _scrollOffset);

            spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(RawPosition.ToPoint(), RawSize.ToPoint());
            foreach (UserInterfaceElement element in ElementContainer)
            {
                element.Transform = Transform with
                {
                    Position = new Measurement(elementPosition, MeasurementType.Pixel)
                };
                element.Draw(gameTime, spriteBatch);
                
                elementPosition += new Vector2(0f, element.RawSize.Y);
            }

            _previousScrollWheelValue = currentScrollWheelValue;
        }

        public override object Clone()
        {
            StackedElementList clone = new StackedElementList(Owner, Transform, Size, _scrollBar.ButtonFontSystem,
                ElementContainer.Elements, Array.Empty<IElementBehaviorScript>())
            {
                _scriptInfos = _scriptInfos
            };

            UserInterfaceManager? manager = GetParentPanel()?.Manager;
            if (manager is not null) clone.LoadScripts(manager);

            return clone;
        }
        
        public void Dispose()
        {
            ElementContainer.Dispose();
        }
    }
}