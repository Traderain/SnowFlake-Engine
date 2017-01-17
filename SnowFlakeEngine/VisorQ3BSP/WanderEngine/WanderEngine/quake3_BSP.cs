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
using System.Collections;
using System.Drawing;
using System.IO;
using System.Text;
using Math3D;
using OpenTK.Graphics.OpenGL;

namespace SnowflakeEngine.WanderEngine
{
    public class BspFile
    {
        private static readonly int LeafBrushSizeInBytes = 4;
        private static readonly int LeafFaceSizeInBytes = 4;
        private static readonly int MaxLumps = 0x11;
        private static readonly int MeshIndexSizeInBytes = 4;
        private static readonly float QuakeEpsilon = 0.03125f;
        private static readonly int VertexSizeInBytes = 0x2c;
        private readonly Vector3F _collisionExtents = new Vector3F();
        private readonly BspHeader _header = new BspHeader();
        private readonly Hashtable _indexTriggerHash = new Hashtable();
        private readonly BspLump[] _lumps = new BspLump[MaxLumps];
        private BspBrush[] _brushes;
        private BspBrushSide[] _brushSides;
        private BspVisData _clusters;
        private Vector3F _collisionEnd = new Vector3F();
        private Vector3F _collisionMax = new Vector3F();
        private Vector3F _collisionMin = new Vector3F();
        private float _collisionOffset;
        private Vector3F _collisionStart = new Vector3F();
        private CollisionTypes _collisionType = CollisionTypes.Ray;
        private string _entityString = "";
        private int _entityStringLength;
        private BspFace[] _faces;
        private BitArray _facesDrawn;
        private float _gamma = 10f;
        private int[] _leafBrushes;
        private int[] _leafFaces;
        private BspLeaf[] _leaves;
        private float[] _lightmapCoords;
        private Texture[] _lightmaps;
        private BspTexture[] _loadTextures;
        private uint[] _meshIndices;
        private BspModel[] _models;
        private BspNode[] _nodes;
        private int _noDrawTextureIndex = -1;
        private int _numBrushes;
        private int _numBrushSides;
        private int _numFaces;
        private int _numLeafBrushes;
        private int _numLeafFaces;
        private int _numLeaves;
        private int _numLightmaps;
        private int _numMeshIndices;
        private int _numModels;
        private int _numNodes;
        private int _numPlanes;
        private int _numShaders;
        private int _numTextures;
        private int _numVertices;
        private BspPlane[] _planes;
        private BspShader[] _shaders;
        private Texture[] _skyBoxTextures;
        private float[] _textureCoords;
        private Texture[] _textures;
        private Trigger[] _triggers;
        private float[] _vertices;
        public CollisionInformation CollisionInfo = new CollisionInformation();
        public BspEntityCollection Entities;

        public BspFile(string fileName)
        {
            LoadBsp(fileName);
        }

        private bool BoxesCollide(float[] mins, float[] maxes, float[] mins2, float[] maxes2)
        {
            var flag = false;
            if ((((maxes2[0] > mins[0]) && (mins2[0] < maxes[0])) && ((maxes2[1] > mins[1]) && (mins2[1] < maxes[1]))) &&
                ((maxes2[2] > mins[2]) && (mins2[2] < maxes[2])))
            {
                flag = true;
            }
            return flag;
        }

        public static void ChangeGamma(ref Bitmap imageBmp, float factor)
        {
            for (var i = 0; i < imageBmp.Width; i++)
            {
                for (var j = 0; j < imageBmp.Height; j++)
                {
                    var num3 = 1f;
                    var num4 = 0f;
                    var pixel = imageBmp.GetPixel(i, j);
                    float r = pixel.R;
                    float g = pixel.G;
                    float b = pixel.B;
                    r = (r*factor)/255f;
                    g = (g*factor)/255f;
                    b = (b*factor)/255f;
                    if (r > 1f)
                    {
                        num4 = 1f/r;
                        if (num4 < num3)
                        {
                            num3 = num4;
                        }
                    }
                    if (g > 1f)
                    {
                        num4 = 1f/g;
                        if (num4 < num3)
                        {
                            num3 = num4;
                        }
                    }
                    if (b > 1f)
                    {
                        num4 = 1f/b;
                        if (num4 < num3)
                        {
                            num3 = num4;
                        }
                    }
                    num3 *= 255f;
                    r *= num3;
                    g *= num3;
                    b *= num3;
                    imageBmp.SetPixel(i, j, Color.FromArgb((int) r, (int) g, (int) b));
                }
            }
        }

        private void CheckBrush(BspBrush currentBrush)
        {
            var num = -1f;
            var num2 = 1f;
            var flag = false;
            var flag2 = false;
            var distance = 0f;
            var normal = new Vector3F();
            for (var i = 0; i < currentBrush.NumSides; i++)
            {
                var side = _brushSides[currentBrush.FirstSide + i];
                var plane = _planes[side.Plane];
                var num5 = 0f;
                var num6 = 0f;
                if (_collisionType == CollisionTypes.Box)
                {
                    var vector2 = new Vector3F();
                    if (plane.Normal.X < 0f)
                    {
                        vector2.X = _collisionMax.X;
                    }
                    else
                    {
                        vector2.X = _collisionMin.X;
                    }
                    if (plane.Normal.Y < 0f)
                    {
                        vector2.Y = _collisionMax.Y;
                    }
                    else
                    {
                        vector2.Y = _collisionMin.Y;
                    }
                    if (plane.Normal.Z < 0f)
                    {
                        vector2.Z = _collisionMax.Z;
                    }
                    else
                    {
                        vector2.Z = _collisionMin.Z;
                    }
                    num5 = ((((_collisionStart.X + vector2.X)*plane.Normal.X) +
                             ((_collisionStart.Y + vector2.Y)*plane.Normal.Y)) +
                            ((_collisionStart.Z + vector2.Z)*plane.Normal.Z)) - plane.Distance;
                    num6 = ((((_collisionEnd.X + vector2.X)*plane.Normal.X) +
                             ((_collisionEnd.Y + vector2.Y)*plane.Normal.Y)) +
                            ((_collisionEnd.Z + vector2.Z)*plane.Normal.Z)) - plane.Distance;
                }
                else
                {
                    num5 = ((((plane.Normal.X*_collisionStart.X) + (plane.Normal.Y*_collisionStart.Y)) +
                             (plane.Normal.Z*_collisionStart.Z)) - plane.Distance) - _collisionOffset;
                    num6 = ((((plane.Normal.X*_collisionEnd.X) + (plane.Normal.Y*_collisionEnd.Y)) +
                             (plane.Normal.Z*_collisionEnd.Z)) - plane.Distance) - _collisionOffset;
                }
                if (num5 > 0f)
                {
                    flag = true;
                }
                if (num6 > 0f)
                {
                    flag2 = true;
                }
                if ((num5 > 0f) && (num6 > 0f))
                {
                    return;
                }
                if ((num5 > 0f) || (num6 > 0f))
                {
                    if (num5 > num6)
                    {
                        var num7 = (num5 - QuakeEpsilon)/(num5 - num6);
                        if (num7 > num)
                        {
                            num = num7;
                            normal = plane.Normal;
                            distance = plane.Distance;
                        }
                    }
                    else
                    {
                        var num8 = (num5 + QuakeEpsilon)/(num5 - num6);
                        if (num8 < num2)
                        {
                            num2 = num8;
                        }
                    }
                }
            }
            if (!flag)
            {
                CollisionInfo.StartsOut = false;
                if (!flag2)
                {
                    CollisionInfo.AllSolid = true;
                }
            }
            else if (((num < num2) && (num > -1f)) && (num < CollisionInfo.Fraction))
            {
                if (num < 0f)
                {
                    num = 0f;
                }
                CollisionInfo.Fraction = num;
                CollisionInfo.Normal = normal;
                CollisionInfo.PlaneDistance = distance;
            }
        }

        private void CheckNode(int NodeIndex, float startFraction, float EndFraction, Vector3F start, Vector3F End)
        {
            if (CollisionInfo.Fraction > startFraction)
            {
                if (NodeIndex < 0)
                {
                    var leaf = _leaves[~NodeIndex];
                    for (var i = 0; i < leaf.NumLeafBrushes; i++)
                    {
                        var currentBrush = _brushes[_leafBrushes[leaf.LeafBrush + i]];
                        if ((currentBrush.NumSides > 0) && ((_loadTextures[currentBrush.TextureIndex].Contents & 1) > 0))
                        {
                            CheckBrush(currentBrush);
                        }
                    }
                }
                else
                {
                    var node = _nodes[NodeIndex];
                    var plane = _planes[node.Plane];
                    var num2 = (((plane.Normal.X*start.X) + (plane.Normal.Y*start.Y)) + (plane.Normal.Z*start.Z)) -
                               plane.Distance;
                    var num3 = (((plane.Normal.X*End.X) + (plane.Normal.Y*End.Y)) + (plane.Normal.Z*End.Z)) -
                               plane.Distance;
                    if (_collisionType == CollisionTypes.Box)
                    {
                        _collisionOffset = (Math.Abs(_collisionExtents.X*plane.Normal.X) +
                                            Math.Abs(_collisionExtents.Y*plane.Normal.Y)) +
                                           Math.Abs(_collisionExtents.Z*plane.Normal.Z);
                    }
                    if ((num2 >= _collisionOffset) && (num3 >= _collisionOffset))
                    {
                        CheckNode(node.Front, startFraction, EndFraction, start, End);
                    }
                    else if ((num2 < -_collisionOffset) && (num3 < -_collisionOffset))
                    {
                        CheckNode(node.Back, startFraction, EndFraction, start, End);
                    }
                    else
                    {
                        var nodeIndex = -1;
                        var front = -1;
                        var num6 = 0f;
                        var num7 = 0f;
                        var end = new Vector3F();
                        if (num2 < num3)
                        {
                            nodeIndex = node.Back;
                            front = node.Front;
                            var num8 = 1f/(num2 - num3);
                            num6 = ((num2 - QuakeEpsilon) - _collisionOffset)*num8;
                            num7 = ((num2 + QuakeEpsilon) + _collisionOffset)*num8;
                        }
                        else if (num3 < num2)
                        {
                            nodeIndex = node.Front;
                            front = node.Back;
                            var num9 = 1f/(num2 - num3);
                            num6 = ((num2 + QuakeEpsilon) + _collisionOffset)*num9;
                            num7 = ((num2 - QuakeEpsilon) - _collisionOffset)*num9;
                        }
                        else
                        {
                            nodeIndex = node.Front;
                            front = node.Back;
                            num6 = 1f;
                            num7 = 0f;
                        }
                        if (num6 < 0f)
                        {
                            num6 = 0f;
                        }
                        else if (num6 > 1f)
                        {
                            num6 = 1f;
                        }
                        if (num7 < 0f)
                        {
                            num7 = 0f;
                        }
                        else if (num7 > 1f)
                        {
                            num7 = 1f;
                        }
                        end = start + (End - start)*num6;
                        var endFraction = startFraction + ((EndFraction - startFraction)*num6);
                        CheckNode(nodeIndex, startFraction, endFraction, start, end);
                        end = start + (End - start)*num7;
                        endFraction = startFraction + ((EndFraction - startFraction)*num7);
                        CheckNode(front, endFraction, EndFraction, end, End);
                    }
                }
            }
        }

        private void DetectCollision(Vector3F start, Vector3F end)
        {
            CollisionInfo = new CollisionInformation();
            _collisionStart = new Vector3F(start.X, start.Y, start.Z);
            _collisionEnd = new Vector3F(end.X, end.Y, end.Z);
            CheckNode(0, 0f, 1f, _collisionStart, _collisionEnd);
            if (CollisionInfo.Fraction == 1f)
            {
                CollisionInfo.EndPoint = _collisionEnd;
            }
            else
            {
                CollisionInfo.EndPoint = _collisionStart + (_collisionEnd - _collisionStart)*CollisionInfo.Fraction;
            }
        }

        public void DetectCollisionBox(Vector3F start, Vector3F end, Vector3F min, Vector3F max)
        {
            _collisionType = CollisionTypes.Box;
            _collisionMin = min;
            _collisionMax = max;
            _collisionExtents.X = (-min.X > max.X) ? -min.X : max.X;
            _collisionExtents.Y = (-min.Y > max.Y) ? -min.Y : max.Y;
            _collisionExtents.Z = (-min.Z > max.Z) ? -min.Z : max.Z;
            DetectCollision(start, end);
        }

        public void DetectCollisionRay(Vector3F start, Vector3F end)
        {
            _collisionType = CollisionTypes.Ray;
            _collisionOffset = 0f;
            DetectCollision(start, end);
        }

        public void DetectCollisionSphere(Vector3F start, Vector3F end, float radius)
        {
            _collisionType = CollisionTypes.Sphere;
            _collisionOffset = radius;
            DetectCollision(start, end);
        }

        public Trigger DetectTriggerCollisions(Vector3F position)
        {
            Trigger trigger = null;
            for (var i = 1; i < _models.Length; i++)
            {
                var model = _models[i];
                if (PointInBox(position, model.Mins, model.Maxes))
                {
                    try
                    {
                        trigger = (Trigger) _indexTriggerHash[i];
                    }
                    catch
                    {
                    }
                    return trigger;
                }
            }
            return trigger;
        }

        private int FindLeaf(Vector3F cameraPosition)
        {
            var index = 0;
            var num2 = 0f;
            while (index >= 0)
            {
                var node = _nodes[index];
                var plane = _planes[node.Plane];
                num2 = (((plane.Normal.X*cameraPosition.X) + (plane.Normal.Y*cameraPosition.Y)) +
                        (plane.Normal.Z*cameraPosition.Z)) - plane.Distance;
                if (num2 >= 0f)
                {
                    index = node.Front;
                }
                else
                {
                    index = node.Back;
                }
            }
            return ~index;
        }

        private bool IsClusterVisible(int currentCluster, int testCluster)
        {
            var flag = true;
            if (((_clusters.BitSets != null) && (currentCluster >= 0)) && (testCluster >= 0))
            {
                var num = _clusters.BitSets[(currentCluster*_clusters.BytesPerCluster) + (testCluster/8)];
                var num2 = num & (1 << (testCluster & 7));
                if (num2 <= 0)
                {
                    flag = false;
                }
            }
            return flag;
        }

        public void LoadBsp(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Open);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int) stream.Length);
            var reader = new BinaryReader(new MemoryStream(buffer));
            _header.Id = Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
            _header.Version = reader.ReadInt32();
            for (var i = 0; i < MaxLumps; i++)
            {
                _lumps[i] = new BspLump();
                _lumps[i].Offset = reader.ReadInt32();
                _lumps[i].Length = reader.ReadInt32();
            }
            if ((_header.Id != "IBSP") && (_header.Version != 0x2e))
            {
                throw new Exception("Wrong file type or version");
            }
            _numVertices = _lumps[10].Length/VertexSizeInBytes;
            _vertices = new float[_numVertices*3];
            _textureCoords = new float[_numVertices*2];
            _lightmapCoords = new float[_numVertices*2];
            _numFaces = _lumps[13].Length/BspFace.SizeInBytes;
            _faces = new BspFace[_numFaces];
            _numTextures = _lumps[1].Length/BspTexture.SizeInBytes;
            _loadTextures = new BspTexture[_numTextures];
            _textures = new Texture[_numTextures];
            _numLightmaps = _lumps[14].Length/BspLightmap.SizeInBytes;
            _lightmaps = new Texture[_numLightmaps];
            _numNodes = _lumps[3].Length/BspNode.SizeInBytes;
            _nodes = new BspNode[_numNodes];
            _numLeaves = _lumps[4].Length/BspLeaf.SizeInBytes;
            _leaves = new BspLeaf[_numLeaves];
            _numLeafFaces = _lumps[5].Length/LeafFaceSizeInBytes;
            _leafFaces = new int[_numLeafFaces];
            _numPlanes = _lumps[2].Length/BspPlane.SizeInBytes;
            _planes = new BspPlane[_numPlanes];
            _numMeshIndices = _lumps[11].Length/MeshIndexSizeInBytes;
            _meshIndices = new uint[_numMeshIndices];
            _numModels = _lumps[7].Length/BspModel.SizeInBytes;
            _models = new BspModel[_numModels];
            _numBrushes = _lumps[8].Length/BspBrush.SizeInBytes;
            _brushes = new BspBrush[_numBrushes];
            _numBrushSides = _lumps[9].Length/BspBrushSide.SizeInBytes;
            _brushSides = new BspBrushSide[_numBrushSides];
            _numLeafBrushes = _lumps[6].Length/LeafBrushSizeInBytes;
            _leafBrushes = new int[_numLeafBrushes];
            _numShaders = _lumps[12].Length/BspShader.SizeInBytes;
            _shaders = new BspShader[_numShaders];
            _clusters = new BspVisData();
            _entityStringLength = _lumps[0].Length;
            reader.BaseStream.Seek(_lumps[0].Offset, SeekOrigin.Begin);
            foreach (var num2 in reader.ReadBytes(_entityStringLength))
            {
                var ch = Convert.ToChar(num2);
                if (ch != '\0')
                {
                    _entityString = _entityString + ch;
                }
            }
            Entities = new BspEntityCollection(_entityString);
            var s = Entities.SeekFirstEntityValue("worldspawn", "ambient");
            try
            {
                _gamma = float.Parse(s);
                _gamma /= 17f;
            }
            catch
            {
            }
            var entityArray = Entities.SeekEntitiesByClassname("trigger_multiple");
            if (entityArray.Length > 0)
            {
                _triggers = new Trigger[entityArray.Length];
                var num3 = 0;
                foreach (var entity in entityArray)
                {
                    try
                    {
                        var trigger = new Trigger();
                        trigger.Name = entity.SeekFirstValue("trigger_name");
                        var str2 = entity.SeekFirstValue("model").Replace("*", string.Empty);
                        trigger.ModelIndex = int.Parse(str2);
                        _indexTriggerHash[trigger.ModelIndex] = trigger;
                        _triggers[num3] = trigger;
                        num3++;
                    }
                    catch
                    {
                    }
                }
            }
            var index = 0;
            var num5 = 0;
            var offset = _lumps[10].Offset;
            for (var j = 0; j < _numVertices; j++)
            {
                reader.BaseStream.Seek(offset + (j*VertexSizeInBytes), SeekOrigin.Begin);
                _vertices[index] = reader.ReadSingle();
                _vertices[index + 2] = -reader.ReadSingle();
                _vertices[index + 1] = reader.ReadSingle();
                _textureCoords[num5] = reader.ReadSingle();
                _textureCoords[num5 + 1] = -reader.ReadSingle();
                _lightmapCoords[num5] = reader.ReadSingle();
                _lightmapCoords[num5 + 1] = -reader.ReadSingle();
                index += 3;
                num5 += 2;
            }
            var num8 = _lumps[13].Offset;
            for (var k = 0; k < _numFaces; k++)
            {
                reader.BaseStream.Seek(num8 + (k*BspFace.SizeInBytes), SeekOrigin.Begin);
                _faces[k] = new BspFace();
                _faces[k].TextureId = reader.ReadInt32();
                _faces[k].Effect = reader.ReadInt32();
                _faces[k].Type = reader.ReadInt32();
                _faces[k].StartVertexIndex = reader.ReadInt32();
                _faces[k].NumVertices = reader.ReadInt32();
                _faces[k].MeshVertexIndex = reader.ReadInt32();
                _faces[k].NumMeshVertices = reader.ReadInt32();
                _faces[k].LightmapId = reader.ReadInt32();
                _faces[k].MapCorner[0] = reader.ReadInt32();
                _faces[k].MapCorner[1] = reader.ReadInt32();
                _faces[k].MapSize[0] = reader.ReadInt32();
                _faces[k].MapSize[1] = reader.ReadInt32();
                _faces[k].MapPosition.X = reader.ReadSingle();
                _faces[k].MapPosition.Y = reader.ReadSingle();
                _faces[k].MapPosition.Z = reader.ReadSingle();
                _faces[k].MapVectors[0].X = reader.ReadSingle();
                _faces[k].MapVectors[0].Y = reader.ReadSingle();
                _faces[k].MapVectors[0].Z = reader.ReadSingle();
                _faces[k].MapVectors[1].X = reader.ReadSingle();
                _faces[k].MapVectors[1].Y = reader.ReadSingle();
                _faces[k].MapVectors[1].Z = reader.ReadSingle();
                _faces[k].Normal.X = reader.ReadSingle();
                _faces[k].Normal.Y = reader.ReadSingle();
                _faces[k].Normal.Z = reader.ReadSingle();
                _faces[k].Size[0] = reader.ReadInt32();
                _faces[k].Size[1] = reader.ReadInt32();
            }
            reader.BaseStream.Seek(_lumps[1].Offset, SeekOrigin.Begin);
            /*
             for (int m = 0; m < this.NumTextures; m++)
             {
                 this.LoadTextures[m] = new BSPTexture();
                 byte[] buffer3 = reader.ReadBytes(0x40);
                 for (int num11 = 0; num11 < 0x40; num11++)
                 {
                     if (buffer3[num11] != 0)
                     {
                         BSPTexture texture1 = this.LoadTextures[m];
                         texture1.Name = texture1.Name + Convert.ToChar(buffer3[num11]);
                     }
                 }
                 this.LoadTextures[m].Flags = reader.ReadInt32();
                 this.LoadTextures[m].Contents = reader.ReadInt32();
                 if (this.LoadTextures[m].Name.IndexOf("bookstore/no_draw") != -1)
                 {
                     this.NoDrawTextureIndex = m;
                 }
             }
             */
            // Miki
            for (var i = 0; i < _numTextures; i++)
            {
                _loadTextures[i] = new BspTexture();

                var nameBytes = reader.ReadBytes(64);
                for (var nameByteIndex = 0; nameByteIndex < 64; nameByteIndex++)
                {
                    if (nameBytes[nameByteIndex] != '\0')
                    {
                        _loadTextures[i].Name += Convert.ToChar(nameBytes[nameByteIndex]);
                    }
                }

                _loadTextures[i].Flags = reader.ReadInt32();
                _loadTextures[i].Contents = reader.ReadInt32();

                //Check for skybox texture
                if (_loadTextures[i].Name.IndexOf(Utility.AdaptRelativePathToPlatform("bookstore/no_draw")) != -1)
                {
                    _noDrawTextureIndex = i;
                }
            }
            var dirPath = Directory.GetCurrentDirectory();
            var texturePath = Utility.AdaptRelativePathToPlatform("../");
            Directory.SetCurrentDirectory(texturePath);

            for (var n = 0; n < _numTextures; n++)
            {
                var path = _loadTextures[n].Name + ".jpg";
                var str4 = _loadTextures[n].Name + ".tga";
                try
                {
                    if (File.Exists(path))
                    {
                        _textures[n] = new Texture(path);
                    }
                    else if (File.Exists(str4))
                    {
                        _textures[n] = new Texture(str4);
                    }
                    else
                    {
                        _textures[n] = null;
                    }
                }
                catch
                {
                    _textures[n] = null;
                }
            }

            _skyBoxTextures = new Texture[6];
            var str5 = "day";
            var hour = DateTime.Now.Hour;
            if ((hour > 6) && (hour < 8))
            {
                str5 = "dawn";
            }
            else if ((hour >= 8) && (hour < 0x12))
            {
                str5 = "day";
            }
            else if ((hour >= 0x12) && (hour < 0x15))
            {
                str5 = "dusk";
            }
            else
            {
                str5 = "night";
            }
            var sky0 = "textures/bookstore/skies/" + str5 + "/negx.jpg";
            var sky1 = "textures/bookstore/skies/" + str5 + "/negy.jpg";
            var sky2 = "textures/bookstore/skies/" + str5 + "/negz.jpg";
            var sky3 = "textures/bookstore/skies/" + str5 + "/posx.jpg";
            var sky4 = "textures/bookstore/skies/" + str5 + "/posy.jpg";
            var sky5 = "textures/bookstore/skies/" + str5 + "/posz.jpg";

            _skyBoxTextures[0] = new Texture(Utility.AdaptRelativePathToPlatform(sky0), true);
            _skyBoxTextures[1] = new Texture(Utility.AdaptRelativePathToPlatform(sky1), true);
            _skyBoxTextures[2] = new Texture(Utility.AdaptRelativePathToPlatform(sky2), true);
            _skyBoxTextures[3] = new Texture(Utility.AdaptRelativePathToPlatform(sky3), true);
            _skyBoxTextures[4] = new Texture(Utility.AdaptRelativePathToPlatform(sky4), true);
            _skyBoxTextures[5] = new Texture(Utility.AdaptRelativePathToPlatform(sky5), true);

            Directory.SetCurrentDirectory(dirPath);

            reader.BaseStream.Seek(_lumps[14].Offset, SeekOrigin.Begin);
            for (var num14 = 0; num14 < _numLightmaps; num14++)
            {
                var buffer4 = reader.ReadBytes(BspLightmap.SizeInBytes);
                var imageBmp = new Bitmap(0x80, 0x80);
                var num15 = 0;
                for (var num16 = 0; num16 < 0x80; num16++)
                {
                    for (var num17 = 0; num17 < 0x80; num17++)
                    {
                        var red = buffer4[num15];
                        var green = buffer4[num15 + 1];
                        var blue = buffer4[num15 + 2];
                        imageBmp.SetPixel(num17, num16, Color.FromArgb(red, green, blue));
                        num15 += 3;
                    }
                }
                ChangeGamma(ref imageBmp, _gamma);
                _lightmaps[num14] = new Texture(imageBmp);
            }
            var num21 = _lumps[3].Offset;
            for (var num22 = 0; num22 < _numNodes; num22++)
            {
                reader.BaseStream.Seek(num21 + (num22*BspNode.SizeInBytes), SeekOrigin.Begin);
                _nodes[num22] = new BspNode();
                _nodes[num22].Plane = reader.ReadInt32();
                _nodes[num22].Front = reader.ReadInt32();
                _nodes[num22].Back = reader.ReadInt32();
                _nodes[num22].Min.X = reader.ReadInt32();
                _nodes[num22].Min.Z = -reader.ReadInt32();
                _nodes[num22].Min.Y = reader.ReadInt32();
                _nodes[num22].Max.X = reader.ReadInt32();
                _nodes[num22].Max.Z = -reader.ReadInt32();
                _nodes[num22].Max.Y = reader.ReadInt32();
            }
            var num23 = _lumps[4].Offset;
            for (var num24 = 0; num24 < _numLeaves; num24++)
            {
                reader.BaseStream.Seek(num23 + (num24*BspLeaf.SizeInBytes), SeekOrigin.Begin);
                _leaves[num24] = new BspLeaf();
                _leaves[num24].Cluster = reader.ReadInt32();
                _leaves[num24].Area = reader.ReadInt32();
                _leaves[num24].Min.X = reader.ReadInt32();
                _leaves[num24].Min.Z = -reader.ReadInt32();
                _leaves[num24].Min.Y = reader.ReadInt32();
                _leaves[num24].Max.X = reader.ReadInt32();
                _leaves[num24].Max.Z = -reader.ReadInt32();
                _leaves[num24].Max.Y = reader.ReadInt32();
                _leaves[num24].LeafFace = reader.ReadInt32();
                _leaves[num24].NumLeafFaces = reader.ReadInt32();
                _leaves[num24].LeafBrush = reader.ReadInt32();
                _leaves[num24].NumLeafBrushes = reader.ReadInt32();
            }
            reader.BaseStream.Seek(_lumps[5].Offset, SeekOrigin.Begin);
            for (var num25 = 0; num25 < _numLeafFaces; num25++)
            {
                _leafFaces[num25] = reader.ReadInt32();
            }
            var num26 = _lumps[2].Offset;
            for (var num27 = 0; num27 < _numPlanes; num27++)
            {
                reader.BaseStream.Seek(num26 + (num27*BspPlane.SizeInBytes), SeekOrigin.Begin);
                _planes[num27] = new BspPlane();
                _planes[num27].Normal.X = reader.ReadSingle();
                _planes[num27].Normal.Z = -reader.ReadSingle();
                _planes[num27].Normal.Y = reader.ReadSingle();
                _planes[num27].Distance = reader.ReadSingle();
            }
            reader.BaseStream.Seek(_lumps[11].Offset, SeekOrigin.Begin);
            for (var num28 = 0; num28 < _numMeshIndices; num28++)
            {
                _meshIndices[num28] = reader.ReadUInt32();
            }
            var num29 = _lumps[7].Offset;
            for (var num30 = 0; num30 < _numModels; num30++)
            {
                reader.BaseStream.Seek(num29 + (num30*BspModel.SizeInBytes), SeekOrigin.Begin);
                _models[num30] = new BspModel();
                _models[num30].Mins[0] = reader.ReadSingle();
                _models[num30].Maxes[2] = -reader.ReadSingle();
                _models[num30].Mins[1] = reader.ReadSingle();
                _models[num30].Maxes[0] = reader.ReadSingle();
                _models[num30].Mins[2] = -reader.ReadSingle();
                _models[num30].Maxes[1] = reader.ReadSingle();
                _models[num30].FirstFace = reader.ReadInt32();
                _models[num30].NumFaces = reader.ReadInt32();
                _models[num30].FirstBrush = reader.ReadInt32();
                _models[num30].NumBrushes = reader.ReadInt32();
            }
            var num31 = _lumps[8].Offset;
            for (var num32 = 0; num32 < _numBrushes; num32++)
            {
                reader.BaseStream.Seek(num31 + (num32*BspBrush.SizeInBytes), SeekOrigin.Begin);
                _brushes[num32] = new BspBrush();
                _brushes[num32].FirstSide = reader.ReadInt32();
                _brushes[num32].NumSides = reader.ReadInt32();
                _brushes[num32].TextureIndex = reader.ReadInt32();
            }
            var num33 = _lumps[9].Offset;
            for (var num34 = 0; num34 < _numBrushSides; num34++)
            {
                reader.BaseStream.Seek(num33 + (num34*BspBrushSide.SizeInBytes), SeekOrigin.Begin);
                _brushSides[num34] = new BspBrushSide();
                _brushSides[num34].Plane = reader.ReadInt32();
                _brushSides[num34].Texture = reader.ReadInt32();
            }
            reader.BaseStream.Seek(_lumps[6].Offset, SeekOrigin.Begin);
            for (var num35 = 0; num35 < _numLeafBrushes; num35++)
            {
                _leafBrushes[num35] = reader.ReadInt32();
            }
            var num36 = _lumps[12].Offset;
            for (var num37 = 0; num37 < _numShaders; num37++)
            {
                reader.BaseStream.Seek(num36 + (num37*BspShader.SizeInBytes), SeekOrigin.Begin);
                _shaders[num37] = new BspShader();
                var buffer5 = reader.ReadBytes(0x40);
                for (var num38 = 0; num38 < 0x40; num38++)
                {
                    if (buffer5[num38] != 0)
                    {
                        var shader1 = _shaders[num37];
                        shader1.Name = shader1.Name + Convert.ToChar(buffer5[num38]);
                    }
                }
                _shaders[num37].BrushIndex = reader.ReadInt32();
                _shaders[num37].ContentFlags = reader.ReadInt32();
            }
            reader.BaseStream.Seek(_lumps[0x10].Offset, SeekOrigin.Begin);
            if (_lumps[0x10].Length > 0)
            {
                _clusters.NumClusters = reader.ReadInt32();
                _clusters.BytesPerCluster = reader.ReadInt32();
                var count = _clusters.NumClusters*_clusters.BytesPerCluster;
                _clusters.BitSets = reader.ReadBytes(count);
            }
            reader.Close();
            if (_noDrawTextureIndex != -1)
            {
                for (var num40 = 0; num40 < _faces.Length; num40++)
                {
                    if (_faces[num40].TextureId == _noDrawTextureIndex)
                    {
                        _faces[num40] = null;
                    }
                }
            }
            _facesDrawn = new BitArray(_numFaces, false);
        }

        private static bool PlaneXIntersect(Vector3F p1, Vector3F p2, float planeX, float planeMinY, float planeMaxY,
            float planeMinZ, float planeMaxZ)
        {
            var num = p2.X - p1.X;
            var num2 = p2.Y - p1.Y;
            var num3 = p2.Z - p1.Z;
            if (num != 0f)
            {
                var num4 = (planeX - p1.X)/num;
                if ((num4 >= 0f) && (num4 <= 1f))
                {
                    var num5 = p1.Y + (num4*num2);
                    var num6 = p1.Z + (num4*num3);
                    if (((num5 >= planeMinY) && (num5 <= planeMaxY)) && ((num6 >= planeMinZ) && (num6 <= planeMaxZ)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool PlaneYIntersect(Vector3F p1, Vector3F p2, float planeY, float planeMinX, float planeMaxX,
            float planeMinZ, float planeMaxZ)
        {
            var num = p2.X - p1.X;
            var num2 = p2.Y - p1.Y;
            var num3 = p2.Z - p1.Z;
            if (num2 != 0f)
            {
                var num4 = (planeY - p1.Y)/num2;
                if ((num4 >= 0f) && (num4 <= 1f))
                {
                    var num5 = p1.X + (num4*num);
                    var num6 = p1.Z + (num4*num3);
                    if (((num5 >= planeMinX) && (num5 <= planeMaxX)) && ((num6 >= planeMinZ) && (num6 <= planeMaxZ)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool PlaneZIntersect(Vector3F p1, Vector3F p2, float planeZ, float planeMinX, float planeMaxX,
            float planeMinY, float planeMaxY)
        {
            var num = p2.X - p1.X;
            var num2 = p2.Y - p1.Y;
            var num3 = p2.Z - p1.Z;
            if (num3 != 0f)
            {
                var num4 = (planeZ - p1.Z)/num3;
                if ((num4 >= 0f) && (num4 <= 1f))
                {
                    var num5 = p1.X + (num4*num);
                    var num6 = p1.Y + (num4*num2);
                    if (((num5 >= planeMinX) && (num5 <= planeMaxX)) && ((num6 >= planeMinY) && (num6 <= planeMaxY)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool PointInBox(Vector3F position, float[] mins, float[] maxes)
        {
            var flag = false;
            if ((((position.X >= mins[0]) && (position.X <= maxes[0])) &&
                 ((position.Y >= mins[1]) && (position.Y <= maxes[1]))) &&
                ((position.Z >= mins[2]) && (position.Z <= maxes[2])))
            {
                flag = true;
            }
            return flag;
        }

        public static bool RayBoxCollision(Vector3F p1, Vector3F p2, Vector3F boxMin, Vector3F boxMax)
        {
            return (PlaneXIntersect(p1, p2, boxMin.X, boxMin.Y, boxMax.Y, boxMin.Z, boxMax.Z) ||
                    (PlaneXIntersect(p1, p2, boxMax.X, boxMin.Y, boxMax.Y, boxMin.Z, boxMax.Z) ||
                     (PlaneYIntersect(p1, p2, boxMin.Y, boxMin.X, boxMax.X, boxMin.Z, boxMax.Z) ||
                      (PlaneYIntersect(p1, p2, boxMax.Y, boxMin.X, boxMax.X, boxMin.Z, boxMax.Z) ||
                       (PlaneZIntersect(p1, p2, boxMin.Z, boxMin.X, boxMax.X, boxMin.Y, boxMax.Y) ||
                        PlaneZIntersect(p1, p2, boxMax.Z, boxMin.X, boxMax.X, boxMin.Y, boxMax.Y))))));
        }

        public void RenderLevel(Vector3F cameraPosition, Frustrum gameFrustrum)
        {
            RenderSkyBox(cameraPosition);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            _facesDrawn.SetAll(false);

            var index = FindLeaf(cameraPosition);
            var cluster = _leaves[index].Cluster;
            var numLeaves = _numLeaves;
            while (numLeaves > 0)
            {
                numLeaves--;
                var leaf = _leaves[numLeaves];
                if (IsClusterVisible(cluster, leaf.Cluster) &&
                    gameFrustrum.BoxInFrustrum(leaf.Min.X, leaf.Min.Y, leaf.Min.Z, leaf.Max.X, leaf.Max.Y, leaf.Max.Z))
                {
                    var numLeafFaces = leaf.NumLeafFaces;
                    while (numLeafFaces > 0)
                    {
                        numLeafFaces--;
                        var num5 = _leafFaces[leaf.LeafFace + numLeafFaces];
                        if ((_faces[num5] != null) && !_facesDrawn.Get(num5))
                        {
                            _facesDrawn.Set(num5, true);
                            RenderFace(_faces[num5].Type, num5);
                        }
                    }
                }
            }
            GL.Disable(EnableCap.Blend);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.DisableClientState(ArrayCap.VertexArray);
        }

        private void RenderFace(int bspFaceType, int faceIndex)
        {
            switch (bspFaceType)
            {
                case 1:
                case 3:
                    RenderPolygonFace(faceIndex);
                    break;

                case 2:
                    break;

                default:
                    return;
            }
        }

        private unsafe void RenderPolygonFace(int faceIndex)
        {
            var currentFace = _faces[faceIndex];

            if (_textures[currentFace.TextureId] != null)
            {
                //Vertices
                fixed (void* vertexPtr = &_vertices[currentFace.StartVertexIndex*3])
                {
                    GL.VertexPointer(3, VertexPointerType.Float, 0, (IntPtr) vertexPtr);
                }

                if (currentFace.LightmapId >= 0)
                {
                    if (Engine.TexUnits > 1)
                    {
                        // Multitexture
                        if (_lightmaps[currentFace.LightmapId] != null)
                        {
                            //GL.ActiveTexture(TextureUnit.Texture1);
                            GL.ClientActiveTexture(TextureUnit.Texture1);
                            GL.Enable(EnableCap.Texture2D);
                            //Lightmap
                            GL.BindTexture(TextureTarget.Texture2D, _lightmaps[currentFace.LightmapId].TextureId);
                            /*GL.Enable(EnableCap.Texture2D);

                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Combine);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Source0Rgb, (int)TextureEnvModeSource.Previous);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Rgb, (int)TextureEnvModeOperandRgb.SrcColor);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineRgb, (int)TextureEnvModeCombine.Modulate);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Rgb, (int)TextureEnvModeSource.Texture);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand1Rgb, (int)TextureEnvModeOperandRgb.SrcColor);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.RgbScale, 2);*/

                            fixed (void* lightmapCoordPtr = &_lightmapCoords[currentFace.StartVertexIndex*2])
                            {
                                GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr) lightmapCoordPtr);
                            }
                        }

                        //GL.ActiveTexture(TextureUnit.Texture0);
                        GL.ClientActiveTexture(TextureUnit.Texture0);
                        GL.Enable(EnableCap.Texture2D);
                        GL.BindTexture(TextureTarget.Texture2D, _textures[currentFace.TextureId].TextureId);

                        fixed (void* textureCoordsPtr = &_textureCoords[currentFace.StartVertexIndex*2])
                        {
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr) textureCoordsPtr);
                        }
                        fixed (uint* indexPtr = &_meshIndices[currentFace.MeshVertexIndex])
                        {
                            GL.DrawElements(BeginMode.Triangles, currentFace.NumMeshVertices,
                                DrawElementsType.UnsignedInt, (IntPtr) indexPtr);
                        }
                        // Desactiva Textura1
                        GL.Disable(EnableCap.Texture2D);
                        GL.ClientActiveTexture(TextureUnit.Texture1);
                    }
                    else
                    {
                        if (_lightmaps[currentFace.LightmapId] != null)
                        {
                            //Lightmap
                            GL.BindTexture(TextureTarget.Texture2D, _lightmaps[currentFace.LightmapId].TextureId);

                            //Setup blending
                            GL.Disable(EnableCap.Blend);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,
                                (int) TextureEnvMode.Replace);

                            //Set texture coordinates
                            fixed (void* lightmapCoordPtr = &_lightmapCoords[currentFace.StartVertexIndex*2])
                            {
                                GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr) lightmapCoordPtr);
                            }

                            //Draw face
                            fixed (uint* indexPtr = &_meshIndices[currentFace.MeshVertexIndex])
                            {
                                GL.DrawElements(BeginMode.Triangles, currentFace.NumMeshVertices,
                                    DrawElementsType.UnsignedInt, (IntPtr) indexPtr);
                            }
                        }
                        GL.BindTexture(TextureTarget.Texture2D, _textures[currentFace.TextureId].TextureId);
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.SrcColor);
                        GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,
                            (int) TextureEnvMode.Replace);

                        fixed (void* textureCoordsPtr = &_textureCoords[currentFace.StartVertexIndex*2])
                        {
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr) textureCoordsPtr);
                        }
                        fixed (uint* indexPtr = &_meshIndices[currentFace.MeshVertexIndex])
                        {
                            GL.DrawElements(BeginMode.Triangles, currentFace.NumMeshVertices,
                                DrawElementsType.UnsignedInt, (IntPtr) indexPtr);
                        }
                    }
                }
            }
        }

        private void RenderSkyBox(Vector3F cameraPosition)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.Disable(EnableCap.TextureGenS);
            GL.Disable(EnableCap.TextureGenT);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.AlphaTest);
            GL.Disable(EnableCap.DepthTest);
            GL.CullFace(CullFaceMode.Back);
            GL.DepthMask(false);
            GL.PushMatrix();
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Replace);
            GL.BindTexture(TextureTarget.Texture2D, _skyBoxTextures[2].TextureId);
            // Front
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y - 10f, cameraPosition.Z - 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y - 10f, cameraPosition.Z - 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y + 10f, cameraPosition.Z - 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y + 10f, cameraPosition.Z - 10f);
            GL.End();
            // Right
            GL.BindTexture(TextureTarget.Texture2D, _skyBoxTextures[0].TextureId);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y - 10f, cameraPosition.Z - 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y - 10f, cameraPosition.Z + 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y + 10f, cameraPosition.Z + 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y + 10f, cameraPosition.Z - 10f);
            GL.End();
            // Back
            GL.BindTexture(TextureTarget.Texture2D, _skyBoxTextures[5].TextureId);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y - 10f, cameraPosition.Z + 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y - 10f, cameraPosition.Z + 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y + 10f, cameraPosition.Z + 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y + 10f, cameraPosition.Z + 10f);
            GL.End();
            // Left
            GL.BindTexture(TextureTarget.Texture2D, _skyBoxTextures[3].TextureId);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y - 10f, cameraPosition.Z + 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y - 10f, cameraPosition.Z - 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y + 10f, cameraPosition.Z - 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y + 10f, cameraPosition.Z + 10f);
            GL.End();
            // Top
            GL.BindTexture(TextureTarget.Texture2D, _skyBoxTextures[4].TextureId);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y + 10f, cameraPosition.Z - 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y + 10f, cameraPosition.Z - 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y + 10f, cameraPosition.Z + 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y + 10f, cameraPosition.Z + 10f);
            GL.End();
            // Bottom
            GL.BindTexture(TextureTarget.Texture2D, _skyBoxTextures[1].TextureId);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y - 10f, cameraPosition.Z + 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y - 10f, cameraPosition.Z + 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(cameraPosition.X + 10f, cameraPosition.Y - 10f, cameraPosition.Z - 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(cameraPosition.X - 10f, cameraPosition.Y - 10f, cameraPosition.Z - 10f);
            GL.End();

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.CullFace(CullFaceMode.Front);
            GL.PopMatrix();
        }
    }

    public class BspBrush
    {
        public static readonly int SizeInBytes = 12;
        public int FirstSide;
        public int NumSides;
        public int TextureIndex;
    }

    public class BspBrushSide
    {
        public static readonly int SizeInBytes = 8;
        public int Plane;
        public int Texture;
    }

    public class BspEntity : ICollection, IEnumerable
    {
        private readonly ArrayList _argValues;

        public BspEntity()
        {
            _argValues = new ArrayList();
        }

        public BspEntity(string entityString)
        {
            _argValues = new ArrayList();
            var flag = true;
            var newArgValue = new ArgValue();
            var index = entityString.IndexOf("\"");
            while (index > -1)
            {
                var num2 = entityString.IndexOf("\"", index + 1);
                if (num2 > -1)
                {
                    var str = entityString.Substring(index + 1, (num2 - index) - 1);
                    if (flag)
                    {
                        newArgValue = new ArgValue();
                        newArgValue.Argument = str;
                        flag = false;
                    }
                    else
                    {
                        newArgValue.Value = str;
                        AddArgValue(newArgValue);
                        flag = true;
                    }
                    index = entityString.IndexOf("\"", num2 + 1);
                }
                else
                {
                    index = -1;
                }
            }
        }

        public void CopyTo(Array array, int index)
        {
            _argValues.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _argValues.GetEnumerator();
        }

        public int Count
        {
            get { return _argValues.Count; }
        }

        public bool IsSynchronized
        {
            get { return _argValues.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return _argValues.SyncRoot; }
        }

        public void AddArgValue(ArgValue newArgValue)
        {
            _argValues.Add(newArgValue);
        }

        public void AddArgValue(string argument, string value)
        {
            _argValues.Add(new ArgValue(argument, value));
        }

        public string SeekFirstValue(string argument)
        {
            var str = "";
            var strArray = SeekValuesByArgument(argument);
            if (strArray.Length > 0)
            {
                str = strArray[0];
            }
            return str;
        }

        public string[] SeekValuesByArgument(string argument)
        {
            var list = new ArrayList();
            foreach (ArgValue value2 in _argValues)
            {
                if (value2.Argument == argument)
                {
                    list.Add(value2.Value);
                }
            }
            return (string[]) list.ToArray(typeof (string));
        }
    }

    public class BspEntityCollection : ICollection, IEnumerable
    {
        private readonly ArrayList _allEntities;

        public BspEntityCollection()
        {
            _allEntities = new ArrayList();
        }

        public BspEntityCollection(string EntityString)
        {
            _allEntities = new ArrayList();
            var index = EntityString.IndexOf("{");
            while (index >= 0)
            {
                var num2 = EntityString.IndexOf("}", index + 1);
                if (num2 > -1)
                {
                    var entityString = EntityString.Substring(index + 1, (num2 - index) - 1);
                    AddEntity(new BspEntity(entityString));
                    index = EntityString.IndexOf("{", num2 + 1);
                }
                else
                {
                    index = -1;
                }
            }
        }

        public void CopyTo(Array array, int index)
        {
            _allEntities.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _allEntities.GetEnumerator();
        }

        public int Count
        {
            get { return _allEntities.Count; }
        }

        public bool IsSynchronized
        {
            get { return _allEntities.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return _allEntities.SyncRoot; }
        }

        public void AddEntity(BspEntity newEntity)
        {
            _allEntities.Add(newEntity);
        }

        public BspEntity[] SeekEntitiesByClassname(string classname)
        {
            var list = new ArrayList();
            foreach (BspEntity entity in _allEntities)
            {
                foreach (ArgValue value2 in entity)
                {
                    if ((value2.Argument == "classname") && (value2.Value == classname))
                    {
                        list.Add(entity);
                    }
                }
            }
            return (BspEntity[]) list.ToArray(typeof (BspEntity));
        }

        public string SeekFirstEntityValue(string classname, string argument)
        {
            var str = "";
            var entityArray = SeekEntitiesByClassname(classname);
            if (entityArray.Length > 0)
            {
                var strArray = entityArray[0].SeekValuesByArgument(argument);
                if (strArray.Length > 0)
                {
                    str = strArray[0];
                }
            }
            return str;
        }
    }

    public class BspFace
    {
        public static readonly int SizeInBytes = 0x68;
        public int Effect;
        public int LightmapId = -1;
        public int[] MapCorner = new int[2];
        public Vector3F MapPosition = new Vector3F();
        public int[] MapSize = new int[2];
        public Vector3F[] MapVectors = new Vector3F[2];
        public int MeshVertexIndex = -1;
        public Vector3F Normal = new Vector3F();
        public int NumMeshVertices;
        public int NumVertices;
        public int[] Size = new int[2];
        public int StartVertexIndex = -1;
        public int TextureId = -1;
        public int Type;

        public BspFace()
        {
            MapVectors[0] = new Vector3F();
            MapVectors[1] = new Vector3F();
        }
    }

    public class BspHeader
    {
        public string Id = "";
        public int Version;
    }

    public class BspLeaf
    {
        public static readonly int SizeInBytes = 48;
        public int Area;
        public int Cluster;
        public int LeafBrush;
        public int LeafFace;
        public Vector3I Max = new Vector3I();
        public Vector3I Min = new Vector3I();
        public int NumLeafBrushes;
        public int NumLeafFaces;
    }

    public class BspLightmap
    {
        public static readonly int SizeInBytes = 49152;
        public byte[] ImageBytes = new byte[128*128*3];
    }

    public class BspLump
    {
        public int Length;
        public int Offset;
    }

    public class BspModel
    {
        public static readonly int SizeInBytes = 40;
        public int FirstBrush;
        public int FirstFace;
        public float[] Maxes = new float[3];
        public float[] Mins = new float[3];
        public int NumBrushes;
        public int NumFaces;
    }

    public class BspNode
    {
        public static readonly int SizeInBytes = 36;
        public int Back;
        public int Front;
        public Vector3I Max = new Vector3I();
        public Vector3I Min = new Vector3I();
        public int Plane;
    }

    public class BspPlane
    {
        public static readonly int SizeInBytes = 0x10;
        public float Distance;
        public Vector3F Normal = new Vector3F();
    }

    public class BspShader
    {
        public static readonly int SizeInBytes = 0x48;
        public int BrushIndex;
        public int ContentFlags;
        public string Name = "";
    }

    public class BspTexture
    {
        public static readonly int SizeInBytes = 72;
        public int Contents;
        public int Flags;
        public string Name = "";
    }

    public class BspVisData
    {
        public byte[] BitSets;
        public int BytesPerCluster;
        public int NumClusters;
    }
}