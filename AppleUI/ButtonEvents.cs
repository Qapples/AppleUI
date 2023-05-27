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

        public static IButtonBehavior? LoadBehaviorScript(IButton button, UserInterfaceManager manager,
            string scriptName)
        {
            IElementBehaviorScript? script =
                manager.LoadElementBehaviorScript($"_{scriptName}", typeof(IButtonBehavior));

            //should always be false since we check if it implements the interface in the LoadElementBehaviorScript
            if (script is not IButtonBehavior buttonBehavior) return null;
            
            //We use the AppendAllEvents method to avoid having to write out all the events manually.
            AppendAllEvents(buttonBehavior.ButtonEvents, buttonBehavior.ButtonEvents);
                
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