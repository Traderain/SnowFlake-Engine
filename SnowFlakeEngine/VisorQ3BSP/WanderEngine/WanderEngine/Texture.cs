#region GPL License

/*
Copyright (c) 2010 Miguel Angel Guirado López

This file is part of VisorQ3BSP.

    VisorQ3BSP is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    VisorQ3BSP is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with VisorQ3BSP.  If not, see <http://www.gnu.org/licenses/>.
 
    This project is based on previous work by Michael Hansen 
    "Game Programming Final" http://www.gamedev.net/community/forums/topic.asp?topic_id=379347.
*/

#endregion

using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace SnowflakeEngine.WanderEngine
{
    public class Texture
    {
        private readonly RotateFlipType rotateFlip = RotateFlipType.Rotate180FlipX;
        public int Height;
        public int TextureID;
        public int Width;

        public Texture(Bitmap ExistingImage)
        {
            LoadBitmap(ExistingImage, false);
        }

        public Texture(string FileName)
            : this(FileName, RotateFlipType.Rotate180FlipX)
        {
        }

        public Texture(string FileName, RotateFlipType pRotateFlip)
        {
            rotateFlip = pRotateFlip;
            var currentImage = new Bitmap(FileName);
            LoadBitmap(currentImage, false);
            currentImage.Dispose();
        }

        public Texture(string FileName, bool IsSkyBox)
        {
            var currentImage = new Bitmap(FileName);
            LoadBitmap(currentImage, IsSkyBox);
            currentImage.Dispose();
        }

        private void LoadBitmap(Bitmap CurrentImage, bool IsSkyBox)
        {
            Width = CurrentImage.Width;
            Height = CurrentImage.Height;
            CurrentImage.RotateFlip(rotateFlip);
            var rect = new Rectangle(0, 0, Width, Height);
            var bitmapdata = CurrentImage.LockBits(rect, ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            TextureID = -1;
            GL.GenTextures(1, out TextureID);
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            if (IsSkyBox)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int) TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int) TextureWrapMode.ClampToEdge);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int) TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int) TextureWrapMode.Repeat);
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear);

            // Requieres OpenGL >= 1.4
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1); // 1 = True
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, Width, Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, bitmapdata.Scan0);
            CurrentImage.UnlockBits(bitmapdata);
        }
    }
}