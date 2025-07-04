﻿using GraphEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Form1

{
    public partial class Form1 : Form
    {
        enum Tool
        {
            None,
            Rectangle,
            Select,
            Ellipse,
            Hexagon,// ← добавили
            Fill,
            Polyline,
            Bezier,
            ClosedPolyline
        }

        Tool currentTool = Tool.None;

        Point startPoint;
        Point endPoint;
        private List<Point> polylinePoints = new List<Point>();
        private bool isDrawingPolyline = false;
        bool isDrawing = false;
        private bool isDragging = false;
        private bool isRotating = false;
        private Point dragStart;




        List<object> shapes = new List<object>();
        private object selectedShape = null;

        private List<Point> bezierPoints = new List<Point>();
        private bool isDrawingBezier = false;


        private int selectedHandleIndex = -1;
        private bool isResizing = false;

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true; // позволяет форме ловить клавиши
            this.KeyDown += Form1_KeyDown;
        }

        private Rectangle GetRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p2.X - p1.X),
                Math.Abs(p2.Y - p1.Y));
        }

        private void buttonEllipse_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Ellipse;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Rectangle;
        }


        private void buttonHexagon_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Hexagon;
        }

        private void buttonPolyline_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Polyline;
            polylinePoints.Clear();
            isDrawingPolyline = false;
        }

        private void buttonBezier_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Bezier;
            bezierPoints.Clear();
            isDrawingBezier = false;
        }

        private void buttonClosedPolyline_Click(object sender, EventArgs e)
        {
            currentTool = Tool.ClosedPolyline;
            polylinePoints.Clear();
            isDrawingPolyline = false;
        }



        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentTool == Tool.Rectangle || currentTool == Tool.Ellipse || currentTool == Tool.Hexagon)
            {
                // если клик попал по уже существующей фигуре — не начинать рисование
                foreach (var shape in shapes)
                {
                    if (shape is RectangleShape rect && rect.Contains(e.Location))
                        return;
                    if (shape is EllipseShape ellipse && ellipse.Contains(e.Location))
                        return;
                    if (shape is HexagonShape hex && hex.Contains(e.Location))
                        return;
                    if (shape is PolylineShape poly && poly.Contains(e.Location))
                        return;
                    if (shape is BezierShape bezier && bezier.Contains(e.Location))
                        return;
                    if (shape is ClosedPolylineShape closed && closed.Contains(e.Location))
                        return;
                }

                startPoint = e.Location;
                isDrawing = true;
                return;
            }

            if (currentTool == Tool.ClosedPolyline)
            {
                if (e.Button == MouseButtons.Right && polylinePoints.Count >= 3)
                {
                    shapes.Add(new ClosedPolylineShape(polylinePoints));
                    polylinePoints.Clear();
                    isDrawingPolyline = false;
                    panel2.Invalidate();
                    return;
                }
                else if (e.Button == MouseButtons.Left)
                {
                    if (!isDrawingPolyline)
                    {
                        polylinePoints.Clear();
                        isDrawingPolyline = true;
                    }

                    polylinePoints.Add(e.Location);
                    panel2.Invalidate();
                    return;
                }
            }



            if (currentTool == Tool.Polyline)
            {
                if (e.Button == MouseButtons.Right) // ПКМ для автоматической ломаной
                {
                    if (polylinePoints.Count >= 2)
                    {
                        shapes.Add(new PolylineShape(polylinePoints));
                        polylinePoints.Clear();
                        isDrawingPolyline = false;
                        panel2.Invalidate();
                    }
                    return;
                }
                else if (e.Button == MouseButtons.Left) // ЛКМ для ручного рисования
                {
                    if (!isDrawingPolyline)
                    {
                        polylinePoints.Clear();
                        polylinePoints.Add(e.Location);
                        isDrawingPolyline = true;
                    }
                    else
                    {
                        polylinePoints.Add(e.Location);
                    }
                    panel2.Invalidate();
                    return;
                }
            }

            if (currentTool == Tool.Bezier)
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Добавление точки
                    bezierPoints.Add(e.Location);
                    isDrawingBezier = true;
                    panel2.Invalidate();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    // Завершение построения при ПКМ
                    if (bezierPoints.Count >= 4 && (bezierPoints.Count - 1) % 3 == 0)
                    {
                        shapes.Add(new BezierShape(new List<Point>(bezierPoints)));
                        bezierPoints.Clear();
                        isDrawingBezier = false;
                        panel2.Invalidate();
                    }
                    else
                    {
                        MessageBox.Show("Для построения кривой Безье нужно 4, 7, 10 и т.д. точек (всегда 1 + 3×N).", "Недостаточно точек", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                return;
            }


            if (currentTool == Tool.Fill)
            {
                using (ColorDialog colorDialog = new ColorDialog())
                {
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bmp = new Bitmap(panel2.Width, panel2.Height);
                        panel2.DrawToBitmap(bmp, panel2.ClientRectangle);

                        Color clickedColor = bmp.GetPixel(e.X, e.Y);
                        Color fillColor = colorDialog.Color;

                        FloodFill(bmp, e.Location, clickedColor, fillColor);
                        panel2.BackgroundImage = bmp;

                        // Обход всех фигур и установка IsFilled
                        foreach (var shape in shapes)
                        {
                            if (shape is RectangleShape rect && rect.Contains(e.Location))
                            {
                                rect.IsFilled = true;
                                break;
                            }
                            else if (shape is EllipseShape ellipse && ellipse.Contains(e.Location))
                            {
                                ellipse.IsFilled = true;
                                break;
                            }
                            else if (shape is HexagonShape hex && hex.Contains(e.Location))
                            {
                                hex.IsFilled = true;
                                break;
                            }
                            else if (shape is PolylineShape polyline && polyline.Contains(e.Location))
                            {
                                polyline.IsFilled = true;
                                break;
                            }
                        }

                        panel2.Invalidate();
                    }
                }
                return;
            }



            if (currentTool == Tool.Select)
            {
                isDrawing = false;
                isDrawingPolyline = false;
                isDrawingBezier = false;
                polylinePoints.Clear();
                bezierPoints.Clear();
            }



            if (currentTool == Tool.Select)
            {
                


                // Попытка начать вращение при зажатом Shift
                if (ModifierKeys == Keys.Shift && selectedShape != null)
                {
                    if ((selectedShape is RectangleShape rectShape && rectShape.Contains(e.Location)) ||
                        (selectedShape is EllipseShape ellipseShape && ellipseShape.Contains(e.Location)) ||
                        (selectedShape is HexagonShape hexagonShape && hexagonShape.Contains(e.Location)))
                    {
                        isRotating = true;
                        dragStart = e.Location;
                        return;
                    }
                }

                // Проверка — попали ли в хэндлы
                if (selectedShape != null)
                {
                    List<Rectangle> handles = null;

                    if (selectedShape is RectangleShape rectShape)
                        handles = rectShape.GetHandles();
                    else if (selectedShape is EllipseShape ellipseShape)
                        handles = ellipseShape.GetHandles();
                    else if (selectedShape is HexagonShape hexagonShape)
                        handles = hexagonShape.GetHandles();
                    else if (selectedShape is PolylineShape polylineShape)
                        handles = polylineShape.GetHandles();
                    else if (selectedShape is BezierShape bezierShape)
                        handles = bezierShape.GetHandles();
                    else if (selectedShape is ClosedPolylineShape closedShape)
                        handles = closedShape.GetHandles();



                    if (handles != null)
                    {
                        for (int i = 0; i < handles.Count; i++)
                        {
                            if (handles[i].Contains(e.Location))
                            {
                                selectedHandleIndex = i;
                                isResizing = true;
                                return;
                            }
                        }
                    }
                }


                selectedShape = null;

              


                // Проверка — попали ли в фигуру
                // Проверка — попали ли в фигуру
                // Проверка — попали ли в фигуру
                selectedShape = null;

                for (int i = shapes.Count - 1; i >= 0; i--)
                {
                    var shape = shapes[i];

                    if (shape is RectangleShape rect && rect.Contains(e.Location) && !rect.IsFilled)
                    {
                        selectedShape = rect;
                        break;
                    }
                    else if (shape is EllipseShape ellipse && ellipse.Contains(e.Location) && !ellipse.IsFilled)
                    {
                        selectedShape = ellipse;
                        break;
                    }
                    else if (shape is HexagonShape hex && hex.Contains(e.Location) && !hex.IsFilled)
                    {
                        selectedShape = hex;
                        break;
                    }
                    else if (shape is PolylineShape poly && poly.Contains(e.Location) && !poly.IsFilled)
                    {
                        selectedShape = poly;
                        break;
                    }
                    else if (shape is BezierShape bezier && bezier.Contains(e.Location) && !bezier.IsFilled)
                    {
                        selectedShape = bezier;
                        break;
                    }
                    else if (shape is ClosedPolylineShape closed && closed.Contains(e.Location))
                    {
                        selectedShape = closed;
                        break;
                    }
                }

                isDrawing = false;
                // Если фигура выбрана и нажали мышь — начать перемещение
                if (selectedShape != null)
                {
                    dragStart = e.Location;
                    isDragging = true;
                }




                panel2.Invalidate();
            }
        }


        public void FloodFill(Bitmap bmp, Point pt, Color targetColor, Color replacementColor)
        {
            if (targetColor.ToArgb() == replacementColor.ToArgb()) return;
            if (pt.X < 0 || pt.Y < 0 || pt.X >= bmp.Width || pt.Y >= bmp.Height) return;

            Queue<Point> pixels = new Queue<Point>();
            pixels.Enqueue(pt);

            while (pixels.Count > 0)
            {
                Point current = pixels.Dequeue();
                if (current.X < 0 || current.Y < 0 || current.X >= bmp.Width || current.Y >= bmp.Height)
                    continue;

                if (bmp.GetPixel(current.X, current.Y).ToArgb() != targetColor.ToArgb())
                    continue;

                bmp.SetPixel(current.X, current.Y, replacementColor);

                pixels.Enqueue(new Point(current.X + 1, current.Y));
                pixels.Enqueue(new Point(current.X - 1, current.Y));
                pixels.Enqueue(new Point(current.X, current.Y + 1));
                pixels.Enqueue(new Point(current.X, current.Y - 1));
            }
        }



        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && (currentTool == Tool.Rectangle || currentTool == Tool.Ellipse || currentTool == Tool.Hexagon))
            {
                endPoint = e.Location;
                panel2.Invalidate(); // перерисовка
                return;
            }

            if (isResizing && selectedShape != null)
            {
                Rectangle rect;
                if (selectedShape is RectangleShape rectShape)
                {
                    rect = rectShape.Rect;
                    switch (selectedHandleIndex)
                    {
                        case 0: rect = Rectangle.FromLTRB(e.X, e.Y, rect.Right, rect.Bottom); break;
                        case 1: rect = Rectangle.FromLTRB(rect.Left, e.Y, e.X, rect.Bottom); break;
                        case 2: rect = Rectangle.FromLTRB(rect.Left, rect.Top, e.X, e.Y); break;
                        case 3: rect = Rectangle.FromLTRB(e.X, rect.Top, rect.Right, e.Y); break;
                    }
                    rectShape.Rect = rect;
                }
                else if (selectedShape is EllipseShape ellipseShape)
                {
                    rect = ellipseShape.Rect;
                    switch (selectedHandleIndex)
                    {
                        case 0: rect = Rectangle.FromLTRB(e.X, e.Y, rect.Right, rect.Bottom); break;
                        case 1: rect = Rectangle.FromLTRB(rect.Left, e.Y, e.X, rect.Bottom); break;
                        case 2: rect = Rectangle.FromLTRB(rect.Left, rect.Top, e.X, e.Y); break;
                        case 3: rect = Rectangle.FromLTRB(e.X, rect.Top, rect.Right, e.Y); break;
                    }
                    ellipseShape.Rect = rect;
                }
                else if (selectedShape is HexagonShape hexagonShape)
                {
                    rect = hexagonShape.Rect;
                    switch (selectedHandleIndex)
                    {
                        case 0: rect = Rectangle.FromLTRB(e.X, e.Y, rect.Right, rect.Bottom); break;
                        case 1: rect = Rectangle.FromLTRB(rect.Left, e.Y, e.X, rect.Bottom); break;
                        case 2: rect = Rectangle.FromLTRB(rect.Left, rect.Top, e.X, e.Y); break;
                        case 3: rect = Rectangle.FromLTRB(e.X, rect.Top, rect.Right, e.Y); break;
                    }
                    if (rect.Width > 0 && rect.Height > 0)
                    {
                        hexagonShape.Rect = rect;
                    }
                }
                
                else if (selectedShape is PolylineShape polylineShape)
                {
                    if (selectedHandleIndex >= 0 && selectedHandleIndex < polylineShape.Points.Count)
                    {
                        polylineShape.MovePoint(selectedHandleIndex, e.Location);
                        panel2.Invalidate();
                        return;
                    }
                }
                else if (selectedShape is BezierShape bezierShape)
                {
                    if (selectedHandleIndex >= 0 && selectedHandleIndex < bezierShape.Points.Count)
                    {
                        bezierShape.MovePoint(selectedHandleIndex, e.Location);
                        panel2.Invalidate();
                        return;
                    }
                }
                else if (selectedShape is ClosedPolylineShape closedShape)
                {
                    if (selectedHandleIndex >= 0 && selectedHandleIndex < closedShape.Points.Count)
                    {
                        closedShape.MovePoint(selectedHandleIndex, e.Location);
                        panel2.Invalidate();
                        return;
                    }
                }




                panel2.Invalidate();
                return;
            }

            // Перемещение
            if (isDragging && selectedShape != null)
            {
                int dx = e.X - dragStart.X;
                int dy = e.Y - dragStart.Y;

                if (selectedShape is RectangleShape rectShape)
                {
                    rectShape.Rect = new Rectangle(
                        rectShape.Rect.X + dx,
                        rectShape.Rect.Y + dy,
                        rectShape.Rect.Width,
                        rectShape.Rect.Height);
                }
                else if (selectedShape is EllipseShape ellipseShape)
                {
                    ellipseShape.Rect = new Rectangle(
                        ellipseShape.Rect.X + dx,
                        ellipseShape.Rect.Y + dy,
                        ellipseShape.Rect.Width,
                        ellipseShape.Rect.Height);
                }
                else if (selectedShape is HexagonShape hexagonShape)
                {
                    hexagonShape.Rect = new Rectangle(
                        hexagonShape.Rect.X + dx,
                        hexagonShape.Rect.Y + dy,
                        hexagonShape.Rect.Width,
                        hexagonShape.Rect.Height);
                }
                else if (selectedShape is PolylineShape polylineShape)
                {
                    List<Point> newPoints = new List<Point>();
                    foreach (var p in polylineShape.Points)
                    {
                        newPoints.Add(new Point(p.X + dx, p.Y + dy));
                    }
                    polylineShape.Points = newPoints;
                    polylineShape.Rect = new Rectangle(
                        polylineShape.Rect.X + dx,
                        polylineShape.Rect.Y + dy,
                        polylineShape.Rect.Width,
                        polylineShape.Rect.Height);
                }
                else if (selectedShape is ClosedPolylineShape closedShape)
                {
                    List<Point> newPoints = new List<Point>();
                    foreach (var p in closedShape.Points)
                    {
                        newPoints.Add(new Point(p.X + dx, p.Y + dy));
                    }
                    closedShape.Points = newPoints;
                    closedShape.Rect = new Rectangle(
                        closedShape.Rect.X + dx,
                        closedShape.Rect.Y + dy,
                        closedShape.Rect.Width,
                        closedShape.Rect.Height);
                }

                dragStart = e.Location;
                panel2.Invalidate();
                return;
            }

            // Вращение
            // Вращение
            if (isRotating && selectedShape != null)
            {
                PointF center;

                if (selectedShape is RectangleShape rectShape)
                {
                    center = new PointF(
                        rectShape.Rect.X + rectShape.Rect.Width / 2f,
                        rectShape.Rect.Y + rectShape.Rect.Height / 2f);

                    float angle = GetAngle(center, dragStart, e.Location);
                    rectShape.Rotation += angle;
                }
                else if (selectedShape is EllipseShape ellipseShape)
                {
                    center = new PointF(
                        ellipseShape.Rect.X + ellipseShape.Rect.Width / 2f,
                        ellipseShape.Rect.Y + ellipseShape.Rect.Height / 2f);

                    float angle = GetAngle(center, dragStart, e.Location);
                    ellipseShape.Rotation += angle;
                }
                else if (selectedShape is HexagonShape hexagonShape)
                {
                    center = new PointF(
                        hexagonShape.Rect.X + hexagonShape.Rect.Width / 2f,
                        hexagonShape.Rect.Y + hexagonShape.Rect.Height / 2f);

                    float angle = GetAngle(center, dragStart, e.Location);
                    hexagonShape.Rotation += angle;
                }
                else if (selectedShape is PolylineShape polylineShape)
                {
                    center = new PointF(
                        polylineShape.Rect.X + polylineShape.Rect.Width / 2f,
                        polylineShape.Rect.Y + polylineShape.Rect.Height / 2f);

                    float angle = GetAngle(center, dragStart, e.Location);
                    polylineShape.Rotation += angle;
                }
                else if (selectedShape is ClosedPolylineShape closedShape)
                {
                    center = new PointF(
                        closedShape.Rect.X + closedShape.Rect.Width / 2f,
                        closedShape.Rect.Y + closedShape.Rect.Height / 2f);

                    float angle = GetAngle(center, dragStart, e.Location);
                    closedShape.Rotation += angle;
                }


                dragStart = e.Location;
                panel2.Invalidate();
                return;
            }
        }





        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            // Завершаем рисование
            if (isDrawing && currentTool == Tool.Rectangle && !isDragging && !isResizing)
            {
                endPoint = e.Location;
                isDrawing = false;
                Rectangle rect = GetRectangle(startPoint, endPoint);
                if (rect.Width > 0 && rect.Height > 0)
                {
                    shapes.Add(new RectangleShape(rect));
                    panel2.Invalidate();
                }
            }
            else if (isDrawing && currentTool == Tool.Ellipse && !isDragging && !isResizing)
            {
                endPoint = e.Location;
                isDrawing = false;
                Rectangle rect = GetRectangle(startPoint, endPoint);
                if (rect.Width > 0 && rect.Height > 0)
                {
                    shapes.Add(new EllipseShape(rect));
                    panel2.Invalidate();
                }
            }
            else if (isDrawing && currentTool == Tool.Hexagon && !isDragging && !isResizing)
            {
                endPoint = e.Location;
                isDrawing = false;
                Rectangle rect = GetRectangle(startPoint, endPoint);
                if (rect.Width > 0 && rect.Height > 0)
                {
                    shapes.Add(new HexagonShape(rect));
                    panel2.Invalidate();
                }
            }
            else if (currentTool == Tool.ClosedPolyline && isDrawingPolyline && e.Clicks == 2 && !isDragging && !isResizing)
            {
                if (polylinePoints.Count >= 3)
                {
                    shapes.Add(new ClosedPolylineShape(polylinePoints));
                    polylinePoints.Clear();
                    isDrawingPolyline = false;
                    panel2.Invalidate();
                }
            }

            else if (currentTool == Tool.Polyline && isDrawingPolyline && !isDragging && !isResizing)
            {
                if (e.Clicks == 2) // Двойной клик завершает ломаную
                {
                    if (polylinePoints.Count >= 2)
                    {
                        shapes.Add(new PolylineShape(polylinePoints));
                        polylinePoints.Clear();
                        isDrawingPolyline = false;
                        panel2.Invalidate();
                    }
                }
            }

            // Завершаем ресайзинг
            if (isResizing)
            {
                isResizing = false;
                selectedHandleIndex = -1;
            }

            // Завершаем перемещение
            if (isDragging)
            {
                isDragging = false;
            }

            // Завершаем вращение
            if (isRotating)
            {
                isRotating = false;
            }

       

        }

        private void buttonChangeColor_Click(object sender, EventArgs e)
        {
            if (selectedShape != null)
            {
                using (ColorDialog colorDialog = new ColorDialog())
                {
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (selectedShape is RectangleShape rect)
                        {
                            rect.Color = colorDialog.Color;
                        }
                        else if (selectedShape is EllipseShape ellipse)
                        {
                            ellipse.Color = colorDialog.Color;
                        }
                        else if (selectedShape is HexagonShape hexagon)
                        {
                            hexagon.Color = colorDialog.Color;
                        }
                        else if (selectedShape is PolylineShape polyline)
                        {
                            polyline.Color = colorDialog.Color;
                        }
                        else if (selectedShape is BezierShape bezier)
{
    bezier.Color = colorDialog.Color;
}
else if (selectedShape is ClosedPolylineShape closed)
{
    closed.Color = colorDialog.Color;
}

                        panel2.Invalidate();
                    }
                }
            }
        }


        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            foreach (var shape in shapes)
            {
                if (shape is RectangleShape rect)
                    rect.Draw(g, shape == selectedShape);
                else if (shape is EllipseShape ellipse)
                    ellipse.Draw(g, shape == selectedShape);
                else if (shape is HexagonShape hexagon)
                    hexagon.Draw(g, shape == selectedShape);
                else if (shape is PolylineShape polyline)
                    polyline.Draw(g, shape == selectedShape);
                else if (shape is BezierShape bezier)
                    bezier.Draw(g, shape == selectedShape);
                else if (shape is ClosedPolylineShape closed)
                    closed.Draw(g, shape == selectedShape);
            }


            if (isDrawing && currentTool == Tool.Rectangle)
            {
                Rectangle tempRect = GetRectangle(startPoint, endPoint);
                using (Pen pen = new Pen(Color.Red, 1))
                    g.DrawRectangle(pen, tempRect);
            }
            else if (isDrawing && currentTool == Tool.Ellipse)
            {
                Rectangle tempRect = GetRectangle(startPoint, endPoint);
                using (Pen pen = new Pen(Color.Red, 1))
                    g.DrawEllipse(pen, tempRect);
            }
            else if (isDrawing && currentTool == Tool.Hexagon)
            {
                Rectangle tempRect = GetRectangle(startPoint, endPoint);
                using (Pen pen = new Pen(Color.Red, 1))
                {
                    Point[] hexPoints = HexagonShape.CalculateHexagonPoints(tempRect); // предполагается метод
                    g.DrawPolygon(pen, hexPoints);
                }


            }
            else if (isDrawingPolyline && currentTool == Tool.ClosedPolyline && polylinePoints.Count >= 2)
            {
                using (Pen pen = new Pen(Color.Orange, 1))
                {
                    g.DrawLines(pen, polylinePoints.ToArray());

                    // Можно нарисовать пунктир от последней точки к первой (для визуального замыкания)
                    if (polylinePoints.Count >= 3)
                    {
                        using (Pen dashedPen = new Pen(Color.Orange, 1))
                        {
                            dashedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                            g.DrawLine(dashedPen, polylinePoints[polylinePoints.Count - 1], polylinePoints[0]);
                        }
                    }
                }

                // Нарисовать точки
                foreach (var pt in polylinePoints)
                {
                    g.FillRectangle(Brushes.Red, pt.X - 2, pt.Y - 2, 4, 4);
                }
            }


            else if (isDrawingPolyline && currentTool == Tool.Polyline && polylinePoints.Count >= 2)
            {
                using (Pen pen = new Pen(Color.Red, 1))
                {
                    g.DrawLines(pen, polylinePoints.ToArray());
                }
            }
            else if (currentTool == Tool.Bezier && bezierPoints.Count >= 4)
            {
                using (Pen pen = new Pen(Color.Blue, 1))
                {
                    for (int i = 0; i <= bezierPoints.Count - 4; i += 3)
                    {
                        g.DrawBezier(pen,
                            bezierPoints[i],
                            bezierPoints[i + 1],
                            bezierPoints[i + 2],
                            bezierPoints[i + 3]);
                    }

                    // Вспомогательные линии (для наглядности)
                    for (int i = 0; i < bezierPoints.Count - 1; i++)
                    {
                        g.DrawLine(Pens.LightGray, bezierPoints[i], bezierPoints[i + 1]);
                    }

                    // Рисуем точки
                    foreach (var pt in bezierPoints)
                    {
                        g.FillRectangle(Brushes.Red, pt.X - 2, pt.Y - 2, 4, 4);
                    }
                }
            }



            if (backgroundImage != null)
            {
                e.Graphics.DrawImage(backgroundImage, 0, 0, panel2.Width, panel2.Height);
            }


        }




        private void ButtonSelect_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Select;
        }

        private void buttonRotate_Click(object sender, EventArgs e)
        {
            if (selectedShape is RectangleShape rect)
                rect.Rotation += 10;
            else if (selectedShape is EllipseShape ellipse)
                ellipse.Rotation += 10;

            panel2.Invalidate();
        }



        private void buttonScale_Click(object sender, EventArgs e)
        {
            if (selectedShape is RectangleShape rect)
            {
                rect.Rect = new Rectangle(
                    rect.Rect.X,
                    rect.Rect.Y,
                    (int)(rect.Rect.Width * 1.1),
                    (int)(rect.Rect.Height * 1.1));
            }
            else if (selectedShape is EllipseShape ellipse)
            {
                ellipse.Rect = new Rectangle(
                    ellipse.Rect.X,
                    ellipse.Rect.Y,
                    (int)(ellipse.Rect.Width * 1.1),
                    (int)(ellipse.Rect.Height * 1.1));
            }

            panel2.Invalidate();
        }



        private void buttonMove_Click(object sender, EventArgs e)
        {
            if (selectedShape is RectangleShape rect)
            {
                rect.Rect = new Rectangle(
                    rect.Rect.X + 10,
                    rect.Rect.Y + 10,
                    rect.Rect.Width,
                    rect.Rect.Height);
            }
            else if (selectedShape is EllipseShape ellipse)
            {
                ellipse.Rect = new Rectangle(
                    ellipse.Rect.X + 10,
                    ellipse.Rect.Y + 10,
                    ellipse.Rect.Width,
                    ellipse.Rect.Height);
            }

            panel2.Invalidate();
        }


        private float GetAngle(PointF center, PointF p1, PointF p2)
        {
            float dx1 = p1.X - center.X;
            float dy1 = p1.Y - center.Y;
            float dx2 = p2.X - center.X;
            float dy2 = p2.Y - center.Y;

            float angle1 = (float)Math.Atan2(dy1, dx1);
            float angle2 = (float)Math.Atan2(dy2, dx2);

            float angle = (float)((angle2 - angle1) * 180 / Math.PI);
            if (angle > 180) angle -= 360;
            else if (angle < -180) angle += 360;
            return angle;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && selectedShape != null)
            {
                shapes.Remove(selectedShape);
                selectedShape = null;
                panel2.Invalidate();
            }
            else if (e.KeyCode == Keys.Enter && currentTool == Tool.Polyline && isDrawingPolyline && polylinePoints.Count >= 2)
            {
                shapes.Add(new PolylineShape(polylinePoints));
                polylinePoints.Clear();
                isDrawingPolyline = false;
                panel2.Invalidate();
            }
            else if (e.KeyCode == Keys.Enter && currentTool == Tool.Bezier && bezierPoints.Count >= 4)
            {
                shapes.Add(new BezierShape(new List<Point>(bezierPoints)));
                bezierPoints.Clear();
                isDrawingBezier = false;
                panel2.Invalidate();
            }
            else if (e.KeyCode == Keys.Enter && currentTool == Tool.ClosedPolyline && isDrawingPolyline && polylinePoints.Count >= 3)
            {
                shapes.Add(new ClosedPolylineShape(polylinePoints));
                polylinePoints.Clear();
                isDrawingPolyline = false;
                panel2.Invalidate();
            }


        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete && selectedShape != null)
            {
                shapes.Remove(selectedShape);
                selectedShape = null;
                panel2.Invalidate();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void buttonFill_Click(object sender, EventArgs e)
        {
            currentTool = Tool.Fill;
        }

        


        private void Form1_Load(object sender, EventArgs e)
        {
            typeof(Panel).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null, panel2, new object[] { true });
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                saveDialog.Title = "Сохранить панель как изображение";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap bmp = new Bitmap(panel2.Width, panel2.Height);
                    panel2.DrawToBitmap(bmp, new Rectangle(0, 0, panel2.Width, panel2.Height));
                    bmp.Save(saveDialog.FileName);
                    bmp.Dispose();
                }
            }
        }

        Image backgroundImage;

        private void загрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif";
                openDialog.Title = "Открыть изображение";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        backgroundImage = Image.FromFile(openDialog.FileName);
                        panel2.Invalidate(); // Перерисовать
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при загрузке изображения:\n" + ex.Message);
                    }
                }
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void оНасToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Это мой графический редактор.\nВерсия 1.0\nРазработчик: Михаил Бутенко", "О нас", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
