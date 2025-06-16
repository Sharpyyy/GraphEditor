using GraphEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            Ellipse, // ← добавили
            Fill
        }

        Tool currentTool = Tool.None;

        Point startPoint;
        Point endPoint;
        bool isDrawing = false;
        private bool isDragging = false;
        private bool isRotating = false;
        private Point dragStart;

        

     
        List<object> shapes = new List<object>();
        private object selectedShape = null;


 
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


        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentTool == Tool.Rectangle || currentTool == Tool.Ellipse)
            {
                startPoint = e.Location;
                isDrawing = true;
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

                        Color targetColor = bmp.GetPixel(e.X, e.Y);
                        FloodFill(bmp, new Point(e.X, e.Y), targetColor, colorDialog.Color);

                        Graphics g = panel2.CreateGraphics();
                        g.DrawImage(bmp, 0, 0);
                        bmp.Dispose();
                    }
                }
            }

            if (currentTool == Tool.Select)
            {
                // Попытка начать вращение при зажатом Shift
                if (ModifierKeys == Keys.Shift && selectedShape != null)
                {
                    if ((selectedShape is RectangleShape rectShape && rectShape.Contains(e.Location)) ||
                        (selectedShape is EllipseShape ellipseShape && ellipseShape.Contains(e.Location)))
                    {
                        isRotating = true;
                        dragStart = e.Location;
                        return;
                    }
                    if (selectedShape != null)
                    {
                        dragStart = e.Location;
                        isDragging = true;
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

                // Проверка — попали ли в фигуру
                selectedShape = null;
                for (int i = shapes.Count - 1; i >= 0; i--)
                {
                    if (shapes[i] is RectangleShape rect && rect.Contains(e.Location))
                    {
                        selectedShape = rect;
                        break;
                    }
                    else if (shapes[i] is EllipseShape ellipse && ellipse.Contains(e.Location))
                    {
                        selectedShape = ellipse;
                        break;
                    }
                }

                // Если фигура выбрана и нажали мышь — начать перемещение
                if (selectedShape != null)
                {
                    dragStart = e.Location;
                    isDragging = true;
                }

                


                panel2.Invalidate();
            }

        }



        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            // Ресайзинг
            if (isResizing && selectedShape != null)
            {
                if (selectedShape is RectangleShape rectShape)
                {
                    var rect = rectShape.Rect;
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
                    var rect = ellipseShape.Rect;
                    switch (selectedHandleIndex)
                    {
                        case 0: rect = Rectangle.FromLTRB(e.X, e.Y, rect.Right, rect.Bottom); break;
                        case 1: rect = Rectangle.FromLTRB(rect.Left, e.Y, e.X, rect.Bottom); break;
                        case 2: rect = Rectangle.FromLTRB(rect.Left, rect.Top, e.X, e.Y); break;
                        case 3: rect = Rectangle.FromLTRB(e.X, rect.Top, rect.Right, e.Y); break;
                    }
                    ellipseShape.Rect = rect;
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

                dragStart = e.Location;
                panel2.Invalidate();
                return;
            }

            // Вращение
            if (isRotating && selectedShape != null)
            {
                Point center;

                if (selectedShape is RectangleShape rectShape)
                {
                    center = new Point(
                        rectShape.Rect.X + rectShape.Rect.Width / 2,
                        rectShape.Rect.Y + rectShape.Rect.Height / 2);

                    float angle = GetAngle(center, dragStart, e.Location);
                    rectShape.Rotation += angle;
                }
                else if (selectedShape is EllipseShape ellipseShape)
                {
                    center = new Point(
                        ellipseShape.Rect.X + ellipseShape.Rect.Width / 2,
                        ellipseShape.Rect.Y + ellipseShape.Rect.Height / 2);

                    float angle = GetAngle(center, dragStart, e.Location);
                    ellipseShape.Rotation += angle;
                }

                dragStart = e.Location;
                panel2.Invalidate();
                return;
            }
        }





        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            // Завершаем рисование
            if (isDrawing && currentTool == Tool.Rectangle)
            {
                endPoint = e.Location;
                isDrawing = false;
                shapes.Add(new RectangleShape(GetRectangle(startPoint, endPoint)));
                panel2.Invalidate();
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

            if (isDrawing && currentTool == Tool.Ellipse)
            {
                endPoint = e.Location;
                isDrawing = false;
                shapes.Add(new EllipseShape(GetRectangle(startPoint, endPoint)));
                panel2.Invalidate();
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
            }


            if (isDrawing && currentTool == Tool.Rectangle)
            {
                Rectangle tempRect = GetRectangle(startPoint, endPoint);
                using (Pen pen = new Pen(Color.Red, 1))
                {
                    g.DrawRectangle(pen, tempRect);
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


        private float GetAngle(Point center, Point p1, Point p2)
        {
            float dx1 = p1.X - center.X;
            float dy1 = p1.Y - center.Y;
            float dx2 = p2.X - center.X;
            float dy2 = p2.Y - center.Y;

            float angle1 = (float)Math.Atan2(dy1, dx1);
            float angle2 = (float)Math.Atan2(dy2, dx2);

            float angle = (float)((angle2 - angle1) * 180 / Math.PI);
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

        private void FloodFill(Bitmap bmp, Point pt, Color targetColor, Color replacementColor)
        {
            if (targetColor.ToArgb() == replacementColor.ToArgb()) return;
            if (bmp.GetPixel(pt.X, pt.Y).ToArgb() != targetColor.ToArgb()) return;

            Queue<Point> pixels = new Queue<Point>();
            pixels.Enqueue(pt);

            while (pixels.Count > 0)
            {
                Point temp = pixels.Dequeue();
                if (temp.X < 0 || temp.Y < 0 || temp.X >= bmp.Width || temp.Y >= bmp.Height)
                    continue;

                if (bmp.GetPixel(temp.X, temp.Y).ToArgb() != targetColor.ToArgb())
                    continue;

                bmp.SetPixel(temp.X, temp.Y, replacementColor);

                pixels.Enqueue(new Point(temp.X - 1, temp.Y));
                pixels.Enqueue(new Point(temp.X + 1, temp.Y));
                pixels.Enqueue(new Point(temp.X, temp.Y - 1));
                pixels.Enqueue(new Point(temp.X, temp.Y + 1));
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            
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
