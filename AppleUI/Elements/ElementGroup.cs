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
        public override string Id { get; set; }
        
        public override Vector2 RawPosition => Transform.GetDrawPosition(Owner);
        public override Vector2 RawSize => Size.GetRawPixelValue(Owner) * Transform.Scale;

        public ElementContainer ElementContainer { get; }

        public Measurement Size { get; private set; }
        
        public IElementBehaviorScript[] Scripts { get; private set; }

        private ElementScriptInfo[] _scriptInfos;

        public ElementGroup(string id, IElementContainer? owner, ElementTransform transform, Measurement size,
            IDictionary<string, UserInterfaceElement> elements, IElementBehaviorScript[] scripts)
        {
            (Id, Owner, Transform, Size) = (id, owner, transform, size);

            ElementContainer = new ElementContainer(this, elements);

            Scripts = scripts;
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        [JsonConstructor]
        public ElementGroup(string id, Vector2 position, MeasurementType positionType, Vector2 scale, Vector2 size,
            MeasurementType sizeType, float rotation, object[]? elements, object[]? scripts)
            : this(id, null, new ElementTransform(new Measurement(position, positionType), scale, rotation),
                new Measurement(size, sizeType),
                elements?.Cast<UserInterfaceElement>().ToDictionary(e => e.Id, e => e) ??
                new Dictionary<string, UserInterfaceElement>(),
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
        }
        
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(RawPosition.ToPoint(), RawSize.ToPoint());
            foreach (UserInterfaceElement element in ElementContainer.Values)
            {
                element.Draw(gameTime, spriteBatch);
            }
        }

        public override object Clone()
        {
            ElementGroup clone = new(GenerateCloneId(Id), Owner, Transform, Size,
                new Dictionary<string, UserInterfaceElement>(), Array.Empty<IElementBehaviorScript>())
            {
                _scriptInfos = _scriptInfos,
            };
            
            ElementContainer.CloneElementsTo(clone.ElementContainer);
            
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