using System;
using System.Reflection;
using AppleUI.Interfaces;
using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework.Input;

namespace AppleUI
{
    public class ButtonEvents
    {
        public delegate void ButtonEventDelegate(IButton thisButton, MouseState mouseState);

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
        
        internal void InvokeOnHover(IButton thisButton, MouseState mouseState) => OnHover(thisButton, mouseState);
        internal void InvokeOnMouseLeave(IButton thisButton, MouseState mouseState) => OnMouseLeave(thisButton, mouseState);
        internal void InvokeOnPress(IButton thisButton, MouseState mouseState) => OnPress(thisButton, mouseState);
        internal void InvokeOnRelease(IButton thisButton, MouseState mouseState) => OnRelease(thisButton, mouseState);
        
        public IButtonBehavior? LoadBehaviorScript(UserInterfaceManager manager, string scriptName)
        {
            IElementBehaviorScript? script =
                manager.LoadElementBehaviorScript($"_{scriptName}", typeof(IButtonBehavior));

            //should always be false since we check if it implements the interface in the LoadElementBehaviorScript
            if (script is not IButtonBehavior buttonBehavior) return null;
            
            //We use the AppendAllEvents method to avoid having to write out all the events manually.
            AppendAllEvents(this, buttonBehavior.ButtonEvents);
                
            return buttonBehavior;
        }
        
        private static void AppendAllEvents<T>(T target, T source)
        {
            if (target is null || source is null) return;
            
            Type targetType = target.GetType();
            Type sourceType = source.GetType();

            foreach (EventInfo providerEvent in sourceType.GetEvents())
            {
                EventInfo? recipientEvent = targetType.GetEvent(providerEvent.Name);

                if (recipientEvent?.EventHandlerType is null || providerEvent?.EventHandlerType is null)
                {
                    continue;
                }
                
                Delegate providerDelegate =
                    Delegate.CreateDelegate(providerEvent.EventHandlerType, source, providerEvent.Name);
                
                recipientEvent.AddEventHandler(target, providerDelegate);
            }
        }
    }
}