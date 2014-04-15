using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Minimalism
{
    class Lines
    {
        private List<Vector2> _filledSpots;
        private int _width;
        private int _height;
        public List<Rectangle> FilledRects; 

        public void Initialize()
        {
            _filledSpots = new List<Vector2>();
            FilledRects = new List<Rectangle>();
            _width = 5;
            _height = 5;
        }

        public void Update(Vector2 playerPosition)
        {
            if (!_filledSpots.Contains(playerPosition))
            {
                _filledSpots.Add(playerPosition);
                FilledRects.Add(new Rectangle((int)playerPosition.X - _width / 2, (int)playerPosition.Y - _height / 2, _width, _height));
            }
        }

        public void Draw(ExtendedSpriteBatch spriteBatch)
        {
            foreach (var rect in FilledRects)
            {
                spriteBatch.FillRectangle(rect, Color.Black );
            }
        }
    }
}
