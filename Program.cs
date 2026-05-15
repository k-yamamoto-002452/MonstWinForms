using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MonstWinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameForm());
        }
    }

    public class GameForm : Form
    {
        private readonly System.Windows.Forms.Timer gameTimer;
        private readonly Ball player;
        private readonly List<Enemy> enemies;
        private Vector2 dragStart;
        private Vector2 dragCurrent;
        private bool dragging;

        public GameForm()
        {
            Text = "モンスト風 PC版";
            ClientSize = new Size(1000, 700);
            BackColor = Color.FromArgb(17, 17, 17);
            DoubleBuffered = true;
            KeyPreview = true;

            player = new Ball(ClientSize.Width / 2f, ClientSize.Height - 120f, 48f, Color.FromArgb(77, 166, 255));

            enemies = new List<Enemy>();
            for (int i = 0; i < 5; i++)
            {
                enemies.Add(new Enemy(140f + i * 120f, 220f, 60f));
            }

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            MouseDown += GameForm_MouseDown;
            MouseMove += GameForm_MouseMove;
            MouseUp += GameForm_MouseUp;
            Resize += GameForm_Resize;
        }

        private void GameForm_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            player.Update(ClientSize.Width, ClientSize.Height);
            Collision();
            Invalidate();
        }

        private void GameForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (player.Velocity.Length() > 0.2f)
            {
                return;
            }

            dragging = true;
            dragStart = new Vector2(e.X, e.Y);
            dragCurrent = dragStart;
        }

        private void GameForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging)
            {
                return;
            }

            dragCurrent = new Vector2(e.X, e.Y);
            Invalidate();
        }

        private void GameForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (!dragging)
            {
                return;
            }

            Vector2 pull = dragStart - dragCurrent;
            float distance = pull.Length();

            if (distance > 5f)
            {
                float power = Math.Min(distance / 8f, 22f);
                Vector2 direction = pull.Normalize();
                player.Velocity = direction * power;
            }

            dragging = false;
        }

        private void Collision()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];

                if (Hit(player, enemy) && !enemy.Cool)
                {
                    enemy.Hp--;

                    Vector2 n = GetNormal(player, enemy);
                    player.Position += n * 4f;
                    player.Velocity = Reflect(player.Velocity, n) * 0.9f;

                    enemy.Cool = true;
                    enemy.CoolCount = 12;

                    if (enemy.Hp <= 0)
                    {
                        enemies.RemoveAt(i);
                    }
                }

                enemy.Update();
            }
        }

        private bool Hit(Ball a, Enemy b)
        {
            float ah = a.Size / 2f;
            float bh = b.Size / 2f;

            return a.Position.X - ah < b.Position.X + bh &&
                   a.Position.X + ah > b.Position.X - bh &&
                   a.Position.Y - ah < b.Position.Y + bh &&
                   a.Position.Y + ah > b.Position.Y - bh;
        }

        private Vector2 GetNormal(Ball a, Enemy b)
        {
            float dx = a.Position.X - b.Position.X;
            float dy = a.Position.Y - b.Position.Y;

            float overlapX = (a.Size / 2f + b.Size / 2f) - Math.Abs(dx);
            float overlapY = (a.Size / 2f + b.Size / 2f) - Math.Abs(dy);

            if (overlapX < overlapY)
            {
                return new Vector2(dx > 0 ? 1f : -1f, 0f);
            }

            return new Vector2(0f, dy > 0 ? 1f : -1f);
        }

        private Vector2 Reflect(Vector2 velocity, Vector2 normal)
        {
            float dot = velocity.X * normal.X + velocity.Y * normal.Y;
            return new Vector2(
                velocity.X - 2f * dot * normal.X,
                velocity.Y - 2f * dot * normal.Y
            );
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.Clear(Color.FromArgb(30, 30, 30));

            player.Draw(g);

            foreach (Enemy enemy in enemies)
            {
                enemy.Draw(g);
            }

            DrawArrow(g);
            DrawUi(g);
        }

        private void DrawArrow(Graphics g)
        {
            if (!dragging)
            {
                return;
            }

            Vector2 pull = dragStart - dragCurrent;

            if (pull.Length() < 5f)
            {
                return;
            }

            Vector2 direction = pull.Normalize();
            Vector2 start = player.Position;
            Vector2 end = start + direction * 100f;

            using (Pen pen = new Pen(Color.FromArgb(220, 255, 255, 255), 4f))
            {
                g.DrawLine(pen, start.X, start.Y, end.X, end.Y);
            }

            float angle = (float)Math.Atan2(direction.Y, direction.X);
            PointF p1 = new PointF(end.X, end.Y);
            PointF p2 = new PointF(
                end.X - 14f * (float)Math.Cos(angle - 0.5f),
                end.Y - 14f * (float)Math.Sin(angle - 0.5f)
            );
            PointF p3 = new PointF(
                end.X - 14f * (float)Math.Cos(angle + 0.5f),
                end.Y - 14f * (float)Math.Sin(angle + 0.5f)
            );

            using (Brush brush = new SolidBrush(Color.White))
            {
                g.FillPolygon(brush, new[] { p1, p2, p3 });
            }
        }

        private void DrawUi(Graphics g)
        {
            using (Brush brush = new SolidBrush(Color.White))
            using (Font font = new Font("Yu Gothic UI", 12f))
            {
                g.DrawString("マウスで引っ張って離すと発射します", font, brush, 12f, 12f);
                g.DrawString("敵を倒した数: " + (5 - enemies.Count), font, brush, 12f, 36f);

                if (enemies.Count == 0)
                {
                    using (Font clearFont = new Font("Yu Gothic UI", 36f, FontStyle.Bold))
                    {
                        g.DrawString("CLEAR!", clearFont, brush, ClientSize.Width / 2f - 90f, ClientSize.Height / 2f - 40f);
                    }
                }
            }
        }
    }

    public class Ball
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public Color Color;
        private readonly float friction;

        public Ball(float x, float y, float size, Color color)
        {
            Position = new Vector2(x, y);
            Velocity = new Vector2(0f, 0f);
            Size = size;
            Color = color;
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

        public void Draw(Graphics g)
        {
            using (Brush brush = new SolidBrush(Color))
            {
                g.FillRectangle(
                    brush,
                    Position.X - Size / 2f,
                    Position.Y - Size / 2f,
                    Size,
                    Size
                );
            }
        }
    }

    public class Enemy
    {
        public Vector2 Position;
        public float Size;
        public int Hp;
        public bool Cool;
        public int CoolCount;

        public Enemy(float x, float y, float size)
        {
            Position = new Vector2(x, y);
            Size = size;
            Hp = 5;
            Cool = false;
            CoolCount = 0;
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
            Color enemyColor = Cool ? Color.FromArgb(255, 140, 140) : Color.FromArgb(255, 85, 85);

            using (Brush brush = new SolidBrush(enemyColor))
            {
                g.FillRectangle(
                    brush,
                    Position.X - Size / 2f,
                    Position.Y - Size / 2f,
                    Size,
                    Size
                );
            }

            using (Brush brush = new SolidBrush(Color.White))
            using (Font font = new Font("Yu Gothic UI", 14f, FontStyle.Bold))
            {
                g.DrawString(Hp.ToString(), font, brush, Position.X - 7f, Position.Y - 11f);
            }
        }
    }

    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public Vector2 Normalize()
        {
            float length = Length();

            if (length == 0f)
            {
                return new Vector2(0f, 0f);
            }

            return new Vector2(X / length, Y / length);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator *(Vector2 a, float value)
        {
            return new Vector2(a.X * value, a.Y * value);
        }
    }
}
