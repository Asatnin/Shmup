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
    class Background
    {
        // ссылка на шейдерную программу
        static ShaderProgram mainProgram;

        // смещение текстуры подвижного фона
        float offsetX = 0.0f;

        // спрайт неподвижного фона
        Texture field;

        // спрайт движущегося фона
        Texture movField;

        // VBO, IBO, VAO для звёзд, фона
        int IBO, fieldVBO, fieldVAO, movFieldVBO, movFieldVAO;

        // конструктор бэкграунда
        public Background()
        {
            field = new Texture();
            field.loadTextureFromFile("Sprites/Background/farback.gif");

            movField = new Texture();
            movField.loadTextureFromFile("Sprites/Background/starfield.png");

            createVertices();
        }

        // вершины и буферы для отрисовки
        void createVertices()
        {
            // вершины для натягивания текстуры звезды
            VertexData[] vertices = new VertexData[4];

            // фон
            vertices[0].position.x = 0.0f; vertices[0].position.y = 0.0f;
            vertices[0].texCoord.s = 0.0f;
            vertices[0].texCoord.t = 0.0f;

            vertices[1].position.x = Program.WIDTH; vertices[1].position.y = 0.0f;
            vertices[1].texCoord.s = Program.WIDTH < field.Width ?
                Program.WIDTH * 1.0f / field.Width : field.Width;
            vertices[1].texCoord.t = 0.0f;

            vertices[2].position.x = Program.WIDTH; vertices[2].position.y = Program.HEIGHT;
            vertices[2].texCoord.s = vertices[1].texCoord.s;
            vertices[2].texCoord.t = Program.HEIGHT < field.Height ?
                Program.HEIGHT * 1.0f / field.Height : field.Height;

            vertices[3].position.x = 0.0f; vertices[3].position.y = Program.HEIGHT;
            vertices[3].texCoord.s = 0.0f;
            vertices[3].texCoord.t = vertices[2].texCoord.t;

            GL.GenBuffers(1, out fieldVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, fieldVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(4 *
                Marshal.SizeOf(typeof(VertexData))), vertices, BufferUsageHint.StaticDraw);

            // подвижный фон
            vertices[0].position.x = 0.0f; vertices[0].position.y = 0.0f;
            vertices[0].texCoord.s = 0.0f; vertices[0].texCoord.t = 0.0f;

            vertices[1].position.x = Program.WIDTH; vertices[1].position.y = 0.0f;
            vertices[1].texCoord.s = Program.WIDTH < movField.Width ?
                Program.WIDTH * 1.0f / movField.Width : movField.Width;
            vertices[1].texCoord.t = 0.0f;

            vertices[2].position.x = Program.WIDTH; vertices[2].position.y = Program.HEIGHT;
            vertices[2].texCoord.s = vertices[1].texCoord.s;
            vertices[2].texCoord.t = Program.HEIGHT < movField.Height ?
                Program.HEIGHT * 1.0f / movField.Height : movField.Height;

            vertices[3].position.x = 0.0f; vertices[3].position.y = Program.HEIGHT;
            vertices[3].texCoord.s = 0.0f;
            vertices[3].texCoord.t = vertices[2].texCoord.t;

            GL.GenBuffers(1, out movFieldVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, movFieldVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(4 *
                Marshal.SizeOf(typeof(VertexData))), vertices, BufferUsageHint.StaticDraw);

            // индексы
            uint[] indices = { 0, 1, 2, 3 };
            GL.GenBuffers(1, out IBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(4 * sizeof(uint)), indices,
                BufferUsageHint.StaticDraw);

            //VAO фона
            GL.GenVertexArrays(1, out fieldVAO);
            GL.BindVertexArray(fieldVAO);

            mainProgram.enableDataPointers();

            GL.BindBuffer(BufferTarget.ArrayBuffer, fieldVBO);
            mainProgram.setTexCoordPointer(Marshal.SizeOf(typeof(VertexData)),
                Marshal.OffsetOf(typeof(VertexData), "texCoord"));
            mainProgram.setVertexPointer(Marshal.SizeOf(typeof(VertexData)),
                Marshal.OffsetOf(typeof(VertexData), "position"));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);

            GL.BindVertexArray(0);

            //VAO подвижного фона
            GL.GenVertexArrays(1, out movFieldVAO);
            GL.BindVertexArray(movFieldVAO);

            mainProgram.enableDataPointers();

            GL.BindBuffer(BufferTarget.ArrayBuffer, movFieldVBO);
            mainProgram.setTexCoordPointer(Marshal.SizeOf(typeof(VertexData)),
                Marshal.OffsetOf(typeof(VertexData), "texCoord"));
            mainProgram.setVertexPointer(Marshal.SizeOf(typeof(VertexData)),
                Marshal.OffsetOf(typeof(VertexData), "position"));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);

            GL.BindVertexArray(0);

            mainProgram.disableDataPointers();
        }

        public void update(long delta)
        {
            // обновляем смещение текстуры подвижного фона
            offsetX += delta * 0.0003f;
            if (offsetX >= movField.Width)
                offsetX = 0.0f;
        }

        // рисуем фон
        public void render()
        {
            mainProgram.setMultiColor(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));

            // фон
            GL.BindTexture(TextureTarget.Texture2D, field.ID); ;

            GL.BindVertexArray(fieldVAO);
            mainProgram.setModelView(Matrix4.Identity);
            mainProgram.updateModelView();

            GL.DrawElements(BeginMode.TriangleFan, 4, DrawElementsType.UnsignedInt, 0);

            // подвижный фон
            GL.BindTexture(TextureTarget.Texture2D, movField.ID);

            GL.BindVertexArray(movFieldVAO);

            mainProgram.setOffsetX(offsetX);
            GL.DrawElements(BeginMode.TriangleFan, 4, DrawElementsType.UnsignedInt, 0);
            mainProgram.setOffsetX(0.0f);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindVertexArray(0);
        }

        // прикрепляем шейдерную программу
        public static void attachProgram(ShaderProgram program)
        {
            mainProgram = program;
        }
    }
}
