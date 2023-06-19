using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IUpdateable = AppleUI.Interfaces.IUpdateable;
using IDrawable = AppleUI.Interfaces.IDrawable;

namespace AppleUI
{
    public class ElementContainer : ICloneable, IDisposable
    {
        public List<IUserInterfaceElement> Elements { get; }
        
        public ElementContainer(List<IUserInterfaceElement> elements)
        {
            Elements = elements;
        }
        
        public ElementContainer()
        {
            Elements = new List<IUserInterfaceElement>();
        }
        
        public void UpdateElements(GameTime gameTime)
        {
            //.ToList() creates a copy of the list so that elements can be removed from the original list while iterating
            foreach (IUserInterfaceElement element in Elements.ToList())
            {
                //if (element is IUpdateable updateable) updateable.Update(this, gameTime);
                if (element is IScriptableElement scriptableElement)
                {
                    foreach (var script in scriptableElement.Scripts.Where(script => script.Enabled))
                    {
                        script.Update(element, gameTime);
                    }
                }
            }
        }

        public void DrawElements(GameTime gameTime, SpriteBatch batch)
        {
            foreach (IUserInterfaceElement element in Elements)
            {
                //if (element is IDrawable drawable) drawable.Draw(this, gameTime, batch);
            }
        }

        public object Clone()
        {
            List<IUserInterfaceElement> clonedElements = new();
            
            foreach (IUserInterfaceElement element in Elements)
            {
                IUserInterfaceElement clonedElement = (IUserInterfaceElement) element.Clone();
                
                if (element is IScriptableElement scriptable and IButton button)
                {
                    button.ButtonEvents.AddEventsFromScripts(scriptable.Scripts);
                }
                
                clonedElements.Add(clonedElement);
            }
            
            return new ElementContainer(clonedElements);
        }
        
        public void Dispose()
        {
            //Dispose elements
            foreach (IDisposable disposableElement in from element in Elements
                     where element is IDisposable
                     select (IDisposable) element)
            {
                disposableElement.Dispose();
            }

            //Dispose scripts
            foreach (IElementBehaviorScript script in (from element in Elements
                         where element is IScriptableElement
                         select ((IScriptableElement) element).Scripts).SelectMany(s => s))
            {
                if (script is IDisposable disposable) disposable.Dispose();
            }
        }
    }
}