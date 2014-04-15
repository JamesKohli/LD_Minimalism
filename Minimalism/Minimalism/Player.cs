using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Minimalism
{
    class Player
    {
        public Vector2 Position;
        public int Width { get; set; }
        public int Height { get; set; }

        //Movement restrictions
        private bool _canMoveX;
        private bool _canMoveY;
        private bool _movingLeft;
        private bool _movingRight;
        private bool _movingUp;
        private bool _movingDown;

        private float _speed;

        public bool _filling;
        private bool _fillingUp;
        private bool _fillingLeft;
        private Vector2 _maxPosition;

        public bool HasNewRectangle;
        public Rectangle NewRectangle;

       


        public void Initialize(int x, int y)
        {
            Position = new Vector2(x, y);
            _speed = 7;
            Width = 8;
            Height = 8;
            _canMoveX = true;
            _canMoveY = true;
            _movingUp = false;
            _movingDown = false;
            _movingRight = false;
            _movingLeft = false;
            _filling = false;
            _fillingUp = false;
            _fillingLeft = false;
            HasNewRectangle = false;

        }

        public void Update(KeyboardState keyboardState, KeyboardState previousKeyboardState)
        {
            SetMoveStates(keyboardState, previousKeyboardState);

            if (_movingLeft){ Position.X -= _speed;}
            if (_movingRight) Position.X += _speed;
            if (_movingUp) Position.Y -= _speed;
            if (_movingDown) Position.Y += _speed;

            CheckFill();

            if (Position.X >= Globals.XMax || Position.X <= Globals.XMin || Position.Y <= Globals.YMin ||
                Position.Y >= Globals.YMax)
            {
                _canMoveX = true;
                _canMoveY = true;
            }

        }

        private void SetMoveStates(KeyboardState keyboardState, KeyboardState previousKeyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Left) && _canMoveX)
            {
                if (_filling)
                {
                    _maxPosition = Position;
                    _fillingLeft = true;
                    if (_movingUp) _fillingUp = true;
                    if (!_movingUp) _fillingUp = false;
                }
                _movingUp = false;
                _movingDown = false;
                _movingLeft = true;
                _movingRight = false;
                _canMoveX = false;

                if (!previousKeyboardState.IsKeyDown(Keys.Left)) Game1.MoveSound.Play();

            }
            if (keyboardState.IsKeyDown(Keys.Right) && _canMoveX)
            {
                if (_filling)
                {
                    _maxPosition = Position;
                    _fillingLeft = false;
                    if (_movingUp) _fillingUp = true;
                    if (!_movingUp) _fillingUp = false;
                }
                _movingUp = false;
                _movingDown = false;
                _movingLeft = false;
                _movingRight = true;
                _canMoveX = false;

                if (!previousKeyboardState.IsKeyDown(Keys.Right)) Game1.MoveSound.Play();
                
            }
            if (keyboardState.IsKeyDown(Keys.Up) && _canMoveY)
            {
                if (_filling)
                {
                    _maxPosition = Position;
                    _fillingUp = false;
                    if (_movingLeft) _fillingLeft = false;
                    if (!_movingLeft) _fillingLeft = true;
                }
                _movingUp = true;
                _movingDown = false;
                _movingLeft = false;
                _movingRight = false;
                _canMoveY = false;

                if (!previousKeyboardState.IsKeyDown(Keys.Up)) Game1.MoveSound.Play();
                
            }
            if (keyboardState.IsKeyDown(Keys.Down) && _canMoveY)
            {
                if (_filling)
                {
                    _maxPosition = Position;
                    _fillingUp = true;
                    if (_movingLeft) _fillingLeft = false;
                    if (!_movingLeft) _fillingLeft = true;
                }
                _movingUp = false;
                _movingDown = true;
                _movingLeft = false;
                _movingRight = false;
                _canMoveY = false;

                if (!previousKeyboardState.IsKeyDown(Keys.Down)) Game1.MoveSound.Play();
            }
        }

        public void CheckFill()
        {
            //Check if not on border, if not, figure out fill conditions
            if (Position.X < Globals.XMax && Position.X > Globals.XMin && Position.Y < Globals.YMax &&
                Position.Y > Globals.YMin)
            {
                //If we just launched off a wall, the max position will be the directly across from the launching point
                if (!_filling)
                {
                    if (_movingLeft)
                    {
                        _maxPosition = new Vector2(Globals.XMin, Position.Y);
                        _fillingLeft = false;
                    }
                    if (_movingRight)
                    {
                        _maxPosition = new Vector2(Globals.XMax, Position.Y);
                        _fillingLeft = true;
                    }
                    if (_movingUp)
                    {
                        _maxPosition = new Vector2(Position.X, Globals.YMax);
                        _fillingUp = false;
                    }
                    if (_movingDown)
                    {
                        _maxPosition = new Vector2(Position.X, Globals.YMin);
                        _fillingUp = true;
                    }

                    if ((_movingLeft || _movingRight) && Position.Y > (Globals.YMax + Globals.YMin) / 2)
                    {
                        _fillingUp = true;
                    }
                    else if ((_movingLeft || _movingRight) && Position.Y < (Globals.YMax + Globals.YMin) / 2)
                    {
                        _fillingUp = false;
                    }
                    else if ((_movingUp || _movingDown) && Position.X > (Globals.XMax + Globals.XMin) / 2)
                    {
                        _fillingLeft = false;
                    }
                    else if ((_movingUp || _movingDown) && Position.X < (Globals.XMax + Globals.XMin) / 2)
                    {
                        _fillingLeft = true;
                    }
                }
                _filling = true;
                
            }
            else
            {
                if (_filling)
                {
                    HasNewRectangle = true;
                    NewRectangle = CreateNewRectangle();
                }
                _filling = false;
            }

            
        }

        /// <summary>
        /// Update the most recently created rectangle
        /// </summary>
        /// <returns></returns>
        private Rectangle CreateNewRectangle()
        {
            int x, y, width, height;
            if (!_fillingLeft)
            {
                x = (int)_maxPosition.X;
                width = Globals.XMax - x;
            }
            else
            {
                x = Globals.XMin;
                width = (int)_maxPosition.X - x;
            }

            if (_fillingUp)
            {
                y = (int)_maxPosition.Y;
                height = Globals.YMax - y;
            }
            else
            {
                y = Globals.YMin;
                height = (int) _maxPosition.Y - y;
            }

            return new Rectangle(x, y, width, height);
        }


        public void Draw(ExtendedSpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle(new Rectangle((int)Position.X-Width/2, (int)Position.Y-Height/2, Width, Height), Color.Black);
            spriteBatch.FillRectangle(new Rectangle((int)_maxPosition.X, (int) _maxPosition.Y, 2, 2), Color.Red );
        }
    }
}
