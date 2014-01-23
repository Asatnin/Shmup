using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Shmup
{
    class Texture : IDisposable
    {
        protected int mTextureID;

        protected int mTextureWidth;
        protected int mTextureHeight;        

        public void Dispose()
        {
            freeTexture();
        }

        protected void freeTexture()
        {
            if (mTextureID != 0)
                GL.DeleteTextures(1, ref mTextureID);
            mTextureID = 0;
        }

        public bool loadTextureFromFile(string filename)
        {
            freeTexture();

            Bitmap bitmap = new Bitmap(filename);
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new Rectangle(0, 0,
                bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            bool success = loadTextureFromPixels32(bitmap.Width, bitmap.Height, data.Scan0);
            bitmap.UnlockBits(data);
            return success;
        }

        protected bool loadTextureFromPixels32(int width, int height, IntPtr pixels)
        {
            mTextureWidth = width;
            mTextureHeight = height;

            GL.GenTextures(1, out mTextureID);

            GL.BindTexture(TextureTarget.Texture2D, mTextureID);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, mTextureWidth,
                mTextureHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return true;
        }

        public int ID
        {
            get
            {
                return mTextureID;
            }
        }

        public int Width
        {
            get
            {
                return mTextureWidth;
            }
        }

        public int Height
        {
            get
            {
                return mTextureHeight;
            }
        }
    }
}
