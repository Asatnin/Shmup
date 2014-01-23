using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shmup
{
    static class ScreenInformation
    {
        /*------- ЗДОРОВЬЕ! --------*/
        static Texture heartTexture; // текстура для отображения жизней
        static StaticSprite[] heartSprites;
        static void makeHeart()
        {
            heartTexture = new Texture();
            heartTexture.loadTextureFromFile("Sprites/Information/heart.png");
            heartSprites = new StaticSprite[3];
            for (int i = 0; i < 3; i++)
                heartSprites[i] = new StaticSprite(heartTexture, i * 15, 0);
        }

        /*------- БОНУСЫ! --------*/
        static List<Bonus> bonus;
        static bool speedFireAvailable = true, bulletAvailable = true,
            moveSpeedAvailable = true;
        static void makeBonus()
        {            
            bonus = new List<Bonus>();
        }
        public static void addBonus(Bonus.BonusType bonusType, float curX, float curY,
            float velX, float velY)
        {
            bonus.Add(new Bonus(bonusType, curX, curY, velX, velY));
        }

        public static void update(long delta)
        {
            for (int i = 0; i < bonus.Count; i++)
            {
                bonus[i].update(delta);
                if (bonus[i].OutOfScreen || !bonus[i].IsAlive)
                {
                    if (bonus[i].ThisType == Bonus.BonusType.bulletBonus)
                        bulletAvailable = true;
                    else
                        if (bonus[i].ThisType == Bonus.BonusType.speedFireBonus)
                            speedFireAvailable = true;
                        else
                            if (bonus[i].ThisType == Bonus.BonusType.speedBonus)
                                moveSpeedAvailable = true;
                    bonus.RemoveAt(i);
                    i--;
                    continue;
                }
                if (bonus[i].ThisType == Bonus.BonusType.bulletBonus)
                    bulletAvailable = false;
                else
                    if (bonus[i].ThisType == Bonus.BonusType.speedFireBonus)
                        speedFireAvailable = false;
                    else
                        if (bonus[i].ThisType == Bonus.BonusType.speedBonus)
                            moveSpeedAvailable = false;
                if (!bonus[i].OutOfScreen && bonus[i].isCollided(Player.BoundingSquare))
                {
                    if (bonus[i].ThisType == Bonus.BonusType.speedFireBonus)
                        Player.setFireDelay(500);
                    if (bonus[i].ThisType == Bonus.BonusType.bulletBonus)
                        Player.changeBullet();
                    if (bonus[i].ThisType == Bonus.BonusType.speedBonus)
                        Player.setMoveSpeed(5.0f, 3.0f);
                    SoundClass.playBonusSelect();
                }
            }
        }

        public static bool IsSpeedFireAvailable
        {
            get
            {
                return speedFireAvailable;
            }
        }

        public static bool IsBulletAvailable
        {
            get
            {
                return bulletAvailable;
            }
        }

        public static bool IsMoveSpeedAvailable
        {
            get
            {
                return moveSpeedAvailable;
            }
        }

        public static void restart()
        {
            bonus.Clear();
        }

        // рисуем информацию
        public static void render()
        {
            for (int i = 0; i < Player.Health; i++)
                heartSprites[i].render();
            for (int i = 0; i < bonus.Count; i++)
                bonus[i].render();
        }

        // статический конструктор
        static ScreenInformation()
        {            
            makeHeart();
            makeBonus();
        }
    }
}
