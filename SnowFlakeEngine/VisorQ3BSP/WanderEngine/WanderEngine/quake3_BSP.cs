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

namespace WanderEngine.WanderEngine
{
    public class BSPFile
    {
        private static readonly int LeafBrushSizeInBytes = 4;
        private static readonly int LeafFaceSizeInBytes = 4;
        private static readonly int MaxLumps = 0x11;
        private static readonly int MeshIndexSizeInBytes = 4;
        private static readonly float QuakeEpsilon = 0.03125f;
        private static readonly int VertexSizeInBytes = 0x2c;
        private readonly Vector3f CollisionExtents = new Vector3f();
        private readonly BSPHeader Header = new BSPHeader();
        private readonly Hashtable IndexTriggerHash = new Hashtable();
        private readonly BSPLump[] Lumps = new BSPLump[MaxLumps];
        private BSPBrush[] Brushes;
        private BSPBrushSide[] BrushSides;
        private BSPVisData Clusters;
        private Vector3f CollisionEnd = new Vector3f();
        public CollisionInformation CollisionInfo = new CollisionInformation();
        private Vector3f CollisionMax = new Vector3f();
        private Vector3f CollisionMin = new Vector3f();
        private float CollisionOffset;
        private Vector3f CollisionStart = new Vector3f();
        private CollisionTypes CollisionType = CollisionTypes.Ray;
        public BSPEntityCollection Entities;
        private string EntityString = "";
        private int EntityStringLength;
        private BSPFace[] Faces;
        private BitArray FacesDrawn;
        private float Gamma = 10f;
        private int[] LeafBrushes;
        private int[] LeafFaces;
        private BSPLeaf[] Leaves;
        private float[] LightmapCoords;
        private Texture[] Lightmaps;
        private BSPTexture[] LoadTextures;
        private uint[] MeshIndices;
        private BSPModel[] Models;
        private BSPNode[] Nodes;
        private int NoDrawTextureIndex = -1;
        private int NumBrushes;
        private int NumBrushSides;
        private int NumFaces;
        private int NumLeafBrushes;
        private int NumLeafFaces;
        private int NumLeaves;
        private int NumLightmaps;
        private int NumMeshIndices;
        private int NumModels;
        private int NumNodes;
        private int NumPlanes;
        private int NumShaders;
        private int NumTextures;
        private int NumVertices;
        private BSPPlane[] Planes;
        private BSPShader[] Shaders;
        private Texture[] SkyBoxTextures;
        private float[] TextureCoords;
        private Texture[] Textures;
        private Trigger[] Triggers;
        private float[] Vertices;

        public BSPFile(string FileName)
        {
            LoadBSP(FileName);
        }

        private bool BoxesCollide(float[] Mins, float[] Maxes, float[] Mins2, float[] Maxes2)
        {
            var flag = false;
            if ((((Maxes2[0] > Mins[0]) && (Mins2[0] < Maxes[0])) && ((Maxes2[1] > Mins[1]) && (Mins2[1] < Maxes[1]))) &&
                ((Maxes2[2] > Mins[2]) && (Mins2[2] < Maxes[2])))
            {
                flag = true;
            }
            return flag;
        }

        public static void ChangeGamma(ref Bitmap ImageBmp, float Factor)
        {
            for (var i = 0; i < ImageBmp.Width; i++)
            {
                for (var j = 0; j < ImageBmp.Height; j++)
                {
                    var num3 = 1f;
                    var num4 = 0f;
                    var pixel = ImageBmp.GetPixel(i, j);
                    float r = pixel.R;
                    float g = pixel.G;
                    float b = pixel.B;
                    r = (r*Factor)/255f;
                    g = (g*Factor)/255f;
                    b = (b*Factor)/255f;
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
                    ImageBmp.SetPixel(i, j, Color.FromArgb((int) r, (int) g, (int) b));
                }
            }
        }

        private void CheckBrush(BSPBrush CurrentBrush)
        {
            var num = -1f;
            var num2 = 1f;
            var flag = false;
            var flag2 = false;
            var distance = 0f;
            var normal = new Vector3f();
            for (var i = 0; i < CurrentBrush.NumSides; i++)
            {
                var side = BrushSides[CurrentBrush.FirstSide + i];
                var plane = Planes[side.Plane];
                var num5 = 0f;
                var num6 = 0f;
                if (CollisionType == CollisionTypes.Box)
                {
                    var vector2 = new Vector3f();
                    if (plane.Normal.X < 0f)
                    {
                        vector2.X = CollisionMax.X;
                    }
                    else
                    {
                        vector2.X = CollisionMin.X;
                    }
                    if (plane.Normal.Y < 0f)
                    {
                        vector2.Y = CollisionMax.Y;
                    }
                    else
                    {
                        vector2.Y = CollisionMin.Y;
                    }
                    if (plane.Normal.Z < 0f)
                    {
                        vector2.Z = CollisionMax.Z;
                    }
                    else
                    {
                        vector2.Z = CollisionMin.Z;
                    }
                    num5 = ((((CollisionStart.X + vector2.X)*plane.Normal.X) +
                             ((CollisionStart.Y + vector2.Y)*plane.Normal.Y)) +
                            ((CollisionStart.Z + vector2.Z)*plane.Normal.Z)) - plane.Distance;
                    num6 = ((((CollisionEnd.X + vector2.X)*plane.Normal.X) +
                             ((CollisionEnd.Y + vector2.Y)*plane.Normal.Y)) +
                            ((CollisionEnd.Z + vector2.Z)*plane.Normal.Z)) - plane.Distance;
                }
                else
                {
                    num5 = ((((plane.Normal.X*CollisionStart.X) + (plane.Normal.Y*CollisionStart.Y)) +
                             (plane.Normal.Z*CollisionStart.Z)) - plane.Distance) - CollisionOffset;
                    num6 = ((((plane.Normal.X*CollisionEnd.X) + (plane.Normal.Y*CollisionEnd.Y)) +
                             (plane.Normal.Z*CollisionEnd.Z)) - plane.Distance) - CollisionOffset;
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

        private void CheckNode(int NodeIndex, float StartFraction, float EndFraction, Vector3f Start, Vector3f End)
        {
            if (CollisionInfo.Fraction > StartFraction)
            {
                if (NodeIndex < 0)
                {
                    var leaf = Leaves[~NodeIndex];
                    for (var i = 0; i < leaf.NumLeafBrushes; i++)
                    {
                        var currentBrush = Brushes[LeafBrushes[leaf.LeafBrush + i]];
                        if ((currentBrush.NumSides > 0) && ((LoadTextures[currentBrush.TextureIndex].Contents & 1) > 0))
                        {
                            CheckBrush(currentBrush);
                        }
                    }
                }
                else
                {
                    var node = Nodes[NodeIndex];
                    var plane = Planes[node.Plane];
                    var num2 = (((plane.Normal.X*Start.X) + (plane.Normal.Y*Start.Y)) + (plane.Normal.Z*Start.Z)) -
                               plane.Distance;
                    var num3 = (((plane.Normal.X*End.X) + (plane.Normal.Y*End.Y)) + (plane.Normal.Z*End.Z)) -
                               plane.Distance;
                    if (CollisionType == CollisionTypes.Box)
                    {
                        CollisionOffset = (Math.Abs(CollisionExtents.X*plane.Normal.X) +
                                           Math.Abs(CollisionExtents.Y*plane.Normal.Y)) +
                                          Math.Abs(CollisionExtents.Z*plane.Normal.Z);
                    }
                    if ((num2 >= CollisionOffset) && (num3 >= CollisionOffset))
                    {
                        CheckNode(node.Front, StartFraction, EndFraction, Start, End);
                    }
                    else if ((num2 < -CollisionOffset) && (num3 < -CollisionOffset))
                    {
                        CheckNode(node.Back, StartFraction, EndFraction, Start, End);
                    }
                    else
                    {
                        var nodeIndex = -1;
                        var front = -1;
                        var num6 = 0f;
                        var num7 = 0f;
                        var end = new Vector3f();
                        if (num2 < num3)
                        {
                            nodeIndex = node.Back;
                            front = node.Front;
                            var num8 = 1f/(num2 - num3);
                            num6 = ((num2 - QuakeEpsilon) - CollisionOffset)*num8;
                            num7 = ((num2 + QuakeEpsilon) + CollisionOffset)*num8;
                        }
                        else if (num3 < num2)
                        {
                            nodeIndex = node.Front;
                            front = node.Back;
                            var num9 = 1f/(num2 - num3);
                            num6 = ((num2 + QuakeEpsilon) + CollisionOffset)*num9;
                            num7 = ((num2 - QuakeEpsilon) - CollisionOffset)*num9;
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
                        end = Start + (End - Start)*num6;
                        var endFraction = StartFraction + ((EndFraction - StartFraction)*num6);
                        CheckNode(nodeIndex, StartFraction, endFraction, Start, end);
                        end = Start + (End - Start)*num7;
                        endFraction = StartFraction + ((EndFraction - StartFraction)*num7);
                        CheckNode(front, endFraction, EndFraction, end, End);
                    }
                }
            }
        }

        private void DetectCollision(Vector3f Start, Vector3f End)
        {
            CollisionInfo = new CollisionInformation();
            CollisionStart = new Vector3f(Start.X, Start.Y, Start.Z);
            CollisionEnd = new Vector3f(End.X, End.Y, End.Z);
            CheckNode(0, 0f, 1f, CollisionStart, CollisionEnd);
            if (CollisionInfo.Fraction == 1f)
            {
                CollisionInfo.EndPoint = CollisionEnd;
            }
            else
            {
                CollisionInfo.EndPoint = CollisionStart + (CollisionEnd - CollisionStart)*CollisionInfo.Fraction;
            }
        }

        public void DetectCollisionBox(Vector3f Start, Vector3f End, Vector3f Min, Vector3f Max)
        {
            CollisionType = CollisionTypes.Box;
            CollisionMin = Min;
            CollisionMax = Max;
            CollisionExtents.X = (-Min.X > Max.X) ? -Min.X : Max.X;
            CollisionExtents.Y = (-Min.Y > Max.Y) ? -Min.Y : Max.Y;
            CollisionExtents.Z = (-Min.Z > Max.Z) ? -Min.Z : Max.Z;
            DetectCollision(Start, End);
        }

        public void DetectCollisionRay(Vector3f Start, Vector3f End)
        {
            CollisionType = CollisionTypes.Ray;
            CollisionOffset = 0f;
            DetectCollision(Start, End);
        }

        public void DetectCollisionSphere(Vector3f Start, Vector3f End, float Radius)
        {
            CollisionType = CollisionTypes.Sphere;
            CollisionOffset = Radius;
            DetectCollision(Start, End);
        }

        public Trigger DetectTriggerCollisions(Vector3f Position)
        {
            Trigger trigger = null;
            for (var i = 1; i < Models.Length; i++)
            {
                var model = Models[i];
                if (PointInBox(Position, model.Mins, model.Maxes))
                {
                    try
                    {
                        trigger = (Trigger) IndexTriggerHash[i];
                    }
                    catch
                    {
                    }
                    return trigger;
                }
            }
            return trigger;
        }

        private int FindLeaf(Vector3f CameraPosition)
        {
            var index = 0;
            var num2 = 0f;
            while (index >= 0)
            {
                var node = Nodes[index];
                var plane = Planes[node.Plane];
                num2 = (((plane.Normal.X*CameraPosition.X) + (plane.Normal.Y*CameraPosition.Y)) +
                        (plane.Normal.Z*CameraPosition.Z)) - plane.Distance;
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

        private bool IsClusterVisible(int CurrentCluster, int TestCluster)
        {
            var flag = true;
            if (((Clusters.BitSets != null) && (CurrentCluster >= 0)) && (TestCluster >= 0))
            {
                var num = Clusters.BitSets[(CurrentCluster*Clusters.BytesPerCluster) + (TestCluster/8)];
                var num2 = num & (1 << (TestCluster & 7));
                if (num2 <= 0)
                {
                    flag = false;
                }
            }
            return flag;
        }

        public void LoadBSP(string FileName)
        {
            var stream = new FileStream(FileName, FileMode.Open);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int) stream.Length);
            var reader = new BinaryReader(new MemoryStream(buffer));
            Header.ID = Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
            Header.Version = reader.ReadInt32();
            for (var i = 0; i < MaxLumps; i++)
            {
                Lumps[i] = new BSPLump();
                Lumps[i].Offset = reader.ReadInt32();
                Lumps[i].Length = reader.ReadInt32();
            }
            if ((Header.ID != "IBSP") && (Header.Version != 0x2e))
            {
                throw new Exception("Wrong file type or version");
            }
            NumVertices = Lumps[10].Length/VertexSizeInBytes;
            Vertices = new float[NumVertices*3];
            TextureCoords = new float[NumVertices*2];
            LightmapCoords = new float[NumVertices*2];
            NumFaces = Lumps[13].Length/BSPFace.SizeInBytes;
            Faces = new BSPFace[NumFaces];
            NumTextures = Lumps[1].Length/BSPTexture.SizeInBytes;
            LoadTextures = new BSPTexture[NumTextures];
            Textures = new Texture[NumTextures];
            NumLightmaps = Lumps[14].Length/BSPLightmap.SizeInBytes;
            Lightmaps = new Texture[NumLightmaps];
            NumNodes = Lumps[3].Length/BSPNode.SizeInBytes;
            Nodes = new BSPNode[NumNodes];
            NumLeaves = Lumps[4].Length/BSPLeaf.SizeInBytes;
            Leaves = new BSPLeaf[NumLeaves];
            NumLeafFaces = Lumps[5].Length/LeafFaceSizeInBytes;
            LeafFaces = new int[NumLeafFaces];
            NumPlanes = Lumps[2].Length/BSPPlane.SizeInBytes;
            Planes = new BSPPlane[NumPlanes];
            NumMeshIndices = Lumps[11].Length/MeshIndexSizeInBytes;
            MeshIndices = new uint[NumMeshIndices];
            NumModels = Lumps[7].Length/BSPModel.SizeInBytes;
            Models = new BSPModel[NumModels];
            NumBrushes = Lumps[8].Length/BSPBrush.SizeInBytes;
            Brushes = new BSPBrush[NumBrushes];
            NumBrushSides = Lumps[9].Length/BSPBrushSide.SizeInBytes;
            BrushSides = new BSPBrushSide[NumBrushSides];
            NumLeafBrushes = Lumps[6].Length/LeafBrushSizeInBytes;
            LeafBrushes = new int[NumLeafBrushes];
            NumShaders = Lumps[12].Length/BSPShader.SizeInBytes;
            Shaders = new BSPShader[NumShaders];
            Clusters = new BSPVisData();
            EntityStringLength = Lumps[0].Length;
            reader.BaseStream.Seek(Lumps[0].Offset, SeekOrigin.Begin);
            foreach (var num2 in reader.ReadBytes(EntityStringLength))
            {
                var ch = Convert.ToChar(num2);
                if (ch != '\0')
                {
                    EntityString = EntityString + ch;
                }
            }
            Entities = new BSPEntityCollection(EntityString);
            var s = Entities.SeekFirstEntityValue("worldspawn", "ambient");
            try
            {
                Gamma = float.Parse(s);
                Gamma /= 17f;
            }
            catch
            {
            }
            var entityArray = Entities.SeekEntitiesByClassname("trigger_multiple");
            if (entityArray.Length > 0)
            {
                Triggers = new Trigger[entityArray.Length];
                var num3 = 0;
                foreach (var entity in entityArray)
                {
                    try
                    {
                        var trigger = new Trigger();
                        trigger.Name = entity.SeekFirstValue("trigger_name");
                        var str2 = entity.SeekFirstValue("model").Replace("*", string.Empty);
                        trigger.ModelIndex = int.Parse(str2);
                        IndexTriggerHash[trigger.ModelIndex] = trigger;
                        Triggers[num3] = trigger;
                        num3++;
                    }
                    catch
                    {
                    }
                }
            }
            var index = 0;
            var num5 = 0;
            var offset = Lumps[10].Offset;
            for (var j = 0; j < NumVertices; j++)
            {
                reader.BaseStream.Seek(offset + (j*VertexSizeInBytes), SeekOrigin.Begin);
                Vertices[index] = reader.ReadSingle();
                Vertices[index + 2] = -reader.ReadSingle();
                Vertices[index + 1] = reader.ReadSingle();
                TextureCoords[num5] = reader.ReadSingle();
                TextureCoords[num5 + 1] = -reader.ReadSingle();
                LightmapCoords[num5] = reader.ReadSingle();
                LightmapCoords[num5 + 1] = -reader.ReadSingle();
                index += 3;
                num5 += 2;
            }
            var num8 = Lumps[13].Offset;
            for (var k = 0; k < NumFaces; k++)
            {
                reader.BaseStream.Seek(num8 + (k*BSPFace.SizeInBytes), SeekOrigin.Begin);
                Faces[k] = new BSPFace();
                Faces[k].TextureID = reader.ReadInt32();
                Faces[k].Effect = reader.ReadInt32();
                Faces[k].Type = reader.ReadInt32();
                Faces[k].StartVertexIndex = reader.ReadInt32();
                Faces[k].NumVertices = reader.ReadInt32();
                Faces[k].MeshVertexIndex = reader.ReadInt32();
                Faces[k].NumMeshVertices = reader.ReadInt32();
                Faces[k].LightmapID = reader.ReadInt32();
                Faces[k].MapCorner[0] = reader.ReadInt32();
                Faces[k].MapCorner[1] = reader.ReadInt32();
                Faces[k].MapSize[0] = reader.ReadInt32();
                Faces[k].MapSize[1] = reader.ReadInt32();
                Faces[k].MapPosition.X = reader.ReadSingle();
                Faces[k].MapPosition.Y = reader.ReadSingle();
                Faces[k].MapPosition.Z = reader.ReadSingle();
                Faces[k].MapVectors[0].X = reader.ReadSingle();
                Faces[k].MapVectors[0].Y = reader.ReadSingle();
                Faces[k].MapVectors[0].Z = reader.ReadSingle();
                Faces[k].MapVectors[1].X = reader.ReadSingle();
                Faces[k].MapVectors[1].Y = reader.ReadSingle();
                Faces[k].MapVectors[1].Z = reader.ReadSingle();
                Faces[k].Normal.X = reader.ReadSingle();
                Faces[k].Normal.Y = reader.ReadSingle();
                Faces[k].Normal.Z = reader.ReadSingle();
                Faces[k].Size[0] = reader.ReadInt32();
                Faces[k].Size[1] = reader.ReadInt32();
            }
            reader.BaseStream.Seek(Lumps[1].Offset, SeekOrigin.Begin);
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
            for (var i = 0; i < NumTextures; i++)
            {
                LoadTextures[i] = new BSPTexture();

                var NameBytes = reader.ReadBytes(64);
                for (var NameByteIndex = 0; NameByteIndex < 64; NameByteIndex++)
                {
                    if (NameBytes[NameByteIndex] != '\0')
                    {
                        LoadTextures[i].Name += Convert.ToChar(NameBytes[NameByteIndex]);
                    }
                }

                LoadTextures[i].Flags = reader.ReadInt32();
                LoadTextures[i].Contents = reader.ReadInt32();

                //Check for skybox texture
                if (LoadTextures[i].Name.IndexOf(Utility.AdaptRelativePathToPlatform("bookstore/no_draw")) != -1)
                {
                    NoDrawTextureIndex = i;
                }
            }
            var dirPath = Directory.GetCurrentDirectory();
            var texturePath = Utility.AdaptRelativePathToPlatform("../");
            Directory.SetCurrentDirectory(texturePath);

            for (var n = 0; n < NumTextures; n++)
            {
                var path = LoadTextures[n].Name + ".jpg";
                var str4 = LoadTextures[n].Name + ".tga";
                try
                {
                    if (File.Exists(path))
                    {
                        Textures[n] = new Texture(path);
                    }
                    else if (File.Exists(str4))
                    {
                        Textures[n] = new Texture(str4);
                    }
                    else
                    {
                        Textures[n] = null;
                    }
                }
                catch
                {
                    Textures[n] = null;
                }
            }

            SkyBoxTextures = new Texture[6];
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

            SkyBoxTextures[0] = new Texture(Utility.AdaptRelativePathToPlatform(sky0), true);
            SkyBoxTextures[1] = new Texture(Utility.AdaptRelativePathToPlatform(sky1), true);
            SkyBoxTextures[2] = new Texture(Utility.AdaptRelativePathToPlatform(sky2), true);
            SkyBoxTextures[3] = new Texture(Utility.AdaptRelativePathToPlatform(sky3), true);
            SkyBoxTextures[4] = new Texture(Utility.AdaptRelativePathToPlatform(sky4), true);
            SkyBoxTextures[5] = new Texture(Utility.AdaptRelativePathToPlatform(sky5), true);

            Directory.SetCurrentDirectory(dirPath);

            reader.BaseStream.Seek(Lumps[14].Offset, SeekOrigin.Begin);
            for (var num14 = 0; num14 < NumLightmaps; num14++)
            {
                var buffer4 = reader.ReadBytes(BSPLightmap.SizeInBytes);
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
                ChangeGamma(ref imageBmp, Gamma);
                Lightmaps[num14] = new Texture(imageBmp);
            }
            var num21 = Lumps[3].Offset;
            for (var num22 = 0; num22 < NumNodes; num22++)
            {
                reader.BaseStream.Seek(num21 + (num22*BSPNode.SizeInBytes), SeekOrigin.Begin);
                Nodes[num22] = new BSPNode();
                Nodes[num22].Plane = reader.ReadInt32();
                Nodes[num22].Front = reader.ReadInt32();
                Nodes[num22].Back = reader.ReadInt32();
                Nodes[num22].Min.X = reader.ReadInt32();
                Nodes[num22].Min.Z = -reader.ReadInt32();
                Nodes[num22].Min.Y = reader.ReadInt32();
                Nodes[num22].Max.X = reader.ReadInt32();
                Nodes[num22].Max.Z = -reader.ReadInt32();
                Nodes[num22].Max.Y = reader.ReadInt32();
            }
            var num23 = Lumps[4].Offset;
            for (var num24 = 0; num24 < NumLeaves; num24++)
            {
                reader.BaseStream.Seek(num23 + (num24*BSPLeaf.SizeInBytes), SeekOrigin.Begin);
                Leaves[num24] = new BSPLeaf();
                Leaves[num24].Cluster = reader.ReadInt32();
                Leaves[num24].Area = reader.ReadInt32();
                Leaves[num24].Min.X = reader.ReadInt32();
                Leaves[num24].Min.Z = -reader.ReadInt32();
                Leaves[num24].Min.Y = reader.ReadInt32();
                Leaves[num24].Max.X = reader.ReadInt32();
                Leaves[num24].Max.Z = -reader.ReadInt32();
                Leaves[num24].Max.Y = reader.ReadInt32();
                Leaves[num24].LeafFace = reader.ReadInt32();
                Leaves[num24].NumLeafFaces = reader.ReadInt32();
                Leaves[num24].LeafBrush = reader.ReadInt32();
                Leaves[num24].NumLeafBrushes = reader.ReadInt32();
            }
            reader.BaseStream.Seek(Lumps[5].Offset, SeekOrigin.Begin);
            for (var num25 = 0; num25 < NumLeafFaces; num25++)
            {
                LeafFaces[num25] = reader.ReadInt32();
            }
            var num26 = Lumps[2].Offset;
            for (var num27 = 0; num27 < NumPlanes; num27++)
            {
                reader.BaseStream.Seek(num26 + (num27*BSPPlane.SizeInBytes), SeekOrigin.Begin);
                Planes[num27] = new BSPPlane();
                Planes[num27].Normal.X = reader.ReadSingle();
                Planes[num27].Normal.Z = -reader.ReadSingle();
                Planes[num27].Normal.Y = reader.ReadSingle();
                Planes[num27].Distance = reader.ReadSingle();
            }
            reader.BaseStream.Seek(Lumps[11].Offset, SeekOrigin.Begin);
            for (var num28 = 0; num28 < NumMeshIndices; num28++)
            {
                MeshIndices[num28] = reader.ReadUInt32();
            }
            var num29 = Lumps[7].Offset;
            for (var num30 = 0; num30 < NumModels; num30++)
            {
                reader.BaseStream.Seek(num29 + (num30*BSPModel.SizeInBytes), SeekOrigin.Begin);
                Models[num30] = new BSPModel();
                Models[num30].Mins[0] = reader.ReadSingle();
                Models[num30].Maxes[2] = -reader.ReadSingle();
                Models[num30].Mins[1] = reader.ReadSingle();
                Models[num30].Maxes[0] = reader.ReadSingle();
                Models[num30].Mins[2] = -reader.ReadSingle();
                Models[num30].Maxes[1] = reader.ReadSingle();
                Models[num30].FirstFace = reader.ReadInt32();
                Models[num30].NumFaces = reader.ReadInt32();
                Models[num30].FirstBrush = reader.ReadInt32();
                Models[num30].NumBrushes = reader.ReadInt32();
            }
            var num31 = Lumps[8].Offset;
            for (var num32 = 0; num32 < NumBrushes; num32++)
            {
                reader.BaseStream.Seek(num31 + (num32*BSPBrush.SizeInBytes), SeekOrigin.Begin);
                Brushes[num32] = new BSPBrush();
                Brushes[num32].FirstSide = reader.ReadInt32();
                Brushes[num32].NumSides = reader.ReadInt32();
                Brushes[num32].TextureIndex = reader.ReadInt32();
            }
            var num33 = Lumps[9].Offset;
            for (var num34 = 0; num34 < NumBrushSides; num34++)
            {
                reader.BaseStream.Seek(num33 + (num34*BSPBrushSide.SizeInBytes), SeekOrigin.Begin);
                BrushSides[num34] = new BSPBrushSide();
                BrushSides[num34].Plane = reader.ReadInt32();
                BrushSides[num34].Texture = reader.ReadInt32();
            }
            reader.BaseStream.Seek(Lumps[6].Offset, SeekOrigin.Begin);
            for (var num35 = 0; num35 < NumLeafBrushes; num35++)
            {
                LeafBrushes[num35] = reader.ReadInt32();
            }
            var num36 = Lumps[12].Offset;
            for (var num37 = 0; num37 < NumShaders; num37++)
            {
                reader.BaseStream.Seek(num36 + (num37*BSPShader.SizeInBytes), SeekOrigin.Begin);
                Shaders[num37] = new BSPShader();
                var buffer5 = reader.ReadBytes(0x40);
                for (var num38 = 0; num38 < 0x40; num38++)
                {
                    if (buffer5[num38] != 0)
                    {
                        var shader1 = Shaders[num37];
                        shader1.Name = shader1.Name + Convert.ToChar(buffer5[num38]);
                    }
                }
                Shaders[num37].BrushIndex = reader.ReadInt32();
                Shaders[num37].ContentFlags = reader.ReadInt32();
            }
            reader.BaseStream.Seek(Lumps[0x10].Offset, SeekOrigin.Begin);
            if (Lumps[0x10].Length > 0)
            {
                Clusters.NumClusters = reader.ReadInt32();
                Clusters.BytesPerCluster = reader.ReadInt32();
                var count = Clusters.NumClusters*Clusters.BytesPerCluster;
                Clusters.BitSets = reader.ReadBytes(count);
            }
            reader.Close();
            if (NoDrawTextureIndex != -1)
            {
                for (var num40 = 0; num40 < Faces.Length; num40++)
                {
                    if (Faces[num40].TextureID == NoDrawTextureIndex)
                    {
                        Faces[num40] = null;
                    }
                }
            }
            FacesDrawn = new BitArray(NumFaces, false);
        }

        private static bool PlaneXIntersect(Vector3f P1, Vector3f P2, float PlaneX, float PlaneMinY, float PlaneMaxY,
            float PlaneMinZ, float PlaneMaxZ)
        {
            var num = P2.X - P1.X;
            var num2 = P2.Y - P1.Y;
            var num3 = P2.Z - P1.Z;
            if (num != 0f)
            {
                var num4 = (PlaneX - P1.X)/num;
                if ((num4 >= 0f) && (num4 <= 1f))
                {
                    var num5 = P1.Y + (num4*num2);
                    var num6 = P1.Z + (num4*num3);
                    if (((num5 >= PlaneMinY) && (num5 <= PlaneMaxY)) && ((num6 >= PlaneMinZ) && (num6 <= PlaneMaxZ)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool PlaneYIntersect(Vector3f P1, Vector3f P2, float PlaneY, float PlaneMinX, float PlaneMaxX,
            float PlaneMinZ, float PlaneMaxZ)
        {
            var num = P2.X - P1.X;
            var num2 = P2.Y - P1.Y;
            var num3 = P2.Z - P1.Z;
            if (num2 != 0f)
            {
                var num4 = (PlaneY - P1.Y)/num2;
                if ((num4 >= 0f) && (num4 <= 1f))
                {
                    var num5 = P1.X + (num4*num);
                    var num6 = P1.Z + (num4*num3);
                    if (((num5 >= PlaneMinX) && (num5 <= PlaneMaxX)) && ((num6 >= PlaneMinZ) && (num6 <= PlaneMaxZ)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool PlaneZIntersect(Vector3f P1, Vector3f P2, float PlaneZ, float PlaneMinX, float PlaneMaxX,
            float PlaneMinY, float PlaneMaxY)
        {
            var num = P2.X - P1.X;
            var num2 = P2.Y - P1.Y;
            var num3 = P2.Z - P1.Z;
            if (num3 != 0f)
            {
                var num4 = (PlaneZ - P1.Z)/num3;
                if ((num4 >= 0f) && (num4 <= 1f))
                {
                    var num5 = P1.X + (num4*num);
                    var num6 = P1.Y + (num4*num2);
                    if (((num5 >= PlaneMinX) && (num5 <= PlaneMaxX)) && ((num6 >= PlaneMinY) && (num6 <= PlaneMaxY)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool PointInBox(Vector3f Position, float[] Mins, float[] Maxes)
        {
            var flag = false;
            if ((((Position.X >= Mins[0]) && (Position.X <= Maxes[0])) &&
                 ((Position.Y >= Mins[1]) && (Position.Y <= Maxes[1]))) &&
                ((Position.Z >= Mins[2]) && (Position.Z <= Maxes[2])))
            {
                flag = true;
            }
            return flag;
        }

        public static bool RayBoxCollision(Vector3f P1, Vector3f P2, Vector3f BoxMin, Vector3f BoxMax)
        {
            return (PlaneXIntersect(P1, P2, BoxMin.X, BoxMin.Y, BoxMax.Y, BoxMin.Z, BoxMax.Z) ||
                    (PlaneXIntersect(P1, P2, BoxMax.X, BoxMin.Y, BoxMax.Y, BoxMin.Z, BoxMax.Z) ||
                     (PlaneYIntersect(P1, P2, BoxMin.Y, BoxMin.X, BoxMax.X, BoxMin.Z, BoxMax.Z) ||
                      (PlaneYIntersect(P1, P2, BoxMax.Y, BoxMin.X, BoxMax.X, BoxMin.Z, BoxMax.Z) ||
                       (PlaneZIntersect(P1, P2, BoxMin.Z, BoxMin.X, BoxMax.X, BoxMin.Y, BoxMax.Y) ||
                        PlaneZIntersect(P1, P2, BoxMax.Z, BoxMin.X, BoxMax.X, BoxMin.Y, BoxMax.Y))))));
        }

        public void RenderLevel(Vector3f CameraPosition, Frustrum GameFrustrum)
        {
            RenderSkyBox(CameraPosition);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            FacesDrawn.SetAll(false);

            var index = FindLeaf(CameraPosition);
            var cluster = Leaves[index].Cluster;
            var numLeaves = NumLeaves;
            while (numLeaves > 0)
            {
                numLeaves--;
                var leaf = Leaves[numLeaves];
                if (IsClusterVisible(cluster, leaf.Cluster) &&
                    GameFrustrum.BoxInFrustrum(leaf.Min.X, leaf.Min.Y, leaf.Min.Z, leaf.Max.X, leaf.Max.Y, leaf.Max.Z))
                {
                    var numLeafFaces = leaf.NumLeafFaces;
                    while (numLeafFaces > 0)
                    {
                        numLeafFaces--;
                        var num5 = LeafFaces[leaf.LeafFace + numLeafFaces];
                        if ((Faces[num5] != null) && !FacesDrawn.Get(num5))
                        {
                            FacesDrawn.Set(num5, true);
                            RenderFace(Faces[num5].Type, num5);
                        }
                    }
                }
            }
            GL.Disable(EnableCap.Blend);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.DisableClientState(ArrayCap.VertexArray);
        }

        private void RenderFace(int BSPFaceType, int FaceIndex)
        {
            switch (BSPFaceType)
            {
                case 1:
                case 3:
                    RenderPolygonFace(FaceIndex);
                    break;

                case 2:
                    break;

                default:
                    return;
            }
        }

        private unsafe void RenderPolygonFace(int FaceIndex)
        {
            var CurrentFace = Faces[FaceIndex];

            if (Textures[CurrentFace.TextureID] != null)
            {
                //Vertices
                fixed (void* VertexPtr = &Vertices[CurrentFace.StartVertexIndex*3])
                {
                    GL.VertexPointer(3, VertexPointerType.Float, 0, (IntPtr) VertexPtr);
                }

                if (CurrentFace.LightmapID >= 0)
                {
                    if (Engine.texUnits > 1)
                    {
                        // Multitexture
                        if (Lightmaps[CurrentFace.LightmapID] != null)
                        {
                            //GL.ActiveTexture(TextureUnit.Texture1);
                            GL.ClientActiveTexture(TextureUnit.Texture1);
                            GL.Enable(EnableCap.Texture2D);
                            //Lightmap
                            GL.BindTexture(TextureTarget.Texture2D, Lightmaps[CurrentFace.LightmapID].TextureID);
                            /*GL.Enable(EnableCap.Texture2D);

                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Combine);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Source0Rgb, (int)TextureEnvModeSource.Previous);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Rgb, (int)TextureEnvModeOperandRgb.SrcColor);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineRgb, (int)TextureEnvModeCombine.Modulate);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Rgb, (int)TextureEnvModeSource.Texture);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand1Rgb, (int)TextureEnvModeOperandRgb.SrcColor);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.RgbScale, 2);*/

                            fixed (void* LightmapCoordPtr = &LightmapCoords[CurrentFace.StartVertexIndex*2])
                            {
                                GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr) LightmapCoordPtr);
                            }
                        }

                        //GL.ActiveTexture(TextureUnit.Texture0);
                        GL.ClientActiveTexture(TextureUnit.Texture0);
                        GL.Enable(EnableCap.Texture2D);
                        GL.BindTexture(TextureTarget.Texture2D, Textures[CurrentFace.TextureID].TextureID);

                        fixed (void* TextureCoordsPtr = &TextureCoords[CurrentFace.StartVertexIndex*2])
                        {
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr) TextureCoordsPtr);
                        }
                        fixed (uint* IndexPtr = &MeshIndices[CurrentFace.MeshVertexIndex])
                        {
                            GL.DrawElements(BeginMode.Triangles, CurrentFace.NumMeshVertices,
                                DrawElementsType.UnsignedInt, (IntPtr) IndexPtr);
                        }
                        // Desactiva Textura1
                        GL.Disable(EnableCap.Texture2D);
                        GL.ClientActiveTexture(TextureUnit.Texture1);
                    }
                    else
                    {
                        if (Lightmaps[CurrentFace.LightmapID] != null)
                        {
                            //Lightmap
                            GL.BindTexture(TextureTarget.Texture2D, Lightmaps[CurrentFace.LightmapID].TextureID);

                            //Setup blending
                            GL.Disable(EnableCap.Blend);
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,
                                (int) TextureEnvMode.Replace);

                            //Set texture coordinates
                            fixed (void* LightmapCoordPtr = &LightmapCoords[CurrentFace.StartVertexIndex*2])
                            {
                                GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr) LightmapCoordPtr);
                            }

                            //Draw face
                            fixed (uint* IndexPtr = &MeshIndices[CurrentFace.MeshVertexIndex])
                            {
                                GL.DrawElements(BeginMode.Triangles, CurrentFace.NumMeshVertices,
                                    DrawElementsType.UnsignedInt, (IntPtr) IndexPtr);
                            }
                        }
                        GL.BindTexture(TextureTarget.Texture2D, Textures[CurrentFace.TextureID].TextureID);
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.SrcColor);
                        GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,
                            (int) TextureEnvMode.Replace);

                        fixed (void* TextureCoordsPtr = &TextureCoords[CurrentFace.StartVertexIndex*2])
                        {
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr) TextureCoordsPtr);
                        }
                        fixed (uint* IndexPtr = &MeshIndices[CurrentFace.MeshVertexIndex])
                        {
                            GL.DrawElements(BeginMode.Triangles, CurrentFace.NumMeshVertices,
                                DrawElementsType.UnsignedInt, (IntPtr) IndexPtr);
                        }
                    }
                }
            }
        }

        private void RenderSkyBox(Vector3f CameraPosition)
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
            GL.BindTexture(TextureTarget.Texture2D, SkyBoxTextures[2].TextureID);
            // Front
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y - 10f, CameraPosition.Z - 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y - 10f, CameraPosition.Z - 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y + 10f, CameraPosition.Z - 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y + 10f, CameraPosition.Z - 10f);
            GL.End();
            // Right
            GL.BindTexture(TextureTarget.Texture2D, SkyBoxTextures[0].TextureID);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y - 10f, CameraPosition.Z - 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y - 10f, CameraPosition.Z + 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y + 10f, CameraPosition.Z + 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y + 10f, CameraPosition.Z - 10f);
            GL.End();
            // Back
            GL.BindTexture(TextureTarget.Texture2D, SkyBoxTextures[5].TextureID);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y - 10f, CameraPosition.Z + 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y - 10f, CameraPosition.Z + 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y + 10f, CameraPosition.Z + 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y + 10f, CameraPosition.Z + 10f);
            GL.End();
            // Left
            GL.BindTexture(TextureTarget.Texture2D, SkyBoxTextures[3].TextureID);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y - 10f, CameraPosition.Z + 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y - 10f, CameraPosition.Z - 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y + 10f, CameraPosition.Z - 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y + 10f, CameraPosition.Z + 10f);
            GL.End();
            // Top
            GL.BindTexture(TextureTarget.Texture2D, SkyBoxTextures[4].TextureID);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y + 10f, CameraPosition.Z - 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y + 10f, CameraPosition.Z - 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y + 10f, CameraPosition.Z + 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y + 10f, CameraPosition.Z + 10f);
            GL.End();
            // Bottom
            GL.BindTexture(TextureTarget.Texture2D, SkyBoxTextures[1].TextureID);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y - 10f, CameraPosition.Z + 10f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y - 10f, CameraPosition.Z + 10f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(CameraPosition.X + 10f, CameraPosition.Y - 10f, CameraPosition.Z - 10f);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(CameraPosition.X - 10f, CameraPosition.Y - 10f, CameraPosition.Z - 10f);
            GL.End();

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.CullFace(CullFaceMode.Front);
            GL.PopMatrix();
        }
    }

    public class BSPBrush
    {
        public static readonly int SizeInBytes = 12;
        public int FirstSide;
        public int NumSides;
        public int TextureIndex;
    }

    public class BSPBrushSide
    {
        public static readonly int SizeInBytes = 8;
        public int Plane;
        public int Texture;
    }

    public class BSPEntity : ICollection, IEnumerable
    {
        private readonly ArrayList ArgValues;

        public BSPEntity()
        {
            ArgValues = new ArrayList();
        }

        public BSPEntity(string EntityString)
        {
            ArgValues = new ArrayList();
            var flag = true;
            var newArgValue = new ArgValue();
            var index = EntityString.IndexOf("\"");
            while (index > -1)
            {
                var num2 = EntityString.IndexOf("\"", index + 1);
                if (num2 > -1)
                {
                    var str = EntityString.Substring(index + 1, (num2 - index) - 1);
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
                    index = EntityString.IndexOf("\"", num2 + 1);
                }
                else
                {
                    index = -1;
                }
            }
        }

        public void CopyTo(Array array, int index)
        {
            ArgValues.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ArgValues.GetEnumerator();
        }

        public int Count
        {
            get { return ArgValues.Count; }
        }

        public bool IsSynchronized
        {
            get { return ArgValues.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return ArgValues.SyncRoot; }
        }

        public void AddArgValue(ArgValue NewArgValue)
        {
            ArgValues.Add(NewArgValue);
        }

        public void AddArgValue(string Argument, string Value)
        {
            ArgValues.Add(new ArgValue(Argument, Value));
        }

        public string SeekFirstValue(string Argument)
        {
            var str = "";
            var strArray = SeekValuesByArgument(Argument);
            if (strArray.Length > 0)
            {
                str = strArray[0];
            }
            return str;
        }

        public string[] SeekValuesByArgument(string Argument)
        {
            var list = new ArrayList();
            foreach (ArgValue value2 in ArgValues)
            {
                if (value2.Argument == Argument)
                {
                    list.Add(value2.Value);
                }
            }
            return (string[]) list.ToArray(typeof (string));
        }
    }

    public class BSPEntityCollection : ICollection, IEnumerable
    {
        private readonly ArrayList AllEntities;

        public BSPEntityCollection()
        {
            AllEntities = new ArrayList();
        }

        public BSPEntityCollection(string EntityString)
        {
            AllEntities = new ArrayList();
            var index = EntityString.IndexOf("{");
            while (index >= 0)
            {
                var num2 = EntityString.IndexOf("}", index + 1);
                if (num2 > -1)
                {
                    var entityString = EntityString.Substring(index + 1, (num2 - index) - 1);
                    AddEntity(new BSPEntity(entityString));
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
            AllEntities.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return AllEntities.GetEnumerator();
        }

        public int Count
        {
            get { return AllEntities.Count; }
        }

        public bool IsSynchronized
        {
            get { return AllEntities.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return AllEntities.SyncRoot; }
        }

        public void AddEntity(BSPEntity NewEntity)
        {
            AllEntities.Add(NewEntity);
        }

        public BSPEntity[] SeekEntitiesByClassname(string Classname)
        {
            var list = new ArrayList();
            foreach (BSPEntity entity in AllEntities)
            {
                foreach (ArgValue value2 in entity)
                {
                    if ((value2.Argument == "classname") && (value2.Value == Classname))
                    {
                        list.Add(entity);
                    }
                }
            }
            return (BSPEntity[]) list.ToArray(typeof (BSPEntity));
        }

        public string SeekFirstEntityValue(string Classname, string Argument)
        {
            var str = "";
            var entityArray = SeekEntitiesByClassname(Classname);
            if (entityArray.Length > 0)
            {
                var strArray = entityArray[0].SeekValuesByArgument(Argument);
                if (strArray.Length > 0)
                {
                    str = strArray[0];
                }
            }
            return str;
        }
    }

    public class BSPFace
    {
        public static readonly int SizeInBytes = 0x68;
        public int Effect;
        public int LightmapID = -1;
        public int[] MapCorner = new int[2];
        public Vector3f MapPosition = new Vector3f();
        public int[] MapSize = new int[2];
        public Vector3f[] MapVectors = new Vector3f[2];
        public int MeshVertexIndex = -1;
        public Vector3f Normal = new Vector3f();
        public int NumMeshVertices;
        public int NumVertices;
        public int[] Size = new int[2];
        public int StartVertexIndex = -1;
        public int TextureID = -1;
        public int Type;

        public BSPFace()
        {
            MapVectors[0] = new Vector3f();
            MapVectors[1] = new Vector3f();
        }
    }

    public class BSPHeader
    {
        public string ID = "";
        public int Version;
    }

    public class BSPLeaf
    {
        public static readonly int SizeInBytes = 48;
        public int Area;
        public int Cluster;
        public int LeafBrush;
        public int LeafFace;
        public Vector3i Max = new Vector3i();
        public Vector3i Min = new Vector3i();
        public int NumLeafBrushes;
        public int NumLeafFaces;
    }

    public class BSPLightmap
    {
        public static readonly int SizeInBytes = 49152;
        public byte[] ImageBytes = new byte[128*128*3];
    }

    public class BSPLump
    {
        public int Length;
        public int Offset;
    }

    public class BSPModel
    {
        public static readonly int SizeInBytes = 40;
        public int FirstBrush;
        public int FirstFace;
        public float[] Maxes = new float[3];
        public float[] Mins = new float[3];
        public int NumBrushes;
        public int NumFaces;
    }

    public class BSPNode
    {
        public static readonly int SizeInBytes = 36;
        public int Back;
        public int Front;
        public Vector3i Max = new Vector3i();
        public Vector3i Min = new Vector3i();
        public int Plane;
    }

    public class BSPPlane
    {
        public static readonly int SizeInBytes = 0x10;
        public float Distance;
        public Vector3f Normal = new Vector3f();
    }

    public class BSPShader
    {
        public static readonly int SizeInBytes = 0x48;
        public int BrushIndex;
        public int ContentFlags;
        public string Name = "";
    }

    public class BSPTexture
    {
        public static readonly int SizeInBytes = 72;
        public int Contents;
        public int Flags;
        public string Name = "";
    }

    public class BSPVisData
    {
        public byte[] BitSets;
        public int BytesPerCluster;
        public int NumClusters;
    }
}