using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaySign.Script.Expand
{
    static class DrawExpand
    {
        static Color defaultColor = Color.Orange;
        static Brush defaultBrush = Brushes.Orange;

        static void DrawRect(Graphics graphics, Rectangle rectangle, Color color, int width)
        {
            Pen pen = new Pen(color, width);
            graphics.DrawRectangle(pen, rectangle);
            graphics.Dispose();
        }

        static void DrawRect(Graphics graphics, MRECT rect, Color color, int width)
        {
            Point point = new Point(rect.left, rect.top);
            Size size = new Size(Math.Abs(rect.right - rect.left), Math.Abs(rect.bottom - rect.top));
            DrawRect(graphics, new Rectangle(point, size), color, width);
        }

        static void DrawString(Graphics graphics, Point point, string word, Brush brush, Font font)
        {
            graphics.DrawString(word, font, brush, point);
            graphics.Dispose();
        }

        static public Image DrawRect(this Image image, MRECT rect, Color color, int width)
        {
            Graphics graphics = Graphics.FromImage(image);
            DrawRect(graphics, rect, color, width);
            return image;
        }

        static public Image DrawRect(this Image image, MRECT rect)
        {
            return DrawRect(image, rect, defaultColor, image.Height / 300 + image.Width / 300);
        }

        static public Image DrawString(this Image image, MRECT rect, string word)
        {
            return DrawString(image, new Point(rect.left, rect.bottom), word, defaultBrush, new Font("微软雅黑", 14));
        }

        static public Image DrawString(this Image image, Point point, string word, Brush brush, Font font)
        {
            Graphics graphics = Graphics.FromImage(image);
            DrawString(graphics, point, word, defaultBrush, font);
            return image;
        }

        static public Bitmap DrawRect(this Bitmap bitmap, MRECT rect, Color color, int width)
        {
            Graphics graphics = Graphics.FromImage(bitmap);
            DrawRect(graphics, rect, color, width);
            return bitmap;
        }

        static public Bitmap DrawRect(this Bitmap bitmap, MRECT rect)
        {
            return DrawRect(bitmap, rect, defaultColor, bitmap.Height / 300 + bitmap.Width / 300);
        }

        static public Bitmap DrawString(this Bitmap bitmap, MRECT rect, string word)
        {
            return DrawString(bitmap, new Point(rect.left, rect.bottom), word, defaultBrush, new Font("微软雅黑", 14));
        }

        static public Bitmap DrawString(this Bitmap bitmap, Point point, string word, Brush brush, Font font)
        {
            Graphics graphics = Graphics.FromImage(bitmap);
            DrawString(graphics, point, word, brush, font);
            return bitmap;
        }


    }
}
