using System.Collections.Generic;

namespace MonstWinForms
{
    public static class StageManager
    {
        public static List<Enemy> CreateStage(int stage)
        {
            List<Enemy> enemies = new List<Enemy>();

            if (stage == 1)
            {
                enemies.Add(new Enemy(220f, 220f, 60f, 4, false));
                enemies.Add(new Enemy(380f, 220f, 60f, 4, false));
                enemies.Add(new Enemy(540f, 220f, 60f, 4, false));
                enemies.Add(new Enemy(700f, 220f, 60f, 4, false));
            }
            else if (stage == 2)
            {
                enemies.Add(new Enemy(200f, 180f, 60f, 5, false));
                enemies.Add(new Enemy(500f, 180f, 60f, 5, false));
                enemies.Add(new Enemy(800f, 180f, 60f, 5, false));
                enemies.Add(new Enemy(350f, 330f, 65f, 6, false));
                enemies.Add(new Enemy(650f, 330f, 65f, 6, false));
            }
            else if (stage == 3)
            {
                enemies.Add(new Enemy(230f, 200f, 60f, 5, false));
                enemies.Add(new Enemy(770f, 200f, 60f, 5, false));
                enemies.Add(new Enemy(300f, 400f, 65f, 6, false));
                enemies.Add(new Enemy(700f, 400f, 65f, 6, false));
                enemies.Add(new Enemy(500f, 250f, 130f, 25, true));
            }

            return enemies;
        }
    }
}