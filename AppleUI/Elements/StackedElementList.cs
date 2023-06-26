using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text.Json.Serialization;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Elements
{
    public sealed class StackedElementList : UserInterfaceElement, IElementContainer, IScriptableElement, IDisposable
    {
        public override Vector2 RawPosition => Transform.GetDrawPosition(Owner);
        public override Vector2 RawSize => Size.GetRawPixelValue(Owner) * Transform.Scale;

        public ElementContainer ElementContainer { get; }

        public Measurement Size { get; private set; }

        public IElementBehaviorScript[] Scripts { get; private set; }

        private ElementScriptInfo[] _scriptInfos;

        public StackedElementList(IElementContainer? owner, ElementTransform transform, Measurement size,
            IEnumerable<UserInterfaceElement> elements, IElementBehaviorScript[] scripts)
        {
            (Owner, Transform, Size) = (owner, transform, size);

            ElementContainer = new ElementContainer(this, elements);

            Scripts = scripts;
            _scriptInfos = Array.Empty<ElementScriptInfo>();
        }

        [JsonConstructor]
        public StackedElementList(Vector2 position, MeasurementType positionType, Vector2 scale, Vector2 size,
            MeasurementType sizeType, float rotation, object[]? elements, object[]? scripts)
            : this(null,
                new ElementTransform(new Measurement(position, positionType), scale, rotation),
                new Measurement(size, sizeType),
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
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;
            Rectangle oldScissorRectangle = graphicsDevice.ScissorRectangle;
            Vector2 elementPosition = Vector2.Zero;
            
            // spriteBatch.End();
            // spriteBatch.Begin(rasterizerState: new RasterizerState() {ScissorTestEnable = true});
            
            spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle((int) RawPosition.X, (int) RawPosition.Y,
                100, 100);
            

            foreach (var element in ElementContainer)
            {
                element.Transform = Transform with
                {
                    Position = new Measurement(elementPosition, MeasurementType.Pixel)
                };

                element.Draw(gameTime, spriteBatch);

                elementPosition += new Vector2(0f, element.RawSize.Y);
            }

            // spriteBatch.End();
            //
            //
            // spriteBatch.Begin(rasterizerState: new RasterizerState() {ScissorTestEnable = true});
            
        }

        public override object Clone()
        {
            StackedElementList clone = new StackedElementList(Owner, Transform, Size, ElementContainer.Elements,
                Array.Empty<IElementBehaviorScript>())
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