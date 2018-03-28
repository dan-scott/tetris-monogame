using Microsoft.Xna.Framework;

namespace Tetris
{
    public struct GridBlock
    {
        public int X;
        public int Y;
        public readonly Color Color;

        public GridBlock((int x, int y) point, Color color)
        {
            X = point.x;
            Y = point.y;
            Color = color;
        }

        public override string ToString()
        {
            return $"{X}:{Y}";
        }
    }
}