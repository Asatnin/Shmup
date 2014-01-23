using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Shmup
{
    // класс - шейдерная программа
    class ShaderProgram : IDisposable
    {
        // id шейдерной программы
        int mProgramID;

        /*------ расположение атрибутов ------*/
        // атрибут координат
        int vertexPositionLocation;
        // атрибут координат текстуры
        int texCoordLocation;
        /*------------------------------------*/

        // юниформ цвета
        int multiColorLocation;

        // юниформ номера текстуры
        int textureUnitLocation;

        // юниформ смещения текстуры
        int offsetXLocation;

        // Projection matrix
        Matrix4 myProjectionMatrix = Matrix4.Identity;
        int projectionMatrixLocation;

        // ModelView matrix
        Matrix4 myModelViewMatrix = Matrix4.Identity;
        int modelViewMatrixLocation;

        public int ID
        {
            get
            {
                return mProgramID;
            }
        }

        // загружаем текст шейдера из файла
        public int loadShaderFromFile(string path, ShaderType shaderType)
        {
            int shaderID = 0;
            string shaderString;
            StreamReader st = new StreamReader(path);

            if (st != null)
            {
                shaderString = st.ReadToEnd();

                shaderID = GL.CreateShader(shaderType);

                GL.ShaderSource(shaderID, shaderString);

                GL.CompileShader(shaderID);

                int shaderCompiled = 0;
                GL.GetShader(shaderID, ShaderParameter.CompileStatus, out shaderCompiled);
                if (shaderCompiled == 0)
                {
                    Console.WriteLine("Unable to compile shader: {0}! Source: {1}", shaderID,
                        shaderString);
                    printShaderLog(shaderID);
                    GL.DeleteShader(shaderID);
                    shaderID = 0;
                }
            }
            else
                Console.WriteLine("Unable to load shader from file {0}!", path);

            return shaderID;
        }

        // создаём шейдерную программу
        public bool loadProgram()
        {
            mProgramID = GL.CreateProgram();

            // создаём вертексный шейдер
            int vertexShader = loadShaderFromFile("Shaders/vertexShader.vert",
                ShaderType.VertexShader);
            if (vertexShader == 0)
            {
                GL.DeleteProgram(mProgramID);
                mProgramID = 0;
                return false;
            }
            GL.AttachShader(mProgramID, vertexShader);

            // создаём фрагментный шейдер
            int fragmentShader = loadShaderFromFile("Shaders/fragmentShader.frag",
                ShaderType.FragmentShader);
            if (fragmentShader == 0)
            {
                GL.DeleteProgram(mProgramID);
                mProgramID = 0;
                return false;
            }
            GL.AttachShader(mProgramID, fragmentShader);

            // линкуем шейдерную программу
            GL.LinkProgram(mProgramID);
            int programSuccess = 0;
            GL.GetProgram(mProgramID, ProgramParameter.LinkStatus, out programSuccess);
            if (programSuccess == 0)
            {
                Console.WriteLine("Error linking program {0}!", mProgramID);
                printProgramLog(mProgramID);
                GL.DeleteProgram(mProgramID);
                mProgramID = 0;
                return false;
            }

            vertexPositionLocation = GL.GetAttribLocation(mProgramID, "vertexPosition");
            if (vertexPositionLocation == -1)
                Console.WriteLine("{0} is not valid glsl program variable!", "vertexPosition");

            texCoordLocation = GL.GetAttribLocation(mProgramID, "LTexCoord");
            if (texCoordLocation == -1)
                Console.WriteLine("{0} is not valid glsl program variable!", "LTexCoord");

            multiColorLocation = GL.GetUniformLocation(mProgramID, "multiColor");
            if (multiColorLocation == -1)
                Console.WriteLine("{0} is not valid glsl program variable!", "multiColor");

            textureUnitLocation = GL.GetUniformLocation(mProgramID, "textureUnit");
            if (textureUnitLocation == -1)
                Console.WriteLine("{0} is not valid glsl program variable!", "textureUnit");

            offsetXLocation = GL.GetUniformLocation(mProgramID, "offsetX");
            if (textureUnitLocation == -1)
                Console.WriteLine("{0} is not valid glsl program variable!", "offsetX");


            projectionMatrixLocation = GL.GetUniformLocation(mProgramID,
                "myProjectionMatrix");
            if (projectionMatrixLocation == -1)
                Console.WriteLine("{0} is not valid glsl program variable!",
                    "myProjectionMatrix");

            modelViewMatrixLocation = GL.GetUniformLocation(mProgramID,
                "myModelViewMatrix");
            if (modelViewMatrixLocation == -1)
                Console.WriteLine("{0} is not valid glsl program variable!",
                    "myModelViewMatrix");

            return true;
        }

        // подобие деструктора
        public void Dispose()
        {
            freeProgram();
        }

        // удаляем шейдерную программу
        public void freeProgram()
        {
            GL.DeleteProgram(mProgramID);
        }

        // присодиняем созданную нами шейдерную программу
        public bool bind()
        {
            GL.UseProgram(mProgramID);

            if (GL.GetError() != ErrorCode.NoError)
            {
                Console.WriteLine("Error binding shader program!");
                printProgramLog(mProgramID);
                return false;
            }

            return true;
        }

        // присоединяем умалчиваемую шейдерную программу
        public void unbind()
        {
            GL.UseProgram(0);
        }

        // печатаем инфу о шейдерной программе
        void printProgramLog(int program)
        {
            if (GL.IsProgram(program))
            {
                string infoLog = GL.GetProgramInfoLog(program);
                if (infoLog.Length > 0)
                    Console.WriteLine(infoLog);
            }
            else
                Console.WriteLine("Name {0} is not a program!", program);
        }

        // печатаем инфу о шейдере-параметре
        void printShaderLog(int shader)
        {
            if (GL.IsShader(shader))
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                if (infoLog.Length > 0)
                    Console.WriteLine(infoLog);
            }
            else
                Console.WriteLine("Name {0} is not a shader!", shader);
        }

        // устанавливаем информацию о вершинах
        public void setVertexPointer(int stride, IntPtr offset)
        {
            GL.VertexAttribPointer(vertexPositionLocation, 2, VertexAttribPointerType.Float,
                false, stride, offset);
        }

        // устанавливаем информацию о текстуре
        public void setTexCoordPointer(int stride, IntPtr offset)
        {
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float,
                false, stride, offset);
        }

        // действия с цветом и номером текстуры
        public void setMultiColor(Vector4 color)
        {
            GL.Uniform4(multiColorLocation, color);
        }

        // устанавливаем юниформ номера текстуры
        public void setTextureUnit(int unit)
        {
            GL.Uniform1(textureUnitLocation, unit);
        }

        // юниформ смещения текстуры
        public void setOffsetX(float offsetX)
        {
            GL.Uniform1(offsetXLocation, offsetX);
        }

        // включаем все атрибуты
        public void enableDataPointers()
        {
            GL.EnableVertexAttribArray(vertexPositionLocation);
            GL.EnableVertexAttribArray(texCoordLocation);
        }

        // выключаем все атрибуты
        public void disableDataPointers()
        {
            GL.DisableVertexAttribArray(texCoordLocation);
            GL.DisableVertexAttribArray(vertexPositionLocation);
        }

        // действия с матрицами
        public void setProjection(Matrix4 matrix)
        {
            myProjectionMatrix = matrix;
        }

        public void setModelView(Matrix4 matrix)
        {
            myModelViewMatrix = matrix;
        }

        public void leftMultProjection(Matrix4 matrix)
        {
            myProjectionMatrix = matrix * myProjectionMatrix;
        }

        public void leftMultModelView(Matrix4 matrix)
        {
            myModelViewMatrix = matrix * myModelViewMatrix;
        }

        public void updateProjection()
        {
            GL.UniformMatrix4(projectionMatrixLocation, false, ref myProjectionMatrix);
        }

        public void updateModelView()
        {
            GL.UniformMatrix4(modelViewMatrixLocation, false, ref myModelViewMatrix);
        }
    }
}