using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

public class BezierShape
{
    public List<Point> Points { get; set; } = new List<Point>();
    public Color Color { get; set; } = Color.Black;
    public Color FillColor { get; set; } = Color.Transparent;
    public float Rotation { get; set; } = 0;
    public bool IsFilled { get; set; } = false;

    private Rectangle _rect;

    public Rectangle Rect
    {
        get => _rect;
        private set => _rect = value;
    }

    public BezierShape(List<Point> points)
    {
        Points = points;
    }

    public void Draw(Graphics g, bool isSelected)
    {
        if (Points.Count < 4) return;

        using (Pen pen = new Pen(Color, 2))
        {
            GraphicsPath path = new GraphicsPath();

            for (int i = 0; i <= Points.Count - 4; i += 3)
            {
                path.AddBezier(Points[i], Points[i + 1], Points[i + 2], Points[i + 3]);
            }

            if (Rotation != 0)
            {
                var bounds = path.GetBounds();
                Matrix matrix = new Matrix();
                matrix.RotateAt(Rotation, new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2));
                path.Transform(matrix);
            }

            g.DrawPath(pen, path);

            if (isSelected)
            {
                foreach (var pt in Points)
                {
                    Rectangle handle = new Rectangle(pt.X - 4, pt.Y - 4, 8, 8);
                    g.FillRectangle(Brushes.White, handle);
                    g.DrawRectangle(Pens.Black, handle);
                }
            }
        }
    }

    public bool Contains(Point p)
    {
        using (GraphicsPath path = new GraphicsPath())
        {
            for (int i = 0; i <= Points.Count - 4; i += 3)
            {
                path.AddBezier(Points[i], Points[i + 1], Points[i + 2], Points[i + 3]);
            }

            using (Pen pen = new Pen(Color, 6)) // "зона попадания"
            {
                return path.IsOutlineVisible(p, pen);
            }
        }
    }

    public void RecalculateRect()
    {
        if (Points.Count == 0) return;

        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var p in Points)
        {
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }

        Rect = Rectangle.FromLTRB(minX, minY, maxX, maxY);
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
            RecalculateRect();
        }
    }
}
