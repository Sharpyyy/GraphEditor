using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GraphEditor
{
    public class PolylineShape
    {
        public List<Point> Points { get; set; }
        public Rectangle Rect { get; set; }
        public float Rotation { get; set; } = 0;
        public Color Color { get; set; } = Color.Black;
        public Color FillColor { get; set; } = Color.Transparent;

        public PolylineShape(List<Point> points)
        {
            Points = new List<Point>(points);
            UpdateRect();
        }

        private void UpdateRect()
        {
            if (Points.Count == 0) return;
            int minX = Points[0].X, maxX = Points[0].X;
            int minY = Points[0].Y, maxY = Points[0].Y;
            foreach (var p in Points)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }
            Rect = new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public void Draw(Graphics g, bool isSelected = false)
        {
            if (Points.Count < 2) return;

            using (Matrix m = new Matrix())
            {
                m.RotateAt(Rotation, new PointF(Rect.X + Rect.Width / 2f, Rect.Y + Rect.Height / 2f));
                g.Transform = m;

                if (FillColor != Color.Transparent && Points.Count >= 3)
                {
                    using (Brush fillBrush = new SolidBrush(FillColor))
                    {
                        g.FillPolygon(fillBrush, Points.ToArray());
                    }
                }

                using (Pen pen = new Pen(Color, isSelected ? 3 : 1))
                {
                    g.DrawLines(pen, Points.ToArray());
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
            if (Points.Count < 2) return false; // Учитываем случай с одной точкой
            using (GraphicsPath path = new GraphicsPath())
            {
                if (Points.Count >= 3)
                {
                    path.AddPolygon(Points.ToArray()); // Для замкнутых полилиний
                }
                else
                {
                    path.AddLines(Points.ToArray()); // Для незамкнутых полилиний
                }
                using (Matrix m = new Matrix())
                {
                    m.RotateAt(Rotation, new PointF(Rect.X + Rect.Width / 2f, Rect.Y + Rect.Height / 2f));
                    path.Transform(m);
                }
                // Проверяем попадание в область фигуры или близость к линиям
                using (Pen pen = new Pen(Color, 5)) // Увеличиваем толщину для упрощения выделения
                {
                    return path.IsVisible(p) || path.IsOutlineVisible(p, pen);
                }
            }
        }

        public List<Rectangle> GetHandles()
        {
            List<Rectangle> handles = new List<Rectangle>();
            int size = 8;
            int half = size / 2;

            foreach (var pt in Points)
            {
                handles.Add(new Rectangle(pt.X - half, pt.Y - half, size, size));
            }

            return handles;
        }

        public void MovePoint(int index, Point newLocation)
        {
            if (index >= 0 && index < Points.Count)
            {
                Points[index] = newLocation;
                UpdateRect();
            }
        }


    }
}