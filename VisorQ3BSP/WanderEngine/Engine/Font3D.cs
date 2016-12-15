﻿
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

namespace WanderEngine
{
    using System;
    using System.Text;
    using OpenTK.Graphics.OpenGL;

    public class Font3D : IDisposable
    {
        private int Base = 0;
        private Texture FontTexture = null;
        private float ScreenHeight = 0f;
        private float ScreenWidth = 0f;

        //public Font3D(SimpleOpenGlControl GlControl, string FontImage)
        public Font3D(int width, int height, string FontImage)
        {
            this.FontTexture = new Texture(FontImage);
            this.Base = GL.GenLists(0x100);
            this.ScreenWidth = width;
            this.ScreenHeight = height;
            float s = 0f;
            float t = 1f;
            GL.BindTexture(TextureTarget.Texture2D, this.FontTexture.TextureID);
            for (int i = 0; i < 0x100; i++)
            {
                GL.NewList(this.Base + i, ListMode.Compile);
                GL.Begin(BeginMode.Quads);
                GL.TexCoord2(s, t - 0.0625f);
                GL.Vertex2(0.0, 0.0);
                GL.TexCoord2(s + 0.0625f, t - 0.0625f);
                GL.Vertex2(8.0, 0.0);
                GL.TexCoord2(s + 0.0625f, t);
                GL.Vertex2(8.0, 8.0);
                GL.TexCoord2(s, t);
                GL.Vertex2(0.0, 8.0);
                GL.End();
                GL.Translate(5.0, 0.0, 0.0);
                GL.EndList();
                s += 0.0625f;
                if (s >= 1f)
                {
                    s = 0f;
                    t -= 0.0625f;
                }
            }
        }

        public void Dispose()
        {
            GL.DeleteLists(this.Base, 0x100);
        }

        public void Render(float X, float Y, float Z, string Text)
        {
            GL.PushMatrix();
            GL.Translate(X, Y, Z);
            float[] buffer = new float[0x10];
            /*float[] matrix = new float[0x10];
            GL.GetFloat(GetPName.ModelviewMatrix, buffer);
            matrix[0] = buffer[0];
            matrix[1] = buffer[4];
            matrix[2] = buffer[8];
            matrix[3] = 0f;
            matrix[4] = buffer[1];
            matrix[5] = buffer[5];
            matrix[6] = buffer[9];
            matrix[7] = 0f;
            matrix[8] = buffer[2];
            matrix[9] = buffer[6];
            matrix[10] = buffer[10];
            matrix[11] = 0f;
            matrix[12] = 0f;
            matrix[13] = 0f;
            matrix[14] = 0f;
            matrix[15] = 1f;
            GL.MultMatrix(matrix);*/
            GL.BindTexture(TextureTarget.Texture2D, this.FontTexture.TextureID);
            GL.Color4(1f, 1f, 1f, 1f);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            GL.Disable(EnableCap.CullFace);
            GL.Translate(Text.Length * -4f, 0f, Text.Length * -4f);
            GL.ListBase((this.Base - 0x20) + 0x80);
            GL.CallLists(Text.Length, ListNameType.UnsignedByte, Encoding.ASCII.GetBytes(Text));
            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.PopMatrix();
        }
    }
}
