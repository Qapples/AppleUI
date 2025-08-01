using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    public sealed class StackedElementList : UserInterfaceElement, IElementContainer, IScriptableElement,
        IScrollableElement, IDisposable
    {
        public override Vector2 RawSize => Size.GetRawPixelValue(OwnerRawSize) * Transform.Scale;

        public ElementContainer ElementContainer { get; }

        public Measurement Size { get; private set; }

        public ScrollBar ScrollBar { get; private set; }

        public IElementBehaviorScript[] Scripts { get; private set; }

        private ElementScriptInfo[] _scriptInfos;

        public StackedElementList(string id, IElementContainer? owner, ElementTransform transform, Measurement size,
            ScrollBar scrollBar, Border? border, IDictionary<ElementId, UserInterfaceElement> elements,
            IElementBehaviorScript[] scripts)
        {
            (Id, Owner, Transform, Size, ScrollBar, Border) =
                (new ElementId(id), owner, transform, size, scrollBar, border);
            ScrollBar.Owner = this;

            ElementContainer = new ElementContainer(this, elements);

            Scripts = scripts;
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        [JsonConstructor]
        public StackedElementList(string id, Vector2 position, MeasurementType positionType,
            PositionBasePoint positionBasePoint, Vector2 scale, Vector2 size, MeasurementType sizeType, float rotation,
            ScrollBar scrollBar, Border? border, object[]? elements, object[]? scripts)
            : this(id, null,
                new ElementTransform(new Measurement(position, positionType), positionBasePoint, scale, rotation),
                new Measurement(size, sizeType), scrollBar, border,
                new Dictionary<ElementId, UserInterfaceElement>(),
                Array.Empty<IElementBehaviorScript>())
        {
            if (elements is not null)
            {
                foreach (UserInterfaceElement element in elements.Cast<UserInterfaceElement>())
                {
                    element.Owner = this;
                }
            }
            
            _scriptInfos = scripts?.Cast<ElementScriptInfo>().ToArray() ?? _scriptInfos;
        }

        public void LoadScripts(UserInterfaceManager manager)
        {
            Scripts = manager.LoadElementBehaviorScripts(this, _scriptInfos);

            foreach (UserInterfaceElement element in ElementContainer.Values)
            {
                if (element is IScriptableElement scriptableElement)
                {
                    scriptableElement.LoadScripts(manager);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            ElementContainer.UpdateElements(gameTime);
            ScrollBar.Update(gameTime);
        }
        
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Border?.DrawBorder(spriteBatch, new RotatableRectangle(RawPosition, RawSize, Transform.Rotation));
            
            ScrollBar.UpdateMaxScrollAmount(ElementContainer.Values);
            ScrollBar.Draw(gameTime, spriteBatch);

            float thisElementSize = ScrollBar.Orientation == Orientation.Vertical ? RawSize.Y : RawSize.X;
            float scrollAmountPixels =
                ScrollBar.ScrollAmountPercent * (ScrollBar.MaxScrollAmountPixels - thisElementSize);

            if (scrollAmountPixels < 0f) scrollAmountPixels = 0f;

            float elementPosY = -scrollAmountPixels;
            
            foreach (UserInterfaceElement element in ElementContainer.Values)
            {
                Rectangle scissorRect = new(RawPosition.ToPoint(), RawSize.ToPoint());
                int xPos = (int) element.Transform.Position.GetRawPixelValue(scissorRect).X;
                
                element.Transform = Transform with
                {
                    Position = new Measurement(new Vector2(xPos, elementPosY), MeasurementType.Pixel)
                };

                spriteBatch.GraphicsDevice.ScissorRectangle = scissorRect;
                element.Draw(gameTime, spriteBatch);

                elementPosY += element.RawSize.Y;
                
                spriteBatch.End();
                spriteBatch.Begin(rasterizerState: ScissorTestEnabled);
            }
        }
        
        private static readonly RasterizerState ScissorTestEnabled = new() { ScissorTestEnable = true };

        public override object Clone()
        {
            StackedElementList clone = new(Id.Name, Owner, Transform, Size,
                (ScrollBar) ScrollBar.Clone(), Border, new Dictionary<ElementId, UserInterfaceElement>(),
                Array.Empty<IElementBehaviorScript>())
            {
                _scriptInfos = _scriptInfos,
            };

            clone.ScrollBar.Owner = clone;

            //Elements in the container load their scripts when they are cloned.
            ElementContainer.CloneElementsTo(clone.ElementContainer);

            UserInterfaceManager? manager = GetParentPanel()?.Manager;
            if (manager is not null)
            {
                //Only load the scripts of the ElementGroup itself and not the elements it contains.
                //This is because the contained elements already loaded their scripts when they were cloned.
                clone.Scripts = manager.LoadElementBehaviorScripts(this, _scriptInfos);
            }

            return clone;
        }

        public void Dispose()
        {
            ElementContainer.Dispose();
            ScrollBar.Dispose();
        }
    }
}