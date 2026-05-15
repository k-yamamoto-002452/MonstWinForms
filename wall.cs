using System.Drawing;

namespace MonstWinForms
{
    public class Wall
    {
        public RectangleF Rect;
        public Color Color;

        public Wall(float x, float y, float width, float height)
        {
            Rect = new RectangleF(x, y, width, height);
            Color = Color.FromArgb(0, 255, 255);
        }

        public void Draw(Graphics g)
        {
            using (Brush brush = new SolidBrush(Color))
            {
                g.FillRectangle(brush, Rect);
            }

            using (Pen pen = new Pen(Color.Cyan, 3f))
            {
                g.DrawRectangle(
                    pen,
                    Rect.X,
                    Rect.Y,
                    Rect.Width,
                    Rect.Height
                );
            }
        }
    }
}
