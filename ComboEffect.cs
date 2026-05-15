using System;
using System.Drawing;

namespace MonstWinForms
{
    public enum ComboType
    {
        Explosion,
        CrossLaser,
        Homing
    }

    public class ComboEffect
    {
        public Vector2 Position;
        public Vector2 Target;
        public ComboType Type;
        public int Life;
        private readonly int maxLife;

        public ComboEffect(Vector2 position, ComboType type)
        {
            Position = position;
            Target = position;
            Type = type;
            Life = 24;
            maxLife = Life;
        }

        public ComboEffect(Vector2 position, Vector2 target, ComboType type)
        {
            Position = position;
            Target = target;
            Type = type;
            Life = 24;
            maxLife = Life;
        }

        public void Update()
        {
            Life--;
        }

        public void Draw(Graphics g)
        {
            if (Type == ComboType.Explosion)
            {
                DrawExplosion(g);
            }
            else if (Type == ComboType.CrossLaser)
            {
                DrawCrossLaser(g);
            }
            else if (Type == ComboType.Homing)
            {
                DrawHoming(g);
            }
        }

        private void DrawExplosion(Graphics g)
        {
            int alpha = Math.Max(0, 180 * Life / maxLife);
            float rate = 1f - Life / (float)maxLife;
            float radius = 180f * rate;

            using (Pen pen = new Pen(Color.FromArgb(alpha, 120, 220, 255), 5f))
            {
                g.DrawEllipse(
                    pen,
                    Position.X - radius,
                    Position.Y - radius,
                    radius * 2f,
                    radius * 2f
                );
            }
        }

        private void DrawCrossLaser(Graphics g)
        {
            int alpha = Math.Max(0, 220 * Life / maxLife);

            using (Pen pen = new Pen(Color.FromArgb(alpha, 255, 255, 120), 8f))
            {
                g.DrawLine(pen, 0f, Position.Y, 1000f, Position.Y);
                g.DrawLine(pen, Position.X, 0f, Position.X, 700f);
            }
        }

        private void DrawHoming(Graphics g)
        {
            int alpha = Math.Max(0, 230 * Life / maxLife);

            using (Pen pen = new Pen(Color.FromArgb(alpha, 255, 130, 255), 5f))
            {
                g.DrawLine(pen, Position.X, Position.Y, Target.X, Target.Y);
            }

            using (Brush brush = new SolidBrush(Color.FromArgb(alpha, 255, 130, 255)))
            {
                g.FillEllipse(brush, Target.X - 10f, Target.Y - 10f, 20f, 20f);
            }
        }
    }
}