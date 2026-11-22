using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
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

        public enum PixelDistances
        {
            MinimumForAppleToSpawn = 700,
            FromSnakeToAppleForCollection = 100,
            DistanceForHeadToCollideWithBody = 100
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

        // player stats
        public class PlayerStatisticsTemplate
        {
            public int _Score { get; private set; }
            public PlayerStatisticsTemplate(int Score)
            {
                _Score = Score;
            }

            public void IncreaseScore()
            {
                _Score++;
            }
        }

        public static PlayerStatisticsTemplate PlayerStatistics = new PlayerStatisticsTemplate(0);

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

        private SnakeHead SnakeHeadObject = new SnakeHead();
        private Apple AppleObject = new Apple((int)Sizes.AppleSizeX, (int)Sizes.AppleSizeY);
        private List<GameObject> SnakeParts = new List<GameObject>();

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

            SnakeParts.Add(SnakeHeadObject);

            SetRandomPosition(AppleObject);

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

            // HANDLES SNAKE BODY

            for (int i = 0; i < SnakeParts.Count; i++)
            {
                SnakeParts[i].OldPositions.Add(SnakeParts[i]._Position);

                if (i < 1)
                    continue;

                if (SnakeParts[i - 1].OldPositions.Count < FrameDelay)
                    continue;

                if (FindDistance(SnakeHeadObject._Position, SnakeParts[i]._Position) < (int)PixelDistances.DistanceForHeadToCollideWithBody)
                    Environment.Exit(0);

                SnakeParts[i]._Position = SnakeParts[i - 1].OldPositions[0];
                SnakeParts[i - 1].OldPositions.RemoveAt(0);
            }

            SnakeParts[SnakeParts.Count - 1].OldPositions.RemoveAt(0);

            // HANDLES MOVEMENT

            if (SnakeHeadObject.MoveDirection != Direction.Down && Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
                SnakeHeadObject.MoveDirection = Direction.Up;
            else if (SnakeHeadObject.MoveDirection != Direction.Right && Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
                SnakeHeadObject.MoveDirection = Direction.Left;
            else if (SnakeHeadObject.MoveDirection != Direction.Left && Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
                SnakeHeadObject.MoveDirection = Direction.Right;
            else if (SnakeHeadObject.MoveDirection != Direction.Up && Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
                SnakeHeadObject.MoveDirection = Direction.Down;

            switch (SnakeHeadObject.MoveDirection)
            {
                case Direction.Up:
                    SnakeHeadObject._Position._Y -= Speed;
                    break;
                case Direction.Left:
                    SnakeHeadObject._Position._X -= Speed;
                    break;
                case Direction.Right:
                    SnakeHeadObject._Position._X += Speed;
                    break;
                case Direction.Down:
                    SnakeHeadObject._Position._Y += Speed;
                    break;
            }

            // HANDLES SNAKE GOING OFF MAP

            if (SnakeHeadObject._Position._X <= 0 || SnakeHeadObject._Position._X + SnakeHeadObject._SizeX >= WindowWidth)
                Environment.Exit(0);
            if (SnakeHeadObject._Position._Y <= 0 || SnakeHeadObject._Position._Y + SnakeHeadObject._SizeY >= WindowHeight)
                Environment.Exit(0);

            // HANDLES COLLECTING APPLE

            if (FindDistance(SnakeHeadObject._Position, AppleObject._Position) < (int)PixelDistances.FromSnakeToAppleForCollection)
            {
                PlayerStatistics.IncreaseScore();
                SnakeParts.Add(new SnakeBody(SnakeParts[SnakeParts.Count - 1]._Position));

                SetRandomPosition(AppleObject);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            
            foreach (var snake in SnakeParts)
                _spriteBatch.Draw(_pixel, 
                new Rectangle(snake._Position._X, snake._Position._Y, snake._SizeX, snake._SizeY), 
                Color.Green
                );

            _spriteBatch.Draw(_appleSprite, 
                new Rectangle(AppleObject._Position._X, AppleObject._Position._Y, AppleObject._SizeX, AppleObject._SizeY), 
                Color.Red
             );

            _spriteBatch.DrawString(
                _font,
                $"Score: {PlayerStatistics._Score}",
                new Vector2(0, 0),
                Color.Red
            );

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
