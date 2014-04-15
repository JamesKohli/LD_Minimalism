using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Minimalism
{
    class Paint
    {
        public Vector2 Position;
        public Rectangle Rect;
        public int Width;
        public int Height;
        private int _xSpeed;
        private int _ySpeed;
        public Color PaintColor;
        public bool Active;
        public int Score;

        public void Initialize(int x, int y)
        {
            Position = new Vector2(x, y);
            _xSpeed = Globals.Rng.Next(-2, 2);
            _ySpeed = Globals.Rng.Next(-2, 2);
            if (_xSpeed == 0) _xSpeed = 1;
            if (_ySpeed == 0) _ySpeed = 1;
            Width = 10;
            Height = 10;
            PaintColor = new Color(Globals.Rng.Next(0, 255), Globals.Rng.Next(0, 255), Globals.Rng.Next(0, 255));
            Active = true;
            Rect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            Score = 100000;
        }

        public void Update(List<PaintedRectangle> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                if (rectangle.Rect.Intersects(new Rectangle((int)Position.X + _xSpeed, (int)Position.Y, Width, Height)))
                {
                    _xSpeed = _xSpeed*-1;
                    break;
                }
            }

            foreach (var rectangle in rectangles)
            {
                if (rectangle.Rect.Intersects(new Rectangle((int)Position.X, (int)Position.Y + _ySpeed, Width, Height)))
                {
                    _ySpeed = _ySpeed * -1;
                    break;
                }
            }

            Position.X += _xSpeed;
            Position.Y += _ySpeed;

            if (Position.X <= Globals.XMin || Position.X >= Globals.XMax) _xSpeed = _xSpeed*-1;
            if (Position.Y <= Globals.YMin || Position.Y >= Globals.YMax) _ySpeed = _ySpeed * -1;
            Rect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        public void Draw(ExtendedSpriteBatch spriteBatch)
        {
            if (Active)
            {
                spriteBatch.DrawRectangle(Rect, Color.Black);
                spriteBatch.FillRectangle(Rect, PaintColor);
            }
        }
    }
}
