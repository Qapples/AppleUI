using AppleUI.Interfaces;
using Microsoft.Xna.Framework;

namespace AppleUI
{
    public readonly record struct ElementTransform(Measurement Position, PositionBasePoint BasePoint, Vector2 Scale, 
        float Rotation)
    {
        public Vector2 GetDrawPosition(Vector2 parentRawPosition, Vector2 parentRawSize, Vector2 elementRawSize) =>
            Position.GetRawPixelValue(parentRawSize) + parentRawPosition + (BasePoint switch
            {
                PositionBasePoint.TopLeft => Vector2.Zero,
                PositionBasePoint.TopMiddle => new Vector2(elementRawSize.X * 0.5f, 0),
                PositionBasePoint.TopRight => new Vector2(elementRawSize.X, 0),
                PositionBasePoint.CenterLeft => new Vector2(0, elementRawSize.Y * 0.5f),
                PositionBasePoint.Center => new Vector2(elementRawSize.X * 0.5f, elementRawSize.Y * 0.5f),
                PositionBasePoint.CenterRight => new Vector2(elementRawSize.X, elementRawSize.Y * 0.5f),
                PositionBasePoint.BottomLeft => new Vector2(0, elementRawSize.Y),
                PositionBasePoint.BottomMiddle => new Vector2(elementRawSize.X * 0.5f, elementRawSize.Y),
                PositionBasePoint.BottomRight => new Vector2(elementRawSize.X, elementRawSize.Y),
                _ => Vector2.Zero
            });
    }
}