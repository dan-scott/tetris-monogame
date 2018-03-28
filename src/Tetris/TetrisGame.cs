using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Tetris
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TetrisGame : Game
    {
        GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Tetromino _current;
        private GridRenderer _gridRenderer;

        private bool _gameOver;
        private bool _paused;
        private int _linesCleared;
        private int _levelLines;
        private int _score;


        private static TimeSpan _tickLength = new TimeSpan(0, 0, 0, 1, 0);
        private static TimeSpan _sinceLastTick;

        public const int GRID_WIDTH = 10;
        public const int GRID_HEIGHT = 16;
        public const int LINES_PER_LEVEL = 2;


        private readonly List<GridBlock> _gridBlocks = new List<GridBlock>();
        private readonly List<GridBlock> _outlineBlocks = new List<GridBlock>();

        private KeyboardState _prevKeyboardState;
        private SpriteFont _font;

        private void Reset()
        {
            _gridBlocks.Clear();
            NextShape();
            _gameOver = false;
            _levelLines = 0;
            _linesCleared = 0;
            _score = 0;
            _tickLength = new TimeSpan(0, 0, 1);
            _paused = false;
        }

        public TetrisGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _current = new Tetromino();
            NextShape();

            _prevKeyboardState = Keyboard.GetState();

            Window.AllowUserResizing = true;

            _outlineBlocks
                .AddRange(
                    Enumerable.Range(-1, GRID_HEIGHT + 1)
                        .Select(y => new GridBlock((-1, y), Color.DarkSlateGray)));

            _outlineBlocks
                .AddRange(Enumerable.Range(-1, GRID_HEIGHT + 1)
                    .Select(y => new GridBlock((GRID_WIDTH, y), Color.DarkSlateGray)));

            _outlineBlocks
                .AddRange(Enumerable.Range(-1, GRID_WIDTH + 2)
                    .Select(x => new GridBlock((x, GRID_HEIGHT), Color.DarkSlateGray)));

            base.Initialize();
        }

        private void NextShape()
        {
            _current.Reset(TetronimoShape.RandomShape());
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Fonts/Font");

            _gridRenderer = new GridRenderer(this, _spriteBatch);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            if (!_gameOver)
            {
                if (!_paused)
                {
                    CheckRows();

                    TickGame(gameTime);
                }

                HandleInput();

                if (!_paused)
                {
                    var hasCollisions = CheckCollisions();

                    CheckGameOver();

                    if (hasCollisions && !_gameOver)
                    {
                        NextShape();
                    }
                }
            }
            else
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    Reset();
                }
            }

            base.Update(gameTime);
        }


        private void CheckGameOver()
        {
            _gameOver = _gridBlocks.Any(block => block.Y == 0);
        }

        private void CheckRows()
        {
            var fullRows = _gridBlocks.GroupBy(block => block.Y).Where(row => row.Count() == GRID_WIDTH).ToList();

            if (!fullRows.Any()) return;

            var removeNums = fullRows.Select(row => row.Key).OrderBy(x => x).ToList();
            _gridBlocks.RemoveAll(block => removeNums.Contains(block.Y));

            foreach (var num in removeNums)
            {
                var toRemove = _gridBlocks.Where(block => block.Y < num).ToList();
                _gridBlocks.RemoveAll(block => block.Y < num);
                _gridBlocks.AddRange(toRemove.Select(block => new GridBlock((block.X, block.Y + 1), block.Color)));
            }

            _linesCleared += removeNums.Count;
            _levelLines += removeNums.Count;
            _score += removeNums.Count * 100;

            if (_levelLines > LINES_PER_LEVEL)
            {
                _levelLines -= LINES_PER_LEVEL;
                _tickLength = new TimeSpan((long) (_tickLength.Ticks * 0.95m));
            }
        }

        private bool CheckCollisions()
        {
            var points = _current.GridPoints();

            var nextPoints = points.Select(val => (x: val.x, y: val.y + 1)).ToArray();

            if (!HasCollisions(nextPoints))
            {
                return false;
            }

            _gridBlocks.AddRange(points.Select(val => new GridBlock(val, _current.Color)));

            return true;
        }

        private bool HasCollisions((int x, int y)[] points)
        {
            return points.Any(val =>
                val.y == GRID_HEIGHT ||
                val.x < 0 ||
                val.x >= GRID_WIDTH ||
                _gridBlocks.Any(block => block.X == val.x && block.Y == val.y));
        }

        private void TickGame(GameTime gameTime)
        {
            _sinceLastTick += gameTime.ElapsedGameTime;

            if (_sinceLastTick < _tickLength) return;

            _sinceLastTick -= _tickLength;
            _current.Down();
        }

        private void HandleInput()
        {
            var currentState = Keyboard.GetState();

            if (!_paused)
            {
                PerformIf(Keys.Up, Rotate, currentState);
                PerformIf(Keys.Down, Drop, currentState);

                PerformIf(Keys.Left, Left, currentState);
                PerformIf(Keys.Right, Right, currentState);
            }

            PerformIf(Keys.P, Pause, currentState);

            _prevKeyboardState = currentState;
        }

        private void PerformIf(Keys key, Action action, KeyboardState currentState)
        {
            if (currentState.IsKeyDown(key) && !_prevKeyboardState.IsKeyDown(key))
            {
                action.Invoke();
            }
        }

        private void Left()
        {
            _current.Left();
            if (HasCollisions(_current.GridPoints()))
            {
                _current.Right();
            }
        }

        private void Right()
        {
            _current.Right();
            if (HasCollisions(_current.GridPoints()))
            {
                _current.Left();
            }
        }

        private void Rotate()
        {
            _current.RotCw();

            var maxX = _current.GridPoints().Max(val => val.x);
            var diff = GRID_WIDTH - 1 - maxX;

            if (diff < 0)
            {
                for (var i = 0; i < -diff; i++) _current.Left();
            }

            if (HasCollisions(_current.GridPoints()))
            {
                _current.RotCcw();
                if (diff < 0)
                {
                    for (var i = 0; i < -diff; i++) _current.Right();
                }
            }
        }

        private void Drop()
        {
            var minDiff = MinDiff();
            for (var i = 1; i < minDiff; i++)
            {
                _current.Down();
            }

            _score += minDiff * 10;
        }

        private void Pause()
        {
            _paused = !_paused;
        }

        private int MinDiff()
        {
            return _current
                .GridPoints()
                .Min(point =>
                {
                    var colGrid = _gridBlocks.Where(grid => grid.X == point.x).ToList();
                    if (!colGrid.Any())
                    {
                        return GRID_HEIGHT - point.y;
                    }

                    var closest = colGrid.Min(grid => grid.Y);
                    return closest - point.y;
                });
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            _current.Render(_gridRenderer);

            DrawGrid();

            DrawScore();

            if (_gameOver)
            {
                _spriteBatch.DrawString(_font, "AH GAME OVER BRO", new Vector2(20, 20), Color.Aqua);
            }

            if (_paused)
            {
                _spriteBatch.DrawString(_font, "PAUSED BRO", new Vector2(20, 20), Color.Aqua);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawScore()
        {
            _spriteBatch.DrawString(_font, $"Level: {_linesCleared / LINES_PER_LEVEL + 1}", new Vector2(20, 40), Color.Yellow);
            _spriteBatch.DrawString(_font, $"Lines cleared: {_linesCleared}", new Vector2(20, 60), Color.Yellow);
            _spriteBatch.DrawString(_font, $"Score: {_score}", new Vector2(20, 80), Color.Yellow);
        }

        private void DrawGrid()
        {
            _gridBlocks.ForEach(block => { _gridRenderer.Render(block.X, block.Y, block.Color); });

            _outlineBlocks.ForEach(block => { _gridRenderer.Render(block.X, block.Y, block.Color); });
        }
    }
}