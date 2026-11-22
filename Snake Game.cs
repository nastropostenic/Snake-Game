using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Snake_Game
{
    public class Snake_Game : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Texture2D _pixel;
        private Texture2D _appleSprite;

        private const int Speed = 5;
        private const int WindowWidth = 1280;
        private const int WindowHeight = 720;
        private const int FrameDelay = 22;
        private const int LoopThroughAvailablePositions = 100;

        public enum GameStates
        {
            Menu,
            Started,
            Ended
        }

        public enum PixelDistances
        {
            MinimumForAppleToSpawn = 700,
            FromSnakeToAppleForCollection = 100,
            DistanceForHeadToCollideWithBody = 50
        }

        public enum Sizes
        {
            SnakePartSizeX = 100,
            SnakePartSizeY = 100,
            AppleSizeX = 50,
            AppleSizeY = 50
        }

        public enum Direction
        {
            Up,
            Left,
            Right,
            Down
        }

        public struct Coordinate
        {
            public int _X;
            public int _Y;

            public Coordinate(int X, int Y)
            {
                _X = X;
                _Y = Y;
            }
        }

        public static int FindDistance(Coordinate point1, Coordinate point2)
        {
            return Convert.ToInt32(
                Math.Sqrt(
                Math.Pow(point2._X - point1._X, 2) 
                + Math.Pow(point2._Y - point1._Y, 2)
            ));
        }

        public static Coordinate RandomPosition(int sizeX, int sizeY)
        {
            int point1 = System.Random.Shared.Next(0, WindowWidth - sizeX);
            int point2 = System.Random.Shared.Next(0, WindowHeight - sizeY);

            return new Coordinate(point1, point2);
        }

        public static void SetRandomPosition(GameObject Object)
        {
            for (int i = 0; i < LoopThroughAvailablePositions; i++)
            {
                Coordinate newPosition = RandomPosition(Object._SizeX, Object._SizeY);
                if (FindDistance(newPosition, CentrePoint) < (int)PixelDistances.MinimumForAppleToSpawn && i < LoopThroughAvailablePositions - 1) 
                    continue;

                Object._Position = RandomPosition(Object._SizeX, Object._SizeY);
                return;
            }
        }

        public static Coordinate CentrePoint = new Coordinate(WindowWidth / 2, WindowHeight / 2);

        // objects

        public class GameObject
        {
            public Coordinate _Position = CentrePoint;
            public int _SizeX = (int)Sizes.SnakePartSizeX;
            public int _SizeY = (int)Sizes.SnakePartSizeY;

            public List<Coordinate> OldPositions = new List<Coordinate>();
        }

        public class SnakeHead : GameObject
        {
            public Direction MoveDirection = Direction.Up;
        }

        public class SnakeBody : GameObject
        {
            public SnakeBody(Coordinate Position)
            {
                _Position = Position;
            }
        }

        public class Apple : GameObject
        {
            public Apple(int sizeX, int sizeY)
            {
                _SizeX = sizeX;
                _SizeY = sizeY;
            }
        }

        // player stats
        public class GameStatisticsTemplate
        {
            public SnakeHead SnakeHeadObject { get; private set; }
            public Apple AppleObject { get; private set; }
            public List<GameObject> SnakeParts = new List<GameObject>();

            public GameStates GameState { get; private set; }
            public int HighScore { get; private set; } = 0;
            public int _Score { get; private set; }

            public GameStatisticsTemplate(int Score)
            {
                _Score = Score;
            }

            public void ResetScore()
            {
                _Score = 0;
            }
            public void IncreaseScore()
            {
                _Score++;
            }

            public void StartGame()
            {
                GameStatistics.SnakeHeadObject = new SnakeHead();
                AppleObject = new Apple((int)Sizes.AppleSizeX, (int)Sizes.AppleSizeY);
                GameStatistics.SnakeParts.Add(GameStatistics.SnakeHeadObject);
                SetRandomPosition(GameStatistics.AppleObject);

                GameState = GameStates.Started;
            }
            public async Task GameOver()
            {
                GameState = GameStates.Ended;

                await Task.Delay(1000);

                HighScore = (HighScore < _Score) ? _Score : HighScore;
                ResetScore();

                GameStatistics.SnakeHeadObject = null;
                AppleObject = null;
                GameStatistics.SnakeParts.Clear();

                await Task.Delay(5000);

                StartGame();
            }
            public void Menu()
            {
                GameState = GameStates.Menu;
            }
        }

        public static GameStatisticsTemplate GameStatistics = new GameStatisticsTemplate(0);

        // game logic

        public Snake_Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = WindowWidth;
            _graphics.PreferredBackBufferHeight = WindowHeight;
            _graphics.ApplyChanges();

            GameStatistics.StartGame();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _font = Content.Load<SpriteFont>("DefaultFont");
            _appleSprite = Content.Load<Texture2D>("apple sprite");
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (GameStatistics.GameState)
            {
                case GameStates.Menu:
                    break;
                case GameStates.Started:
                    // HANDLES SNAKE BODY

                    for (int i = 0; i < GameStatistics.SnakeParts.Count; i++)
                    {
                        GameStatistics.SnakeParts[i].OldPositions.Add(GameStatistics.SnakeParts[i]._Position);

                        if (i < 1)
                            continue;

                        if (GameStatistics.SnakeParts[i - 1].OldPositions.Count < FrameDelay)
                            continue;

                        if (FindDistance(GameStatistics.SnakeHeadObject._Position, GameStatistics.SnakeParts[i]._Position) < (int)PixelDistances.DistanceForHeadToCollideWithBody)
                            GameStatistics.GameOver();

                        GameStatistics.SnakeParts[i]._Position = GameStatistics.SnakeParts[i - 1].OldPositions[0];
                        GameStatistics.SnakeParts[i - 1].OldPositions.RemoveAt(0);
                    }

                    GameStatistics.SnakeParts[GameStatistics.SnakeParts.Count - 1].OldPositions.RemoveAt(0);

                    // HANDLES MOVEMENT

                    if (GameStatistics.SnakeHeadObject.MoveDirection != Direction.Down && (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up)))
                        GameStatistics.SnakeHeadObject.MoveDirection = Direction.Up;
                    else if (GameStatistics.SnakeHeadObject.MoveDirection != Direction.Right && (Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left)))
                        GameStatistics.SnakeHeadObject.MoveDirection = Direction.Left;
                    else if (GameStatistics.SnakeHeadObject.MoveDirection != Direction.Left && (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right)))
                        GameStatistics.SnakeHeadObject.MoveDirection = Direction.Right;
                    else if (GameStatistics.SnakeHeadObject.MoveDirection != Direction.Up && (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down)))
                        GameStatistics.SnakeHeadObject.MoveDirection = Direction.Down;

                    switch (GameStatistics.SnakeHeadObject.MoveDirection)
                    {
                        case Direction.Up:
                            GameStatistics.SnakeHeadObject._Position._Y -= Speed;
                            break;
                        case Direction.Left:
                            GameStatistics.SnakeHeadObject._Position._X -= Speed;
                            break;
                        case Direction.Right:
                            GameStatistics.SnakeHeadObject._Position._X += Speed;
                            break;
                        case Direction.Down:
                            GameStatistics.SnakeHeadObject._Position._Y += Speed;
                            break;
                    }

                    // HANDLES SNAKE GOING OFF MAP

                    if (GameStatistics.SnakeHeadObject._Position._X <= 0 || GameStatistics.SnakeHeadObject._Position._X + GameStatistics.SnakeHeadObject._SizeX >= WindowWidth ||
                        GameStatistics.SnakeHeadObject._Position._Y <= 0 || GameStatistics.SnakeHeadObject._Position._Y + GameStatistics.SnakeHeadObject._SizeY >= WindowHeight)
                    { GameStatistics.GameOver(); }

                    // HANDLES COLLECTING APPLE

                    if (FindDistance(GameStatistics.SnakeHeadObject._Position, GameStatistics.AppleObject._Position) < (int)PixelDistances.FromSnakeToAppleForCollection)
                    {
                        GameStatistics.IncreaseScore();
                        GameStatistics.SnakeParts.Add(new SnakeBody(GameStatistics.SnakeParts[GameStatistics.SnakeParts.Count - 1]._Position));

                        SetRandomPosition(GameStatistics.AppleObject);
                    }
                    break;
                case GameStates.Ended:
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            switch (GameStatistics.GameState)
            {
                case GameStates.Menu:
                    break;
                case GameStates.Started:
                    foreach (var snake in GameStatistics.SnakeParts)
                        _spriteBatch.Draw(_pixel,
                        new Rectangle(snake._Position._X, snake._Position._Y, snake._SizeX, snake._SizeY),
                        Color.Green
                        );

                    _spriteBatch.Draw(_appleSprite,
                        new Rectangle(GameStatistics.AppleObject._Position._X, GameStatistics.AppleObject._Position._Y, GameStatistics.AppleObject._SizeX, GameStatistics.AppleObject._SizeY),
                        Color.Red
                     );

                    _spriteBatch.DrawString(
                        _font,
                        $"Score: {GameStatistics._Score}",
                        new Vector2(0, 0),
                        Color.Red
                    );

                    _spriteBatch.DrawString(
                        _font,
                        $"High Score: {GameStatistics.HighScore}",
                        new Vector2(0, _font.LineSpacing),
                        Color.Red
                    );
                    break;
                case GameStates.Ended:
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
