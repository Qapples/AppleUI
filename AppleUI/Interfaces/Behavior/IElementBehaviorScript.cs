using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AppleUI.Interfaces.Behavior
{
    public interface IElementBehaviorScript
    {
        bool Enabled { get; set; }
        Dictionary<string, object> Arguments { get; set; }

        void Update(IUserInterfaceElement element, GameTime gameTime);
    }
}