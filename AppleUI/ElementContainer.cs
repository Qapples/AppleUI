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
    public class ElementContainer : IDictionary<string, UserInterfaceElement>, IDisposable
    {
        public IElementContainer Owner { get; }
        
        internal Dictionary<string, UserInterfaceElement> Elements { get; set; }
        
        public UserInterfaceElement this[string key]
        {
            get => Elements[key];
            set
            {
                RemoveElementFromOwner(value);
                value.SetOwnerFieldInternal(Owner);
                
                if (TryGetValue(key, out UserInterfaceElement? existingElement))
                {
                    existingElement.SetOwnerFieldInternal(null);
                }
                
                Elements[key] = value;
            }
        }
        
        public ICollection<string> Keys => Elements.Keys;
        public ICollection<UserInterfaceElement> Values => Elements.Values;

        public int Count => Elements.Count;
        public bool IsReadOnly => false;
        
        public ElementContainer(IElementContainer owner)
        {
            Owner = owner;
            Elements = new Dictionary<string, UserInterfaceElement>();
        }

        public ElementContainer(IElementContainer owner, IDictionary<string, UserInterfaceElement> elements) :
            this(owner)
        {
            foreach (UserInterfaceElement element in elements.Values.ToList())
            {
                Add(element);
            }
        }

        private static void RemoveElementFromOwner(UserInterfaceElement element)
        {
            element.Owner?.ElementContainer.Elements.Remove(element.Id);
        }

        internal void LoadAllElementScripts(UserInterfaceManager manager)
        {
            foreach (UserInterfaceElement element in Elements.Values)
            {
                if (element is not IScriptableElement scriptableElement) continue;

                scriptableElement.LoadScripts(manager);
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

        public void Add(string id, UserInterfaceElement element)
        {
            element.Id = id;
            
            RemoveElementFromOwner(element);
            element.SetOwnerFieldInternal(Owner);

            Elements.Add(id, element);
        }
        
        public void Add(UserInterfaceElement element) => Add(element.Id, element);

        public void Add(KeyValuePair<string, UserInterfaceElement> item) => Add(item.Key, item.Value);

        public bool Remove(string id)
        {
            if (!Elements.ContainsKey(id)) return false;
            
            Elements[id].SetOwnerFieldInternal(null);
            return Elements.Remove(id);
        }

        public bool Remove(KeyValuePair<string, UserInterfaceElement> item) => Remove(item.Key);

        public bool Contains(KeyValuePair<string, UserInterfaceElement> item) => Elements.Contains(item);
        
        public bool ContainsKey(string key) => Elements.ContainsKey(key);
        
        public void Clear()
        {
            foreach (UserInterfaceElement element in Elements.Values)
            {
                element.SetOwnerFieldInternal(null);
            }
            
            Elements.Clear();
        }

        public void CopyTo(KeyValuePair<string, UserInterfaceElement>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, UserInterfaceElement>>) Elements).CopyTo(array, arrayIndex);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out UserInterfaceElement value) =>
            Elements.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, UserInterfaceElement>> GetEnumerator() => Elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void CloneElementsTo(ElementContainer otherContainer)
        {
            foreach (UserInterfaceElement element in Elements.Values)
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