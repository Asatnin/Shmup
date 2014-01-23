using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Shmup
{
    class Enemy
    {
        // типы вражеских кораблей
        public enum Ships
        {
            Little, Medium, Big
        }

        Ships thisType;

        // ссылка на шейдерную программу
        static ShaderProgram mainProgram;

        // спрайт противника
        Texture shipSprite;

        // спрайт снаряда
        Texture bulletSprite;

        // массив рисуемых снарядов
        List<Bullet> bullets = ScreenBullets.EnemiesBullets;

        // буферы вершин и текстур
        int VBO, IBO, VAO;

        // скорости по оси X и оси Y
        float velX, velY;

        // текущая позиция
        float curX, curY;

        // ширина и высота
        float width, height;

        // задержка между вылетами снарядов и общее время полёта пули
        long fireDelay, curTime = 0;

        // флаг первого выстрела
        bool firstShot = false;

        // количество жизней противника
        int health;

        // параметры снаряда
        float bulletX, bulletY, bulletVelX, bulletVelY;

        bool haveGun;

        // конструктор
        public Enemy(Texture shipSprite, Texture bulletSprite, float velX, float velY,
            float curX, float curY, float width, float height, int fireDelay, int health,
            float bulletX, float bulletY, float bulletVelX, float bulletVelY, Ships shipType,
            bool haveGun) 
        {
            this.shipSprite = shipSprite;
            this.bulletSprite = bulletSprite;

            // ширина и высота
            this.width = width;
            this.height = height;

            // вершины
            createVertices();

            // устанавливаем значения скорости...
            this.velX = velX;
            this.velY = velY;

            // ... и текущей позиции
            this.curX = curX;
            this.curY = curY;

            // задержка между вылетами снаряда
            this.fireDelay = fireDelay;

            // жизни
            this.health = health;

            // парметры снаряда
            this.bulletX = bulletX;
            this.bulletY = bulletY;
            this.bulletVelX = bulletVelX;
            this.bulletVelY = bulletVelY;

            // тип
            thisType = shipType;

            // может ли стрелять
            this.haveGun = haveGun;
        }

        // вершины и буфер
        void createVertices()
        {
            VertexData[] vertices = new VertexData[4];

            vertices[0].position.x = 0.0f;
            vertices[0].position.y = 0.0f;
            vertices[0].texCoord.s = 0.0f;
            vertices[0].texCoord.t = 0.0f;

            vertices[1].position.x = width;
            vertices[1].position.y = 0.0f;
            vertices[1].texCoord.s = 1.0f;
            vertices[1].texCoord.t = 0.0f;

            vertices[2].position.x = width;
            vertices[2].position.y = height;
            vertices[2].texCoord.s = 1.0f;
            vertices[2].texCoord.t = 1.0f;

            vertices[3].position.x = 0.0f;
            vertices[3].position.y = height;
            vertices[3].texCoord.s = 0.0f;
            vertices[3].texCoord.t = 1.0f;

            // загоням в буфер
            GL.GenBuffers(1, out VBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(4 *
                Marshal.SizeOf(typeof(VertexData))), vertices, BufferUsageHint.StaticDraw);

            // IBO
            uint[] indices = { 0, 1, 2, 3 };
            GL.GenBuffers(1, out IBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(4 * sizeof(uint)), indices,
                BufferUsageHint.StaticDraw);

            // VAO
            GL.GenVertexArrays(1, out VAO);
            GL.BindVertexArray(VAO);

            mainProgram.enableDataPointers();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            mainProgram.setTexCoordPointer(Marshal.SizeOf(typeof(VertexData)),
                Marshal.OffsetOf(typeof(VertexData), "texCoord"));
            mainProgram.setVertexPointer(Marshal.SizeOf(typeof(VertexData)),
                Marshal.OffsetOf(typeof(VertexData), "position"));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);

            GL.BindVertexArray(0);

            mainProgram.disableDataPointers();
        }

        
        // обновляем позицию
        public void update(long delta)
        {
            curX += velX * delta * 0.001f;
            curY += velY * delta * 0.001f;

            // если можно стрелять...
            curTime += delta;
            if (haveGun && canFire())
            {
                if (!firstShot)
                {
                    firstShot = true;
                    curTime = fireDelay - 200;
                }
                else
                    if (curTime >= fireDelay)
                    {
                        fire();
                        curTime = 0;
                    }
            }
        }

        // стреляем противником!
        void fire()
        {
            bullets.Add(new Bullet(bulletSprite, curX + bulletX, curY + bulletY, bulletVelX,
                bulletVelY));            
        }

        // перешёл ли корабль в область видимости, чтобы стрелять
        bool canFire()
        {
            if (curX + width <= Program.WIDTH && curY + height >= 0 &&
                curY + height <= Program.HEIGHT)
                return true;
            return false;
        }

        // рисуем
        public void render()
        {
            GL.BindTexture(TextureTarget.Texture2D, shipSprite.ID);
            GL.BindVertexArray(VAO);

            mainProgram.setMultiColor(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            mainProgram.setModelView(Matrix4.CreateTranslation(curX, curY, 0.0f));
            mainProgram.updateModelView();
            GL.DrawElements(BeginMode.TriangleFan, 4, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        // обрамляющий прямоугольник
        public BoundingRectangle BoundingSquare
        {
            get
            {
                return new BoundingRectangle(curX, curY, width, height - 10);
            }
        }

        // убавляем жизни
        public void decreaseHealth(int delta)
        {
            health -= delta;
        }

        // жив ли противник
        public bool Alive
        {
            get
            {
                return health > 0;
            }
        }

        // виден ли противник
        public bool OutOfScreen
        {
            get
            {
                return curX + shipSprite.Width < 0; //||
                    //curY > Program.HEIGHT || curY + shipSprite.Height < 0;
            }
        }

        // текущая позиция середины корабля противника
        public VertexPosition Position
        {
            get
            {
                return new VertexPosition(curX + width / 2, curY + height / 2);
            }
        }

        // столкнулcя ли противник с чем-то
        public bool isCollided(BoundingRectangle boundingSquare)
        {
            // столкнулся ли снаряд относительно прямоугольника...
            if (curX >= boundingSquare.Right ||
                curX + shipSprite.Width <= boundingSquare.Left ||
                curY >= boundingSquare.Bottom || curY + shipSprite.Height <= boundingSquare.Top)
                return false;
            else
            {
                health = 0;
                return true;
            }
        }

        public Ships ThisType
        {
            get
            {
                return thisType;
            }
        }

        // прикрепляем шейдерную программу
        public static void attachProgram(ShaderProgram program)
        {
            mainProgram = program;
        }
    }
}
