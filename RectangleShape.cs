using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Form1
{
    public class RectangleShape
    {
        public Rectangle Rect { get; set; }
        public float Rotation { get; set; } = 0;
        public Color Color { get; set; } = Color.Black;

        public float RotationAngle = 0f;

        public bool IsFilled { get; set; } = false;
        public Color FillColor { get; set; } = Color.Transparent;

        public RectangleShape(Rectangle rect)
        {
            Rect = rect;
        }

        public List<Rectangle> GetHandles()
        {
            List<Rectangle> handles = new List<Rectangle>();

            int size = 8; // размер точек
            int half = size / 2;

            // Центры углов
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

        public void Draw(Graphics g, bool isSelected = false)
        {
            using (Matrix m = new Matrix())
            {
                m.RotateAt(Rotation, new PointF(Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2));
                g.Transform = m;
            }

            // Заливка
            if (FillColor != Color.Transparent)
            {
                using (Brush brush = new SolidBrush(FillColor))
                {
                    g.FillRectangle(brush, Rect);
                }
            }

            // Контур
            using (Pen pen = new Pen(Color))
            {
                g.DrawRectangle(pen, Rect);
            }

            g.ResetTransform();

            // Хэндлы
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
                path.AddRectangle(Rect);
                using (Matrix m = new Matrix())
                {
                    m.RotateAt(Rotation, new PointF(Rect.X + Rect.Width / 2f, Rect.Y + Rect.Height / 2f));
                    path.Transform(m);
                }
                return path.IsVisible(p);
            }
        }

        public GraphicsPath GetTransformedPath()
        {
            var path = new GraphicsPath();
            path.AddRectangle(Rect);
            var matrix = new Matrix();
            matrix.RotateAt(RotationAngle, new PointF(Rect.X + Rect.Width / 2f, Rect.Y + Rect.Height / 2f));
            path.Transform(matrix);
            return path;
        }

    }
}
