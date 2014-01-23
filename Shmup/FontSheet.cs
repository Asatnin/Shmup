using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Shmup
{
    unsafe class FontSheet : Texture
    {
        // ссылка на шейдерную программу
        static ShaderProgram mainProgram;

        private int[] mPixels = null;

        private List<LFRect> mClips = new List<LFRect>();

        private int mVertexDataBuffer;
        int* mIndexBuffers;

        float mSpace; // кол-во пикселей между буквами
        float mLineHeight; // разница между самым высоким и самым низким пикселями
        float mNewLine; // разница между строками

        float coef; // коэффициент изменения размера шрифта

        VertexData[] vertexData; // координаты вершин и текстурные координаты

        public FontSheet(float coef)
        {
            this.coef = coef;
        }

        public bool loadPixelsFromFile(string filename)
        {
            Bitmap bitmap = new Bitmap(filename);
            System.Drawing.Imaging.BitmapData data =
                bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            mTextureWidth = bitmap.Width;
            mTextureHeight = bitmap.Height;

            int size = bitmap.Width * bitmap.Height;
            mPixels = new int[size];
            Marshal.Copy(data.Scan0, mPixels, 0, size);

            bitmap.UnlockBits(data);

            return true;
        }

        public bool loadTextureFromPixels32()
        {
            if (mTextureID == 0 && mPixels != null)
            {
                GL.GenTextures(1, out mTextureID);

                GL.BindTexture(TextureTarget.Texture2D, mTextureID);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    mTextureWidth, mTextureHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte,
                    mPixels);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.Linear);

                GL.BindTexture(TextureTarget.Texture2D, 0);

                return true;
            }
            else
                return false;
        }

        unsafe bool generateDataBuffer()
        {
            if (mTextureID != 0 && mClips.Count > 0)
            {
                int totalSprites = mClips.Count;
                vertexData = new VertexData[totalSprites * 4];
                mIndexBuffers = (int*)Marshal.AllocHGlobal(totalSprites * sizeof(int)).ToPointer();

                GL.GenBuffers(1, out mVertexDataBuffer);

                GL.GenBuffers(totalSprites, mIndexBuffers);

                float tw = mTextureWidth;
                float th = mTextureHeight;
                uint[] spriteIndices = new uint[4];
                int j = -1;

                float vTop = 0.0f;
                float vBottom = 0.0f;
                float vLeft = 0.0f;
                float vRight = 0.0f;

                for (uint i = 0; i < totalSprites; i++)
                {
                    spriteIndices[0] = i * 4;
                    spriteIndices[1] = i * 4 + 1;
                    spriteIndices[2] = i * 4 + 2;
                    spriteIndices[3] = i * 4 + 3;

                    //устанавливаем координаты вершин для наших спрайтов
                    //с началом коорадинат в левом верхнем углу
                    j++;
                    vTop = 0.0f;
                    vBottom = mClips[j].h * coef;
                    vLeft = 0.0f;
                    vRight = mClips[j].w * coef;

                    //верхний левый угол
                    vertexData[spriteIndices[0]].position.x = vLeft;
                    vertexData[spriteIndices[0]].position.y = vTop;
                    vertexData[spriteIndices[0]].texCoord.s = mClips[j].x / tw;
                    vertexData[spriteIndices[0]].texCoord.t = mClips[j].y / th;

                    //верхний правый угол
                    vertexData[spriteIndices[1]].position.x = vRight;
                    vertexData[spriteIndices[1]].position.y = vTop;
                    vertexData[spriteIndices[1]].texCoord.s = (mClips[j].x + mClips[j].w) / tw;
                    vertexData[spriteIndices[1]].texCoord.t = mClips[j].y / th;

                    //нижний правый угол
                    vertexData[spriteIndices[2]].position.x = vRight;
                    vertexData[spriteIndices[2]].position.y = vBottom;
                    vertexData[spriteIndices[2]].texCoord.s = (mClips[j].x + mClips[j].w) / tw;
                    vertexData[spriteIndices[2]].texCoord.t = (mClips[j].y + mClips[j].h) / th;

                    //нижний левый угол
                    vertexData[spriteIndices[3]].position.x = vLeft;
                    vertexData[spriteIndices[3]].position.y = vBottom;
                    vertexData[spriteIndices[3]].texCoord.s = mClips[j].x / tw;
                    vertexData[spriteIndices[3]].texCoord.t = (mClips[j].y + mClips[j].h) / th;

                    //помещаем в память наш индексный массив
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, mIndexBuffers[j]);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(4 * sizeof(uint)),
                        spriteIndices, BufferUsageHint.StaticDraw);
                }

                //помещаем в память
                GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexDataBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer,
                    (IntPtr)(4 * totalSprites * sizeof(VertexData)), vertexData,
                    BufferUsageHint.StaticDraw);

                return true;
            }
            else
                return false;
        }

        public void changeCoef(float newCoef)
        {
            coef = newCoef;
        }

        public unsafe bool loadBitmap(string filename)
        {
            bool success = true;

            //цвет: ARGB (Alpha-Red-Green-Blue)
            int blackPixel = -16777216;
            byte* colors;

            if (loadPixelsFromFile(filename))
            {
                float cellW = mTextureWidth / 16.0f;
                float cellH = mTextureHeight / 16.0f;

                int top = (int)cellH;
                int bottom = 0;
                int aBottom = 0;

                int pX = 0;
                int pY = 0;

                int bX = 0;
                int bY = 0;

                uint currentChar = 0;
                LFRect nextClip = new LFRect();

                for (int rows = 0; rows < 16; rows++)
                    for (int cols = 0; cols < 16; cols++)
                    {
                        bX = (int)cellW * cols;
                        bY = (int)cellH * rows;

                        nextClip.x = cellW * cols;
                        nextClip.y = cellH * rows;

                        nextClip.w = cellW;
                        nextClip.h = cellH;

                        //ищем крайний левый пиксель
                        for (int pCol = 0; pCol < cellW; pCol++)
                            for (int pRow = 0; pRow < cellH; pRow++)
                            {
                                pX = bX + pCol;
                                pY = bY + pRow;

                                if (getPixel32(pX, pY) != blackPixel)
                                {
                                    nextClip.x = pX;

                                    pCol = (int)cellW;
                                    pRow = (int)cellH;
                                }
                            }

                        //ищем крайний правый пиксель
                        for (int pCol_w = (int)cellW - 1; pCol_w >= 0; pCol_w--)
                            for (int pRow_w = 0; pRow_w < cellH; pRow_w++)
                            {
                                pX = bX + pCol_w;
                                pY = bY + pRow_w;

                                if (getPixel32(pX, pY) != blackPixel)
                                {
                                    nextClip.w = (pX - nextClip.x) + 1;

                                    pCol_w = -1;
                                    pRow_w = (int)cellH;
                                }
                            }

                        //ищем крайний верхний пиксель
                        for (int pRow = 0; pRow < cellH; pRow++)
                            for (int pCol = 0; pCol < cellW; pCol++)
                            {
                                pX = bX + pCol;
                                pY = bY + pRow;

                                if (getPixel32(pX, pY) != blackPixel)
                                {
                                    if (pRow < top)
                                        top = pRow;

                                    pCol = (int)cellW;
                                    pRow = (int)cellH;
                                }
                            }

                        //ищем крайний нижний пиксель
                        for (int pRow_b = (int)cellH - 1; pRow_b >= 0; pRow_b--)
                            for (int pCol_b = 0; pCol_b < cellW; pCol_b++)
                            {
                                pX = bX + pCol_b;
                                pY = bY + pRow_b;

                                if (getPixel32(pX, pY) != blackPixel)
                                {
                                    if (currentChar == 'A')
                                        aBottom = pRow_b;

                                    if (pRow_b > bottom)
                                        bottom = pRow_b;

                                    pRow_b = -1;
                                    pCol_b = (int)cellW;
                                }
                            }

                        mClips.Add(new LFRect(nextClip));
                        currentChar++;
                    }

                for (int i = 0; i < mClips.Count; i++)
                {
                    mClips[i].y += top;
                    mClips[i].h -= top;
                }

                int PIXEL_COUNT = mTextureHeight * mTextureWidth;
                for (int i = 0; i < PIXEL_COUNT; i++)
                {
                    fixed (int* p = &mPixels[i])
                    {
                        colors = (byte*)p;

                        if (mPixels[i] == blackPixel)
                            mPixels[i] = 0;
                    }
                }

                loadTextureFromPixels32();
                generateDataBuffer();

                GL.BindTexture(TextureTarget.Texture2D, mTextureID);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int)TextureParameterName.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int)TextureParameterName.ClampToBorder);

                mSpace = cellW / 2;
                mNewLine = aBottom - top;
                mLineHeight = bottom - top;

                return success;
            }
            else
                return false;
        }

        public int getPixel32(int x, int y)
        {
            return mPixels[y * mTextureWidth + x];
        }

        public void renderText(float x, float y, string text, Vector4 color)
        {
            if (mTextureID != 0)
            {
                float dx = x;
                float dy = y;

                //GL.Translate(x, y, 0.0f);
                mainProgram.setModelView(Matrix4.CreateTranslation(x, y, 0.0f));

                GL.BindTexture(TextureTarget.Texture2D, mTextureID);

                //GL.EnableClientState(ArrayCap.VertexArray);
                //GL.EnableClientState(ArrayCap.TextureCoordArray);
                mainProgram.enableDataPointers();

                GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexDataBuffer);

                /*GL.TexCoordPointer(2, TexCoordPointerType.Float, sizeof(VertexData),
                    Marshal.OffsetOf(typeof(VertexData), "texCoord"));*/
                mainProgram.setTexCoordPointer(sizeof(VertexData),
                    Marshal.OffsetOf(typeof(VertexData), "texCoord"));

                /*GL.VertexPointer(2, VertexPointerType.Float, sizeof(VertexData),
                    Marshal.OffsetOf(typeof(VertexData), "position"));*/
                mainProgram.setVertexPointer(sizeof(VertexData),
                    Marshal.OffsetOf(typeof(VertexData), "position"));

                mainProgram.setMultiColor(color);

                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == ' ')
                    {
                        //GL.Translate(mSpace, 0.0f, 0.0f);
                        mainProgram.leftMultModelView(Matrix4.CreateTranslation(mSpace * coef, 0.0f,
                            0.0f));
                        dx += mSpace * coef;
                    }
                    else
                        if (text[i] == '\n')
                        {
                            //GL.Translate(x - dx, mNewLine, 0.0f);
                            mainProgram.leftMultModelView(Matrix4.CreateTranslation(x - dx,
                                mNewLine, 0.0f));
                            dx = x;
                            dy = y;
                        }
                        else
                        {
                            int ascii = text[i];

                            mainProgram.updateModelView();

                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mIndexBuffers[ascii]);
                            GL.DrawElements(BeginMode.Quads, 4, DrawElementsType.UnsignedInt,
                                0);

                            //GL.Translate(mClips[ascii].w, 0.0f, 0.0f);
                            mainProgram.leftMultModelView(Matrix4.CreateTranslation(
                                mClips[ascii].w * coef, 0.0f, 0.0f));
                            dx += mClips[ascii].w * coef;
                        }
                }

                //GL.DisableClientState(ArrayCap.TextureCoordArray);
                //GL.DisableClientState(ArrayCap.VertexArray);
                mainProgram.disableDataPointers();
            }
        }

        // прикрепляем шейдерную программу
        public static void attachProgram(ShaderProgram program)
        {
            mainProgram = program;
        }
    }
}
