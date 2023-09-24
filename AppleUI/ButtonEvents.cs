using System;
using System.Diagnostics;
using System.Reflection;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework.Input;

namespace AppleUI
{
    public class ButtonEvents
    {
        public delegate void ButtonEventDelegate(UserInterfaceElement thisButtonElement, MouseState mouseState);

        public event ButtonEventDelegate OnHover;
        public event ButtonEventDelegate OnMouseLeave;
        public event ButtonEventDelegate OnPress;
        public event ButtonEventDelegate OnRelease;

        public ButtonEvents(ButtonEventDelegate onHover, ButtonEventDelegate onMouseLeave, ButtonEventDelegate onPress,
            ButtonEventDelegate onRelease)
        {
            OnHover = onHover;
            OnMouseLeave = onMouseLeave;
            OnPress = onPress;
            OnRelease = onRelease;
        }
        
        public ButtonEvents()
        {
            OnHover = (_, _) => { };
            OnMouseLeave = (_, _) => { };
            OnPress = (_, _) => { };
            OnRelease = (_, _) => { };
        }

        internal void InvokeOnHover(UserInterfaceElement thisButtonElement, MouseState mouseState) =>
            OnHover(thisButtonElement, mouseState);

        internal void InvokeOnMouseLeave(UserInterfaceElement thisButtonElement, MouseState mouseState) =>
            OnMouseLeave(thisButtonElement, mouseState);

        internal void InvokeOnPress(UserInterfaceElement thisButtonElement, MouseState mouseState) =>
            OnPress(thisButtonElement, mouseState);

        internal void InvokeOnRelease(UserInterfaceElement thisButtonElement, MouseState mouseState) =>
            OnRelease(thisButtonElement, mouseState);

        public void AddEventsFromScripts(IElementBehaviorScript[] scripts)
        {
            foreach (IElementBehaviorScript script in scripts)
            {
                //should always be false since we check if it implements the interface in the LoadElementBehaviorScript
                if (script is not IButtonBehavior buttonBehavior) return;

                OnHover += buttonBehavior.ButtonEvents.OnHover;
                OnMouseLeave += buttonBehavior.ButtonEvents.OnMouseLeave;
                OnPress += buttonBehavior.ButtonEvents.OnPress;
                OnRelease += buttonBehavior.ButtonEvents.OnRelease;
            }
        }
    }
}