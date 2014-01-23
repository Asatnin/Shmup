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
    class Explosion
    {
        // ссылка на шейдерную программу
        static ShaderProgram mainProgram;

        // спрайт взрыва
        Texture sprite;

        // общее количество фреймов
        int frames;

        // количество фреймов в одной строке по-горизонтали
        int horizontal_frames;

        // в течение какого времени воспроизводить взрыв
        int time;

        // общее время показа, время между фреймами
        long allTime = 0, curTime = 0;

        // задержка между фреймами
        float delay;

        // VBO, IBO, texVBO
        int VBO, IBO, texVBO;

        float width, height;

        // текущий кадр анимации
        int currentFrame = 0;

        // клипы для анимации
        TexturePosition[][] clips;

        // место для отрисовки
        float curX, curY;

        // конструктор
        public Explosion(Texture sprite, int frames, int horizontal_frames, int time,
            float curX, float curY)
        {
            this.sprite = sprite;

            this.frames = frames;

            this.horizontal_frames = horizontal_frames;

            this.time = time;

            delay = time * 1.0f / frames;

            width = sprite.Width * 1.0f / horizontal_frames;
            height = sprite.Height * 1.0f / (frames / horizontal_frames);

            this.curX = curX;
            this.curY = curY;

            createVertices();
        }

        // вершины и буферы
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

            createTextureCoord();
        }

        // текстурные координаты
        void createTextureCoord()
        {
            clips = new TexturePosition[frames][];

            for (int i = 0; i < frames; i++)
            {
                int row = i / horizontal_frames, column = i % horizontal_frames;

                clips[i] = new TexturePosition[4];
                for (int j = 0; j < clips[i].Length; j++)
                {
                    clips[i][j].s = (width * column + width * (j == 0 || j == 3 ? 0 : 1)) / sprite.Width;
                    clips[i][j].t = (height * row + height * (j > 1 ? 1 : 0)) / sprite.Height;
                }
            }

            // загоняем первый клип в буфер
            GL.GenBuffers(1, out texVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, texVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(4 *
                Marshal.SizeOf(typeof(TexturePosition))), clips[0],
                BufferUsageHint.DynamicDraw);
        }

        // обновляем
        public void update(long delta)
        {
            if (allTime < time)
            {
                // выбираем подходящую анимацию
                int prevFrame = currentFrame;
                allTime += delta;
                curTime += delta;
                if (curTime >= delay)
                {
                    currentFrame++;
                    if (currentFrame >= frames)
                        currentFrame = 0;
                    curTime = 0;
                }

                if (prevFrame != currentFrame)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, texVBO);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(4 *
                        Marshal.SizeOf(typeof(TexturePosition))), clips[currentFrame]);
                }
            }
        }

        // рисуем
        public void render()
        {
            if (allTime < time)
            {
                mainProgram.enableDataPointers();

                GL.BindTexture(TextureTarget.Texture2D, sprite.ID);

                GL.BindBuffer(BufferTarget.ArrayBuffer, texVBO);
                mainProgram.setTexCoordPointer(0, IntPtr.Zero);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                mainProgram.setVertexPointer(0, IntPtr.Zero);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);

                mainProgram.setMultiColor(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                mainProgram.setModelView(Matrix4.CreateTranslation(curX - width / 2,
                    curY - height / 2, 0.0f));
                mainProgram.updateModelView();
                GL.DrawElements(BeginMode.TriangleFan, 4, DrawElementsType.UnsignedInt, 0);

                GL.BindTexture(TextureTarget.Texture2D, 0);

                mainProgram.disableDataPointers();
            }
        }

        public bool Exist
        {
            get
            {
                return allTime < time;
            }
        }

        // прикрепляем шейдерную программу
        public static void attachProgram(ShaderProgram program)
        {
            mainProgram = program;
        }
    }
}
