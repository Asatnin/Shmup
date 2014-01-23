using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Shmup
{
    class StaticSprite
    {
        // ссылка на шейдерную программу
        protected static ShaderProgram mainProgram;

        // ссылка на текстуру
        protected Texture sprite;

        // ширина, высота
        protected int width, height;

        // позиция верхнего левого угла для рисования
        protected float curX, curY;

        // движение
        protected float velX = 0.0f, velY = 0.0f;

        // IBO, VBO, VAO...
        protected int VBO, IBO, VAO;

        // конструктор
        public StaticSprite(Texture sprite, float curX, float curY)
        {
            this.sprite = sprite;

            width = sprite.Width;
            height = sprite.Height;

            this.curX = curX;
            this.curY = curY;

            createVertices();
        }

        public StaticSprite(Texture sprite, float curX, float curY, float velX, float velY)
            : this(sprite, curX, curY)
        {
            this.velX = velX;
            this.velY = velY;
        }

        // конструктор
        public StaticSprite(Texture sprite, float curX, float curY, int width, int height)
        {
            this.sprite = sprite;

            this.width = width;
            this.height = height;

            this.curX = curX;
            this.curY = curY;

            createVertices();
        }

        public StaticSprite(Texture sprite, float curX, float curY, int width, int height,
            float velX, float velY)
            : this(sprite, curX, curY, width, height)
        {
            this.velX = velX;
            this.velY = velY;
        }

        // вершины
        protected void createVertices()
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

        // рисуем
        public void render()
        {
            GL.BindTexture(TextureTarget.Texture2D, sprite.ID);
            GL.BindVertexArray(VAO);

            mainProgram.setMultiColor(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            mainProgram.setModelView(Matrix4.CreateTranslation(curX, curY, 0.0f));
            mainProgram.updateModelView();
            GL.DrawElements(BeginMode.TriangleFan, 4, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        // обновляем
        public void update(long delta)
        {
            curX += delta * velX * 0.001f;
            curY += delta * velY * 0.001f;
        }

        public bool OutOfScreen
        {
            get
            {
                return curX > Program.WIDTH || curX + sprite.Width < 0 ||
                    curY > Program.HEIGHT || curY + sprite.Height < 0;
            }
        }

        // прикрепляем шейдерную программу
        public static void attachProgram(ShaderProgram program)
        {
            mainProgram = program;
        }
    }
}
