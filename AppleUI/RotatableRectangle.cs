using System;
using Microsoft.Xna.Framework;

namespace AppleUI
{
    public struct RotatableRectangle
    {
        public Rectangle Rectangle;
        public float RotationRadians;

        public RotatableRectangle(Rectangle rectangle, float rotationRadians)
        {
            Rectangle = rectangle;
            RotationRadians = rotationRadians;
        }

        public RotatableRectangle(int x, int y, int width, int height, float rotationRadians)
        {
            Rectangle = new Rectangle(x, y, width, height);
            RotationRadians = rotationRadians;
        }

        public RotatableRectangle(Vector2 position, Vector2 size, float rotationRadians)
        {
            Rectangle = new Rectangle(position.ToPoint(), size.ToPoint());
            RotationRadians = rotationRadians;
        }

        public bool Intersects(RotatableRectangle other)
        {
            Span<Vector2> thisPolyCorners = stackalloc Vector2[4];
            Span<Vector2> otherPolyCorners = stackalloc Vector2[4];

            Polygon thisPoly = new(this, thisPolyCorners);
            Polygon otherPoly = new(other, otherPolyCorners);

            return thisPoly.Intersects(otherPoly);
        }

        public bool Intersects(Rectangle rectangle)
        {
            Span<Vector2> thisPolyCorners = stackalloc Vector2[4];
            Span<Vector2> otherPolyCorners = stackalloc Vector2[4];

            Polygon thisPoly = new(this, thisPolyCorners);
            Polygon otherPoly = new(new RotatableRectangle(rectangle, 0f), otherPolyCorners);

            return thisPoly.Intersects(otherPoly);
        }

        public bool Contains(Vector2 point)
        {
            Span<Vector2> thisPolyCorners = stackalloc Vector2[4];
            Span<Vector2> pointPolyCorner = stackalloc Vector2[1] { point };
            
            Polygon thisPoly = new(this, thisPolyCorners);
            Polygon pointPoly = new(pointPolyCorner);

            return thisPoly.Intersects(pointPoly);
        }
        
        public bool Contains(Point point) => Contains(point.ToVector2());

        private readonly ref struct Polygon
        {
            private readonly ReadOnlySpan<Vector2> _corners;

            public Polygon(ReadOnlySpan<Vector2> corners)
            {
                _corners = corners;
            }

            public Polygon(RotatableRectangle rotRectangle, Span<Vector2> cornersBuffer)
            {
                Rectangle rect = rotRectangle.Rectangle;

                cornersBuffer[0] = new Vector2(rect.Left, rect.Top);
                cornersBuffer[1] = new Vector2(rect.Right, rect.Top);
                cornersBuffer[2] = new Vector2(rect.Right, rect.Bottom);
                cornersBuffer[3] = new Vector2(rect.Left, rect.Bottom);

                //Rotate corners relative to the center
                for (int i = 0; i < 4; i++)
                {
                    cornersBuffer[i] -= rect.Center.ToVector2();
                    cornersBuffer[i] =
                        Vector2.Transform(cornersBuffer[i], Matrix.CreateRotationZ(rotRectangle.RotationRadians));
                    cornersBuffer[i] += rect.Center.ToVector2();
                }

                _corners = cornersBuffer;
            }

            public bool Intersects(Polygon other)
            {
                for (int i = 0; i < _corners.Length; i++)
                {
                    Vector2 edgeStart = _corners[i];
                    Vector2 edgeEnd = _corners[(i + 1) % _corners.Length];
                    Vector2 edge = edgeEnd - edgeStart;

                    Vector2 normal = new Vector2(-edge.Y, edge.X);

                    float minA = float.MaxValue;
                    float maxA = float.MinValue;

                    foreach (Vector2 corner in _corners)
                    {
                        float projection = Vector2.Dot(corner, normal);
                        minA = Math.Min(minA, projection);
                        maxA = Math.Max(maxA, projection);
                    }

                    float minB = float.MaxValue;
                    float maxB = float.MinValue;

                    foreach (var corner in other._corners)
                    {
                        float projection = Vector2.Dot(corner, normal);
                        minB = Math.Min(minB, projection);
                        maxB = Math.Max(maxB, projection);
                    }

                    if (maxA < minB || maxB < minA)
                        return false;
                }

                return true;
            }
        }
    }
}