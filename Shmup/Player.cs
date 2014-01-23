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
    class Player
    {
        // ссылка на шейдерную программу
        static ShaderProgram mainProgram;

        // ссылка на клавиатуру
        static KeyboardDevice keyboard;

        // спрайт персонажа
        Texture ship;

        // название файла спрайта
        string filename = "Sprites/Ships/Player.png";

        // VBO, IBO, VAO для персонажа
        int VBO, IBO, texVBO;

        // ширина и длина части текстуры с одним кораблём
        static int width, height;

        // обновляющиеся значения текстурных координат
        TexturePosition[][] clips;

        // количество кадров анимации
        int frames = 4;

        // текущий кадр анимации
        int currentFrame = 0;

        // задержка перед включением анимации
        long delay = 0;

        // текущая позиция левого верхнего угла персонажа
        static float curX, curY;

        // движение при нажатии клавиши
        static float stepX = 3.0f, stepY = 2.0f;

        // спрайт пули
        static Texture bulletSprite, superBulletSprite, currentBullet;

        // массив рисуемых снарядов
        List<Bullet> bullets = ScreenBullets.PlayerBullets;

        // задержка перед вылетом следующего снаряда и прошедшее время с последнего выстрела
        static long fireDelay = 800, curTime = 500;

        // смещение по вертикали для выравнивания позиции снаряда
        float offsetY = 8.0f;

        // количество жизней игрока
        static int health = 3;

        // набранные очки
        static int points;

        // конструктор
        public Player()
        {
            // загружаем текстуру
            ship = new Texture();
            ship.loadTextureFromFile(filename);

            // размеры части текстуры
            width = ship.Width / 2;
            height = ship.Height / 2;

            createVertices();

            // начальное положение персонажа
            curX = 0.0f;
            curY = Program.HEIGHT / 2.0f;

            // загружаем текстуру снаряда
            bulletSprite = new Texture();
            bulletSprite.loadTextureFromFile("Sprites/Bullets/bullet.png");
            superBulletSprite = new Texture();
            superBulletSprite.loadTextureFromFile("Sprites/Bullets/superBullet.png");
            currentBullet = bulletSprite;
        }

        // пересоздание на начальной позиции
        public void restart()
        {
            // начальное положение персонажа
            curX = 0.0f;
            curY = Program.HEIGHT / 2.0f;

            currentBullet = bulletSprite;
            stepX = 3.0f;
            stepY = 2.0f;
            fireDelay = 500;
            points = 0;

            health = 3;
        }

        // вершины для отрисовки квада
        void createVertices()
        {
            VertexPosition[] vertices = new VertexPosition[4];

            // сами вершины
            vertices[0].x = 0.0f;
            vertices[0].y = 0.0f;

            vertices[1].x = width;
            vertices[1].y = 0.0f;

            vertices[2].x = width;
            vertices[2].y = height;

            vertices[3].x = 0.0f;
            vertices[3].y = height;

            // загоняем вершины в буфер
            GL.GenBuffers(1, out VBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(4 *
                Marshal.SizeOf(typeof(VertexPosition))), vertices, BufferUsageHint.StaticDraw);

            // индексы
            uint[] indices = { 0, 1, 2, 3 };

            // загоняем индексы в буфер
            GL.GenBuffers(1, out IBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(4 * sizeof(uint)), indices,
                BufferUsageHint.StaticDraw);

            // генерим четыре клипа для анимации
            clips = new TexturePosition[frames][];

            // первый клип
            clips[0] = new TexturePosition[4];

            clips[0][0].s = 0.0f;
            clips[0][0].t = 0.5f;

            clips[0][1].s = 0.0f;
            clips[0][1].t = 0.0f;

            clips[0][2].s = 0.5f;
            clips[0][2].t = 0.0f;

            clips[0][3].s = 0.5f;
            clips[0][3].t = 0.5f;

            // второй клип
            clips[1] = new TexturePosition[4];

            clips[1][0].s = 0.5f;
            clips[1][0].t = 0.5f;

            clips[1][1].s = 0.5f;
            clips[1][1].t = 0.0f;

            clips[1][2].s = 1.0f;
            clips[1][2].t = 0.0f;

            clips[1][3].s = 1.0f;
            clips[1][3].t = 0.5f;

            // третий клип
            clips[2] = new TexturePosition[4];

            clips[2][0].s = 0.5f;
            clips[2][0].t = 1.0f;

            clips[2][1].s = 0.5f;
            clips[2][1].t = 0.5f;

            clips[2][2].s = 1.0f;
            clips[2][2].t = 0.5f;

            clips[2][3].s = 1.0f;
            clips[2][3].t = 1.0f;

            // четвёртый клип
            clips[3] = new TexturePosition[4];

            clips[3][0].s = 0.0f;
            clips[3][0].t = 1.0f;

            clips[3][1].s = 0.0f;
            clips[3][1].t = 0.5f;

            clips[3][2].s = 0.5f;
            clips[3][2].t = 0.5f;

            clips[3][3].s = 0.5f;
            clips[3][3].t = 1.0f;

            // загоняем первый клип в буфер
            GL.GenBuffers(1, out texVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, texVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(4 *
                Marshal.SizeOf(typeof(TexturePosition))), clips[0],
                BufferUsageHint.DynamicDraw);
        }

        // обновляем положение персонажа
        public void update(long delta)
        {
            // выбираем подходящую анимацию
            int prevFrame = currentFrame;
            delay += delta;
            if (delay >= 150)
            {
                currentFrame = 0;
                delay = 0;
            }
            else
                if (delay >= 110)
                        currentFrame = 3;
                else
                    if (delay >= 70)
                        currentFrame = 2;
                    else
                        if (delay >= 40)
                            currentFrame = 1;

            if (prevFrame != currentFrame)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, texVBO);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(4 *
                    Marshal.SizeOf(typeof(TexturePosition))), clips[currentFrame]);
            }

            // движение вправо
            if (keyboard[Key.Right] && curX + width + stepX <= Program.WIDTH)
                curX += stepX;
            // движение влево
            if (keyboard[Key.Left] && curX - stepX >= 0.0f)
                curX -= stepX;
            // движение вверх
            if (keyboard[Key.Up] && curY - stepY >= 0.0f)
                curY -= stepY;
            // движение вниз
            if (keyboard[Key.Down] && curY + height + stepY <= Program.HEIGHT)
                curY += stepY;

            // стреляем!
            curTime += delta;
            if (keyboard[Key.Space])
                fire();
        }

        // рисуем
        public void render()
        {
            //рисуем персонажа
            mainProgram.enableDataPointers();

            GL.BindTexture(TextureTarget.Texture2D, ship.ID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, texVBO);
            mainProgram.setTexCoordPointer(0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            mainProgram.setVertexPointer(0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);

            mainProgram.setMultiColor(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            //mainProgram.setModelView(Matrix4.Identity);
            mainProgram.setModelView(Matrix4.CreateTranslation(curX, curY, 0.0f));
            mainProgram.updateModelView();
            GL.DrawElements(BeginMode.TriangleFan, 4, DrawElementsType.UnsignedInt, 0);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            mainProgram.disableDataPointers();           
        }

        // стреляем!
        void fire()
        {
            // если можно стрелять
            if (curTime >= fireDelay)
            {
                Bullet.BulletType bulletType = Bullet.BulletType.Usual;
                if (currentBullet == superBulletSprite)
                    bulletType = Bullet.BulletType.Super;
                bullets.Add(new Bullet(currentBullet, curX + width, curY + offsetY, 400, 0,
                    bulletType));
                bullets.Add(new Bullet(currentBullet, curX + width,
                    curY + height - 2 * offsetY - 1.0f, 400, 0, bulletType));
                curTime = 0;
            }
        }

        public static void changeBullet()
        {
            if (currentBullet == bulletSprite)
                currentBullet = superBulletSprite;
            else
                currentBullet = bulletSprite;
        }

        // обрамляющий прямоугольник
        public static BoundingRectangle BoundingSquare
        {
            get
            {
                return new BoundingRectangle(curX + 10, curY, width - 20, height - 5);
            }
        }

        // убавляем жизни
        public static void decreaseHealth(int delta)
        {
            health -= delta;
            currentBullet = bulletSprite;
            stepX = 3.0f;
            stepY = 2.0f;
            fireDelay = 500;
        }

        // жив ли игрок
        public static bool Alive
        {
            get
            {
                return health > 0;
            }
        }

        // текущая позиция середины корабля
        public static VertexPosition Position
        {
            get
            {
                return new VertexPosition(curX + width / 2, curY + height / 2);
            }
        }

        // количество жизней у игрока
        public static int Health
        {
            get
            {
                return health;
            }
        }

        // устанавливаем время между выстрелами
        public static void setFireDelay(long time)
        {
            fireDelay = time;
        }

        // устанавливает скорость перемещения
        public static void setMoveSpeed(float x, float y)
        {
            stepX = x;
            stepY = y;
        }

        public static bool HasSpeedFireBonus
        {
            get
            {
                return fireDelay != 500;
            }
        }

        public static bool HasBulletBonus
        {
            get
            {
                return currentBullet == superBulletSprite;
            }
        }

        public static bool HasSpeedBonus
        {
            get
            {
                return stepX > 4.0f;
            }
        }

        public static int Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
            }
        }

        // прикрепляем шейдерную программу
        public static void attachProgram(ShaderProgram program)
        {
            mainProgram = program;
        }

        // прикрепляем клавиатуру
        public static void attachKeyboard(KeyboardDevice keys)
        {
            keyboard = keys;
        }
    }
}
