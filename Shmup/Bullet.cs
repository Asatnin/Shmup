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
    class Bullet
    {
        public enum BulletType
        {
            Usual, Super
        }

        BulletType thisType = BulletType.Usual;

        // ссылка на шейдерную программу
        static ShaderProgram mainProgram;

        // спрайт снаряда
        Texture sprite;

        // начальные координаты верхнего левого угла
        float curX, curY;

        // id буферов
        int VBO, IBO, VAO;

        // скорость по горизонтальной оси
        float velX, velY;

        // существует ли снаряд
        bool isAlive = true;

        // конструктор для снаряда
        public Bullet(Texture texture, float curX, float curY, float velX, float velY)
        {
            // загружаем текстуры снаряда
            sprite = texture;

            // начальные координаты
            this.curX = curX;
            this.curY = curY;

            // буферы вершин
            createVertices();

            // скорости по осям
            this.velX = velX;
            this.velY = velY;
        }

        public Bullet(Texture texture, float curX, float curY, float velX, float velY, BulletType
            bulletType)
            : this(texture, curX, curY, velX, velY)
        {
            thisType = bulletType;
        }

        // буферы вершин
        void createVertices()
        {
            // вершины
            VertexData[] vertices = new VertexData[4];

            vertices[0].position.x = 0.0f;
            vertices[0].position.y = 0.0f;
            vertices[0].texCoord.s = 0.0f;
            vertices[0].texCoord.t = 0.0f;

            vertices[1].position.x = sprite.Width;
            vertices[1].position.y = 0.0f;
            vertices[1].texCoord.s = 1.0f;
            vertices[1].texCoord.t = 0.0f;

            vertices[2].position.x = sprite.Width;
            vertices[2].position.y = sprite.Height;
            vertices[2].texCoord.s = 1.0f;
            vertices[2].texCoord.t = 1.0f;

            vertices[3].position.x = 0.0f;
            vertices[3].position.y = sprite.Height;
            vertices[3].texCoord.s = 0.0f;
            vertices[3].texCoord.t = 1.0f;

            // загоняем в VBO
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

        // обновляем координаты
        public void update(long delta)
        {
            curX += delta * velX * 0.001f;
            curY += delta * velY * 0.001f;
        }

        // рисуем
        public void render()
        {
            GL.BindTexture(TextureTarget.Texture2D, sprite.ID);

            GL.BindVertexArray(VAO);

            mainProgram.setMultiColor(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            //mainProgram.setModelView(Matrix4.Identity);
            mainProgram.setModelView(Matrix4.CreateTranslation(curX, curY, 0.0f));
            mainProgram.updateModelView();
            GL.DrawElements(BeginMode.TriangleFan, 4, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        // прикрепляем шейдерную программу
        public static void attachProgram(ShaderProgram program)
        {
            mainProgram = program;
        }        

        // вылетел ли снаряд за пределы экрана
        public bool OutOfScreen
        {
            get
            {
                return curX > Program.WIDTH || curX + sprite.Width < 0 ||
                    curY > Program.HEIGHT || curY + sprite.Height < 0;
            }
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

        // существует ли снаряд?
        public bool Alive
        {
            get
            {
                return isAlive;
            }
        }

        public BulletType ThisType
        {
            get
            {
                return thisType;
            }
        }

        // текущая позиция середины снаряда
        public VertexPosition Position
        {
            get
            {
                return new VertexPosition(curX + sprite.Width / 2, curY + sprite.Height / 2);
            }
        }
    }
}
