using System;
using AppleUI.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI
{
    /// <summary>
    /// Represents a UI element. All UI elements should implement this abstract class.
    /// </summary>
    public abstract class UserInterfaceElement : ICloneable
    {
        private ElementId _id;

        /// <summary>
        /// Identifies this element in the <see cref="IElementContainer"/> that owns it.
        /// </summary>
        public ElementId Id
        {
            get => _owner is null ? _id = new ElementId(_id.Name, 0) : _id;
            set
            {
                //The user should have no control over UniqueId, and it should be determined by whats available in
                //the container.
                
                if (_owner is null)
                {
                    _id = value;
                    return;
                }

                _owner.ElementContainer.Elements.Remove(_id);

                int uniqueId = _owner.ElementContainer.GetLowestAvailableUniqueId(value.Name);
                _id = new ElementId(value.Name, uniqueId);
                
                _owner.ElementContainer.Elements.Add(_id, this);
            }
        }
        
        private IElementContainer? _owner;

        /// <summary>
        /// The <see cref="IElementContainer"/> object that owns this element. If this property is null, then this
        /// element does not have an owner.
        /// </summary>
        public IElementContainer? Owner
        {
            get => _owner;
            set
            {
                if (_owner == value) return;

                _owner?.ElementContainer.Elements.Remove(_id);

                if (value is null)
                {
                    _id = new ElementId(_id.Name, 0);
                    return;
                }

                int uniqueId = value.ElementContainer.GetLowestAvailableUniqueId(_id.Name);
                _id = new ElementId(_id.Name, uniqueId);

                value.ElementContainer.Elements.Add(_id, this);
                _owner = value;
            }
        }

        internal void SetOwnerFieldInternal(IElementContainer? value) => _owner = value;
        
        public virtual ElementTransform Transform { get; set; }
        
        public abstract Vector2 RawPosition { get; }
        public abstract Vector2 RawSize { get; }
        
        public Border? Border { get; protected set; }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
        
        public abstract object Clone();
        
        public Panel? GetParentPanel()
        {
            if (Owner is Panel panel) return panel;

            return Owner is not UserInterfaceElement element ? null : element.GetParentPanel();
        }
    }

    /// <summary>
    /// Identifies elements in a <see cref="ElementContainer"/>.
    /// </summary>
    public readonly struct ElementId
    {
        /// <summary>
        /// The name of the element.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// An id uniquely identifying elements that share the same name. If an element's name is
        /// unique in its container, then its UniqueId will be 0. If two elements share the same name in a container,
        /// then one element will have a UniqueId of 0 while the other will have a UniqueId of 1. This value will be
        /// updated depending on the ElementContainer the element is apart of. If the element has no container/owner,
        /// then this value will be zero.
        /// </summary>
        public readonly int UniqueId;
        
        public ElementId(string name) => (Name, UniqueId) = (name, 0);
        internal ElementId(string name, int uniqueId) => (Name, UniqueId) = (name, uniqueId);
        
        public override int GetHashCode() => HashCode.Combine(Name, UniqueId);

        public static implicit operator ElementId((string Name, int UniqueId) value) => new(value.Name, value.UniqueId);
    }
}