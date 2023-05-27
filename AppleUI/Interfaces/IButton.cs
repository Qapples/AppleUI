using AppleUI.Interfaces.Behavior;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AppleUI.Interfaces
{
    public interface IButton : IUserInterfaceElement, IButtonBehavior
    {
        Measurement ButtonSize { get; set; }
    }
}