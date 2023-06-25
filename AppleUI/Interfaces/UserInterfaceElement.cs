using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI.Interfaces
{
    /// <summary>
    /// Represents a UI element. All UI elements should implement this abstract class.
    /// </summary>
    public abstract class UserInterfaceElement : ICloneable
    {
        private IElementContainer? _owner;

        /// <summary>
        /// The <see cref="IElementContainer"/> object that owns this element. If this property is null, then this
        /// element does not have an owner.
        /// </summary>
        // Set should be protected internal so that the owner can be set by the ElementContainer class.
        // If only C# had the friend keyword...
        public IElementContainer? Owner
        {
            get => _owner;
            set
            {
                if (_owner == value) return;
                
                _owner?.ElementContainer.Elements.Remove(this);
                
                if (value is null) return;
                
                value.ElementContainer.Elements.Add(this);
                _owner = value;
            }
        }

        internal void SetOwnerFieldInternal(IElementContainer? value) => _owner = value;
        
        public ElementTransform Transform { get; set; }
        
        public abstract Vector2 RawPosition { get; }
        public abstract Vector2 RawSize { get; }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        public abstract object Clone();
        
        public Panel? GetParentPanel()
        {
            if (Owner is Panel panel) return panel;

            return Owner is not UserInterfaceElement element ? null : element.GetParentPanel();
        }
    }
}