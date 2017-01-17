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

namespace SnowflakeEngine.WanderEngine
{
    public class Md2Model
    {
        private static readonly int HeaderSizeInBytes = 0x44;
        private static readonly int SkinNameSizeInBytes = 0x40;
        private int _animEndFrame = 0x27;
        private float _animPercent = 4f;
        private int _animStartFrame;
        private int _currentFrame;
        private Md2Face[] _faces;
        private Md2Frame[] _frames;
        //private GlCommand[] GlCommands = null;
        private Md2Header _header;
        private float _interpolation;
        private float _maskB;
        private float _maskG;
        private float _maskR;
        private AnimationState _mModelState = AnimationState.Stand;
        private Texture _modelTexture;
        private int _nextFrame;
        private int _numFaces;
        private int _numFramePoints;
        private int _numFrames;
        private int _numSkins;
        private int _numTextureCoords;
        private int _numVertices;
        private int _pointsPerFrame;
        private string[] _skins;
        private Vector2F[] _textureCoords;
        private bool _useMask;
        private Vector3F[] _vertices;
        public float Pitch = 0f;
        public Vector3F Position = new Vector3F();
        public bool RepeatAnimation = true;
        public float Yaw = 0f;

        public Md2Model(string fileName, string textureFileName)
        {
            LoadModel(fileName, textureFileName);
        }

        public Vector3F BoundMax { get; } = new Vector3F();
        public Vector3F BoundMin { get; } = new Vector3F();
        public Vector3F Center { get; } = new Vector3F();

        public AnimationState ModelState
        {
            get { return _mModelState; }
            set { SetModelState(value); }
        }

        public void Animate(int startFrame, int endFrame, float percent)
        {
            if (startFrame > _currentFrame)
            {
                _currentFrame = startFrame;
            }
            if (_interpolation > 1f)
            {
                _interpolation = 0f;
                _currentFrame++;
                if (RepeatAnimation)
                {
                    if (_currentFrame >= endFrame)
                    {
                        _currentFrame = startFrame;
                    }
                    _nextFrame = _currentFrame + 1;
                    if (_nextFrame >= endFrame)
                    {
                        _nextFrame = startFrame;
                    }
                }
                else if (_currentFrame >= _animEndFrame)
                {
                    _currentFrame = _animEndFrame;
                    _nextFrame = _currentFrame;
                }
                else
                {
                    _nextFrame = _currentFrame + 1;
                }
            }
            var num = _currentFrame*_header.NumPoints;
            var num2 = _nextFrame*_header.NumPoints;
            GL.PushMatrix();
            GL.Translate(Position.X, Position.Y, Position.Z);
            GL.Rotate(Yaw, 0f, 1f, 0f);
            GL.Rotate(Pitch, 0f, 0f, 1f);
            if (_useMask)
            {
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate);
                GL.Color4(_maskR, _maskG, _maskB, 0.9f);
            }
            GL.CullFace(CullFaceMode.Back);
            GL.BindTexture(TextureTarget.Texture2D, _modelTexture.TextureId);
            GL.Begin(BeginMode.Triangles);
            for (var i = 0; i < _numFaces; i++)
            {
                var vector = _vertices[num + _faces[i].VertexIndices[0]];
                var vector2 = _vertices[num + _faces[i].VertexIndices[2]];
                var vector3 = _vertices[num + _faces[i].VertexIndices[1]];
                var vector4 = _vertices[num2 + _faces[i].VertexIndices[0]];
                var vector5 = _vertices[num2 + _faces[i].VertexIndices[2]];
                var vector6 = _vertices[num2 + _faces[i].VertexIndices[1]];
                var vector7 = new Vector3F
                {
                    X = vector.X + (_interpolation*(vector4.X - vector.X)),
                    Y = vector.Y + (_interpolation*(vector4.Y - vector.Y)),
                    Z = vector.Z + (_interpolation*(vector4.Z - vector.Z))
                };
                GL.TexCoord2(_textureCoords[_faces[i].TextureIndices[0]].X,
                    _textureCoords[_faces[i].TextureIndices[0]].Y);
                GL.Vertex3(vector7.X, vector7.Y, vector7.Z);
                var vector8 = new Vector3F
                {
                    X = vector2.X + (_interpolation*(vector5.X - vector2.X)),
                    Y = vector2.Y + (_interpolation*(vector5.Y - vector2.Y)),
                    Z = vector2.Z + (_interpolation*(vector5.Z - vector2.Z))
                };
                GL.TexCoord2(_textureCoords[_faces[i].TextureIndices[2]].X,
                    _textureCoords[_faces[i].TextureIndices[2]].Y);
                GL.Vertex3(vector8.X, vector8.Y, vector8.Z);
                var vector9 = new Vector3F
                {
                    X = vector3.X + (_interpolation*(vector6.X - vector3.X)),
                    Y = vector3.Y + (_interpolation*(vector6.Y - vector3.Y)),
                    Z = vector3.Z + (_interpolation*(vector6.Z - vector3.Z))
                };
                GL.TexCoord2(_textureCoords[_faces[i].TextureIndices[1]].X,
                    _textureCoords[_faces[i].TextureIndices[1]].Y);
                GL.Vertex3(vector9.X, vector9.Y, vector9.Z);
            }
            GL.End();
            GL.CullFace(CullFaceMode.Front);
            GL.PopMatrix();
            if (_useMask)
            {
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Replace);
            }
            _interpolation += percent;
        }

        private void LoadModel(string fileName, string textureFileName)
        {
            var reader = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read));
            var headerBytes = reader.ReadBytes(HeaderSizeInBytes);
            _header = new Md2Header(headerBytes);
            _numSkins = _header.NumSkins;
            _skins = new string[_numSkins];
            _numFaces = _header.NumTriangles;
            _faces = new Md2Face[_numFaces];
            _numFrames = _header.NumFrames;
            _numFramePoints = _header.NumPoints;
            _pointsPerFrame = _header.NumTriangles*3;
            _frames = new Md2Frame[_numFrames];
            _numTextureCoords = _header.NumTextureCoords;
            _textureCoords = new Vector2F[_numTextureCoords];
            _numVertices = _header.NumFrames*_header.NumPoints;
            _vertices = new Vector3F[_numVertices];
            reader.BaseStream.Seek(_header.SkinsOffset, SeekOrigin.Begin);
            for (var i = 0; i < _numSkins; i++)
            {
                var stringData = reader.ReadBytes(SkinNameSizeInBytes);
                _skins[i] = ParseString(stringData);
            }
            _modelTexture = new Texture(textureFileName);
            reader.BaseStream.Seek(_header.TextureCoordsOffset, SeekOrigin.Begin);
            for (var j = 0; j < _numTextureCoords; j++)
            {
                _textureCoords[j] = new Vector2F();
                var num3 = reader.ReadInt16();
                var num4 = reader.ReadInt16();
                _textureCoords[j].X = num3/((float) _header.SkinWidth);
                _textureCoords[j].Y = -(num4/((float) _header.SkinHeight));
            }
            reader.BaseStream.Seek(_header.TrianglesOffset, SeekOrigin.Begin);
            for (var k = 0; k < _numFaces; k++)
            {
                var reader2 = new BinaryReader(new MemoryStream(reader.ReadBytes(Md2Face.SizeInBytes)));
                _faces[k] = new Md2Face();
                _faces[k].VertexIndices[0] = reader2.ReadInt16();
                _faces[k].VertexIndices[1] = reader2.ReadInt16();
                _faces[k].VertexIndices[2] = reader2.ReadInt16();
                _faces[k].TextureIndices[0] = reader2.ReadInt16();
                _faces[k].TextureIndices[1] = reader2.ReadInt16();
                _faces[k].TextureIndices[2] = reader2.ReadInt16();
            }
            reader.BaseStream.Seek(_header.FramesOffset, SeekOrigin.Begin);
            for (var m = 0; m < _numFrames; m++)
            {
                _frames[m] = new Md2Frame();
                _frames[m].FramePoints = new Md2FramePoint[_header.NumPoints];
                _frames[m].Scale.X = reader.ReadSingle();
                _frames[m].Scale.Y = reader.ReadSingle();
                _frames[m].Scale.Z = reader.ReadSingle();
                _frames[m].Translate.X = reader.ReadSingle();
                _frames[m].Translate.Y = reader.ReadSingle();
                _frames[m].Translate.Z = reader.ReadSingle();
                _frames[m].Name = ParseString(reader.ReadBytes(0x10));
                for (var num7 = 0; num7 < _numFramePoints; num7++)
                {
                    _frames[m].FramePoints[num7] = new Md2FramePoint();
                    _frames[m].FramePoints[num7].ScaledVertex[0] = reader.ReadByte();
                    _frames[m].FramePoints[num7].ScaledVertex[1] = reader.ReadByte();
                    _frames[m].FramePoints[num7].ScaledVertex[2] = reader.ReadByte();
                    _frames[m].FramePoints[num7].LightNormalIndex = reader.ReadByte();
                }
            }
            var index = 0;
            for (var n = 0; n < _numFrames; n++)
            {
                for (var num10 = 0; num10 < _numFramePoints; num10++)
                {
                    _vertices[index] = new Vector3F();
                    _vertices[index].X = (_frames[n].FramePoints[num10].ScaledVertex[0]*_frames[n].Scale.X) +
                                         _frames[n].Translate.X;
                    _vertices[index].Z =
                        -((_frames[n].FramePoints[num10].ScaledVertex[1]*_frames[n].Scale.Y) + _frames[n].Translate.Y);
                    _vertices[index].Y = (_frames[n].FramePoints[num10].ScaledVertex[2]*_frames[n].Scale.Z) +
                                         _frames[n].Translate.Z;
                    if (_vertices[index].X < BoundMin.X)
                    {
                        BoundMin.X = _vertices[index].X;
                    }
                    if (_vertices[index].Y < BoundMin.Y)
                    {
                        BoundMin.Y = _vertices[index].Y;
                    }
                    if (_vertices[index].Z < BoundMin.Z)
                    {
                        BoundMin.Z = _vertices[index].Z;
                    }
                    if (_vertices[index].X > BoundMax.X)
                    {
                        BoundMax.X = _vertices[index].X;
                    }
                    if (_vertices[index].Y > BoundMax.Y)
                    {
                        BoundMax.Y = _vertices[index].Y;
                    }
                    if (_vertices[index].Z > BoundMax.Z)
                    {
                        BoundMax.Z = _vertices[index].Z;
                    }
                    index++;
                }
            }
            Center.X = (Math.Abs(BoundMax.X) - Math.Abs(BoundMin.X))/2f;
            Center.Y = (Math.Abs(BoundMax.Y) - Math.Abs(BoundMin.Y))/2f;
            Center.Z = (Math.Abs(BoundMax.Z) - Math.Abs(BoundMin.Z))/2f;
            reader.Close();
        }

        private string ParseString(byte[] stringData)
        {
            var str = "";
            for (var i = 0; i < stringData.Length; i++)
            {
                if (stringData[i] != 0)
                {
                    str = str + Convert.ToChar(stringData[i]);
                }
            }
            return str;
        }

        public bool RayCollides(Vector3F start, Vector3F end)
        {
            return BspFile.RayBoxCollision(start, end, BoundMin + Position, BoundMax + Position);
        }

        private void SetModelState(AnimationState newState)
        {
            _mModelState = newState;
            switch (newState)
            {
                case AnimationState.Stand:
                    _animStartFrame = 0;
                    _animEndFrame = 30;
                    _animPercent = 4f;
                    break;

                case AnimationState.Run:
                    _animStartFrame = 40;
                    _animEndFrame = 0x2d;
                    _animPercent = 4f;
                    break;

                case AnimationState.Attack:
                    _animStartFrame = 0x2e;
                    _animEndFrame = 0x35;
                    _animPercent = 0.5f;
                    break;

                case AnimationState.PainA:
                    _animStartFrame = 0x36;
                    _animEndFrame = 0x39;
                    _animPercent = 4f;
                    break;

                case AnimationState.PainB:
                    _animStartFrame = 0x3a;
                    _animEndFrame = 0x3d;
                    _animPercent = 4f;
                    break;

                case AnimationState.PainC:
                    _animStartFrame = 0x3e;
                    _animEndFrame = 0x41;
                    _animPercent = 4f;
                    break;

                case AnimationState.Jump:
                    _animStartFrame = 0x42;
                    _animEndFrame = 0x47;
                    _animPercent = 4f;
                    break;

                case AnimationState.Flip:
                    _animStartFrame = 0x48;
                    _animEndFrame = 0x53;
                    _animPercent = 4f;
                    break;

                case AnimationState.Salute:
                    _animStartFrame = 0x54;
                    _animEndFrame = 0x5e;
                    _animPercent = 4f;
                    break;

                case AnimationState.FallBack:
                    _animStartFrame = 0x5f;
                    _animEndFrame = 0x6f;
                    _animPercent = 4f;
                    break;

                case AnimationState.Wave:
                    _animStartFrame = 0x70;
                    _animEndFrame = 0x7a;
                    _animPercent = 4f;
                    break;

                case AnimationState.Point:
                    _animStartFrame = 0x7b;
                    _animEndFrame = 0x86;
                    _animPercent = 4f;
                    break;

                case AnimationState.CrouchStand:
                    _animStartFrame = 0x87;
                    _animEndFrame = 0x99;
                    _animPercent = 4f;
                    break;

                case AnimationState.CrouchWalk:
                    _animStartFrame = 0x9a;
                    _animEndFrame = 0x9f;
                    _animPercent = 4f;
                    break;

                case AnimationState.CrouchAttack:
                    _animStartFrame = 160;
                    _animEndFrame = 0xa8;
                    _animPercent = 4f;
                    break;

                case AnimationState.CrouchPain:
                    _animStartFrame = 0xa9;
                    _animEndFrame = 0xac;
                    _animPercent = 4f;
                    break;

                case AnimationState.CrouchDeath:
                    _animStartFrame = 0xad;
                    _animEndFrame = 0xb1;
                    _animPercent = 4f;
                    break;

                case AnimationState.DeathFallBack:
                    _animStartFrame = 0xb2;
                    _animEndFrame = 0xb7;
                    _animPercent = 4f;
                    break;

                case AnimationState.DeathFallFoward:
                    _animStartFrame = 0xb8;
                    _animEndFrame = 0xbd;
                    _animPercent = 3f;
                    break;

                case AnimationState.DeathFallBackSlow:
                    _animStartFrame = 190;
                    _animEndFrame = 0xc5;
                    _animPercent = 4f;
                    break;

                case AnimationState.Boom:
                    _animStartFrame = 0xc6;
                    _animEndFrame = 0xc6;
                    _animPercent = 4f;
                    break;
            }
            _currentFrame = _animStartFrame;
            _nextFrame = _currentFrame + 1;
            _interpolation = 0f;
        }

        public void Update(float timeElapsed)
        {
            _useMask = false;
            Animate(_animStartFrame, _animEndFrame, _animPercent*timeElapsed);
        }

        public void Update(float timeElapsed, float r, float g, float b)
        {
            _useMask = true;
            _maskR = r;
            _maskG = g;
            _maskB = b;
            Animate(_animStartFrame, _animEndFrame, _animPercent*timeElapsed);
        }
    }

    public class Md2Face
    {
        public static readonly int SizeInBytes = 12;
        public short[] TextureIndices = new short[3];
        public short[] VertexIndices = new short[3];
    }

    public class Md2Frame
    {
        public Md2FramePoint[] FramePoints;
        public string Name = "";
        public Vector3F Scale = new Vector3F();
        public Vector3F Translate = new Vector3F();
    }

    public class Md2FramePoint
    {
        public byte LightNormalIndex;
        public byte[] ScaledVertex = new byte[3];
    }

    public class Md2Header
    {
        public int EndOffset;
        public int FrameSize;
        public int FramesOffset;
        public int GlCommandsOffset;
        public int Identity;
        public int NumFrames;
        public int NumGlCommands;
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

        public Md2Header()
        {
        }

        public Md2Header(byte[] headerBytes)
        {
            var reader = new BinaryReader(new MemoryStream(headerBytes));
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
            NumGlCommands = reader.ReadInt32();
            NumFrames = reader.ReadInt32();
            SkinsOffset = reader.ReadInt32();
            TextureCoordsOffset = reader.ReadInt32();
            TrianglesOffset = reader.ReadInt32();
            FramesOffset = reader.ReadInt32();
            GlCommandsOffset = reader.ReadInt32();
            EndOffset = reader.ReadInt32();
            reader.Close();
        }
    }
}