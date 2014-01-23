using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shmup
{
    class Opponent
    {
        // названия файлов со спрайтами противников
        string enemy1 = "Sprites/Ships/enemy1.png", enemy2 = "Sprites/Ships/enemy2rotated.png",
            enemy3 = "Sprites/Ships/enemy3rotated.png",
            fastEnemy = "Sprites/Ships/fastEnemy.png";

        // текстуры противников
        Texture enemy1Texture, enemy2Texture, enemy3Texture, fastEnemyTexture;

        // текстуры снарядов противников
        Texture bulletSprite1, bulletSprite2, bulletSprite3;

        // рисуемые противники
        static List<Enemy> enemies = new List<Enemy>();

        // задержка между появлениями врагов, общее время
        long allTime = 0;
        int bigShipDelay = 9000, bigShipCurTime, bigShipBoundDelay = 7000,
            mediumShipDelay = 6000, mediumShipCurTime, mediumShipBoundDelay = 5000,
            littleShipDelay = 3000, littleShipCurTime, littleShipBoundDelay = 6000, coef1,
            coef2, coef3, bigShipLowBound = 6000, mediumShipLowBound = 4000,
            littleShipLowBound = 4000, dif;

        // рандомизатор
        Random rand = new Random();

        // увеличение жизней вражеских кораблей
        static int enemyHealthAddition = 0;

        // конструктор
        public Opponent()
        {
            enemy1Texture = new Texture();
            enemy1Texture.loadTextureFromFile(enemy1);
            enemy2Texture = new Texture();
            enemy2Texture.loadTextureFromFile(enemy2);
            enemy3Texture = new Texture();
            enemy3Texture.loadTextureFromFile(enemy3);
            fastEnemyTexture = new Texture();
            fastEnemyTexture.loadTextureFromFile(fastEnemy);

            bulletSprite1 = new Texture();
            bulletSprite1.loadTextureFromFile("Sprites/Bullets/bullet2.png");

            bulletSprite2 = new Texture();
            bulletSprite2.loadTextureFromFile("Sprites/Bullets/bullet3.png");

            bulletSprite3 = new Texture();
            bulletSprite3.loadTextureFromFile("Sprites/Bullets/bullet4.png");
        }

        // рестарт оппонента
        public void restart()
        {
            bigShipCurTime = 0;
            mediumShipCurTime = 0;
            littleShipCurTime = 0;
            bigShipBoundDelay = 7000;
            bigShipDelay = 9000;
            mediumShipDelay = 6000;
            mediumShipBoundDelay = 5000;
            littleShipBoundDelay = 6000;
            littleShipDelay = 3000;
            bigShipLowBound = 6000;
            mediumShipLowBound = 4000;
            littleShipLowBound = 4000;
            allTime = 0;
            enemies.Clear();
        }

        // обновляем противников
        public void update(int delta)
        {
            // изменяем время
            bigShipCurTime += delta;
            mediumShipCurTime += delta;
            littleShipCurTime += delta;
            allTime += delta;

            // увеличиваем количество жизней противника
            enemyHealthAddition = Player.Points / 250;

            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].OutOfScreen)
                {
                    enemies.RemoveAt(i);
                    i--;
                    continue;
                }

                // проверка на столкновение с игроком
                if (Player.Alive && enemies[i].Alive &&
                    enemies[i].isCollided(Player.BoundingSquare))
                {
                    Player.decreaseHealth(1);

                    // если жизни у игрока закончились, то взрываем его корабли
                    if (!Player.Alive)
                        ScreenEffects.Explosions.Add(new Explosion(ScreenEffects.SpriteExp2, 8,
                            4, 500, Player.Position.x, Player.Position.y));
                }

                if (!enemies[i].Alive)
                {
                    // рисуем взрыв корабля
                    ScreenEffects.Explosions.Add(new Explosion(ScreenEffects.SpriteExp2, 8, 4,
                        500, enemies[i].Position.x, enemies[i].Position.y));

                    // включаем звук
                    SoundClass.playShipExplosion();

                    // проверка на выпадение бонуса
                    bool dropped = false;
                    if (ScreenInformation.IsSpeedFireAvailable && !Player.HasSpeedFireBonus &&
                        rand.Next(20) == 5)
                    {
                        ScreenInformation.addBonus(Bonus.BonusType.speedFireBonus,
                            enemies[i].Position.x, enemies[i].Position.y, -50, 0);
                        dropped = true;
                    }
                    if (ScreenInformation.IsBulletAvailable && !dropped &&
                        !Player.HasBulletBonus && rand.Next(20) == 5)
                    {
                        ScreenInformation.addBonus(Bonus.BonusType.bulletBonus,
                            enemies[i].Position.x, enemies[i].Position.y, -50, 0);
                        dropped = true;
                    }

                    if (ScreenInformation.IsMoveSpeedAvailable && !dropped &&
                        !Player.HasSpeedBonus && rand.Next(20) == 5)
                    {
                        ScreenInformation.addBonus(Bonus.BonusType.speedBonus,
                            enemies[i].Position.x, enemies[i].Position.y, -50, 0);
                        dropped = true;
                    }

                    // и добавляем очки игроку
                    switch (enemies[i].ThisType)
                    {
                        case Enemy.Ships.Little:
                            Player.Points = Player.Points + 5;
                            break;
                        case Enemy.Ships.Medium:
                            Player.Points = Player.Points + 10;
                            break;
                        case Enemy.Ships.Big:
                            Player.Points = Player.Points + 20;
                            break;
                    }

                    enemies.RemoveAt(i);
                    i--;
                    continue;
                }

                enemies[i].update(delta);
            }

            // добавление диагональных персонажей-врагов
            if (mediumShipCurTime >= mediumShipDelay)
            {
                switch (rand.Next(2))
                {
                    case 0:
                        addDiagonalFromTopEnemy(); // ищо враги
                        break;

                    case 1:
                        addDiagonalFromBottomEnemy(); // и ещё :)
                        break;
                }
                mediumShipDelay = rand.Next(mediumShipLowBound, mediumShipBoundDelay);
                if (coef1 != Player.Points / 150)
                {
                    dif = Player.Points / 150 * mediumShipBoundDelay / 20;
                    if (mediumShipLowBound - dif >= 2000)
                    {
                        mediumShipBoundDelay -= dif;
                        mediumShipLowBound -= dif;
                    }
                }
                Console.WriteLine(mediumShipBoundDelay);
                coef1 = Player.Points / 150;
            }            

            // добавляем больших персонажей-врагов
            if (bigShipCurTime >= bigShipDelay)
            {
                addUsualEnemy();
                bigShipDelay = rand.Next(bigShipLowBound, bigShipBoundDelay);
                if (coef2 != Player.Points / 150)
                {
                    dif = Player.Points / 150 * mediumShipBoundDelay / 20;
                    if (bigShipLowBound - dif >= 2200)
                    {
                        bigShipBoundDelay -= dif;
                        bigShipLowBound -= dif;
                    }
                }
                coef2 = Player.Points / 150;
            }

            // добавляем маленьких персонажей-врагов
            if (littleShipCurTime >= littleShipDelay)
            {
                addFastEnemy();
                littleShipDelay = rand.Next(littleShipLowBound, littleShipBoundDelay);
                if (coef3 != Player.Points / 150)
                {
                    dif = Player.Points / 150 * mediumShipBoundDelay / 20;
                    if (littleShipLowBound - dif >= 1500)
                    {
                        littleShipBoundDelay -= dif;
                        littleShipLowBound -= dif;
                    }
                }
                coef3 = Player.Points / 150;
            }
        }

        // рисуем противников
        public void render()
        {
            for (int i = 0; i < enemies.Count; i++)
                enemies[i].render();
        }

        // добавляем врагов, стреляющих и движущихся медленно по-горизонтали
        void addUsualEnemy()
        {
            enemies.Add(new Enemy(enemy1Texture, bulletSprite1, -100, 0, 800,
                    rand.Next(0, Program.HEIGHT - enemy1Texture.Height), enemy1Texture.Width,
                    enemy1Texture.Height, 2000, 3 + enemyHealthAddition, -5.0f, 27.0f, -200.0f,
                    0.0f, Enemy.Ships.Big, true));
            bigShipCurTime = 0;
        }

        // добавляем врагов, движущихся по-диагонали сверху вниз и справа налево
        void addDiagonalFromTopEnemy()
        {
            int x = rand.Next(Program.WIDTH / 2 - 1, Program.WIDTH);
            enemies.Add(new Enemy(enemy2Texture, bulletSprite2, -70, 70, x,
                    -enemy2Texture.Height, enemy2Texture.Width * 3.0f / 4.0f,
                    enemy2Texture.Height * 3.0f / 4.0f, 1000, 2 + enemyHealthAddition, 10.0f,
                    21.0f, -200, 200, Enemy.Ships.Medium, true));
            mediumShipCurTime = 0;
        }

        // добавляем врагов, стреляющих и движущихся по-диагонали снизу вверх
        void addDiagonalFromBottomEnemy()
        {
            int x = rand.Next(Program.WIDTH / 2 - 1, Program.WIDTH);
            enemies.Add(new Enemy(enemy3Texture, bulletSprite2, -70, -70, x,
                enemy3Texture.Height + Program.HEIGHT, enemy3Texture.Width * 3.0f / 4.0f,
                enemy3Texture.Height * 3.0f / 4.0f, 1000, 2 + enemyHealthAddition, 17.0f,
                12.0f, -200, -200, Enemy.Ships.Medium, true));
            mediumShipCurTime = 0;
        }

        // добавляем быстрых врагов, которые стреляют, но слишком поздно, поэтому без вреда
        void addFastEnemy()
        {
            enemies.Add(new Enemy(fastEnemyTexture, bulletSprite3, -250, 0, 800,
                Player.Position.y - 10, fastEnemyTexture.Width, fastEnemyTexture.Height, 3000,
                1 + enemyHealthAddition, 8, 12, -350, 0, Enemy.Ships.Little, true));
            littleShipCurTime = 0;
        }

        // возвращаем массив противников
        public static List<Enemy> Enemies
        {
            get
            {
                return enemies;
            }
        }
    }
}
