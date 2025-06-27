using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GraphEditor
{
    public class HexagonShape
    {
        public Rectangle Rect { get; set; }
        public float Rotation { get; set; } = 0;
        public Color Color { get; set; } = Color.Black;
        public Color FillColor { get; set; } = Color.Transparent;
        public bool IsFilled { get; set; } = false;
        public HexagonShape(Rectangle rect)
        {
            Rect = rect;
        }

        public static Point[] CalculateHexagonPoints(Rectangle bounds)
        {
            Point[] points = new Point[6];

            int centerX = bounds.X + bounds.Width / 2;
            int centerY = bounds.Y + bounds.Height / 2;
            int radiusX = bounds.Width / 2;
            int radiusY = bounds.Height / 2;

            for (int i = 0; i < 6; i++)
            {
                double angle = Math.PI / 3 * i; // 60 градусов
                points[i] = new Point(
                    centerX + (int)(radiusX * Math.Cos(angle)),
                    centerY + (int)(radiusY * Math.Sin(angle))
                );
            }

            return points;
        }
        public void Draw(Graphics g, bool isSelected = false)
        {
            Point[] points = CalculateHexagonPoints(Rect);

            // Заливка
            if (FillColor != Color.Transparent)
            {
                using (Brush brush = new SolidBrush(FillColor))
                {
                    g.FillPolygon(brush, points);
                }
            }

            // Контур
            using (Pen pen = new Pen(Color))
            {
                g.DrawPolygon(pen, points);
            }

            g.ResetTransform();
  

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
                path.AddPolygon(GetHexagonPoints());
                using (Matrix m = new Matrix())
                {
                    m.RotateAt(Rotation, new PointF(Rect.X + Rect.Width / 2f, Rect.Y + Rect.Height / 2f));
                    path.Transform(m);
                }
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

        private PointF[] GetHexagonPoints()
        {
            PointF[] points = new PointF[6];
            float centerX = Rect.X + Rect.Width / 2f;
            float centerY = Rect.Y + Rect.Height / 2f;
            float radiusX = Rect.Width / 2f;
            float radiusY = Rect.Height / 2f;

            for (int i = 0; i < 6; i++)
            {
                float angle = (float)(Math.PI / 3 * i); // 60 градусов между вершинами
                points[i] = new PointF(
                    centerX + radiusX * (float)Math.Cos(angle),
                    centerY + radiusY * (float)Math.Sin(angle));
            }

            return points;
        }
    }
}