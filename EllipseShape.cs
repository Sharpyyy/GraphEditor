using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GraphEditor
{
    public class EllipseShape
    {
        public Rectangle Rect { get; set; }
        public float Rotation { get; set; } = 0;
        public Color Color { get; set; } = Color.Black;
        public Color FillColor { get; set; } = Color.Transparent;
        public bool IsFilled { get; set; } = false;
        public EllipseShape(Rectangle rect)
        {
            Rect = rect;
        }

        public void Draw(Graphics g, bool isSelected = false)
        {
            using (Matrix m = new Matrix())
            {
                m.RotateAt(Rotation, new PointF(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2));
                g.Transform = m;
                if (FillColor != Color.Transparent)
                {
                    using (Brush brush = new SolidBrush(FillColor))
                    {
                        g.FillEllipse(brush, Rect);
                    }
                }

                using (Pen pen = new Pen(Color, isSelected ? 3 : 1))
                {
                    g.DrawEllipse(pen, Rect);
                }

                g.ResetTransform();
            }

            if (isSelected)
            {
                foreach (var handle in GetHandles())
                {
                    g.FillRectangle(Brushes.White, handle);
                    g.DrawRectangle(Pens.Black, handle);
                }
            }
        }

        public bool Contains(Point p)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(Rect);
                return path.IsVisible(p);
            }
        }

        public List<Rectangle> GetHandles()
        {
            List<Rectangle> handles = new List<Rectangle>();
            int size = 8;
            int half = size / 2;

            Point[] points = new Point[]
            {
                new Point(Rect.Left, Rect.Top),
                new Point(Rect.Right, Rect.Top),
                new Point(Rect.Right, Rect.Bottom),
                new Point(Rect.Left, Rect.Bottom)
            };

            foreach (var p in points)
            {
                handles.Add(new Rectangle(p.X - half, p.Y - half, size, size));
            }

            return handles;
        }
    }
}
