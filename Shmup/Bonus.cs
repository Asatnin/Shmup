using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shmup
{
    class Bonus : StaticSprite
    {
        public enum BonusType
        {
            speedFireBonus, bulletBonus, speedBonus
        }

        static Texture[] bonusTextures;
        BonusType thisType;
        bool isAlive = true;

        static Bonus()
        {
            bonusTextures = new Texture[4];
            bonusTextures[0] = new Texture();
            bonusTextures[0].loadTextureFromFile("Sprites/Bonuses/speedFireBonus.png");
            bonusTextures[1] = new Texture();
            bonusTextures[1].loadTextureFromFile("Sprites/Bonuses/bulletBonus.png");
            bonusTextures[2] = new Texture();
            bonusTextures[2].loadTextureFromFile("Sprites/Bonuses/moveSpeedBonus.png");
        }

        public Bonus(Bonus.BonusType bonusType, float curX, float curY, float velX, float velY)
            : base(bonusTextures[bonusType - BonusType.speedFireBonus], curX, curY, 30, 30,
            velX, velY)
        {
            thisType = bonusType;
        }

        // столкнулась ли с чем-то
        public bool isCollided(BoundingRectangle boundingSquare)
        {
            // столкнулся ли снаряд относительно прямоугольника...
            if (curX >= boundingSquare.Right || curX + sprite.Width <= boundingSquare.Left ||
                curY >= boundingSquare.Bottom || curY + sprite.Height <= boundingSquare.Top)
                return false;
            else
            {
                isAlive = false;
                return true;
            }
        }

        public bool IsAlive
        {
            get
            {
                return isAlive;
            }
        }

        public BonusType ThisType
        {
            get
            {
                return thisType;
            }
        }
    }
}
