using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tetris
{
    public class GridRenderer
    {
        private readonly int _blockLength;
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _blockTexture;
        private Point _topLeft;

        public GridRenderer(Game game, SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
            _blockTexture = GenerateBlockTexture(game.GraphicsDevice);

            var width = game.Window.ClientBounds.Width;
            var height = game.Window.ClientBounds.Height;

            var blockWidth = width / 30;
            var blockHeight = height / 20;

            _blockLength = Math.Min(blockWidth, blockHeight);

            _topLeft = new Point(10 * _blockLength, 3 * _blockLength);
        }

        public void Render(int x, int y, Color color)
        {
            var rect = new Rectangle(
                _topLeft.X + x * _blockLength, 
                _topLeft.Y + y * _blockLength, 
                _blockLength,
                _blockLength);
            
            _spriteBatch.Draw(_blockTexture, rect, color);
        }

        private static Texture2D GenerateBlockTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 100, 100);
            var fill = new Color(Color.Black, 0.0f);
            var blockColorData = Enumerable.Range(1, 100 * 100).Select(i =>
            {
                var mod = i % 100;
                if (mod < 5 || mod > 95 || i < 500 || i >= 9500)
                {
                    return fill;
                }

                if (mod < 10 || mod > 90)
                {
                    return Color.White;
                }

                if (i < 1000 || i >= 9000)
                {
                    return Color.White;
                }

                return fill;
            }).ToArray();
            texture.SetData(blockColorData);
            return texture;
        }
    }
}