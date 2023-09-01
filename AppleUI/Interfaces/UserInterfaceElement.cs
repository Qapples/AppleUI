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
        private ElementId _id;

        /// <summary>
        /// Identifies this element in the <see cref="IElementContainer"/> that owns it.
        /// </summary>
        public ElementId Id
        {
            get => _id;
            set
            {
                if (_owner is null)
                {
                    _id = value;
                    return;
                }

                int uniqueId = _owner.ElementContainer.GetLowestAvailableUniqueId(value.Name);

                _id = new ElementId(value.Name, uniqueId);
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
                if (_owner == value || value is null) return;
                
                value.ElementContainer.Add(Id, this);
                _owner = value;
            }
        }

        internal void SetOwnerFieldInternal(IElementContainer? value) => _owner = value;
        
        public virtual ElementTransform Transform { get; set; }
        
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
        /// updated depending on the ElementContainer the element is apart of.
        /// </summary>
        public readonly int UniqueId;


        public ElementId(string name) => (Name, UniqueId) = (name, 0);
        internal ElementId(string name, int uniqueId) => (Name, UniqueId) = (name, uniqueId);
        
        public override int GetHashCode() => HashCode.Combine(Name, UniqueId);

        public static implicit operator ElementId((string Name, int UniqueId) value) => new(value.Name, value.UniqueId);
    }
}