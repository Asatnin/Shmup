using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shmup
{
    static class ScreenBullets
    {
        // ссылка на шейдерную программу
        static ShaderProgram mainProgram;

        // снаряды игрока
        static List<Bullet> playerBullets = new List<Bullet>();

        // снаряды противников
        static List<Bullet> enemiesBullets = new List<Bullet>();

        // свойство снарядов игрока
        public static List<Bullet> PlayerBullets
        {
            get
            {
                return playerBullets;
            }
        }

        // свойство снарядов противника
        public static List<Bullet> EnemiesBullets
        {
            get
            {
                return enemiesBullets;
            }
        }

        // обновляем снаряды
        public static void update(long delta)
        {
            // обновляем снаряды игрока
            for (int i = 0; i < playerBullets.Count; i++)
            {
                playerBullets[i].update(delta);

                // проверка снаряда на видимость и существование
                if (playerBullets[i].OutOfScreen || !playerBullets[i].Alive)
                {
                    playerBullets.RemoveAt(i);
                    i--;
                    continue;
                }

                // проверка на то, попали ли мы во врага
                List<Enemy> enemies = Opponent.Enemies;
                for (int j = 0; j < enemies.Count; j++)
                {
                    // если попали и противник виден
                    if (!enemies[j].OutOfScreen && playerBullets[i].isCollided(
                        enemies[j].BoundingSquare))
                    {
                        // то уменьшаем жизни противнику
                        if (playerBullets[i].ThisType == Bullet.BulletType.Usual)
                            enemies[j].decreaseHealth(1);
                        else
                            enemies[j].decreaseHealth(2);                         

                        // и рисуем взрыв снаряда
                        ScreenEffects.Explosions.Add(new Explosion(ScreenEffects.SpriteExp1,
                            8, 8, 50, playerBullets[i].Position.x,
                            playerBullets[i].Position.y));

                        SoundClass.playBulletExplosion();
                    }
                }
            }


            // обновляем снаряды противника
            for (int i = 0; i < enemiesBullets.Count; i++)
            {
                enemiesBullets[i].update(delta);
                if (enemiesBullets[i].OutOfScreen || !enemiesBullets[i].Alive)
                {
                    enemiesBullets.RemoveAt(i);
                    i--;
                    continue;
                }

                // проверяем, попал ли снаряд в игрока
                if (Player.Alive && enemiesBullets[i].isCollided(Player.BoundingSquare))
                {
                    // уменьшаем жизни игроку
                    Player.decreaseHealth(1);

                    //и рисуем взрыв снаряда
                    ScreenEffects.Explosions.Add(new Explosion(ScreenEffects.SpriteExp1, 8, 8,
                        50, enemiesBullets[i].Position.x, enemiesBullets[i].Position.y));
                    SoundClass.playBulletExplosion();
                    enemiesBullets.RemoveAt(i);
                    i--;

                    // если жизни у игрока закончились, то взрываем его корабль и включаем звук
                    if (!Player.Alive)
                    {
                        ScreenEffects.Explosions.Add(new Explosion(ScreenEffects.SpriteExp2, 8,
                            4, 500, Player.Position.x, Player.Position.y));
                        SoundClass.playShipExplosion();
                        SoundClass.stopLoopMusic();
                        Program.setState(Program.GameState.gameOver);
                    }
                }
            }
        }

        // рисуем снаряды
        public static void render()
        {
            // рисуем снаряды игрока
            for (int i = 0; i < playerBullets.Count; i++)
                playerBullets[i].render(); 

            // рисуем снаряды противника
            for (int i = 0; i < enemiesBullets.Count; i++)
                enemiesBullets[i].render();
             
        }

        public static void restart()
        {
            playerBullets.Clear();
            enemiesBullets.Clear();
        }

        // прикрепляем шейдерную программу
        public static void attachProgram(ShaderProgram program)
        {
            mainProgram = program;
        }
    }
}
