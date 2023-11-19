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

        /// <summary>
        /// The raw position of the owner of this element in pixels. Can be overrided via the
        /// internal <see cref="OwnerRawPositionOverride"/> field. This value is <see cref="Vector2.Zero"/> if both
        /// <see cref="OwnerRawPositionOverride"/> and <see cref="Owner"/> are null.
        /// </summary>
        public Vector2 OwnerRawPosition => OwnerRawPositionOverride ?? _owner?.RawPosition ?? Vector2.Zero;

        /// <summary>
        /// The raw size of the owner of this element in pixels. Can be overrided via the
        /// internal <see cref="OwnerRawSizeOverride"/> field. This value is <see cref="Vector2.One"/> if both
        /// <see cref="OwnerRawSizeOverride"/> and <see cref="Owner"/> are null.
        /// </summary>
        public Vector2 OwnerRawSize => OwnerRawSizeOverride ?? _owner?.RawSize ?? Vector2.One;

        protected internal Vector2? OwnerRawPositionOverride;
        protected internal Vector2? OwnerRawSizeOverride;

        internal void SetOwnerFieldInternal(IElementContainer? value) => _owner = value;
        
        /// <summary>
        /// Represents the transform of this element.
        /// </summary>
        public virtual ElementTransform Transform { get; set; }

        /// <summary>
        /// The raw position of this element in pixels. This value is directly used to draw the element to the screen.
        /// </summary>
        public virtual Vector2 RawPosition => Transform.GetDrawPosition(OwnerRawPosition, OwnerRawSize);
        
        /// <summary>
        /// The raw size of this element in pixels. This value is directly used to draw the element to the screen.
        /// </summary>
        public abstract Vector2 RawSize { get; }
        
        /// <summary>
        /// The border that is drawn around this element. If this value is null, then no border is drawn.
        /// </summary>
        public Border? Border { get; protected set; }

        /// <summary>
        /// Determines whether or not this element is the focused element for the <see cref="UserInterfaceManager"/>
        /// that owns it. In other words, checks if the manager's (that contains this element)
        /// <see cref="UserInterfaceManager.FocusedElement"/> is equal to this element.
        /// </summary>
        public bool IsFocusedElement => GetParentPanel()?.Manager?.FocusedElement == this;

        /// <summary>
        /// Update this element along with any scripts associated with it.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> of the <see cref="Game"/> this element is a part of.</param>
        public abstract void Update(GameTime gameTime);
        
        /// <summary>
        /// Draws this element.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> of the <see cref="Game"/> this element is a part of.</param>
        /// <param name="spriteBatch"><see cref="SpriteBatch"/> object used to the draw the element.</param>
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
        
        /// <summary>
        /// Creates a recursive deep clone of this object and places it under the same parent.
        /// </summary>
        /// <returns>A deep clone of this element.</returns>
        public abstract object Clone();
        
        /// <summary>
        /// Returns the panel that owns this element and the container(s) it falls under.
        /// </summary>
        /// <returns>The panel that owns this element and the container(s) it falls under, null if the element and its
        /// container(s) are not part of a panel.</returns>
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