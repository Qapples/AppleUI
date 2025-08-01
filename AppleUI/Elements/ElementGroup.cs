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
    public sealed class ElementGroup : UserInterfaceElement, IElementContainer, IScriptableElement
    {
        public override Vector2 RawSize => Size.GetRawPixelValue(OwnerRawSize) * Transform.Scale;

        public ElementContainer ElementContainer { get; }

        public Measurement Size { get; private set; }
        
        public IElementBehaviorScript[] Scripts { get; private set; }

        private ElementScriptInfo[] _scriptInfos;

        public ElementGroup(string id, IElementContainer? owner, ElementTransform transform, Measurement size,
            Border? border, IDictionary<ElementId, UserInterfaceElement> elements, IElementBehaviorScript[] scripts)
        {
            (Id, Owner, Transform, Size, Border) = (new ElementId(id), owner, transform, size, border);

            ElementContainer = new ElementContainer(this, elements);

            Scripts = scripts;
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        [JsonConstructor]
        public ElementGroup(string id, Vector2 position, MeasurementType positionType,
            PositionBasePoint positionBasePoint, Vector2 scale, Vector2 size, MeasurementType sizeType, float rotation,
            Border? border, object[]? elements, object[]? scripts)
            : this(id, null,
                new ElementTransform(new Measurement(position, positionType), positionBasePoint, scale, rotation),
                new Measurement(size, sizeType),
                border,
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
            ElementContainer.LoadAllElementScripts(manager);
        }

        public override void Update(GameTime gameTime)
        {
            ElementContainer.UpdateElements(gameTime);
        }
        
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Border?.DrawBorder(spriteBatch, new RotatableRectangle(RawPosition, RawSize, Transform.Rotation));
            
            foreach (UserInterfaceElement element in ElementContainer.Values)
            {
                spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(RawPosition.ToPoint(), RawSize.ToPoint());
                element.Draw(gameTime, spriteBatch);
                
                spriteBatch.End();
                spriteBatch.Begin(rasterizerState: ScissorTestEnabled);
            }
        }
        
        private static readonly RasterizerState ScissorTestEnabled = new() { ScissorTestEnable = true };

        public override object Clone()
        {
            ElementGroup clone = new(Id.Name, Owner, Transform, Size, Border,
                new Dictionary<ElementId, UserInterfaceElement>(), Array.Empty<IElementBehaviorScript>())
            {
                _scriptInfos = _scriptInfos,
            };
            
            //Elements in the container load their scripts when they are cloned.
            ElementContainer.CloneElementsTo(clone.ElementContainer);
            
            UserInterfaceManager? manager = GetParentPanel()?.Manager;
            if (manager is not null)
            {
                //Only load the scripts of the ElementGroup itself and not the elements it contains.
                //This is because the contained elements already loaded their scripts when they were cloned.
                clone.Scripts = manager.LoadElementBehaviorScripts(clone, _scriptInfos);
            }

            return clone;
        }
        
        public void Dispose()
        {
            ElementContainer.Dispose();
        }
    }
}