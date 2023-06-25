using AppleUI.Interfaces;
using Microsoft.Xna.Framework;

namespace AppleUI;

public record struct ElementTransform(Measurement Position, Vector2 Scale, float Rotation)
{
    public Vector2 GetDrawPosition(IElementContainer? container) =>
        GetDrawPosition(container?.RawPosition ?? Vector2.Zero, container?.RawSize ?? Vector2.One);

    public Vector2 GetDrawPosition(Vector2 parentRawPosition, Vector2 parentRawSize) =>
        Position.GetRawPixelValue(parentRawSize) + parentRawPosition;
}