using Microsoft.Xna.Framework.Input;

namespace AppleUI.Interfaces.Behavior
{
    public interface IButtonBehavior
    {
        void OnHover(IButton thisButton, MouseState mouseState);
        void OnMouseLeave(IButton thisButton, MouseState mouseState);

        void OnPress(IButton thisButton, MouseState mouseState);
        void OnRelease(IButton thisButton, MouseState mouseState);
    }
}