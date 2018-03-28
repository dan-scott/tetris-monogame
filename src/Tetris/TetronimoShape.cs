using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tetris
{
    public class TetronimoShape
    {
        private const int BLOCK_SIZE = 20;

        private readonly Dictionary<Orient, (int x, int y)[]> _shapes;

        private TetronimoShape(Orient[] shape, Color color)
        {
            Color = color;
            _shapes = BuildOrientations(shape);
        }

        public Color Color { get; }

        private Dictionary<Orient, (int x, int y)[]> BuildOrientations(Orient[] shape)
        {
            var orientations = new Dictionary<Orient, (int x, int y)[]>();
            foreach (var orientation in new[] {Orient.E, Orient.N, Orient.S, Orient.W})
            {
                var x = 0;
                var y = 0;
                var positions = new List<(int x, int y)>{(x, y)};
                foreach (var dir in shape)
                {
                    var (dirX, dirY) = Movement[Reorient(dir, orientation)];
                    x += dirX;
                    y += dirY;
                    positions.Add((x, y));
                }

                var minX = positions.Min(pos => pos.x);
                var minY = positions.Min(pos => pos.y);

                orientations[orientation] = positions
                    .Select(val => (x: val.x - minX, y: val.y - minY))
                    .GroupBy(val => val)
                    .Select(val => val.First())
                    .ToArray();
            }

            return orientations;
        }

        public void Render(GridRenderer renderer, (int x, int y) position, Orient orientation)
        {
            foreach (var point in _shapes[orientation])
            {
                renderer.Render(point.x + position.x, point.y + position.y, Color);
            }
        }

        public (int x, int y)[] GetPoints(int x, int y, Orient orientation)
        {
            return _shapes[orientation].Select(pos => (x: pos.x + x, y: pos.y + y)).ToArray();
        }

        private static Orient Reorient(Orient direction, Orient orientation)
        {
            return (Orient) (((int) direction + (int) orientation) % 4);
        }

        private static readonly Dictionary<Orient, (int x, int y)> Movement =
            new Dictionary<Orient, (int x, int y)>
            {
                [Orient.N] = (0, -1),
                [Orient.E] = (1, 0),
                [Orient.S] = (0, 1),
                [Orient.W] = (-1, 0),
            };

        private static readonly Orient[] ShapeI = {Orient.S, Orient.S, Orient.S};
        private static readonly Orient[] ShapeJ = {Orient.E, Orient.E, Orient.S};
        private static readonly Orient[] ShapeL = {Orient.N, Orient.E, Orient.E};
        private static readonly Orient[] ShapeO = {Orient.E, Orient.S, Orient.W};
        private static readonly Orient[] ShapeS = {Orient.E, Orient.N, Orient.E};
        private static readonly Orient[] ShapeT = {Orient.E, Orient.S, Orient.N, Orient.E};
        private static readonly Orient[] ShapeZ = {Orient.E, Orient.S, Orient.E};

        public static readonly TetronimoShape I = new TetronimoShape(ShapeI, Color.Red);
        public static readonly TetronimoShape J = new TetronimoShape(ShapeJ, Color.Magenta);
        public static readonly TetronimoShape L = new TetronimoShape(ShapeL, Color.Yellow);
        public static readonly TetronimoShape O = new TetronimoShape(ShapeO, Color.Cyan);
        public static readonly TetronimoShape S = new TetronimoShape(ShapeS, Color.Blue);
        public static readonly TetronimoShape T = new TetronimoShape(ShapeT, Color.Silver);
        public static readonly TetronimoShape Z = new TetronimoShape(ShapeZ, Color.Green);

        private static readonly TetronimoShape[] AllShapes =
        {
            I, J, L, O, S, T, Z,
        };

        private static readonly Random Random = new Random();

        public static TetronimoShape RandomShape()
        {
             return AllShapes[Random.Next(0, AllShapes.Length - 1)];
        }
    }
}