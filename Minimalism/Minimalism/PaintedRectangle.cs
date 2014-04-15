using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Minimalism
{
    class PaintedRectangle
    {
        public Rectangle Rect;
        public Color RectColor;

        public PaintedRectangle(Rectangle rectangle, Color color)
        {
            Rect = rectangle;
            RectColor = color;
        }
    }
}
