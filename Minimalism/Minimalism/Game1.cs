using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Minimalism
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ExtendedSpriteBatch spriteBatch;

        //I/O

        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;

        //Player
        private Player _player;

        //Paint
        private Lines _lines;
        private List<PaintedRectangle> _rectangles;
        private List<Paint> _paints;

        private int _level;
        private int _score;

        private bool _levelStart;
        private bool _levelEnd;

        private Song _themeSong;

        private SpriteFont _spriteFont;
        private SpriteFont _newLevelFont;

        private SoundEffect collisionSound;
        private SoundEffect _paintSound;
        public static SoundEffect MoveSound;
        private SoundEffect _blendedColorSound;
        private SoundEffect _filledRect;

        private bool _titleScreen;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
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
            Globals.XMin = 20;
            Globals.XMax = GraphicsDevice.Viewport.Width - 20;
            Globals.YMin = 20;
            Globals.YMax = GraphicsDevice.Viewport.Height - 100;
            _rectangles = new List<PaintedRectangle>();
            _player = new Player();
            _lines = new Lines();
            _player.Initialize(Globals.XMin, Globals.YMin);
            _lines.Initialize();
            _paints = new List<Paint>();
            _level = 0;
            _score = 0;
            _levelStart = true;
            _levelEnd = false;
            _titleScreen = true;

            for (var n = 0; n < 6; n++)
            {
                var newPaint = new Paint();
                newPaint.Initialize((Globals.XMax + Globals.XMin) / 2 + Globals.Rng.Next(-25, 25), (Globals.YMax + Globals.YMin) / 2 + Globals.Rng.Next(-25, 25));
                _paints.Add(newPaint);
            }


                base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new ExtendedSpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("PlaqueFont");
            _newLevelFont = Content.Load<SpriteFont>("NewLevelFont");
            collisionSound = Content.Load<SoundEffect>("Collision");
            _paintSound = Content.Load<SoundEffect>("PaintSound");
            MoveSound = Content.Load<SoundEffect>("Move");
            _themeSong = Content.Load<Song>("Minimalist2");
            _blendedColorSound = Content.Load<SoundEffect>("BlendedColorSound");
            _filledRect = Content.Load<SoundEffect>("FilledRect");

            MediaPlayer.Play(_themeSong);
            MediaPlayer.IsRepeating = true;

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            if (_levelStart)
            {
                for (var n = _paints.Count - 1; n >= 0; n--)
                {
                    _paints[n].Update(_rectangles);
                    MathHelper.Clamp(_paints[n].Position.X, Globals.XMin, Globals.XMax);
                    MathHelper.Clamp(_paints[n].Position.X, Globals.XMin, Globals.XMax);
                    if (!_paints[n].Active) _paints.Remove(_paints[n]);
                }
                if (_currentKeyboardState.IsKeyDown(Keys.Enter))
                {
                    _levelStart = false;
                    _titleScreen = false;
                    _level++;
                    InitializeLevel();
                }
            }
            else if (_levelEnd)
            {
                if (_currentKeyboardState.IsKeyDown(Keys.Enter))
                {
                    _levelEnd = false;
                    InitializeLevel();
                }
            } else
            {
                UpdatePlayer();
                UpdateRectangles();
                CollisionDetection();
                _lines.Update(_player.Position);
                for (var n = _paints.Count - 1; n >= 0; n--)
                {
                    _paints[n].Update(_rectangles);
                    MathHelper.Clamp(_paints[n].Position.X, Globals.XMin, Globals.XMax);
                    MathHelper.Clamp(_paints[n].Position.X, Globals.XMin, Globals.XMax);
                    if (!_paints[n].Active) _paints.Remove(_paints[n]);
                }
                if (_paints.Count == 0)
                {
                    if (_level*50000 > _score)
                    {
                        _levelEnd = true;
                    }
                    else
                    {
                        _levelStart = true;
                    }
                }
            }

            base.Update(gameTime);
        }

        private void InitializeLevel()
        {
            _rectangles = new List<PaintedRectangle>();
            _player = new Player();
            _lines = new Lines();
            _player.Initialize(Globals.XMin, Globals.YMin);
            _lines.Initialize();
            _paints = new List<Paint>();
            
            _score = 0;

            for (var n = 0; n < _level; n++)
            {
                var newPaint = new Paint();
                newPaint.Initialize((Globals.XMax + Globals.XMin) / 2 + Globals.Rng.Next(-125, 125), (Globals.YMax + Globals.YMin) / 2 + Globals.Rng.Next(-125, 125));
                _paints.Add(newPaint);
            }
        }

        /// <summary>
        /// Check if paint hits the line
        /// </summary>
        private void CollisionDetection()
        {
            foreach (var paint in _paints)
            {
                foreach (var spot in _lines.FilledRects)
                {
                    if (spot.Intersects(paint.Rect) && spot.X+2 != Globals.XMax && spot.X+2 != Globals.XMin && spot.Y+2 != Globals.YMax && spot.Y+2 != Globals.YMin)
                    {
                        bool collision = true;
                        foreach (var rect in _rectangles)
                        {
                            if (spot.Intersects(rect.Rect))
                            {
                                collision = false;
                            }
                        }

                        if (collision)
                        {
                            _score -= 1000;
                            collisionSound.Play();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If there is a new rectangle, draw it and capture any paints it got
        /// </summary>
        private void UpdateRectangles()
        {
            if (_player.HasNewRectangle)
            {
                var color = Color.White;
                _filledRect.Play();

                //Check if any paints intersect the new rectangle
                foreach (var paint in _paints)
                {

                    if (paint.Rect.Intersects(_player.NewRectangle))
                    {
                        paint.Active = false;
                        if (color == Color.White)
                        {
                            color = paint.PaintColor;
                            _score += paint.Score;
                            _paintSound.Play();
                        }
                        else
                        {
                            _blendedColorSound.Play();
                        }

                    }
                }

                _rectangles.Add(new PaintedRectangle(_player.NewRectangle, color));
                _player.HasNewRectangle = false;
            }
        }

        public static Color Blend(Color color, Color backColor, double amount)
        {
            byte r = (byte)((color.R * amount) + backColor.R * (1 - amount));
            byte g = (byte)((color.G * amount) + backColor.G * (1 - amount));
            byte b = (byte)((color.B * amount) + backColor.B * (1 - amount));
            return new Color(r, g, b);
        }

        private void UpdatePlayer()
        {
            _player.Update(_currentKeyboardState, _previousKeyboardState);
            _player.Position.X = MathHelper.Clamp(_player.Position.X, Globals.XMin, Globals.XMax);
            _player.Position.Y = MathHelper.Clamp(_player.Position.Y, Globals.YMin, Globals.YMax);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);

            spriteBatch.Begin();

            if (_titleScreen)
            {
                spriteBatch.FillRectangle(new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                spriteBatch.DrawString(_spriteFont, "You are Piet Mondrian, minimalist extrodinaire, \n and today you will paint your masterpiece! Your \n goal is to create a painting worth " +
                                       "$1,000,000. \n To do this, you have prepared a series of beautiful \n and carefully crafted colors. You must separate \n each of these colors into a separate " +
                                       "compartment \n on your canvas.  Each time you mix multiple colors \n in a box, you lose potential value. Be careful! \n As you improve, " +
                                                    "you can create more and more \n complex paintings. Good luck!", new Vector2(220, 100), Color.Black);

                spriteBatch.DrawString(_newLevelFont, "Press <ENTER> to begin",
                                       new Vector2(180, GraphicsDevice.Viewport.Height/3*2), Color.Black);
                foreach (var paint in _paints)
                {
                    paint.Draw(spriteBatch);
                }
            }
            else
            {

                DrawScore();

                spriteBatch.FillRectangle(
                    new Rectangle(Globals.XMin, Globals.YMin, (Globals.XMax - Globals.XMin),
                                  (Globals.YMax - Globals.YMin)), Color.LightGray);
                spriteBatch.DrawLine(new Vector2(Globals.XMin, Globals.YMin), new Vector2(Globals.XMin, Globals.YMax),
                                     Color.Black);
                spriteBatch.DrawLine(new Vector2(Globals.XMin, Globals.YMin), new Vector2(Globals.XMax, Globals.YMin),
                                     Color.Black);
                spriteBatch.DrawLine(new Vector2(Globals.XMax, Globals.YMin), new Vector2(Globals.XMax, Globals.YMax),
                                     Color.Black);
                spriteBatch.DrawLine(new Vector2(Globals.XMin, Globals.YMax), new Vector2(Globals.XMax, Globals.YMax),
                                     Color.Black);
                DrawRectangles(spriteBatch);
                _lines.Draw(spriteBatch);
                foreach (var paint in _paints)
                {
                    paint.Draw(spriteBatch);
                }
                _player.Draw(spriteBatch);

                if (_levelEnd)
                {
                    //  spriteBatch.FillRectangle(new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                    spriteBatch.DrawString(_newLevelFont,
                                           "No good!",
                                           new Vector2(GraphicsDevice.Viewport.Width/2 - 100,
                                                       GraphicsDevice.Viewport.Height/2 - 50), Color.Black);
                    spriteBatch.DrawString(_newLevelFont,
                                           "You need to separate out more colors.",
                                           new Vector2(GraphicsDevice.Viewport.Width / 2 - 380,
                                                       GraphicsDevice.Viewport.Height / 2 - 10), Color.Black);
                    spriteBatch.DrawString(_newLevelFont, "Press <ENTER> to retry",
                                           new Vector2(GraphicsDevice.Viewport.Width/2 - 250,
                                                       GraphicsDevice.Viewport.Height/2 + 30), Color.Black);

                }

                if (_levelStart)
                {
                    //  spriteBatch.FillRectangle(new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                    if (_score > 1000000)
                    {
                        spriteBatch.DrawString(_newLevelFont, "Congratulations!",
                                               new Vector2(GraphicsDevice.Viewport.Width/2 - 180,
                                                           GraphicsDevice.Viewport.Height/2 - 150), Color.Black);
                        spriteBatch.DrawString(_newLevelFont, "You made a painting worth $" + (_score.ToString("#,##0")),
                                               new Vector2(30,
                                                           GraphicsDevice.Viewport.Height/2 - 100), Color.Black);
                    }
                    spriteBatch.DrawString(_newLevelFont, "Opus #" + (_level + 1),
                                           new Vector2(GraphicsDevice.Viewport.Width/2 - 80,
                                                       GraphicsDevice.Viewport.Height/2 - 50), Color.Black);
                    spriteBatch.DrawString(_newLevelFont, "Press <ENTER> to begin",
                                           new Vector2(GraphicsDevice.Viewport.Width/2 - 250,
                                                       GraphicsDevice.Viewport.Height/2 + 30), Color.Black);

                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
        
        /// <summary>
        /// Draw the score and level on a plaque beneath the painting
        /// </summary>
        private void DrawScore()
        {
            spriteBatch.FillRectangle(
                new Rectangle(GraphicsDevice.Viewport.Width/2 - 75, GraphicsDevice.Viewport.Height - 75, 150, 60), Color.Gold);
            spriteBatch.DrawRectangle(
                new Rectangle(GraphicsDevice.Viewport.Width/2 - 75, GraphicsDevice.Viewport.Height - 75, 150, 60),
                Color.RosyBrown);
            spriteBatch.DrawString(_spriteFont, "Opus #" + _level.ToString(), new Vector2(GraphicsDevice.Viewport.Width/2-30, GraphicsDevice.Viewport.Height-70), Color.Black);
            var score = _score.ToString("#,##0");
            spriteBatch.DrawString(_spriteFont, "$" + score, new Vector2(GraphicsDevice.Viewport.Width / 2 - 35, GraphicsDevice.Viewport.Height - 45), Color.Black);
        }

        private void DrawRectangles(ExtendedSpriteBatch spriteBatch)
        {
            foreach (var rect in _rectangles.Reverse<PaintedRectangle>())
            {
                spriteBatch.FillRectangle(rect.Rect, rect.RectColor);
            }
        }
    }
}
