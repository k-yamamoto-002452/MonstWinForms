using System.Drawing;
using System.Drawing.Drawing2D;

namespace MonstWinForms
{
    public static class ImageHelper
    {
        public static void DrawCircleImage(Graphics g, Image image, float x, float y, float size)
        {
            GraphicsState state = g.Save();

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(x, y, size, size);
                g.SetClip(path);
                g.DrawImage(image, x, y, size, size);
            }

            g.Restore(state);
        }
    }
}