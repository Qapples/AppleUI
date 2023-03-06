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
}