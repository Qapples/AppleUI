using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        public RotatableRectangle(float x, float y, float width, float height, float rotationRadians)
        {
            Rectangle = new Rectangle((int) x, (int) y, (int) width, (int) height);
            RotationRadians = rotationRadians;
        }

        public RotatableRectangle(Point position, Point size, float rotationRadians)
        {
            Rectangle = new Rectangle(position, size);
            RotationRadians = rotationRadians;
        }

        public RotatableRectangle(Vector2 position, Vector2 size, float rotationRadians) : this(position.ToPoint(), size.ToPoint(), rotationRadians)
        {
        }
        
        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Color color)
        {
            spriteBatch.Draw(texture, Rectangle, null, color, RotationRadians, Vector2.Zero, SpriteEffects.None, 0f);
        }

        public Span<Vector2> GetCorners(Span<Vector2> cornersBuffer)
        {
            Vector2 topLeft = new(Rectangle.Left, Rectangle.Top);
            
            cornersBuffer[0] = new Vector2(Rectangle.Left, Rectangle.Top);
            cornersBuffer[1] = new Vector2(Rectangle.Right, Rectangle.Top);
            cornersBuffer[2] = new Vector2(Rectangle.Right, Rectangle.Bottom);
            cornersBuffer[3] = new Vector2(Rectangle.Left, Rectangle.Bottom);
            
            //Rotate corners relative to the top left.
            
            for (int i = 0; i < 4; i++)
            {
                cornersBuffer[i] -= topLeft;
                cornersBuffer[i] =
                    Vector2.Transform(cornersBuffer[i], Matrix.CreateRotationZ(RotationRadians));
                cornersBuffer[i] += topLeft;
            }

            return cornersBuffer;
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
                rotRectangle.GetCorners(cornersBuffer);
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