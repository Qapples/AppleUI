using Microsoft.Xna.Framework;

namespace AppleUI.Interfaces
{
    public interface IElementContainer
    {
        ElementContainer ElementContainer { get; }
        
        Vector2 RawPosition { get; }
        Vector2 RawSize { get; }
    }
}