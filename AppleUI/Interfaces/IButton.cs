using AppleUI.Interfaces.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AppleUI.Interfaces
{
    public interface IButton : IUserInterfaceElement
    {
        public delegate void ButtonEventDelegate(IButton thisButton, MouseState mouseState);

        event ButtonEventDelegate OnHover;
        event ButtonEventDelegate OnMouseLeave;
        
        event ButtonEventDelegate OnPress;
        event ButtonEventDelegate OnRelease;
    }

    public static class ButtonInterfaceExtensions
    {
        public static IButtonBehavior? LoadBehaviorScript(this IButton thisButton, UserInterfaceManager manager,
            string scriptName)
        {
            IElementBehaviorScript? script = manager.LoadElementBehaviorScript($"_{scriptName}", typeof(IButtonBehavior));

            //should always be true since we check if it implements the interface in the LoadElementBehaviorScript
            if (script is IButtonBehavior buttonBehavior)
            {
                thisButton.OnHover += buttonBehavior.OnHover;
                thisButton.OnMouseLeave += buttonBehavior.OnMouseLeave;

                thisButton.OnPress += buttonBehavior.OnPress;
                thisButton.OnRelease += buttonBehavior.OnRelease;

                return buttonBehavior;
            }

            return null;
        }
    }
}