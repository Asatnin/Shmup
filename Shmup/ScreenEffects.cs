using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shmup
{
    static class ScreenEffects
    {
        // спрайт взрыва снаряда
        static Texture spriteExp1 = new Texture();

        // спрайт взрыва корабля
        static Texture spriteExp2 = new Texture();

        // взрывы
        static List<Explosion> explosions = new List<Explosion>();

        static ScreenEffects()
        {
            spriteExp1.loadTextureFromFile("Sprites/Explosions/Explode1.png");
            spriteExp2.loadTextureFromFile("Sprites/Explosions/Explode3.png");
        }

        public static void update(long delta)
        {
            for (int i = 0; i < explosions.Count; i++)
            {
                explosions[i].update(delta);
                if (!explosions[i].Exist)
                {
                    explosions.RemoveAt(i);
                    i--;
                }
            }
        }

        public static void render()
        {
            for (int i = 0; i < explosions.Count; i++)
                explosions[i].render();
        }

        public static void restart()
        {
            explosions.Clear();
        }

        // свойство взрывов
        public static List<Explosion> Explosions
        {
            get
            {
                return explosions;
            }
        }

        // свойство текстуры взрыва снаряда
        public static Texture SpriteExp1
        {
            get
            {
                return spriteExp1;
            }

        }

        // свойство текстуры взрыва корабля
        public static Texture SpriteExp2
        {
            get
            {
                return spriteExp2;
            }
        }
    }
}
