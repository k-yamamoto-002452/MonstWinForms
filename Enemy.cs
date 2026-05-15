using System.Drawing;

namespace MonstWinForms
{
    public class Enemy
    {
        public Vector2 Position;
        public float Size;
        public int Hp;
        public bool Cool;
        public int CoolCount;
        public bool IsBoss;

        public Enemy(float x, float y, float size, int hp, bool isBoss)
        {
            Position = new Vector2(x, y);
            Size = size;
            Hp = hp;
            Cool = false;
            CoolCount = 0;
            IsBoss = isBoss;
        }

        public void Update()
        {
            if (!Cool)
            {
                return;
            }

            CoolCount--;

            if (CoolCount <= 0)
            {
                Cool = false;
            }
        }

        public void Draw(Graphics g)
        {
            Color enemyColor;

            if (IsBoss)
            {
                enemyColor = Cool ? Color.FromArgb(255, 170, 120) : Color.FromArgb(190, 70, 255);
            }
            else
            {
                enemyColor = Cool ? Color.FromArgb(255, 140, 140) : Color.FromArgb(255, 85, 85);
            }

            using (Brush brush = new SolidBrush(enemyColor))
            {
                g.FillEllipse(
                    brush,
                    Position.X - Size / 2f,
                    Position.Y - Size / 2f,
                    Size,
                    Size
                );
            }

            using (Pen pen = new Pen(Color.White, IsBoss ? 4f : 2f))
            {
                g.DrawEllipse(
                    pen,
                    Position.X - Size / 2f,
                    Position.Y - Size / 2f,
                    Size,
                    Size
                );
            }

            using (Brush brush = new SolidBrush(Color.White))
            using (Font font = new Font("Yu Gothic UI", IsBoss ? 18f : 14f, FontStyle.Bold))
            {
                string text = Hp.ToString();
                SizeF textSize = g.MeasureString(text, font);
                g.DrawString(text, font, brush, Position.X - textSize.Width / 2f, Position.Y - textSize.Height / 2f);
            }

            if (IsBoss)
            {
                using (Brush brush = new SolidBrush(Color.White))
                using (Font font = new Font("Yu Gothic UI", 11f, FontStyle.Bold))
                {
                    g.DrawString("BOSS", font, brush, Position.X - 25f, Position.Y - Size / 2f - 24f);
                }
            }
        }
    }
}