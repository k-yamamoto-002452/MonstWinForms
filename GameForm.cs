using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MonstWinForms
{
    public class GameForm : Form
    {
        private readonly System.Windows.Forms.Timer gameTimer;
        private readonly List<Ball> players;
        private readonly List<Enemy> enemies;
        private readonly List<ComboEffect> effects;
        private readonly HashSet<int> comboUsed;
        private readonly List<Wall> walls;
        private Vector2 dragStart;
        private Vector2 dragCurrent;
        private bool dragging;
        private int currentPlayerIndex;
        private int stage;
        private int shotCount;
        private bool shotMoving;
        private bool gameClear;

        public GameForm()
        {
            Text = "モンスト風 PC版";
            ClientSize = new Size(1000, 700);
            BackColor = Color.FromArgb(17, 17, 17);
            DoubleBuffered = true;
            KeyPreview = true;

            players = new List<Ball>();
            enemies = new List<Enemy>();
            effects = new List<ComboEffect>();
            comboUsed = new HashSet<int>();
            walls = new List<Wall>();

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            MouseDown += GameForm_MouseDown;
            MouseMove += GameForm_MouseMove;
            MouseUp += GameForm_MouseUp;
            KeyDown += GameForm_KeyDown;
            Resize += GameForm_Resize;

            Restart();
        }

        private Ball CurrentPlayer
        {
            get
            {
                return players[currentPlayerIndex];
            }
        }

        private void Restart()
        {
            stage = 1;
            shotCount = 0;
            currentPlayerIndex = 0;
            dragging = false;
            shotMoving = false;
            gameClear = false;

            players.Clear();
            players.Add(new Ball(ClientSize.Width / 2f - 90f, ClientSize.Height - 120f, 48f, Color.FromArgb(77, 166, 255), ComboType.Explosion));
            players.Add(new Ball(ClientSize.Width / 2f, ClientSize.Height - 120f, 48f, Color.FromArgb(120, 255, 150), ComboType.CrossLaser));
            players.Add(new Ball(ClientSize.Width / 2f + 90f, ClientSize.Height - 120f, 48f, Color.FromArgb(255, 210, 80), ComboType.Homing));

            LoadStage();
            Invalidate();
        }

        private void LoadStage()
        {
            enemies.Clear();
            effects.Clear();
            comboUsed.Clear();
            walls.Clear();
            dragging = false;
            shotMoving = false;

            ResetPlayers();

            enemies.AddRange(StageManager.CreateStage(stage));

            if (stage == 1)
            {
                walls.Add(new Wall(350f, 250f, 300f, 30f));
            }
            else if (stage == 2)
            {
                walls.Add(new Wall(150f, 220f, 250f, 30f));
                walls.Add(new Wall(600f, 420f, 250f, 30f));
            }
            else if (stage == 3)
            {
                walls.Add(new Wall(220f, 180f, 560f, 30f));
                walls.Add(new Wall(220f, 500f, 560f, 30f));
                
            }
        }
        

        private void ResetPlayers()
        {
            float y = ClientSize.Height - 120f;

            for (int i = 0; i < players.Count; i++)
            {
                players[i].Position = new Vector2(ClientSize.Width / 2f - 90f + i * 90f, y);
                players[i].Velocity = new Vector2(0f, 0f);
            }
        }

        private void GameForm_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.R)
            {
                Restart();
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!gameClear)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].Update(ClientSize.Width, ClientSize.Height);
                }

                UpdateEnemies(); // ここで敵の更新を呼び出す

                Collision();
                UpdateEffects();
                CheckTurn();
                CheckStageClear();
            }

            Invalidate();
        }

        private void UpdateEnemies()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update();
            }
        }

        private void CheckTurn()
        {
            if (!shotMoving)
            {
                return;
            }

            if (AllPlayersStopped())
            {
                shotMoving = false;
                comboUsed.Clear();
                currentPlayerIndex++;

                if (currentPlayerIndex >= players.Count)
                {
                    currentPlayerIndex = 0;
                }
            }
        }

        private bool AllPlayersStopped()
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Velocity.Length() > 0.2f)
                {
                    return false;
                }
            }

            return true;
        }

        private void CheckStageClear()
        {
            if (enemies.Count > 0)
            {
                return;
            }

            if (!AllPlayersStopped())
            {
                return;
            }

            if (stage >= 3)
            {
                gameClear = true;
                return;
            }

            stage++;
            LoadStage();
        }

        private void UpdateEffects()
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Update();

                if (effects[i].Life <= 0)
                {
                    effects.RemoveAt(i);
                }
            }
        }

        private void GameForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (gameClear)
            {
                return;
            }

            if (!AllPlayersStopped())
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
                CurrentPlayer.Velocity = direction * power;
                shotCount++;
                shotMoving = true;
                comboUsed.Clear();
            }

            dragging = false;
        }

        private void Collision()
        {
            for (int p = 0; p < players.Count; p++)
            {
                Ball ball = players[p];

                for (int w = 0; w < walls.Count; w++)
                {
                    Wall wall = walls[w];

                    RectangleF ballRect = new RectangleF(
                        ball.Position.X - ball.Size / 2f,
                        ball.Position.Y - ball.Size / 2f,
                        ball.Size,
                        ball.Size
                    );

                    if (wall.Rect.IntersectsWith(ballRect))
                    {
                        float left = Math.Abs(ball.Position.X - wall.Rect.Left);
                        float right = Math.Abs(ball.Position.X - wall.Rect.Right);
                        float top = Math.Abs(ball.Position.Y - wall.Rect.Top);
                        float bottom = Math.Abs(ball.Position.Y - wall.Rect.Bottom);

                        float min = Math.Min(
                            Math.Min(left, right),
                            Math.Min(top, bottom)
                        );

                        if (min == left || min == right)
                        {
                            ball.Velocity.X *= -1f;
                        }
                        else
                        {
                            ball.Velocity.Y *= -1f;
                        }

                        ball.Velocity *= 0.95f;
                    }
                }

                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    Enemy enemy = enemies[i];

                    if (HitCircle(ball.Position, ball.Size / 2f, enemy.Position, enemy.Size / 2f) && !enemy.Cool)
                    {
                        enemy.Hp--;

                        Vector2 normal = GetCircleNormal(ball.Position, enemy.Position);
                        ball.Position += normal * 4f;
                        ball.Velocity = Reflect(ball.Velocity, normal) * 0.92f;

                        enemy.Cool = true;
                        enemy.CoolCount = 10;

                        if (enemy.Hp <= 0)
                        {
                            enemies.RemoveAt(i);
                        }
                    }
                }
            }

            Ball current = CurrentPlayer;

            if (current.Velocity.Length() > 0.2f)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (i == currentPlayerIndex)
                    {
                        continue;
                    }

                    Ball friend = players[i];

                    if (HitCircle(current.Position, current.Size / 2f, friend.Position, friend.Size / 2f))
                    {
                        Vector2 normal = GetCircleNormal(current.Position, friend.Position);
                        current.Position += normal * 4f;
                        current.Velocity = Reflect(current.Velocity, normal) * 0.9f;

                        if (!comboUsed.Contains(i))
                        {
                            comboUsed.Add(i);
                            FriendCombo(friend);
                        }
                    }
                }
            }
        }
        private void FriendCombo(Ball friend)
        {
            if (friend.ComboType == ComboType.Explosion)
            {
                ExplosionCombo(friend.Position);
            }
            else if (friend.ComboType == ComboType.CrossLaser)
            {
                CrossLaserCombo(friend.Position);
            }
            else if (friend.ComboType == ComboType.Homing)
            {
                HomingCombo(friend.Position);
            }
        }

        private void ExplosionCombo(Vector2 center)
        {
            effects.Add(new ComboEffect(center, ComboType.Explosion));

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                float distance = (enemy.Position - center).Length();

                if (distance <= 180f)
                {
                    DamageEnemy(i, 2);
                }
            }
        }

        private void CrossLaserCombo(Vector2 center)
        {
            effects.Add(new ComboEffect(center, ComboType.CrossLaser));

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];

                bool hitHorizontal = Math.Abs(enemy.Position.Y - center.Y) <= enemy.Size / 2f + 20f;
                bool hitVertical = Math.Abs(enemy.Position.X - center.X) <= enemy.Size / 2f + 20f;

                if (hitHorizontal || hitVertical)
                {
                    DamageEnemy(i, 3);
                }
            }
        }

        private void HomingCombo(Vector2 center)
        {
            int targetIndex = -1;
            float nearest = float.MaxValue;

            for (int i = 0; i < enemies.Count; i++)
            {
                float distance = (enemies[i].Position - center).Length();

                if (distance < nearest)
                {
                    nearest = distance;
                    targetIndex = i;
                }
            }

            if (targetIndex == -1)
            {
                return;
            }

            Vector2 target = enemies[targetIndex].Position;
            effects.Add(new ComboEffect(center, target, ComboType.Homing));
            DamageEnemy(targetIndex, 4);
        }

        private void DamageEnemy(int index, int damage)
        {
            if (index < 0 || index >= enemies.Count)
            {
                return;
            }

            enemies[index].Hp -= damage;
            enemies[index].Cool = true;
            enemies[index].CoolCount = 18;

            if (enemies[index].Hp <= 0)
            {
                enemies.RemoveAt(index);
            }
        }

        private bool HitCircle(Vector2 a, float ar, Vector2 b, float br)
        {
            return (a - b).Length() < ar + br;
        }

        private Vector2 GetCircleNormal(Vector2 a, Vector2 b)
        {
            Vector2 diff = a - b;

            if (diff.Length() == 0f)
            {
                return new Vector2(1f, 0f);
            }

            return diff.Normalize();
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
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(30, 30, 30));

            for (int i = 0; i < walls.Count; i++)
            {
                walls[i].Draw(g);
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Draw(g);
            }

            for (int i = 0; i < players.Count; i++)
            {
                players[i].Draw(g, i == currentPlayerIndex, i + 1);
            }

            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].Draw(g);
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
            Vector2 start = CurrentPlayer.Position;
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
                g.DrawString("ステージ: " + stage + " / 3", font, brush, 12f, 36f);
                g.DrawString("ショット数: " + shotCount, font, brush, 12f, 60f);
                g.DrawString("現在の味方: " + (currentPlayerIndex + 1), font, brush, 12f, 84f);
                g.DrawString("友情: " + GetComboName(CurrentPlayer.ComboType), font, brush, 12f, 108f);
                g.DrawString("Rキーでリスタート", font, brush, 12f, 132f);

                if (gameClear)
                {
                    using (Font clearFont = new Font("Yu Gothic UI", 36f, FontStyle.Bold))
                    {
                        g.DrawString("ALL CLEAR!", clearFont, brush, ClientSize.Width / 2f - 140f, ClientSize.Height / 2f - 40f);
                    }
                }
                else if (enemies.Count == 0)
                {
                    using (Font clearFont = new Font("Yu Gothic UI", 30f, FontStyle.Bold))
                    {
                        g.DrawString("STAGE CLEAR!", clearFont, brush, ClientSize.Width / 2f - 150f, ClientSize.Height / 2f - 40f);
                    }
                }
            }
        }

        private string GetComboName(ComboType comboType)
        {
            if (comboType == ComboType.Explosion)
            {
                return "爆発";
            }

            if (comboType == ComboType.CrossLaser)
            {
                return "十字レーザー";
            }

            if (comboType == ComboType.Homing)
            {
                return "ホーミング";
            }

            return "";
        }
    }
}