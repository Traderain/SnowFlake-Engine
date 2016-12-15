
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

using System;
using System.Drawing;

using OpenTK.Graphics.OpenGL;

namespace WanderEngine
{
    public class Text2D : IDisposable
    {
        private int fontbase; // Base Display List For The Font
        public int textureID = 0;
        int fontWidth = 8;

        public Text2D(string filename)
        {
        	this.fontWidth = 16;
            BuildFont(filename);
        }

        // --- Private Static Methods ---
        #region BuildFont()
        /// <summary>
        ///     Build our font display list.
        /// </summary>
        private void BuildFont(string fileName)
        {
            float cx;                                                           // Holds Our X Character Coord
            float cy;

            Texture tex = new Texture(fileName, RotateFlipType.RotateNoneFlipY);
            textureID = tex.TextureID;

            // Holds Our Y Character Coord
            fontbase = GL.GenLists(256);                                      // Creating 256 Display Lists
            GL.BindTexture(TextureTarget.Texture2D, textureID);                     // Select Our Font Texture
            for (int loop = 0; loop < 256; loop++)
            {                                 // Loop Through All 256 Lists
                cx = ((float)(loop % fontWidth)) / fontWidth;                             // X Position Of Current Character
                cy = ((float)(loop / fontWidth)) / fontWidth;                             // Y Position Of Current Character
                GL.NewList(fontbase + loop, ListMode.Compile);                   // Start Building A List
                GL.Begin(BeginMode.Quads);                                    // Use A Quad For Each Character
                GL.TexCoord2(cx, 1 - cy);                            // Texture Coord (Top Left)
                GL.Vertex2(0, 0);                                    // Vertex Coord (Bottom Left)
                GL.TexCoord2(cx + 0.0625f, 1 - cy);                  // Texture Coord (Top Right)
                GL.Vertex2(fontWidth, 0);                                   // Vertex Coord (Bottom Right)
                GL.TexCoord2(cx + 0.0625f, 1 - cy - 0.0625f);        // Texture Coord (Bottom Right)
                GL.Vertex2(fontWidth, fontWidth);                                  // Vertex Coord (Top Right)
                GL.TexCoord2(cx, 1 - cy - 0.0625f);                  // Texture Coord (Bottom Left)
                GL.Vertex2(0, fontWidth);                                   // Vertex Coord (Top Left)
                GL.End();                                                 // Done Building Our Quad (Character)
                GL.Translate(10, 0, 0);                                  // Move To The Right Of The Character
                GL.EndList();                                                 // Done Building The Display List
            }                                                                   // Loop Until All 256 Are Built
        }
        #endregion BuildFont()

        #region glPrint(int x, int y, string text, int charset)
        /// <summary>
        ///     Where the printing happens.
        /// </summary>
        /// <param name="x">
        ///     X coordinate.
        /// </param>
        /// <param name="y">
        ///     Y coordinate.
        /// </param>
        /// <param name="text">
        ///     The text to print.
        /// </param>
        /// <param name="charset">
        ///     The character set.
        /// </param>
        public void glPrint(int x, int y, string text, int charset)
        {
            if (charset > 1)
            {
                charset = 1;
            }
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            GL.BindTexture(TextureTarget.Texture2D, textureID);                     // Select Our Font Texture
            GL.Disable(EnableCap.DepthTest);                                     // Disables Depth Testing
            GL.MatrixMode(MatrixMode.Projection);                                  // Select The Projection Matrix
            GL.PushMatrix();                                                  // Store The Projection Matrix
            GL.LoadIdentity();                                            // Reset The Projection Matrix
            GL.Ortho(0, 640, 480, 0, -1, 1);                              // Set Up An Ortho Screen
            GL.MatrixMode(MatrixMode.Modelview);                               // Select The Modelview Matrix
            GL.PushMatrix();                                              // Store The Modelview Matrix
            GL.LoadIdentity();                                        // Reset The Modelview Matrix
            GL.Translate(x, y, 0);                                   // Position The Text (0,0 - Bottom Left)
            GL.ListBase(fontbase - 32 + (128 * charset));             // Choose The Font Set (0 or 1)
            // .NET: We can't draw text directly, it's a string!
            byte[] textbytes = new byte[text.Length];
            for (int i = 0; i < text.Length; i++) textbytes[i] = (byte)text[i];
            GL.CallLists(text.Length, ListNameType.UnsignedByte, textbytes);// Write The Text To The Screen
            GL.MatrixMode(MatrixMode.Projection);                          // Select The Projection Matrix
            GL.PopMatrix();                                               // Restore The Old Projection Matrix
            GL.MatrixMode(MatrixMode.Modelview);                               // Select The Modelview Matrix
            GL.PopMatrix();                                                   // Restore The Old Projection Matrix
            GL.Enable(EnableCap.DepthTest);                                      // Enables Depth Testing
            GL.Disable(EnableCap.Blend);
       }
        #endregion glPrint(int x, int y, string text, int charset)

        #region Miembros de IDisposable

        public void Dispose()
        {
            GL.DeleteLists(fontbase, 256);  // Delete All 256 Display Lists
        }

        #endregion
    }
}
