using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GraphEditor
{
    internal class ClosedPolylineShape
    {
        public List<Point> Points { get; set; }
        public Color Color { get; set; } = Color.Black;
        public Color FillColor { get; set; } = Color.Transparent;
        public float Rotation { get; set; } = 0;
        public Rectangle Rect { get; set; }

        public ClosedPolylineShape(List<Point> points)
        {
            Points = new List<Point>(points);
            Rect = GetBoundingBox(points);
        }

        public void Draw(Graphics g, bool isSelected)
        {
            using (Pen pen = new Pen(Color, 2))
            using (Brush brush = new SolidBrush(FillColor))
            {
                Point[] transformed = GetTransformedPoints();
                if (FillColor != Color.Transparent)
                    g.FillPolygon(brush, transformed);
                g.DrawPolygon(pen, transformed);

                if (isSelected)
                {
                    foreach (var pt in transformed)
                        g.FillRectangle(Brushes.Red, pt.X - 3, pt.Y - 3, 6, 6);
                }
            }
        }

        public bool Contains(Point point)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddPolygon(GetTransformedPoints());
                using (var region = new Region(path))
                    return region.IsVisible(point);
            }
        }

        public List<Rectangle> GetHandles()
        {
            var list = new List<Rectangle>();
            foreach (var p in GetTransformedPoints())
                list.Add(new Rectangle(p.X - 3, p.Y - 3, 6, 6));
            return list;
        }

        public void MovePoint(int index, Point newPosition)
        {
            if (index >= 0 && index < Points.Count)
            {
                Points[index] = newPosition;
                Rect = GetBoundingBox(Points);
            }
        }

        private Rectangle GetBoundingBox(List<Point> points)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var p in points)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        private Point[] GetTransformedPoints()
        {
            if (Rotation == 0) return Points.ToArray();

            List<Point> transformed = new List<Point>();
            float cx = Rect.X + Rect.Width / 2f;
            float cy = Rect.Y + Rect.Height / 2f;
            double angleRad = Rotation * Math.PI / 180;

            foreach (var pt in Points)
            {
                float dx = pt.X - cx;
                float dy = pt.Y - cy;
                int xNew = (int)(cx + dx * Math.Cos(angleRad) - dy * Math.Sin(angleRad));
                int yNew = (int)(cy + dx * Math.Sin(angleRad) + dy * Math.Cos(angleRad));
                transformed.Add(new Point(xNew, yNew));
            }

            return transformed.ToArray();
        }
    }
}
