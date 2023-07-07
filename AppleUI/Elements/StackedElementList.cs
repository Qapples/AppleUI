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
    public sealed class StackedElementList : UserInterfaceElement, IElementContainer, IScriptableElement,
        IScrollableElement, IDisposable
    {
        public override Vector2 RawPosition => Transform.GetDrawPosition(Owner);
        public override Vector2 RawSize => Size.GetRawPixelValue(Owner) * Transform.Scale;

        public ElementContainer ElementContainer { get; }

        public Measurement Size { get; private set; }

        public ScrollBar ScrollBar { get; private set; }

        public IElementBehaviorScript[] Scripts { get; private set; }

        private ElementScriptInfo[] _scriptInfos;

        public StackedElementList(IElementContainer? owner, ElementTransform transform, Measurement size,
            ScrollBar scrollBar, IEnumerable<UserInterfaceElement> elements, IElementBehaviorScript[] scripts)
        {
            (Owner, Transform, Size, ScrollBar) = (owner, transform, size, scrollBar);
            ScrollBar.Owner = this;
            
            ElementContainer = new ElementContainer(this, elements);
            
            Scripts = scripts;
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        [JsonConstructor]
        public StackedElementList(Vector2 position, MeasurementType positionType, Vector2 scale, Vector2 size,
            MeasurementType sizeType, float rotation, ScrollBar scrollBar, object[]? elements, object[]? scripts)
            : this(null,
                new ElementTransform(new Measurement(position, positionType), scale, rotation),
                new Measurement(size, sizeType), scrollBar,
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
            
            ScrollBar.Update(gameTime);
        }
        
        
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            ScrollBar.UpdateMaxScrollAmount(ElementContainer);
            ScrollBar.Draw(gameTime, spriteBatch);

            float thisElementSize = ScrollBar.Orientation == Orientation.Vertical ? RawSize.Y : RawSize.X;
            float scrollAmountPixels =
                ScrollBar.ScrollAmountPercent * (ScrollBar.MaxScrollAmountPixels - thisElementSize);

            Vector2 elementPosition = new Vector2(0f, -scrollAmountPixels);

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
        }

        public override object Clone()
        {
            StackedElementList clone = new StackedElementList(Owner, Transform, Size, (ScrollBar) ScrollBar.Clone(),
                ElementContainer.Elements, Array.Empty<IElementBehaviorScript>())
            {
                _scriptInfos = _scriptInfos,
            };
            
            clone.ScrollBar.Owner = clone;

            UserInterfaceManager? manager = GetParentPanel()?.Manager;
            if (manager is not null) clone.LoadScripts(manager);

            return clone;
        }
        
        public void Dispose()
        {
            ElementContainer.Dispose();
            ScrollBar.Dispose();
        }
    }
}