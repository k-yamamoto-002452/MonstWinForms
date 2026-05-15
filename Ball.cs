using System.Drawing;

namespace MonstWinForms
{
    public class Ball
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public Color Color;
        public ComboType ComboType;
        private readonly float friction;

        public Ball(float x, float y, float size, Color color, ComboType comboType)
        {
            Position = new Vector2(x, y);
            Velocity = new Vector2(0f, 0f);
            Size = size;
            Color = color;
            ComboType = comboType;
            friction = 0.985f;
        }

        public void Update(int width, int height)
        {
            Position += Velocity;
            Velocity *= friction;

            if (Velocity.Length() < 0.05f)
            {
                Velocity = new Vector2(0f, 0f);
            }

            Wall(width, height);
        }

        private void Wall(int width, int height)
        {
            float half = Size / 2f;

            if (Position.X - half < 0f)
            {
                Position.X = half;
                Velocity.X *= -1f;
            }

            if (Position.X + half > width)
            {
                Position.X = width - half;
                Velocity.X *= -1f;
            }

            if (Position.Y - half < 0f)
            {
                Position.Y = half;
                Velocity.Y *= -1f;
            }

            if (Position.Y + half > height)
            {
                Position.Y = height - half;
                Velocity.Y *= -1f;
            }
        }

        public void Draw(Graphics g, bool active, int number)
        {
            using (Brush brush = new SolidBrush(Color))
            {
                g.FillEllipse(
                    brush,
                    Position.X - Size / 2f,
                    Position.Y - Size / 2f,
                    Size,
                    Size
                );
            }

            if (active)
            {
                using (Pen pen = new Pen(Color.White, 4f))
                {
                    g.DrawEllipse(
                        pen,
                        Position.X - Size / 2f - 4f,
                        Position.Y - Size / 2f - 4f,
                        Size + 8f,
                        Size + 8f
                    );
                }
            }

            using (Brush brush = new SolidBrush(Color.White))
            using (Font font = new Font("Yu Gothic UI", 13f, FontStyle.Bold))
            {
                g.DrawString(number.ToString(), font, brush, Position.X - 7f, Position.Y - 11f);
            }
        }
    }
}