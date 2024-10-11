using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI
{
    public sealed class ElementContainer : IDictionary<ElementId, UserInterfaceElement>, IDisposable
    {
        //For consistency purposes, elements will be added to the container via setting its owner property which
        //adds/removes elements to the containers depending on what its set to. 
        
        public IElementContainer Owner { get; }
        
        internal Dictionary<ElementId, UserInterfaceElement> Elements { get; set; }
        
        public UserInterfaceElement this[ElementId key]
        {
            get => Elements[key];
            set => value.Owner = Owner;
        }
        
        public ICollection<ElementId> Keys => Elements.Keys;
        public ICollection<UserInterfaceElement> Values => Elements.Values;

        public int Count => Elements.Count;
        public bool IsReadOnly => false;
        
        public ElementContainer(IElementContainer owner)
        {
            Owner = owner;
            Elements = new Dictionary<ElementId, UserInterfaceElement>();
        }
        
        public ElementContainer(IElementContainer owner, IDictionary<ElementId, UserInterfaceElement> elements) :
            this(owner)
        {
            foreach (UserInterfaceElement element in elements.Values.ToList())
            {
                //Modifying an Element's Owner could result in a NullReferenceException if the owner is being assigned
                //at the same time it is being instantiated ( i.e. elements = new ElementContainer(this, elements) )
                //We must manually add/remove elements.

                element.Owner?.ElementContainer.Elements.Remove(element.Id);
                
                int uniqueId = GetLowestAvailableUniqueId(element.Id.Name);
                element.Id = new ElementId(element.Id.Name, uniqueId);
                
                Elements.Add(element.Id, element);

                element.SetOwnerFieldInternal(owner);
            }
        }

        public void LoadAllElementScripts(UserInterfaceManager manager)
        {
            foreach (UserInterfaceElement element in Elements.Values)
            {
                if (element is not IScriptableElement scriptableElement) continue;

                scriptableElement.LoadScripts(manager);
            }
        }

        public void InitializeAllElementScripts(bool recursive)
        {
            foreach (UserInterfaceElement element in Elements.Values.ToList())
            {
                if (recursive && element is IElementContainer elementContainer)
                {
                    elementContainer.ElementContainer.InitializeAllElementScripts(recursive);
                }
                
                if (element is not IScriptableElement scriptableElement) continue;

                scriptableElement.InitScripts();
            }
        }

        public void UpdateElements(GameTime gameTime)
        {
            //.ToList() creates a copy of the list so that elements can be removed from the original list while iterating
            foreach (UserInterfaceElement element in Elements.Values.ToList())
            {
                element.Update(gameTime);

                if (element is IScriptableElement scriptableElement)
                {
                    foreach (IElementBehaviorScript script in scriptableElement.Scripts)
                    {
                        if (!script.Enabled) continue;

                        script.Update(element, gameTime);
                    }
                }
            }
        }

        public void DrawElements(GameTime gameTime, SpriteBatch batch)
        {
            foreach (UserInterfaceElement element in Elements.Values)
            {
                element.Draw(gameTime, batch);
            }
        }

        public int GetLowestAvailableUniqueId(string name)
        {
            int uniqueId = 0;
            while (Elements.ContainsKey((name, uniqueId))) uniqueId++;

            return uniqueId;
        }

        public void Add(ElementId id, UserInterfaceElement element)
        {
            element.Id = id;
            this[id] = element;
        }
        
        public void Add(UserInterfaceElement element) => Add(element.Id, element);

        public void Add(KeyValuePair<ElementId, UserInterfaceElement> item) => Add(item.Key, item.Value);

        public bool Remove(ElementId id)
        {
            if (!Elements.ContainsKey(id)) return false;

            this[id].Owner = null;
            return true;
        }

        public bool Remove(KeyValuePair<ElementId, UserInterfaceElement> item) => Remove(item.Key);

        public bool Contains(KeyValuePair<ElementId, UserInterfaceElement> item) => Elements.Contains(item);
        
        public bool ContainsKey(ElementId key) => Elements.ContainsKey(key);
        
        public void Clear()
        {
            foreach (UserInterfaceElement element in Elements.Values)
            {
                element.Owner = null;
            }
            
            Elements.Clear();
        }

        public void CopyTo(KeyValuePair<ElementId, UserInterfaceElement>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<ElementId, UserInterfaceElement>>) Elements).CopyTo(array, arrayIndex);
        }

        public bool TryGetValue(ElementId key, [MaybeNullWhen(false)] out UserInterfaceElement value) =>
            Elements.TryGetValue(key, out value);

        public bool TryGetValueWithCast<T>(ElementId key, [MaybeNullWhen(false)] out T value) where T : class
        {
            value = default;
            return TryGetValue(key, out var elem) && (value = elem as T) is not null;
        }

        public bool TryFindElement<T>(string query, [MaybeNullWhen(false)] out T value) where T : class
        {
            int sepIndex = query.IndexOf('/');
            if (sepIndex == -1)
            {
                return TryGetValueWithCast((query, 0), out value);
            }

            string elemName = query[..sepIndex];
            string nextQuery = query[(sepIndex + 1)..];

            if (elemName == ".." && (Owner as UserInterfaceElement)?.Owner is { } parentContainer)
            {
                return parentContainer.ElementContainer.TryFindElement(nextQuery, out value);
            }

            if (TryGetValueWithCast<IElementContainer>((elemName, 0), out var container))
            {
                return container.ElementContainer.TryFindElement(nextQuery, out value);
            }

            value = null;
            return false;
        }

        public IEnumerator<KeyValuePair<ElementId, UserInterfaceElement>> GetEnumerator() => Elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void CloneElementsTo(ElementContainer otherContainer)
        {
            foreach (UserInterfaceElement element in Elements.Values.ToList())
            {
                UserInterfaceElement elementClone = (UserInterfaceElement) element.Clone();
                otherContainer[elementClone.Id] = elementClone;
            }
        }
        
        public void Dispose()
        {
            //Dispose elements. Their scripts should be disposed by the element itself.
            foreach (UserInterfaceElement element in Elements.Values)
            {
                if (element is IDisposable disposable) disposable.Dispose();
            }
    
            //Since the container is disposed, reset the owner of all elements to null
            Clear();
        }
    }
}