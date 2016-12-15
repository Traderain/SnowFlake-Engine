
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
using System.IO;
using OpenTK.Graphics.OpenGL;
using Math3D;

namespace WanderEngine
{

    public class MD2Model
    {
        private int AnimEndFrame = 0x27;
        private float AnimPercent = 4f;
        private int AnimStartFrame = 0;
        private int CurrentFrame = 0;
        private MD2Face[] Faces = null;
        private MD2Frame[] Frames = null;
        //private GlCommand[] GlCommands = null;
        private MD2Header Header = null;
        private static readonly int HeaderSizeInBytes = 0x44;
        private float Interpolation = 0f;
        private Vector3f m_BoundMax = new Vector3f();
        private Vector3f m_BoundMin = new Vector3f();
        private Vector3f m_Center = new Vector3f();
        private AnimationState m_ModelState = AnimationState.Stand;
        private float MaskB = 0f;
        private float MaskG = 0f;
        private float MaskR = 0f;
        private Texture ModelTexture;
        private int NextFrame = 0;
        private int NumFaces = 0;
        private int NumFramePoints = 0;
        private int NumFrames = 0;
        private int NumSkins = 0;
        private int NumTextureCoords = 0;
        private int NumVertices = 0;
        public float Pitch = 0f;
        private int PointsPerFrame = 0;
        public Vector3f Position = new Vector3f();
        public bool RepeatAnimation = true;
        private static readonly int SkinNameSizeInBytes = 0x40;
        private string[] Skins = null;
        private Vector2f[] TextureCoords = null;
        private bool UseMask = false;
        private Vector3f[] Vertices = null;
        public float Yaw = 0f;

        public MD2Model(string FileName, string TextureFileName)
        {
            this.LoadModel(FileName, TextureFileName);
        }

        public void Animate(int StartFrame, int EndFrame, float Percent)
        {
            if (StartFrame > this.CurrentFrame)
            {
                this.CurrentFrame = StartFrame;
            }
            if (this.Interpolation > 1f)
            {
                this.Interpolation = 0f;
                this.CurrentFrame++;
                if (this.RepeatAnimation)
                {
                    if (this.CurrentFrame >= EndFrame)
                    {
                        this.CurrentFrame = StartFrame;
                    }
                    this.NextFrame = this.CurrentFrame + 1;
                    if (this.NextFrame >= EndFrame)
                    {
                        this.NextFrame = StartFrame;
                    }
                }
                else if (this.CurrentFrame >= this.AnimEndFrame)
                {
                    this.CurrentFrame = this.AnimEndFrame;
                    this.NextFrame = this.CurrentFrame;
                }
                else
                {
                    this.NextFrame = this.CurrentFrame + 1;
                }
            }
            int num = this.CurrentFrame * this.Header.NumPoints;
            int num2 = this.NextFrame * this.Header.NumPoints;
            GL.PushMatrix();
            GL.Translate(this.Position.X, this.Position.Y, this.Position.Z);
            GL.Rotate(this.Yaw, 0f, 1f, 0f);
            GL.Rotate(this.Pitch, 0f, 0f, 1f);
            if (this.UseMask)
            {
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
                GL.Color4(this.MaskR, this.MaskG, this.MaskB, 0.9f);
            }
            GL.CullFace(CullFaceMode.Back);
            GL.BindTexture(TextureTarget.Texture2D, this.ModelTexture.TextureID);
            GL.Begin(BeginMode.Triangles);
            for (int i = 0; i < this.NumFaces; i++)
            {
                Vector3f vector = this.Vertices[num + this.Faces[i].VertexIndices[0]];
                Vector3f vector2 = this.Vertices[num + this.Faces[i].VertexIndices[2]];
                Vector3f vector3 = this.Vertices[num + this.Faces[i].VertexIndices[1]];
                Vector3f vector4 = this.Vertices[num2 + this.Faces[i].VertexIndices[0]];
                Vector3f vector5 = this.Vertices[num2 + this.Faces[i].VertexIndices[2]];
                Vector3f vector6 = this.Vertices[num2 + this.Faces[i].VertexIndices[1]];
                Vector3f vector7 = new Vector3f();
                vector7.X = vector.X + (this.Interpolation * (vector4.X - vector.X));
                vector7.Y = vector.Y + (this.Interpolation * (vector4.Y - vector.Y));
                vector7.Z = vector.Z + (this.Interpolation * (vector4.Z - vector.Z));
                GL.TexCoord2(this.TextureCoords[this.Faces[i].TextureIndices[0]].X, this.TextureCoords[this.Faces[i].TextureIndices[0]].Y);
                GL.Vertex3(vector7.X, vector7.Y, vector7.Z);
                Vector3f vector8 = new Vector3f();
                vector8.X = vector2.X + (this.Interpolation * (vector5.X - vector2.X));
                vector8.Y = vector2.Y + (this.Interpolation * (vector5.Y - vector2.Y));
                vector8.Z = vector2.Z + (this.Interpolation * (vector5.Z - vector2.Z));
                GL.TexCoord2(this.TextureCoords[this.Faces[i].TextureIndices[2]].X, this.TextureCoords[this.Faces[i].TextureIndices[2]].Y);
                GL.Vertex3(vector8.X, vector8.Y, vector8.Z);
                Vector3f vector9 = new Vector3f();
                vector9.X = vector3.X + (this.Interpolation * (vector6.X - vector3.X));
                vector9.Y = vector3.Y + (this.Interpolation * (vector6.Y - vector3.Y));
                vector9.Z = vector3.Z + (this.Interpolation * (vector6.Z - vector3.Z));
                GL.TexCoord2(this.TextureCoords[this.Faces[i].TextureIndices[1]].X, this.TextureCoords[this.Faces[i].TextureIndices[1]].Y);
                GL.Vertex3(vector9.X, vector9.Y, vector9.Z);
            }
            GL.End();
            GL.CullFace(CullFaceMode.Front);
            GL.PopMatrix();
            if (this.UseMask)
            {
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Replace);
            }
            this.Interpolation += Percent;
        }

        private void LoadModel(string FileName, string TextureFileName)
        {
            BinaryReader reader = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read));
            byte[] headerBytes = reader.ReadBytes(HeaderSizeInBytes);
            this.Header = new MD2Header(headerBytes);
            this.NumSkins = this.Header.NumSkins;
            this.Skins = new string[this.NumSkins];
            this.NumFaces = this.Header.NumTriangles;
            this.Faces = new MD2Face[this.NumFaces];
            this.NumFrames = this.Header.NumFrames;
            this.NumFramePoints = this.Header.NumPoints;
            this.PointsPerFrame = this.Header.NumTriangles * 3;
            this.Frames = new MD2Frame[this.NumFrames];
            this.NumTextureCoords = this.Header.NumTextureCoords;
            this.TextureCoords = new Vector2f[this.NumTextureCoords];
            this.NumVertices = this.Header.NumFrames * this.Header.NumPoints;
            this.Vertices = new Vector3f[this.NumVertices];
            reader.BaseStream.Seek((long)this.Header.SkinsOffset, SeekOrigin.Begin);
            for (int i = 0; i < this.NumSkins; i++)
            {
                byte[] stringData = reader.ReadBytes(SkinNameSizeInBytes);
                this.Skins[i] = this.ParseString(stringData);
            }
            this.ModelTexture = new Texture(TextureFileName);
            reader.BaseStream.Seek((long)this.Header.TextureCoordsOffset, SeekOrigin.Begin);
            for (int j = 0; j < this.NumTextureCoords; j++)
            {
                this.TextureCoords[j] = new Vector2f();
                short num3 = reader.ReadInt16();
                short num4 = reader.ReadInt16();
                this.TextureCoords[j].X = ((float)num3) / ((float)this.Header.SkinWidth);
                this.TextureCoords[j].Y = -(((float)num4) / ((float)this.Header.SkinHeight));
            }
            reader.BaseStream.Seek((long)this.Header.TrianglesOffset, SeekOrigin.Begin);
            for (int k = 0; k < this.NumFaces; k++)
            {
                BinaryReader reader2 = new BinaryReader(new MemoryStream(reader.ReadBytes(MD2Face.SizeInBytes)));
                this.Faces[k] = new MD2Face();
                this.Faces[k].VertexIndices[0] = reader2.ReadInt16();
                this.Faces[k].VertexIndices[1] = reader2.ReadInt16();
                this.Faces[k].VertexIndices[2] = reader2.ReadInt16();
                this.Faces[k].TextureIndices[0] = reader2.ReadInt16();
                this.Faces[k].TextureIndices[1] = reader2.ReadInt16();
                this.Faces[k].TextureIndices[2] = reader2.ReadInt16();
            }
            reader.BaseStream.Seek((long)this.Header.FramesOffset, SeekOrigin.Begin);
            for (int m = 0; m < this.NumFrames; m++)
            {
                this.Frames[m] = new MD2Frame();
                this.Frames[m].FramePoints = new MD2FramePoint[this.Header.NumPoints];
                this.Frames[m].Scale.X = reader.ReadSingle();
                this.Frames[m].Scale.Y = reader.ReadSingle();
                this.Frames[m].Scale.Z = reader.ReadSingle();
                this.Frames[m].Translate.X = reader.ReadSingle();
                this.Frames[m].Translate.Y = reader.ReadSingle();
                this.Frames[m].Translate.Z = reader.ReadSingle();
                this.Frames[m].Name = this.ParseString(reader.ReadBytes(0x10));
                for (int num7 = 0; num7 < this.NumFramePoints; num7++)
                {
                    this.Frames[m].FramePoints[num7] = new MD2FramePoint();
                    this.Frames[m].FramePoints[num7].ScaledVertex[0] = reader.ReadByte();
                    this.Frames[m].FramePoints[num7].ScaledVertex[1] = reader.ReadByte();
                    this.Frames[m].FramePoints[num7].ScaledVertex[2] = reader.ReadByte();
                    this.Frames[m].FramePoints[num7].LightNormalIndex = reader.ReadByte();
                }
            }
            int index = 0;
            for (int n = 0; n < this.NumFrames; n++)
            {
                for (int num10 = 0; num10 < this.NumFramePoints; num10++)
                {
                    this.Vertices[index] = new Vector3f();
                    this.Vertices[index].X = (this.Frames[n].FramePoints[num10].ScaledVertex[0] * this.Frames[n].Scale.X) + this.Frames[n].Translate.X;
                    this.Vertices[index].Z = -((this.Frames[n].FramePoints[num10].ScaledVertex[1] * this.Frames[n].Scale.Y) + this.Frames[n].Translate.Y);
                    this.Vertices[index].Y = (this.Frames[n].FramePoints[num10].ScaledVertex[2] * this.Frames[n].Scale.Z) + this.Frames[n].Translate.Z;
                    if (this.Vertices[index].X < this.m_BoundMin.X)
                    {
                        this.m_BoundMin.X = this.Vertices[index].X;
                    }
                    if (this.Vertices[index].Y < this.m_BoundMin.Y)
                    {
                        this.m_BoundMin.Y = this.Vertices[index].Y;
                    }
                    if (this.Vertices[index].Z < this.m_BoundMin.Z)
                    {
                        this.m_BoundMin.Z = this.Vertices[index].Z;
                    }
                    if (this.Vertices[index].X > this.m_BoundMax.X)
                    {
                        this.m_BoundMax.X = this.Vertices[index].X;
                    }
                    if (this.Vertices[index].Y > this.m_BoundMax.Y)
                    {
                        this.m_BoundMax.Y = this.Vertices[index].Y;
                    }
                    if (this.Vertices[index].Z > this.m_BoundMax.Z)
                    {
                        this.m_BoundMax.Z = this.Vertices[index].Z;
                    }
                    index++;
                }
            }
            this.m_Center.X = (Math.Abs(this.BoundMax.X) - Math.Abs(this.BoundMin.X)) / 2f;
            this.m_Center.Y = (Math.Abs(this.BoundMax.Y) - Math.Abs(this.BoundMin.Y)) / 2f;
            this.m_Center.Z = (Math.Abs(this.BoundMax.Z) - Math.Abs(this.BoundMin.Z)) / 2f;
            reader.Close();
        }

        private string ParseString(byte[] StringData)
        {
            string str = "";
            for (int i = 0; i < StringData.Length; i++)
            {
                if (StringData[i] != 0)
                {
                    str = str + Convert.ToChar(StringData[i]);
                }
            }
            return str;
        }

        public bool RayCollides(Vector3f Start, Vector3f End)
        {
            return BSPFile.RayBoxCollision(Start, End, this.BoundMin + this.Position, this.BoundMax + this.Position);
        }

        private void SetModelState(AnimationState NewState)
        {
            this.m_ModelState = NewState;
            switch (NewState)
            {
                case AnimationState.Stand:
                    this.AnimStartFrame = 0;
                    this.AnimEndFrame = 30;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.Run:
                    this.AnimStartFrame = 40;
                    this.AnimEndFrame = 0x2d;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.Attack:
                    this.AnimStartFrame = 0x2e;
                    this.AnimEndFrame = 0x35;
                    this.AnimPercent = 0.5f;
                    break;

                case AnimationState.PainA:
                    this.AnimStartFrame = 0x36;
                    this.AnimEndFrame = 0x39;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.PainB:
                    this.AnimStartFrame = 0x3a;
                    this.AnimEndFrame = 0x3d;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.PainC:
                    this.AnimStartFrame = 0x3e;
                    this.AnimEndFrame = 0x41;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.Jump:
                    this.AnimStartFrame = 0x42;
                    this.AnimEndFrame = 0x47;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.Flip:
                    this.AnimStartFrame = 0x48;
                    this.AnimEndFrame = 0x53;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.Salute:
                    this.AnimStartFrame = 0x54;
                    this.AnimEndFrame = 0x5e;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.FallBack:
                    this.AnimStartFrame = 0x5f;
                    this.AnimEndFrame = 0x6f;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.Wave:
                    this.AnimStartFrame = 0x70;
                    this.AnimEndFrame = 0x7a;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.Point:
                    this.AnimStartFrame = 0x7b;
                    this.AnimEndFrame = 0x86;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.CrouchStand:
                    this.AnimStartFrame = 0x87;
                    this.AnimEndFrame = 0x99;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.CrouchWalk:
                    this.AnimStartFrame = 0x9a;
                    this.AnimEndFrame = 0x9f;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.CrouchAttack:
                    this.AnimStartFrame = 160;
                    this.AnimEndFrame = 0xa8;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.CrouchPain:
                    this.AnimStartFrame = 0xa9;
                    this.AnimEndFrame = 0xac;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.CrouchDeath:
                    this.AnimStartFrame = 0xad;
                    this.AnimEndFrame = 0xb1;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.DeathFallBack:
                    this.AnimStartFrame = 0xb2;
                    this.AnimEndFrame = 0xb7;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.DeathFallFoward:
                    this.AnimStartFrame = 0xb8;
                    this.AnimEndFrame = 0xbd;
                    this.AnimPercent = 3f;
                    break;

                case AnimationState.DeathFallBackSlow:
                    this.AnimStartFrame = 190;
                    this.AnimEndFrame = 0xc5;
                    this.AnimPercent = 4f;
                    break;

                case AnimationState.Boom:
                    this.AnimStartFrame = 0xc6;
                    this.AnimEndFrame = 0xc6;
                    this.AnimPercent = 4f;
                    break;
            }
            this.CurrentFrame = this.AnimStartFrame;
            this.NextFrame = this.CurrentFrame + 1;
            this.Interpolation = 0f;
        }

        public void Update(float TimeElapsed)
        {
            this.UseMask = false;
            this.Animate(this.AnimStartFrame, this.AnimEndFrame, this.AnimPercent * TimeElapsed);
        }

        public void Update(float TimeElapsed, float R, float G, float B)
        {
            this.UseMask = true;
            this.MaskR = R;
            this.MaskG = G;
            this.MaskB = B;
            this.Animate(this.AnimStartFrame, this.AnimEndFrame, this.AnimPercent * TimeElapsed);
        }

        public Vector3f BoundMax
        {
            get
            {
                return this.m_BoundMax;
            }
        }

        public Vector3f BoundMin
        {
            get
            {
                return this.m_BoundMin;
            }
        }

        public Vector3f Center
        {
            get
            {
                return this.m_Center;
            }
        }

        public AnimationState ModelState
        {
            get
            {
                return this.m_ModelState;
            }
            set
            {
                this.SetModelState(value);
            }
        }
    }

    public class MD2Face
    {
        public static readonly int SizeInBytes = 12;
        public short[] TextureIndices = new short[3];
        public short[] VertexIndices = new short[3];
    }
    public class MD2Frame
    {
        public MD2FramePoint[] FramePoints;
        public string Name = "";
        public Vector3f Scale = new Vector3f();
        public Vector3f Translate = new Vector3f();
    }
    public class MD2FramePoint
    {
        public byte LightNormalIndex;
        public byte[] ScaledVertex = new byte[3];
    }

    public class MD2Header
    {
        public int EndOffset;
        public int FrameSize;
        public int FramesOffset;
        public int GLCommandsOffset;
        public int Identity;
        public int NumFrames;
        public int NumGLCommands;
        public int NumPoints;
        public int NumSkins;
        public int NumTextureCoords;
        public int NumTriangles;
        public int SkinHeight;
        public int SkinsOffset;
        public int SkinWidth;
        public int TextureCoordsOffset;
        public int TrianglesOffset;
        public int Version;

        public MD2Header()
        {
        }

        public MD2Header(byte[] HeaderBytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(HeaderBytes));
            this.Identity = reader.ReadInt32();
            this.Version = reader.ReadInt32();
            if (this.Version != 8)
            {
                throw new Exception("Must be a version 8 MD2 model");
            }
            this.SkinWidth = reader.ReadInt32();
            this.SkinHeight = reader.ReadInt32();
            this.FrameSize = reader.ReadInt32();
            this.NumSkins = reader.ReadInt32();
            this.NumPoints = reader.ReadInt32();
            this.NumTextureCoords = reader.ReadInt32();
            this.NumTriangles = reader.ReadInt32();
            this.NumGLCommands = reader.ReadInt32();
            this.NumFrames = reader.ReadInt32();
            this.SkinsOffset = reader.ReadInt32();
            this.TextureCoordsOffset = reader.ReadInt32();
            this.TrianglesOffset = reader.ReadInt32();
            this.FramesOffset = reader.ReadInt32();
            this.GLCommandsOffset = reader.ReadInt32();
            this.EndOffset = reader.ReadInt32();
            reader.Close();
        }
    }
}
