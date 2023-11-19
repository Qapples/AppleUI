using AppleUI.Interfaces;
using Microsoft.Xna.Framework;

namespace AppleUI
{
    public readonly record struct ElementTransform(Measurement Position, Vector2 Scale, float Rotation)
    {
        public Vector2 GetDrawPosition(Vector2 parentRawPosition, Vector2 parentRawSize) =>
            Position.GetRawPixelValue(parentRawSize) + parentRawPosition;
    }
}