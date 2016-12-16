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
using Math3D;
using OpenTK.Graphics.OpenGL;

namespace WanderEngine.WanderEngine
{
    public class MD2Model
    {
        private static readonly int HeaderSizeInBytes = 0x44;
        private static readonly int SkinNameSizeInBytes = 0x40;
        private int AnimEndFrame = 0x27;
        private float AnimPercent = 4f;
        private int AnimStartFrame;
        private int CurrentFrame;
        private MD2Face[] Faces;
        private MD2Frame[] Frames;
        //private GlCommand[] GlCommands = null;
        private MD2Header Header;
        private float Interpolation;
        private AnimationState m_ModelState = AnimationState.Stand;
        private float MaskB;
        private float MaskG;
        private float MaskR;
        private Texture ModelTexture;
        private int NextFrame;
        private int NumFaces;
        private int NumFramePoints;
        private int NumFrames;
        private int NumSkins;
        private int NumTextureCoords;
        private int NumVertices;
        public float Pitch = 0f;
        private int PointsPerFrame;
        public Vector3f Position = new Vector3f();
        public bool RepeatAnimation = true;
        private string[] Skins;
        private Vector2f[] TextureCoords;
        private bool UseMask;
        private Vector3f[] Vertices;
        public float Yaw = 0f;

        public MD2Model(string FileName, string TextureFileName)
        {
            LoadModel(FileName, TextureFileName);
        }

        public Vector3f BoundMax { get; } = new Vector3f();
        public Vector3f BoundMin { get; } = new Vector3f();
        public Vector3f Center { get; } = new Vector3f();

        public AnimationState ModelState
        {
            get { return m_ModelState; }
            set { SetModelState(value); }
        }

        public void Animate(int StartFrame, int EndFrame, float Percent)
        {
            if (StartFrame > CurrentFrame)
            {
                CurrentFrame = StartFrame;
            }
            if (Interpolation > 1f)
            {
                Interpolation = 0f;
                CurrentFrame++;
                if (RepeatAnimation)
                {
                    if (CurrentFrame >= EndFrame)
                    {
                        CurrentFrame = StartFrame;
                    }
                    NextFrame = CurrentFrame + 1;
                    if (NextFrame >= EndFrame)
                    {
                        NextFrame = StartFrame;
                    }
                }
                else if (CurrentFrame >= AnimEndFrame)
                {
                    CurrentFrame = AnimEndFrame;
                    NextFrame = CurrentFrame;
                }
                else
                {
                    NextFrame = CurrentFrame + 1;
                }
            }
            var num = CurrentFrame*Header.NumPoints;
            var num2 = NextFrame*Header.NumPoints;
            GL.PushMatrix();
            GL.Translate(Position.X, Position.Y, Position.Z);
            GL.Rotate(Yaw, 0f, 1f, 0f);
            GL.Rotate(Pitch, 0f, 0f, 1f);
            if (UseMask)
            {
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate);
                GL.Color4(MaskR, MaskG, MaskB, 0.9f);
            }
            GL.CullFace(CullFaceMode.Back);
            GL.BindTexture(TextureTarget.Texture2D, ModelTexture.TextureID);
            GL.Begin(BeginMode.Triangles);
            for (var i = 0; i < NumFaces; i++)
            {
                var vector = Vertices[num + Faces[i].VertexIndices[0]];
                var vector2 = Vertices[num + Faces[i].VertexIndices[2]];
                var vector3 = Vertices[num + Faces[i].VertexIndices[1]];
                var vector4 = Vertices[num2 + Faces[i].VertexIndices[0]];
                var vector5 = Vertices[num2 + Faces[i].VertexIndices[2]];
                var vector6 = Vertices[num2 + Faces[i].VertexIndices[1]];
                var vector7 = new Vector3f();
                vector7.X = vector.X + (Interpolation*(vector4.X - vector.X));
                vector7.Y = vector.Y + (Interpolation*(vector4.Y - vector.Y));
                vector7.Z = vector.Z + (Interpolation*(vector4.Z - vector.Z));
                GL.TexCoord2(TextureCoords[Faces[i].TextureIndices[0]].X, TextureCoords[Faces[i].TextureIndices[0]].Y);
                GL.Vertex3(vector7.X, vector7.Y, vector7.Z);
                var vector8 = new Vector3f();
                vector8.X = vector2.X + (Interpolation*(vector5.X - vector2.X));
                vector8.Y = vector2.Y + (Interpolation*(vector5.Y - vector2.Y));
                vector8.Z = vector2.Z + (Interpolation*(vector5.Z - vector2.Z));
                GL.TexCoord2(TextureCoords[Faces[i].TextureIndices[2]].X, TextureCoords[Faces[i].TextureIndices[2]].Y);
                GL.Vertex3(vector8.X, vector8.Y, vector8.Z);
                var vector9 = new Vector3f();
                vector9.X = vector3.X + (Interpolation*(vector6.X - vector3.X));
                vector9.Y = vector3.Y + (Interpolation*(vector6.Y - vector3.Y));
                vector9.Z = vector3.Z + (Interpolation*(vector6.Z - vector3.Z));
                GL.TexCoord2(TextureCoords[Faces[i].TextureIndices[1]].X, TextureCoords[Faces[i].TextureIndices[1]].Y);
                GL.Vertex3(vector9.X, vector9.Y, vector9.Z);
            }
            GL.End();
            GL.CullFace(CullFaceMode.Front);
            GL.PopMatrix();
            if (UseMask)
            {
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Replace);
            }
            Interpolation += Percent;
        }

        private void LoadModel(string FileName, string TextureFileName)
        {
            var reader = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read));
            var headerBytes = reader.ReadBytes(HeaderSizeInBytes);
            Header = new MD2Header(headerBytes);
            NumSkins = Header.NumSkins;
            Skins = new string[NumSkins];
            NumFaces = Header.NumTriangles;
            Faces = new MD2Face[NumFaces];
            NumFrames = Header.NumFrames;
            NumFramePoints = Header.NumPoints;
            PointsPerFrame = Header.NumTriangles*3;
            Frames = new MD2Frame[NumFrames];
            NumTextureCoords = Header.NumTextureCoords;
            TextureCoords = new Vector2f[NumTextureCoords];
            NumVertices = Header.NumFrames*Header.NumPoints;
            Vertices = new Vector3f[NumVertices];
            reader.BaseStream.Seek(Header.SkinsOffset, SeekOrigin.Begin);
            for (var i = 0; i < NumSkins; i++)
            {
                var stringData = reader.ReadBytes(SkinNameSizeInBytes);
                Skins[i] = ParseString(stringData);
            }
            ModelTexture = new Texture(TextureFileName);
            reader.BaseStream.Seek(Header.TextureCoordsOffset, SeekOrigin.Begin);
            for (var j = 0; j < NumTextureCoords; j++)
            {
                TextureCoords[j] = new Vector2f();
                var num3 = reader.ReadInt16();
                var num4 = reader.ReadInt16();
                TextureCoords[j].X = num3/((float) Header.SkinWidth);
                TextureCoords[j].Y = -(num4/((float) Header.SkinHeight));
            }
            reader.BaseStream.Seek(Header.TrianglesOffset, SeekOrigin.Begin);
            for (var k = 0; k < NumFaces; k++)
            {
                var reader2 = new BinaryReader(new MemoryStream(reader.ReadBytes(MD2Face.SizeInBytes)));
                Faces[k] = new MD2Face();
                Faces[k].VertexIndices[0] = reader2.ReadInt16();
                Faces[k].VertexIndices[1] = reader2.ReadInt16();
                Faces[k].VertexIndices[2] = reader2.ReadInt16();
                Faces[k].TextureIndices[0] = reader2.ReadInt16();
                Faces[k].TextureIndices[1] = reader2.ReadInt16();
                Faces[k].TextureIndices[2] = reader2.ReadInt16();
            }
            reader.BaseStream.Seek(Header.FramesOffset, SeekOrigin.Begin);
            for (var m = 0; m < NumFrames; m++)
            {
                Frames[m] = new MD2Frame();
                Frames[m].FramePoints = new MD2FramePoint[Header.NumPoints];
                Frames[m].Scale.X = reader.ReadSingle();
                Frames[m].Scale.Y = reader.ReadSingle();
                Frames[m].Scale.Z = reader.ReadSingle();
                Frames[m].Translate.X = reader.ReadSingle();
                Frames[m].Translate.Y = reader.ReadSingle();
                Frames[m].Translate.Z = reader.ReadSingle();
                Frames[m].Name = ParseString(reader.ReadBytes(0x10));
                for (var num7 = 0; num7 < NumFramePoints; num7++)
                {
                    Frames[m].FramePoints[num7] = new MD2FramePoint();
                    Frames[m].FramePoints[num7].ScaledVertex[0] = reader.ReadByte();
                    Frames[m].FramePoints[num7].ScaledVertex[1] = reader.ReadByte();
                    Frames[m].FramePoints[num7].ScaledVertex[2] = reader.ReadByte();
                    Frames[m].FramePoints[num7].LightNormalIndex = reader.ReadByte();
                }
            }
            var index = 0;
            for (var n = 0; n < NumFrames; n++)
            {
                for (var num10 = 0; num10 < NumFramePoints; num10++)
                {
                    Vertices[index] = new Vector3f();
                    Vertices[index].X = (Frames[n].FramePoints[num10].ScaledVertex[0]*Frames[n].Scale.X) +
                                        Frames[n].Translate.X;
                    Vertices[index].Z =
                        -((Frames[n].FramePoints[num10].ScaledVertex[1]*Frames[n].Scale.Y) + Frames[n].Translate.Y);
                    Vertices[index].Y = (Frames[n].FramePoints[num10].ScaledVertex[2]*Frames[n].Scale.Z) +
                                        Frames[n].Translate.Z;
                    if (Vertices[index].X < BoundMin.X)
                    {
                        BoundMin.X = Vertices[index].X;
                    }
                    if (Vertices[index].Y < BoundMin.Y)
                    {
                        BoundMin.Y = Vertices[index].Y;
                    }
                    if (Vertices[index].Z < BoundMin.Z)
                    {
                        BoundMin.Z = Vertices[index].Z;
                    }
                    if (Vertices[index].X > BoundMax.X)
                    {
                        BoundMax.X = Vertices[index].X;
                    }
                    if (Vertices[index].Y > BoundMax.Y)
                    {
                        BoundMax.Y = Vertices[index].Y;
                    }
                    if (Vertices[index].Z > BoundMax.Z)
                    {
                        BoundMax.Z = Vertices[index].Z;
                    }
                    index++;
                }
            }
            Center.X = (Math.Abs(BoundMax.X) - Math.Abs(BoundMin.X))/2f;
            Center.Y = (Math.Abs(BoundMax.Y) - Math.Abs(BoundMin.Y))/2f;
            Center.Z = (Math.Abs(BoundMax.Z) - Math.Abs(BoundMin.Z))/2f;
            reader.Close();
        }

        private string ParseString(byte[] StringData)
        {
            var str = "";
            for (var i = 0; i < StringData.Length; i++)
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
            return BSPFile.RayBoxCollision(Start, End, BoundMin + Position, BoundMax + Position);
        }

        private void SetModelState(AnimationState NewState)
        {
            m_ModelState = NewState;
            switch (NewState)
            {
                case AnimationState.Stand:
                    AnimStartFrame = 0;
                    AnimEndFrame = 30;
                    AnimPercent = 4f;
                    break;

                case AnimationState.Run:
                    AnimStartFrame = 40;
                    AnimEndFrame = 0x2d;
                    AnimPercent = 4f;
                    break;

                case AnimationState.Attack:
                    AnimStartFrame = 0x2e;
                    AnimEndFrame = 0x35;
                    AnimPercent = 0.5f;
                    break;

                case AnimationState.PainA:
                    AnimStartFrame = 0x36;
                    AnimEndFrame = 0x39;
                    AnimPercent = 4f;
                    break;

                case AnimationState.PainB:
                    AnimStartFrame = 0x3a;
                    AnimEndFrame = 0x3d;
                    AnimPercent = 4f;
                    break;

                case AnimationState.PainC:
                    AnimStartFrame = 0x3e;
                    AnimEndFrame = 0x41;
                    AnimPercent = 4f;
                    break;

                case AnimationState.Jump:
                    AnimStartFrame = 0x42;
                    AnimEndFrame = 0x47;
                    AnimPercent = 4f;
                    break;

                case AnimationState.Flip:
                    AnimStartFrame = 0x48;
                    AnimEndFrame = 0x53;
                    AnimPercent = 4f;
                    break;

                case AnimationState.Salute:
                    AnimStartFrame = 0x54;
                    AnimEndFrame = 0x5e;
                    AnimPercent = 4f;
                    break;

                case AnimationState.FallBack:
                    AnimStartFrame = 0x5f;
                    AnimEndFrame = 0x6f;
                    AnimPercent = 4f;
                    break;

                case AnimationState.Wave:
                    AnimStartFrame = 0x70;
                    AnimEndFrame = 0x7a;
                    AnimPercent = 4f;
                    break;

                case AnimationState.Point:
                    AnimStartFrame = 0x7b;
                    AnimEndFrame = 0x86;
                    AnimPercent = 4f;
                    break;

                case AnimationState.CrouchStand:
                    AnimStartFrame = 0x87;
                    AnimEndFrame = 0x99;
                    AnimPercent = 4f;
                    break;

                case AnimationState.CrouchWalk:
                    AnimStartFrame = 0x9a;
                    AnimEndFrame = 0x9f;
                    AnimPercent = 4f;
                    break;

                case AnimationState.CrouchAttack:
                    AnimStartFrame = 160;
                    AnimEndFrame = 0xa8;
                    AnimPercent = 4f;
                    break;

                case AnimationState.CrouchPain:
                    AnimStartFrame = 0xa9;
                    AnimEndFrame = 0xac;
                    AnimPercent = 4f;
                    break;

                case AnimationState.CrouchDeath:
                    AnimStartFrame = 0xad;
                    AnimEndFrame = 0xb1;
                    AnimPercent = 4f;
                    break;

                case AnimationState.DeathFallBack:
                    AnimStartFrame = 0xb2;
                    AnimEndFrame = 0xb7;
                    AnimPercent = 4f;
                    break;

                case AnimationState.DeathFallFoward:
                    AnimStartFrame = 0xb8;
                    AnimEndFrame = 0xbd;
                    AnimPercent = 3f;
                    break;

                case AnimationState.DeathFallBackSlow:
                    AnimStartFrame = 190;
                    AnimEndFrame = 0xc5;
                    AnimPercent = 4f;
                    break;

                case AnimationState.Boom:
                    AnimStartFrame = 0xc6;
                    AnimEndFrame = 0xc6;
                    AnimPercent = 4f;
                    break;
            }
            CurrentFrame = AnimStartFrame;
            NextFrame = CurrentFrame + 1;
            Interpolation = 0f;
        }

        public void Update(float TimeElapsed)
        {
            UseMask = false;
            Animate(AnimStartFrame, AnimEndFrame, AnimPercent*TimeElapsed);
        }

        public void Update(float TimeElapsed, float R, float G, float B)
        {
            UseMask = true;
            MaskR = R;
            MaskG = G;
            MaskB = B;
            Animate(AnimStartFrame, AnimEndFrame, AnimPercent*TimeElapsed);
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
            var reader = new BinaryReader(new MemoryStream(HeaderBytes));
            Identity = reader.ReadInt32();
            Version = reader.ReadInt32();
            if (Version != 8)
            {
                throw new Exception("Must be a version 8 MD2 model");
            }
            SkinWidth = reader.ReadInt32();
            SkinHeight = reader.ReadInt32();
            FrameSize = reader.ReadInt32();
            NumSkins = reader.ReadInt32();
            NumPoints = reader.ReadInt32();
            NumTextureCoords = reader.ReadInt32();
            NumTriangles = reader.ReadInt32();
            NumGLCommands = reader.ReadInt32();
            NumFrames = reader.ReadInt32();
            SkinsOffset = reader.ReadInt32();
            TextureCoordsOffset = reader.ReadInt32();
            TrianglesOffset = reader.ReadInt32();
            FramesOffset = reader.ReadInt32();
            GLCommandsOffset = reader.ReadInt32();
            EndOffset = reader.ReadInt32();
            reader.Close();
        }
    }
}