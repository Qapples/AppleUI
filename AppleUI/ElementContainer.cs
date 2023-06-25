using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI
{
    public class ElementContainer : IList<UserInterfaceElement>, IDisposable
    {
        public IElementContainer Owner { get; }
        
        internal List<UserInterfaceElement> Elements { get; private set; }
        
        public int Count => Elements.Count;
        public bool IsReadOnly => false;
        
        public UserInterfaceElement this[int index]
        {
            get => Elements[index];
            set => Elements[index] = value;
        }

        public ElementContainer(IElementContainer owner)
        {
            Owner = owner;
            Elements = new List<UserInterfaceElement>();
        }

        public ElementContainer(IElementContainer owner, IEnumerable<UserInterfaceElement> elements) : this(owner)
        {
            foreach (UserInterfaceElement element in elements.ToList())
            {
                UserInterfaceElement clonedElement = (UserInterfaceElement) element.Clone();
                RemoveElementFromOwner(clonedElement);
                clonedElement.SetOwnerFieldInternal(owner);
                
                Elements.Add(clonedElement);
            }
        }

        private static void RemoveElementFromOwner(UserInterfaceElement element)
        {
            element.Owner?.ElementContainer.Elements.Remove(element);
        }

        internal void LoadAllElementScripts(UserInterfaceManager manager)
        {
            foreach (UserInterfaceElement element in Elements)
            {
                if (element is not IScriptableElement scriptableElement) continue;
                
                scriptableElement.LoadScripts(manager);
            }
        }
        
        public void UpdateElements(GameTime gameTime)
        {
            //.ToList() creates a copy of the list so that elements can be removed from the original list while iterating
            foreach (UserInterfaceElement element in Elements.ToList())
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
            foreach (UserInterfaceElement element in Elements)
            {
                element.Draw(gameTime, batch);
            }
        }

        public void Add(UserInterfaceElement element)
        {
            RemoveElementFromOwner(element);
            element.SetOwnerFieldInternal(Owner);
            
            Elements.Add(element);
        }

        public void Clear()
        {
            for (int i = Elements.Count - 1; i > -1; i--)
            {
                RemoveAt(i);
            }
        }

        public bool Contains(UserInterfaceElement element) => Elements.Contains(element);

        public void CopyTo(UserInterfaceElement[] array, int arrayIndex) => Elements.CopyTo(array, arrayIndex);
        
        public IEnumerator<UserInterfaceElement> GetEnumerator() => Elements.GetEnumerator();

        public int IndexOf(UserInterfaceElement element) => Elements.IndexOf(element);

        public void Insert(int index, UserInterfaceElement element)
        {
            RemoveElementFromOwner(element);
            element.SetOwnerFieldInternal(Owner);
            
            Elements.Insert(index, element);
        }

        public bool Remove(UserInterfaceElement element)
        {
            element.SetOwnerFieldInternal(null);
            return Elements.Remove(element);
        }

        public void RemoveAt(int index)
        {
            UserInterfaceElement element = Elements[index];
            element.SetOwnerFieldInternal(null);
            
            Elements.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();

        public void Dispose()
        {
            //Dispose elements
            foreach (UserInterfaceElement element in Elements)
            {
                if (element is IDisposable disposable) disposable.Dispose();
            }

            //Dispose scripts
            foreach (IElementBehaviorScript script in (from element in Elements
                         where element is IScriptableElement
                         select ((IScriptableElement) element).Scripts).SelectMany(s => s))
            {
                if (script is IDisposable disposable) disposable.Dispose();
            }
            
            //Since the container is disposed, reset the owner of all elements to null
            Elements.Clear();
        }
    }
}