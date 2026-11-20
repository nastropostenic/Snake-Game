using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace Snake_Game
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;
        private Texture2D _AppleSprite;

        public const int Speed = 5;
        public const int windowWidth = 1280;
        public const int windowHeight = 720;

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
            return Convert.ToInt32(Math.Sqrt(Math.Pow(point2._X - point1._X, 2) + Math.Pow(point2._Y - point1._Y, 2)));
        }

        public class Object
        {

        }

        public class Snake
        {
            public Direction MoveDirection = Direction.Up;
            public Coordinate _Position = new Coordinate(windowWidth / 2, windowHeight / 2);
            public int _SizeX = 100;
            public int _SizeY = 100;
        }

        public class Apple
        {
            public Coordinate _Position = new Coordinate(windowWidth / 2, windowHeight / 2);
            public int _SizeX = 100;
            public int _SizeY = 100;

            //public Coordinate _TopLeft => new Coordinate(_Position._X - _SizeX / 2, _Position._Y - _SizeY / 2);
            //public Coordinate _TopRight => new Coordinate(_Position._X + _SizeX / 2, _Position._Y - _SizeY / 2);
            //public Coordinate _BottomLeft => new Coordinate(_Position._X - _SizeX / 2, _Position._Y + _SizeY / 2);
            //public Coordinate _BottomRight => new Coordinate(_Position._X + _SizeX / 2, _Position._Y + _SizeY / 2);

            public Apple(int sizeX, int sizeY)
            {
                _SizeX = sizeX;
                _SizeY = sizeY;
            }
        }

        Snake SnakeObject = new Snake();
        Apple AppleObject = new Apple(50, 50);

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = windowWidth;
            _graphics.PreferredBackBufferHeight = windowHeight;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _AppleSprite = Content.Load<Texture2D>("apple sprite");
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            // HANDLES MOVEMENT

            if (Keyboard.GetState().IsKeyDown(Keys.W))
                SnakeObject.MoveDirection = Direction.Up;
            else if (Keyboard.GetState().IsKeyDown(Keys.A))
                SnakeObject.MoveDirection = Direction.Left;
            else if (Keyboard.GetState().IsKeyDown(Keys.D))
                SnakeObject.MoveDirection = Direction.Right;
            else if (Keyboard.GetState().IsKeyDown(Keys.S))
                SnakeObject.MoveDirection = Direction.Down;

            switch (SnakeObject.MoveDirection)
            {
                case Direction.Up:
                    SnakeObject._Position._Y -= Speed;
                    break;
                case Direction.Left:
                    SnakeObject._Position._X -= Speed;
                    break;
                case Direction.Right:
                    SnakeObject._Position._X += Speed;
                    break;
                case Direction.Down:
                    SnakeObject._Position._Y += Speed;
                    break;
            }

            // HANDLES SNAKE GOING OFF MAP

            if (SnakeObject._Position._X <= 0 || SnakeObject._Position._X + SnakeObject._SizeX >= windowWidth)
                SnakeObject._Position._X = windowWidth / 2;
            if (SnakeObject._Position._Y <= 0 || SnakeObject._Position._Y + SnakeObject._SizeY >= windowHeight)
                SnakeObject._Position._Y = windowHeight / 2;

            // HANDLES COLLECTING APPLE

            if (FindDistance(SnakeObject._Position, AppleObject._Position) < 100)
            {
                while (true)
                {
                    int positionX = System.Random.Shared.Next(0, windowWidth - AppleObject._SizeX);
                    int positionY = System.Random.Shared.Next(0, windowHeight - AppleObject._SizeY);
                    if (FindDistance(SnakeObject._Position, new Coordinate(positionX, positionY)) > 650)
                    {
                        AppleObject._Position._X = System.Random.Shared.Next(0, windowWidth - AppleObject._SizeX);
                        AppleObject._Position._Y = System.Random.Shared.Next(0, windowHeight - AppleObject._SizeY);
                        break;
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
 
            _spriteBatch.Draw(_pixel, new Rectangle(SnakeObject._Position._X, SnakeObject._Position._Y, SnakeObject._SizeX, SnakeObject._SizeY), Color.Green);
            _spriteBatch.Draw(_AppleSprite, new Rectangle(AppleObject._Position._X, AppleObject._Position._Y, AppleObject._SizeX, AppleObject._SizeY), Color.Red);
 
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
