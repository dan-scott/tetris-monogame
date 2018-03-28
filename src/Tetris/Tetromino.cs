using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tetris
{
    public class Tetromino
    {
        private TetronimoShape _shape;
        private int _x;
        private int _y;
        private Orient _orientation;

        public Color Color => _shape.Color;

        public void Reset(TetronimoShape shape)
        {
            _orientation = Orient.N;
            _shape = shape;
            _x = 4;
            _y = 0;
        }

        public (int x, int y)[] GridPoints()
        {
            return _shape.GetPoints(_x, _y, _orientation);
        } 

        public void RotCw()
        {
            _orientation = (Orient)(((int)_orientation + 1) % 4);
        }

        public void RotCcw()
        {
            _orientation = (Orient)(((int)_orientation - 1) % 4);
        }

        public void Left()
        {
            _x--;
        }

        public void Right()
        {
            _x++;
        }

        public void Down()
        {
            _y++;
        }

        public void Up()
        {
            _y--;
        }

        public void Render(GridRenderer renderer)
        {
            _shape.Render(renderer, (_x, _y), _orientation);
        }
    }
}