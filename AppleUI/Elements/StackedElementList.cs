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
        public override Vector2 RawPosition => Transform.GetDrawPosition(Owner);
        public override Vector2 RawSize => Size.GetRawPixelValue(Owner) * Transform.Scale;

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
        public StackedElementList(string id, Vector2 position, MeasurementType positionType, Vector2 scale,
            Vector2 size, MeasurementType sizeType, float rotation, ScrollBar scrollBar, Border? border,
            object[]? elements, object[]? scripts)
            : this(id, null, new ElementTransform(new Measurement(position, positionType), scale, rotation),
                new Measurement(size, sizeType), scrollBar, border,
                elements?.Cast<UserInterfaceElement>().ToDictionary(e => e.Id, e => e) ??
                new Dictionary<ElementId, UserInterfaceElement>(),
                Array.Empty<IElementBehaviorScript>())
        {
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

            Vector2 elementPosition = new Vector2(0f, -scrollAmountPixels);

            spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(RawPosition.ToPoint(), RawSize.ToPoint());
            foreach (UserInterfaceElement element in ElementContainer.Values)
            {
                element.Transform = Transform with
                {
                    Position = new Measurement(elementPosition, MeasurementType.Pixel)
                };
                element.Draw(gameTime, spriteBatch);
                
                elementPosition += new Vector2(0f, element.RawSize.Y);
                
                spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(RawPosition.ToPoint(), RawSize.ToPoint());
            }
        }

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
                Scripts = manager.LoadElementBehaviorScripts(this, _scriptInfos);
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