
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
using OpenTK.Graphics.OpenGL;
using Math3D;

namespace WanderEngine
{
    //TODO: Change this to parse hl2 and hl1 stuff
    public class BSPFile
    {
        private BSPBrush[] Brushes = null;
        private BSPBrushSide[] BrushSides = null;
        private BSPVisData Clusters = null;
        private Vector3f CollisionEnd = new Vector3f();
        private Vector3f CollisionExtents = new Vector3f();
        public CollisionInformation CollisionInfo = new CollisionInformation();
        private Vector3f CollisionMax = new Vector3f();
        private Vector3f CollisionMin = new Vector3f();
        private float CollisionOffset = 0f;
        private Vector3f CollisionStart = new Vector3f();
        private CollisionTypes CollisionType = CollisionTypes.Ray;
        public BSPEntityCollection Entities;
        private string EntityString = "";
        private int EntityStringLength = 0;
        private BSPFace[] Faces = null;
        private BitArray FacesDrawn;
        private float Gamma = 10f;
        private BSPHeader Header = new BSPHeader();
        private Hashtable IndexTriggerHash = new Hashtable();
        private int[] LeafBrushes = null;
        private static readonly int LeafBrushSizeInBytes = 4;
        private int[] LeafFaces = null;
        private static readonly int LeafFaceSizeInBytes = 4;
        private BSPLeaf[] Leaves = null;
        private float[] LightmapCoords = null;
        private Texture[] Lightmaps = null;
        private BSPTexture[] LoadTextures = null;
        private BSPLump[] Lumps = new BSPLump[MaxLumps];
        private static readonly int MaxLumps = 0x11;
        private static readonly int MeshIndexSizeInBytes = 4;
        private uint[] MeshIndices = null;
        private BSPModel[] Models = null;
        private BSPNode[] Nodes = null;
        private int NoDrawTextureIndex = -1;
        private int NumBrushes = 0;
        private int NumBrushSides = 0;
        private int NumFaces = 0;
        private int NumLeafBrushes = 0;
        private int NumLeafFaces = 0;
        private int NumLeaves = 0;
        private int NumLightmaps = 0;
        private int NumMeshIndices = 0;
        private int NumModels = 0;
        private int NumNodes = 0;
        private int NumPlanes = 0;
        private int NumShaders = 0;
        private int NumTextures = 0;
        private int NumVertices = 0;
        private BSPPlane[] Planes = null;
        private static readonly float QuakeEpsilon = 0.03125f;
        private BSPShader[] Shaders = null;
        private Texture[] SkyBoxTextures = null;
        private float[] TextureCoords = null;
        private Texture[] Textures = null;
        private Trigger[] Triggers = null;
        private static readonly int VertexSizeInBytes = 0x2c;
        private float[] Vertices = null;

        public BSPFile(string FileName)
        {
            this.LoadBSP(FileName);
        }

        private bool BoxesCollide(float[] Mins, float[] Maxes, float[] Mins2, float[] Maxes2)
        {
            bool flag = false;
            if ((((Maxes2[0] > Mins[0]) && (Mins2[0] < Maxes[0])) && ((Maxes2[1] > Mins[1]) && (Mins2[1] < Maxes[1]))) && ((Maxes2[2] > Mins[2]) && (Mins2[2] < Maxes[2])))
            {
                flag = true;
            }
            return flag;
        }

        public static void ChangeGamma(ref Bitmap ImageBmp, float Factor)
        {
            for (int i = 0; i < ImageBmp.Width; i++)
            {
                for (int j = 0; j < ImageBmp.Height; j++)
                {
                    float num3 = 1f;
                    float num4 = 0f;
                    Color pixel = ImageBmp.GetPixel(i, j);
                    float r = pixel.R;
                    float g = pixel.G;
                    float b = pixel.B;
                    r = (r * Factor) / 255f;
                    g = (g * Factor) / 255f;
                    b = (b * Factor) / 255f;
                    if (r > 1f)
                    {
                        num4 = 1f / r;
                        if (num4 < num3)
                        {
                            num3 = num4;
                        }
                    }
                    if (g > 1f)
                    {
                        num4 = 1f / g;
                        if (num4 < num3)
                        {
                            num3 = num4;
                        }
                    }
                    if (b > 1f)
                    {
                        num4 = 1f / b;
                        if (num4 < num3)
                        {
                            num3 = num4;
                        }
                    }
                    num3 *= 255f;
                    r *= num3;
                    g *= num3;
                    b *= num3;
                    ImageBmp.SetPixel(i, j, Color.FromArgb((int)r, (int)g, (int)b));
                }
            }
        }

        private void CheckBrush(BSPBrush CurrentBrush)
        {
            float num = -1f;
            float num2 = 1f;
            bool flag = false;
            bool flag2 = false;
            float distance = 0f;
            Vector3f normal = new Vector3f();
            for (int i = 0; i < CurrentBrush.NumSides; i++)
            {
                BSPBrushSide side = this.BrushSides[CurrentBrush.FirstSide + i];
                BSPPlane plane = this.Planes[side.Plane];
                float num5 = 0f;
                float num6 = 0f;
                if (this.CollisionType == CollisionTypes.Box)
                {
                    Vector3f vector2 = new Vector3f();
                    if (plane.Normal.X < 0f)
                    {
                        vector2.X = this.CollisionMax.X;
                    }
                    else
                    {
                        vector2.X = this.CollisionMin.X;
                    }
                    if (plane.Normal.Y < 0f)
                    {
                        vector2.Y = this.CollisionMax.Y;
                    }
                    else
                    {
                        vector2.Y = this.CollisionMin.Y;
                    }
                    if (plane.Normal.Z < 0f)
                    {
                        vector2.Z = this.CollisionMax.Z;
                    }
                    else
                    {
                        vector2.Z = this.CollisionMin.Z;
                    }
                    num5 = ((((this.CollisionStart.X + vector2.X) * plane.Normal.X) + ((this.CollisionStart.Y + vector2.Y) * plane.Normal.Y)) + ((this.CollisionStart.Z + vector2.Z) * plane.Normal.Z)) - plane.Distance;
                    num6 = ((((this.CollisionEnd.X + vector2.X) * plane.Normal.X) + ((this.CollisionEnd.Y + vector2.Y) * plane.Normal.Y)) + ((this.CollisionEnd.Z + vector2.Z) * plane.Normal.Z)) - plane.Distance;
                }
                else
                {
                    num5 = ((((plane.Normal.X * this.CollisionStart.X) + (plane.Normal.Y * this.CollisionStart.Y)) + (plane.Normal.Z * this.CollisionStart.Z)) - plane.Distance) - this.CollisionOffset;
                    num6 = ((((plane.Normal.X * this.CollisionEnd.X) + (plane.Normal.Y * this.CollisionEnd.Y)) + (plane.Normal.Z * this.CollisionEnd.Z)) - plane.Distance) - this.CollisionOffset;
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
                        float num7 = (num5 - QuakeEpsilon) / (num5 - num6);
                        if (num7 > num)
                        {
                            num = num7;
                            normal = plane.Normal;
                            distance = plane.Distance;
                        }
                    }
                    else
                    {
                        float num8 = (num5 + QuakeEpsilon) / (num5 - num6);
                        if (num8 < num2)
                        {
                            num2 = num8;
                        }
                    }
                }
            }
            if (!flag)
            {
                this.CollisionInfo.StartsOut = false;
                if (!flag2)
                {
                    this.CollisionInfo.AllSolid = true;
                }
            }
            else if (((num < num2) && (num > -1f)) && (num < this.CollisionInfo.Fraction))
            {
                if (num < 0f)
                {
                    num = 0f;
                }
                this.CollisionInfo.Fraction = num;
                this.CollisionInfo.Normal = normal;
                this.CollisionInfo.PlaneDistance = distance;
            }
        }

        private void CheckNode(int NodeIndex, float StartFraction, float EndFraction, Vector3f Start, Vector3f End)
        {
            if (this.CollisionInfo.Fraction > StartFraction)
            {
                if (NodeIndex < 0)
                {
                    BSPLeaf leaf = this.Leaves[~NodeIndex];
                    for (int i = 0; i < leaf.NumLeafBrushes; i++)
                    {
                        BSPBrush currentBrush = this.Brushes[this.LeafBrushes[leaf.LeafBrush + i]];
                        if ((currentBrush.NumSides > 0) && ((this.LoadTextures[currentBrush.TextureIndex].Contents & 1) > 0))
                        {
                            this.CheckBrush(currentBrush);
                        }
                    }
                }
                else
                {
                    BSPNode node = this.Nodes[NodeIndex];
                    BSPPlane plane = this.Planes[node.Plane];
                    float num2 = (((plane.Normal.X * Start.X) + (plane.Normal.Y * Start.Y)) + (plane.Normal.Z * Start.Z)) - plane.Distance;
                    float num3 = (((plane.Normal.X * End.X) + (plane.Normal.Y * End.Y)) + (plane.Normal.Z * End.Z)) - plane.Distance;
                    if (this.CollisionType == CollisionTypes.Box)
                    {
                        this.CollisionOffset = (Math.Abs((float)(this.CollisionExtents.X * plane.Normal.X)) + Math.Abs((float)(this.CollisionExtents.Y * plane.Normal.Y))) + Math.Abs((float)(this.CollisionExtents.Z * plane.Normal.Z));
                    }
                    if ((num2 >= this.CollisionOffset) && (num3 >= this.CollisionOffset))
                    {
                        this.CheckNode(node.Front, StartFraction, EndFraction, Start, End);
                    }
                    else if ((num2 < -this.CollisionOffset) && (num3 < -this.CollisionOffset))
                    {
                        this.CheckNode(node.Back, StartFraction, EndFraction, Start, End);
                    }
                    else
                    {
                        int nodeIndex = -1;
                        int front = -1;
                        float num6 = 0f;
                        float num7 = 0f;
                        Vector3f end = new Vector3f();
                        if (num2 < num3)
                        {
                            nodeIndex = node.Back;
                            front = node.Front;
                            float num8 = 1f / (num2 - num3);
                            num6 = ((num2 - QuakeEpsilon) - this.CollisionOffset) * num8;
                            num7 = ((num2 + QuakeEpsilon) + this.CollisionOffset) * num8;
                        }
                        else if (num3 < num2)
                        {
                            nodeIndex = node.Front;
                            front = node.Back;
                            float num9 = 1f / (num2 - num3);
                            num6 = ((num2 + QuakeEpsilon) + this.CollisionOffset) * num9;
                            num7 = ((num2 - QuakeEpsilon) - this.CollisionOffset) * num9;
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
                        end = Start + ((Vector3f)((End - Start) * num6));
                        float endFraction = StartFraction + ((EndFraction - StartFraction) * num6);
                        this.CheckNode(nodeIndex, StartFraction, endFraction, Start, end);
                        end = Start + ((Vector3f)((End - Start) * num7));
                        endFraction = StartFraction + ((EndFraction - StartFraction) * num7);
                        this.CheckNode(front, endFraction, EndFraction, end, End);
                    }
                }
            }
        }

        private void DetectCollision(Vector3f Start, Vector3f End)
        {
            this.CollisionInfo = new CollisionInformation();
            this.CollisionStart = new Vector3f(Start.X, Start.Y, Start.Z);
            this.CollisionEnd = new Vector3f(End.X, End.Y, End.Z);
            this.CheckNode(0, 0f, 1f, this.CollisionStart, this.CollisionEnd);
            if (this.CollisionInfo.Fraction == 1f)
            {
                this.CollisionInfo.EndPoint = this.CollisionEnd;
            }
            else
            {
                this.CollisionInfo.EndPoint = this.CollisionStart + ((Vector3f)((this.CollisionEnd - this.CollisionStart) * this.CollisionInfo.Fraction));
            }
        }

        public void DetectCollisionBox(Vector3f Start, Vector3f End, Vector3f Min, Vector3f Max)
        {
            this.CollisionType = CollisionTypes.Box;
            this.CollisionMin = Min;
            this.CollisionMax = Max;
            this.CollisionExtents.X = (-Min.X > Max.X) ? -Min.X : Max.X;
            this.CollisionExtents.Y = (-Min.Y > Max.Y) ? -Min.Y : Max.Y;
            this.CollisionExtents.Z = (-Min.Z > Max.Z) ? -Min.Z : Max.Z;
            this.DetectCollision(Start, End);
        }

        public void DetectCollisionRay(Vector3f Start, Vector3f End)
        {
            this.CollisionType = CollisionTypes.Ray;
            this.CollisionOffset = 0f;
            this.DetectCollision(Start, End);
        }

        public void DetectCollisionSphere(Vector3f Start, Vector3f End, float Radius)
        {
            this.CollisionType = CollisionTypes.Sphere;
            this.CollisionOffset = Radius;
            this.DetectCollision(Start, End);
        }

        public Trigger DetectTriggerCollisions(Vector3f Position)
        {
            Trigger trigger = null;
            for (int i = 1; i < this.Models.Length; i++)
            {
                BSPModel model = this.Models[i];
                if (this.PointInBox(Position, model.Mins, model.Maxes))
                {
                    try
                    {
                        trigger = (Trigger)this.IndexTriggerHash[i];
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
            int index = 0;
            float num2 = 0f;
            while (index >= 0)
            {
                BSPNode node = this.Nodes[index];
                BSPPlane plane = this.Planes[node.Plane];
                num2 = (((plane.Normal.X * CameraPosition.X) + (plane.Normal.Y * CameraPosition.Y)) + (plane.Normal.Z * CameraPosition.Z)) - plane.Distance;
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
            bool flag = true;
            if (((this.Clusters.BitSets != null) && (CurrentCluster >= 0)) && (TestCluster >= 0))
            {
                byte num = this.Clusters.BitSets[(CurrentCluster * this.Clusters.BytesPerCluster) + (TestCluster / 8)];
                int num2 = num & (((int)1) << (TestCluster & 7));
                if (num2 <= 0)
                {
                    flag = false;
                }
            }
            return flag;
        }

        public void LoadBSP(string FileName)
        {
            FileStream stream = new FileStream(FileName, FileMode.Open);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            BinaryReader reader = new BinaryReader(new MemoryStream(buffer));
            this.Header.ID = Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
            this.Header.Version = reader.ReadInt32();
            for (int i = 0; i < MaxLumps; i++)
            {
                this.Lumps[i] = new BSPLump();
                this.Lumps[i].Offset = reader.ReadInt32();
                this.Lumps[i].Length = reader.ReadInt32();
            }
            if ((this.Header.ID != "IBSP") && (this.Header.Version != 0x2e))
            {
                throw new Exception("Wrong file type or version");
            }
            this.NumVertices = this.Lumps[10].Length / VertexSizeInBytes;
            this.Vertices = new float[this.NumVertices * 3];
            this.TextureCoords = new float[this.NumVertices * 2];
            this.LightmapCoords = new float[this.NumVertices * 2];
            this.NumFaces = this.Lumps[13].Length / BSPFace.SizeInBytes;
            this.Faces = new BSPFace[this.NumFaces];
            this.NumTextures = this.Lumps[1].Length / BSPTexture.SizeInBytes;
            this.LoadTextures = new BSPTexture[this.NumTextures];
            this.Textures = new Texture[this.NumTextures];
            this.NumLightmaps = this.Lumps[14].Length / BSPLightmap.SizeInBytes;
            this.Lightmaps = new Texture[this.NumLightmaps];
            this.NumNodes = this.Lumps[3].Length / BSPNode.SizeInBytes;
            this.Nodes = new BSPNode[this.NumNodes];
            this.NumLeaves = this.Lumps[4].Length / BSPLeaf.SizeInBytes;
            this.Leaves = new BSPLeaf[this.NumLeaves];
            this.NumLeafFaces = this.Lumps[5].Length / LeafFaceSizeInBytes;
            this.LeafFaces = new int[this.NumLeafFaces];
            this.NumPlanes = this.Lumps[2].Length / BSPPlane.SizeInBytes;
            this.Planes = new BSPPlane[this.NumPlanes];
            this.NumMeshIndices = this.Lumps[11].Length / MeshIndexSizeInBytes;
            this.MeshIndices = new uint[this.NumMeshIndices];
            this.NumModels = this.Lumps[7].Length / BSPModel.SizeInBytes;
            this.Models = new BSPModel[this.NumModels];
            this.NumBrushes = this.Lumps[8].Length / BSPBrush.SizeInBytes;
            this.Brushes = new BSPBrush[this.NumBrushes];
            this.NumBrushSides = this.Lumps[9].Length / BSPBrushSide.SizeInBytes;
            this.BrushSides = new BSPBrushSide[this.NumBrushSides];
            this.NumLeafBrushes = this.Lumps[6].Length / LeafBrushSizeInBytes;
            this.LeafBrushes = new int[this.NumLeafBrushes];
            this.NumShaders = this.Lumps[12].Length / BSPShader.SizeInBytes;
            this.Shaders = new BSPShader[this.NumShaders];
            this.Clusters = new BSPVisData();
            this.EntityStringLength = this.Lumps[0].Length;
            reader.BaseStream.Seek((long)this.Lumps[0].Offset, SeekOrigin.Begin);
            foreach (byte num2 in reader.ReadBytes(this.EntityStringLength))
            {
                char ch = Convert.ToChar(num2);
                if (ch != '\0')
                {
                    this.EntityString = this.EntityString + ch;
                }
            }
            this.Entities = new BSPEntityCollection(this.EntityString);
            string s = this.Entities.SeekFirstEntityValue("worldspawn", "ambient");
            try
            {
                this.Gamma = float.Parse(s);
                this.Gamma /= 17f;
            }
            catch
            {

            }
            BSPEntity[] entityArray = this.Entities.SeekEntitiesByClassname("trigger_multiple");
            if (entityArray.Length > 0)
            {
                this.Triggers = new Trigger[entityArray.Length];
                int num3 = 0;
                foreach (BSPEntity entity in entityArray)
                {
                    try
                    {
                        Trigger trigger = new Trigger();
                        trigger.Name = entity.SeekFirstValue("trigger_name");
                        string str2 = entity.SeekFirstValue("model").Replace("*", string.Empty);
                        trigger.ModelIndex = int.Parse(str2);
                        this.IndexTriggerHash[trigger.ModelIndex] = trigger;
                        this.Triggers[num3] = trigger;
                        num3++;
                    }
                    catch
                    {
                    }
                }
            }
            int index = 0;
            int num5 = 0;
            int offset = this.Lumps[10].Offset;
            for (int j = 0; j < this.NumVertices; j++)
            {
                reader.BaseStream.Seek((long)(offset + (j * VertexSizeInBytes)), SeekOrigin.Begin);
                this.Vertices[index] = reader.ReadSingle();
                this.Vertices[index + 2] = -reader.ReadSingle();
                this.Vertices[index + 1] = reader.ReadSingle();
                this.TextureCoords[num5] = reader.ReadSingle();
                this.TextureCoords[num5 + 1] = -reader.ReadSingle();
                this.LightmapCoords[num5] = reader.ReadSingle();
                this.LightmapCoords[num5 + 1] = -reader.ReadSingle();
                index += 3;
                num5 += 2;
            }
            int num8 = this.Lumps[13].Offset;
            for (int k = 0; k < this.NumFaces; k++)
            {
                reader.BaseStream.Seek((long)(num8 + (k * BSPFace.SizeInBytes)), SeekOrigin.Begin);
                this.Faces[k] = new BSPFace();
                this.Faces[k].TextureID = reader.ReadInt32();
                this.Faces[k].Effect = reader.ReadInt32();
                this.Faces[k].Type = reader.ReadInt32();
                this.Faces[k].StartVertexIndex = reader.ReadInt32();
                this.Faces[k].NumVertices = reader.ReadInt32();
                this.Faces[k].MeshVertexIndex = reader.ReadInt32();
                this.Faces[k].NumMeshVertices = reader.ReadInt32();
                this.Faces[k].LightmapID = reader.ReadInt32();
                this.Faces[k].MapCorner[0] = reader.ReadInt32();
                this.Faces[k].MapCorner[1] = reader.ReadInt32();
                this.Faces[k].MapSize[0] = reader.ReadInt32();
                this.Faces[k].MapSize[1] = reader.ReadInt32();
                this.Faces[k].MapPosition.X = reader.ReadSingle();
                this.Faces[k].MapPosition.Y = reader.ReadSingle();
                this.Faces[k].MapPosition.Z = reader.ReadSingle();
                this.Faces[k].MapVectors[0].X = reader.ReadSingle();
                this.Faces[k].MapVectors[0].Y = reader.ReadSingle();
                this.Faces[k].MapVectors[0].Z = reader.ReadSingle();
                this.Faces[k].MapVectors[1].X = reader.ReadSingle();
                this.Faces[k].MapVectors[1].Y = reader.ReadSingle();
                this.Faces[k].MapVectors[1].Z = reader.ReadSingle();
                this.Faces[k].Normal.X = reader.ReadSingle();
                this.Faces[k].Normal.Y = reader.ReadSingle();
                this.Faces[k].Normal.Z = reader.ReadSingle();
                this.Faces[k].Size[0] = reader.ReadInt32();
                this.Faces[k].Size[1] = reader.ReadInt32();
            }
            reader.BaseStream.Seek((long)this.Lumps[1].Offset, SeekOrigin.Begin);
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
            for (int i = 0; i < NumTextures; i++)
            {
                LoadTextures[i] = new BSPTexture();

                byte[] NameBytes = reader.ReadBytes(64);
                for (int NameByteIndex = 0; NameByteIndex < 64; NameByteIndex++)
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
            String dirPath = Directory.GetCurrentDirectory();
            String texturePath = Utility.AdaptRelativePathToPlatform("../");
            Directory.SetCurrentDirectory(texturePath);

            for (int n = 0; n < this.NumTextures; n++)
            {
                string path = this.LoadTextures[n].Name + ".jpg";
                string str4 = this.LoadTextures[n].Name + ".tga";
                try
                {
                    if (File.Exists(path))
                    {
                        this.Textures[n] = new Texture(path);
                    }
                    else if (File.Exists(str4))
                    {
                        this.Textures[n] = new Texture(str4);
                    }
                    else
                    {
                        this.Textures[n] = null;
                    }
                }
                catch
                {
                    this.Textures[n] = null;
                }
            }

            this.SkyBoxTextures = new Texture[6];
            string str5 = "day";
            int hour = DateTime.Now.Hour;
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
            string sky0 = "textures/bookstore/skies/" + str5 + "/negx.jpg";
            string sky1 = "textures/bookstore/skies/" + str5 + "/negy.jpg";
            string sky2 = "textures/bookstore/skies/" + str5 + "/negz.jpg";
            string sky3 = "textures/bookstore/skies/" + str5 + "/posx.jpg";
            string sky4 = "textures/bookstore/skies/" + str5 + "/posy.jpg";
            string sky5 = "textures/bookstore/skies/" + str5 + "/posz.jpg";

            this.SkyBoxTextures[0] = new Texture(Utility.AdaptRelativePathToPlatform(sky0), true);
            this.SkyBoxTextures[1] = new Texture(Utility.AdaptRelativePathToPlatform(sky1), true);
            this.SkyBoxTextures[2] = new Texture(Utility.AdaptRelativePathToPlatform(sky2), true);
            this.SkyBoxTextures[3] = new Texture(Utility.AdaptRelativePathToPlatform(sky3), true);
            this.SkyBoxTextures[4] = new Texture(Utility.AdaptRelativePathToPlatform(sky4), true);
            this.SkyBoxTextures[5] = new Texture(Utility.AdaptRelativePathToPlatform(sky5), true);

            Directory.SetCurrentDirectory(dirPath);

            reader.BaseStream.Seek((long)this.Lumps[14].Offset, SeekOrigin.Begin);
            for (int num14 = 0; num14 < this.NumLightmaps; num14++)
            {
                byte[] buffer4 = reader.ReadBytes(BSPLightmap.SizeInBytes);
                Bitmap imageBmp = new Bitmap(0x80, 0x80);
                int num15 = 0;
                for (int num16 = 0; num16 < 0x80; num16++)
                {
                    for (int num17 = 0; num17 < 0x80; num17++)
                    {
                        byte red = buffer4[num15];
                        byte green = buffer4[num15 + 1];
                        byte blue = buffer4[num15 + 2];
                        imageBmp.SetPixel(num17, num16, Color.FromArgb(red, green, blue));
                        num15 += 3;
                    }
                }
                ChangeGamma(ref imageBmp, this.Gamma);
                this.Lightmaps[num14] = new Texture(imageBmp);
            }
            int num21 = this.Lumps[3].Offset;
            for (int num22 = 0; num22 < this.NumNodes; num22++)
            {
                reader.BaseStream.Seek((long)(num21 + (num22 * BSPNode.SizeInBytes)), SeekOrigin.Begin);
                this.Nodes[num22] = new BSPNode();
                this.Nodes[num22].Plane = reader.ReadInt32();
                this.Nodes[num22].Front = reader.ReadInt32();
                this.Nodes[num22].Back = reader.ReadInt32();
                this.Nodes[num22].Min.X = reader.ReadInt32();
                this.Nodes[num22].Min.Z = -reader.ReadInt32();
                this.Nodes[num22].Min.Y = reader.ReadInt32();
                this.Nodes[num22].Max.X = reader.ReadInt32();
                this.Nodes[num22].Max.Z = -reader.ReadInt32();
                this.Nodes[num22].Max.Y = reader.ReadInt32();
            }
            int num23 = this.Lumps[4].Offset;
            for (int num24 = 0; num24 < this.NumLeaves; num24++)
            {
                reader.BaseStream.Seek((long)(num23 + (num24 * BSPLeaf.SizeInBytes)), SeekOrigin.Begin);
                this.Leaves[num24] = new BSPLeaf();
                this.Leaves[num24].Cluster = reader.ReadInt32();
                this.Leaves[num24].Area = reader.ReadInt32();
                this.Leaves[num24].Min.X = reader.ReadInt32();
                this.Leaves[num24].Min.Z = -reader.ReadInt32();
                this.Leaves[num24].Min.Y = reader.ReadInt32();
                this.Leaves[num24].Max.X = reader.ReadInt32();
                this.Leaves[num24].Max.Z = -reader.ReadInt32();
                this.Leaves[num24].Max.Y = reader.ReadInt32();
                this.Leaves[num24].LeafFace = reader.ReadInt32();
                this.Leaves[num24].NumLeafFaces = reader.ReadInt32();
                this.Leaves[num24].LeafBrush = reader.ReadInt32();
                this.Leaves[num24].NumLeafBrushes = reader.ReadInt32();
            }
            reader.BaseStream.Seek((long)this.Lumps[5].Offset, SeekOrigin.Begin);
            for (int num25 = 0; num25 < this.NumLeafFaces; num25++)
            {
                this.LeafFaces[num25] = reader.ReadInt32();
            }
            int num26 = this.Lumps[2].Offset;
            for (int num27 = 0; num27 < this.NumPlanes; num27++)
            {
                reader.BaseStream.Seek((long)(num26 + (num27 * BSPPlane.SizeInBytes)), SeekOrigin.Begin);
                this.Planes[num27] = new BSPPlane();
                this.Planes[num27].Normal.X = reader.ReadSingle();
                this.Planes[num27].Normal.Z = -reader.ReadSingle();
                this.Planes[num27].Normal.Y = reader.ReadSingle();
                this.Planes[num27].Distance = reader.ReadSingle();
            }
            reader.BaseStream.Seek((long)this.Lumps[11].Offset, SeekOrigin.Begin);
            for (int num28 = 0; num28 < this.NumMeshIndices; num28++)
            {
                this.MeshIndices[num28] = reader.ReadUInt32();
            }
            int num29 = this.Lumps[7].Offset;
            for (int num30 = 0; num30 < this.NumModels; num30++)
            {
                reader.BaseStream.Seek((long)(num29 + (num30 * BSPModel.SizeInBytes)), SeekOrigin.Begin);
                this.Models[num30] = new BSPModel();
                this.Models[num30].Mins[0] = reader.ReadSingle();
                this.Models[num30].Maxes[2] = -reader.ReadSingle();
                this.Models[num30].Mins[1] = reader.ReadSingle();
                this.Models[num30].Maxes[0] = reader.ReadSingle();
                this.Models[num30].Mins[2] = -reader.ReadSingle();
                this.Models[num30].Maxes[1] = reader.ReadSingle();
                this.Models[num30].FirstFace = reader.ReadInt32();
                this.Models[num30].NumFaces = reader.ReadInt32();
                this.Models[num30].FirstBrush = reader.ReadInt32();
                this.Models[num30].NumBrushes = reader.ReadInt32();
            }
            int num31 = this.Lumps[8].Offset;
            for (int num32 = 0; num32 < this.NumBrushes; num32++)
            {
                reader.BaseStream.Seek((long)(num31 + (num32 * BSPBrush.SizeInBytes)), SeekOrigin.Begin);
                this.Brushes[num32] = new BSPBrush();
                this.Brushes[num32].FirstSide = reader.ReadInt32();
                this.Brushes[num32].NumSides = reader.ReadInt32();
                this.Brushes[num32].TextureIndex = reader.ReadInt32();
            }
            int num33 = this.Lumps[9].Offset;
            for (int num34 = 0; num34 < this.NumBrushSides; num34++)
            {
                reader.BaseStream.Seek((long)(num33 + (num34 * BSPBrushSide.SizeInBytes)), SeekOrigin.Begin);
                this.BrushSides[num34] = new BSPBrushSide();
                this.BrushSides[num34].Plane = reader.ReadInt32();
                this.BrushSides[num34].Texture = reader.ReadInt32();
            }
            reader.BaseStream.Seek((long)this.Lumps[6].Offset, SeekOrigin.Begin);
            for (int num35 = 0; num35 < this.NumLeafBrushes; num35++)
            {
                this.LeafBrushes[num35] = reader.ReadInt32();
            }
            int num36 = this.Lumps[12].Offset;
            for (int num37 = 0; num37 < this.NumShaders; num37++)
            {
                reader.BaseStream.Seek((long)(num36 + (num37 * BSPShader.SizeInBytes)), SeekOrigin.Begin);
                this.Shaders[num37] = new BSPShader();
                byte[] buffer5 = reader.ReadBytes(0x40);
                for (int num38 = 0; num38 < 0x40; num38++)
                {
                    if (buffer5[num38] != 0)
                    {
                        BSPShader shader1 = this.Shaders[num37];
                        shader1.Name = shader1.Name + Convert.ToChar(buffer5[num38]);
                    }
                }
                this.Shaders[num37].BrushIndex = reader.ReadInt32();
                this.Shaders[num37].ContentFlags = reader.ReadInt32();
            }
            reader.BaseStream.Seek((long)this.Lumps[0x10].Offset, SeekOrigin.Begin);
            if (this.Lumps[0x10].Length > 0)
            {
                this.Clusters.NumClusters = reader.ReadInt32();
                this.Clusters.BytesPerCluster = reader.ReadInt32();
                int count = this.Clusters.NumClusters * this.Clusters.BytesPerCluster;
                this.Clusters.BitSets = reader.ReadBytes(count);
            }
            reader.Close();
            if (this.NoDrawTextureIndex != -1)
            {
                for (int num40 = 0; num40 < this.Faces.Length; num40++)
                {
                    if (this.Faces[num40].TextureID == this.NoDrawTextureIndex)
                    {
                        this.Faces[num40] = null;
                    }
                }
            }
            this.FacesDrawn = new BitArray(this.NumFaces, false);
        }

        private static bool PlaneXIntersect(Vector3f P1, Vector3f P2, float PlaneX, float PlaneMinY, float PlaneMaxY, float PlaneMinZ, float PlaneMaxZ)
        {
            float num = P2.X - P1.X;
            float num2 = P2.Y - P1.Y;
            float num3 = P2.Z - P1.Z;
            if (num != 0f)
            {
                float num4 = (PlaneX - P1.X) / num;
                if ((num4 >= 0f) && (num4 <= 1f))
                {
                    float num5 = P1.Y + (num4 * num2);
                    float num6 = P1.Z + (num4 * num3);
                    if (((num5 >= PlaneMinY) && (num5 <= PlaneMaxY)) && ((num6 >= PlaneMinZ) && (num6 <= PlaneMaxZ)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool PlaneYIntersect(Vector3f P1, Vector3f P2, float PlaneY, float PlaneMinX, float PlaneMaxX, float PlaneMinZ, float PlaneMaxZ)
        {
            float num = P2.X - P1.X;
            float num2 = P2.Y - P1.Y;
            float num3 = P2.Z - P1.Z;
            if (num2 != 0f)
            {
                float num4 = (PlaneY - P1.Y) / num2;
                if ((num4 >= 0f) && (num4 <= 1f))
                {
                    float num5 = P1.X + (num4 * num);
                    float num6 = P1.Z + (num4 * num3);
                    if (((num5 >= PlaneMinX) && (num5 <= PlaneMaxX)) && ((num6 >= PlaneMinZ) && (num6 <= PlaneMaxZ)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool PlaneZIntersect(Vector3f P1, Vector3f P2, float PlaneZ, float PlaneMinX, float PlaneMaxX, float PlaneMinY, float PlaneMaxY)
        {
            float num = P2.X - P1.X;
            float num2 = P2.Y - P1.Y;
            float num3 = P2.Z - P1.Z;
            if (num3 != 0f)
            {
                float num4 = (PlaneZ - P1.Z) / num3;
                if ((num4 >= 0f) && (num4 <= 1f))
                {
                    float num5 = P1.X + (num4 * num);
                    float num6 = P1.Y + (num4 * num2);
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
            bool flag = false;
            if ((((Position.X >= Mins[0]) && (Position.X <= Maxes[0])) && ((Position.Y >= Mins[1]) && (Position.Y <= Maxes[1]))) && ((Position.Z >= Mins[2]) && (Position.Z <= Maxes[2])))
            {
                flag = true;
            }
            return flag;
        }

        public static bool RayBoxCollision(Vector3f P1, Vector3f P2, Vector3f BoxMin, Vector3f BoxMax)
        {
            return (PlaneXIntersect(P1, P2, BoxMin.X, BoxMin.Y, BoxMax.Y, BoxMin.Z, BoxMax.Z) || (PlaneXIntersect(P1, P2, BoxMax.X, BoxMin.Y, BoxMax.Y, BoxMin.Z, BoxMax.Z) || (PlaneYIntersect(P1, P2, BoxMin.Y, BoxMin.X, BoxMax.X, BoxMin.Z, BoxMax.Z) || (PlaneYIntersect(P1, P2, BoxMax.Y, BoxMin.X, BoxMax.X, BoxMin.Z, BoxMax.Z) || (PlaneZIntersect(P1, P2, BoxMin.Z, BoxMin.X, BoxMax.X, BoxMin.Y, BoxMax.Y) || PlaneZIntersect(P1, P2, BoxMax.Z, BoxMin.X, BoxMax.X, BoxMin.Y, BoxMax.Y))))));
        }

        public void RenderLevel(Vector3f CameraPosition, Frustrum GameFrustrum)
        {
            this.RenderSkyBox(CameraPosition);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            this.FacesDrawn.SetAll(false);

            int index = this.FindLeaf(CameraPosition);
            int cluster = this.Leaves[index].Cluster;
            int numLeaves = this.NumLeaves;
            while (numLeaves > 0)
            {
                numLeaves--;
                BSPLeaf leaf = this.Leaves[numLeaves];
                if (this.IsClusterVisible(cluster, leaf.Cluster) && GameFrustrum.BoxInFrustrum((float)leaf.Min.X, (float)leaf.Min.Y, (float)leaf.Min.Z, (float)leaf.Max.X, (float)leaf.Max.Y, (float)leaf.Max.Z))
                {
                    int numLeafFaces = leaf.NumLeafFaces;
                    while (numLeafFaces > 0)
                    {
                        numLeafFaces--;
                        int num5 = this.LeafFaces[leaf.LeafFace + numLeafFaces];
                        if ((this.Faces[num5] != null) && !this.FacesDrawn.Get(num5))
                        {
                            this.FacesDrawn.Set(num5, true);
                            this.RenderFace(this.Faces[num5].Type, num5);
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
                    this.RenderPolygonFace(FaceIndex);
                    break;

                case 2:
                    break;

                default:
                    return;
            }
        }

        private unsafe void RenderPolygonFace(int FaceIndex)
        {
            BSPFace CurrentFace = Faces[FaceIndex];

            if (Textures[CurrentFace.TextureID] != null)
            {
                //Vertices
                fixed (void* VertexPtr = &Vertices[CurrentFace.StartVertexIndex * 3])
                {
                    GL.VertexPointer(3, VertexPointerType.Float, 0, (IntPtr)VertexPtr);
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

                            fixed (void* LightmapCoordPtr = &LightmapCoords[CurrentFace.StartVertexIndex * 2])
                            {
                                GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr)LightmapCoordPtr);
                            }
                        }
                        
                        //GL.ActiveTexture(TextureUnit.Texture0);
                        GL.ClientActiveTexture(TextureUnit.Texture0);
                        GL.Enable(EnableCap.Texture2D);
                        GL.BindTexture(TextureTarget.Texture2D, this.Textures[CurrentFace.TextureID].TextureID);

                        fixed (void* TextureCoordsPtr = &TextureCoords[CurrentFace.StartVertexIndex * 2])
                        {
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr)TextureCoordsPtr);
                        }
                        fixed (uint* IndexPtr = &MeshIndices[CurrentFace.MeshVertexIndex])
                        {
                            GL.DrawElements(BeginMode.Triangles, CurrentFace.NumMeshVertices,
                                DrawElementsType.UnsignedInt, (IntPtr)IndexPtr);
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
                            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Replace);

                            //Set texture coordinates
                            fixed (void* LightmapCoordPtr = &LightmapCoords[CurrentFace.StartVertexIndex * 2])
                            {
                                GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr)LightmapCoordPtr);
                            }

                            //Draw face
                            fixed (uint* IndexPtr = &MeshIndices[CurrentFace.MeshVertexIndex])
                            {
                                GL.DrawElements(BeginMode.Triangles, CurrentFace.NumMeshVertices,
                                    DrawElementsType.UnsignedInt, (IntPtr)IndexPtr);
                            }
                        }
                        GL.BindTexture(TextureTarget.Texture2D, this.Textures[CurrentFace.TextureID].TextureID);
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.SrcColor);
                        GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Replace);

                        fixed (void* TextureCoordsPtr = &TextureCoords[CurrentFace.StartVertexIndex * 2])
                        {
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, (IntPtr)TextureCoordsPtr);
                        }
                        fixed (uint* IndexPtr = &MeshIndices[CurrentFace.MeshVertexIndex])
                        {
                            GL.DrawElements(BeginMode.Triangles, CurrentFace.NumMeshVertices,
                                DrawElementsType.UnsignedInt, (IntPtr)IndexPtr);
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
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Replace);
            GL.BindTexture(TextureTarget.Texture2D, this.SkyBoxTextures[2].TextureID);
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
            GL.BindTexture(TextureTarget.Texture2D, this.SkyBoxTextures[0].TextureID);
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
            GL.BindTexture(TextureTarget.Texture2D, this.SkyBoxTextures[5].TextureID);
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
            GL.BindTexture(TextureTarget.Texture2D, this.SkyBoxTextures[3].TextureID);
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
            GL.BindTexture(TextureTarget.Texture2D, this.SkyBoxTextures[4].TextureID);
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
            GL.BindTexture(TextureTarget.Texture2D, this.SkyBoxTextures[1].TextureID);
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
        public int FirstSide = 0;
        public int NumSides = 0;
        public static readonly int SizeInBytes = 12;
        public int TextureIndex = 0;
    }
    public class BSPBrushSide
    {
        public int Plane = 0;
        public static readonly int SizeInBytes = 8;
        public int Texture = 0;
    }

    public class BSPEntity : ICollection, IEnumerable
    {
        private ArrayList ArgValues;

        public BSPEntity()
        {
            this.ArgValues = new ArrayList();
        }

        public BSPEntity(string EntityString)
        {
            this.ArgValues = new ArrayList();
            bool flag = true;
            ArgValue newArgValue = new ArgValue();
            int index = EntityString.IndexOf("\"");
            while (index > -1)
            {
                int num2 = EntityString.IndexOf("\"", (int)(index + 1));
                if (num2 > -1)
                {
                    string str = EntityString.Substring(index + 1, (num2 - index) - 1);
                    if (flag)
                    {
                        newArgValue = new ArgValue();
                        newArgValue.Argument = str;
                        flag = false;
                    }
                    else
                    {
                        newArgValue.Value = str;
                        this.AddArgValue(newArgValue);
                        flag = true;
                    }
                    index = EntityString.IndexOf("\"", (int)(num2 + 1));
                }
                else
                {
                    index = -1;
                }
            }
        }

        public void AddArgValue(ArgValue NewArgValue)
        {
            this.ArgValues.Add(NewArgValue);
        }

        public void AddArgValue(string Argument, string Value)
        {
            this.ArgValues.Add(new ArgValue(Argument, Value));
        }

        public void CopyTo(Array array, int index)
        {
            this.ArgValues.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.ArgValues.GetEnumerator();
        }

        public string SeekFirstValue(string Argument)
        {
            string str = "";
            string[] strArray = this.SeekValuesByArgument(Argument);
            if (strArray.Length > 0)
            {
                str = strArray[0];
            }
            return str;
        }

        public string[] SeekValuesByArgument(string Argument)
        {
            ArrayList list = new ArrayList();
            foreach (ArgValue value2 in this.ArgValues)
            {
                if (value2.Argument == Argument)
                {
                    list.Add(value2.Value);
                }
            }
            return (string[])list.ToArray(typeof(string));
        }

        public int Count
        {
            get
            {
                return this.ArgValues.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this.ArgValues.IsSynchronized;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.ArgValues.SyncRoot;
            }
        }
    }

    public class BSPEntityCollection : ICollection, IEnumerable
    {
        private ArrayList AllEntities;

        public BSPEntityCollection()
        {
            this.AllEntities = new ArrayList();
        }

        public BSPEntityCollection(string EntityString)
        {
            this.AllEntities = new ArrayList();
            int index = EntityString.IndexOf("{");
            while (index >= 0)
            {
                int num2 = EntityString.IndexOf("}", (int)(index + 1));
                if (num2 > -1)
                {
                    string entityString = EntityString.Substring(index + 1, (num2 - index) - 1);
                    this.AddEntity(new BSPEntity(entityString));
                    index = EntityString.IndexOf("{", (int)(num2 + 1));
                }
                else
                {
                    index = -1;
                }
            }
        }

        public void AddEntity(BSPEntity NewEntity)
        {
            this.AllEntities.Add(NewEntity);
        }

        public void CopyTo(Array array, int index)
        {
            this.AllEntities.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.AllEntities.GetEnumerator();
        }

        public BSPEntity[] SeekEntitiesByClassname(string Classname)
        {
            ArrayList list = new ArrayList();
            foreach (BSPEntity entity in this.AllEntities)
            {
                foreach (ArgValue value2 in entity)
                {
                    if ((value2.Argument == "classname") && (value2.Value == Classname))
                    {
                        list.Add(entity);
                    }
                }
            }
            return (BSPEntity[])list.ToArray(typeof(BSPEntity));
        }

        public string SeekFirstEntityValue(string Classname, string Argument)
        {
            string str = "";
            BSPEntity[] entityArray = this.SeekEntitiesByClassname(Classname);
            if (entityArray.Length > 0)
            {
                string[] strArray = entityArray[0].SeekValuesByArgument(Argument);
                if (strArray.Length > 0)
                {
                    str = strArray[0];
                }
            }
            return str;
        }

        public int Count
        {
            get
            {
                return this.AllEntities.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this.AllEntities.IsSynchronized;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.AllEntities.SyncRoot;
            }
        }
    }

    public class BSPFace
    {
        public int Effect = 0;
        public int LightmapID = -1;
        public int[] MapCorner = new int[2];
        public Vector3f MapPosition = new Vector3f();
        public int[] MapSize = new int[2];
        public Vector3f[] MapVectors = new Vector3f[2];
        public int MeshVertexIndex = -1;
        public Vector3f Normal = new Vector3f();
        public int NumMeshVertices = 0;
        public int NumVertices = 0;
        public int[] Size = new int[2];
        public static readonly int SizeInBytes = 0x68;
        public int StartVertexIndex = -1;
        public int TextureID = -1;
        public int Type = 0;

        public BSPFace()
        {
            this.MapVectors[0] = new Vector3f();
            this.MapVectors[1] = new Vector3f();
        }
    }
    public class BSPHeader
    {
        public string ID = "";
        public int Version = 0;
    }
    public class BSPLeaf
    {
        public int Area = 0;
        public int Cluster = 0;
        public int LeafBrush = 0;
        public int LeafFace = 0;
        public Vector3i Max = new Vector3i();
        public Vector3i Min = new Vector3i();
        public int NumLeafBrushes = 0;
        public int NumLeafFaces = 0;
        public static readonly int SizeInBytes = 48;
    }
    public class BSPLightmap
    {
        public byte[] ImageBytes = new byte[128 * 128 * 3];
        public static readonly int SizeInBytes = 49152;
    }
    public class BSPLump
    {
        public int Length = 0;
        public int Offset = 0;
    }
    public class BSPModel
    {
        public int FirstBrush = 0;
        public int FirstFace = 0;
        public float[] Maxes = new float[3];
        public float[] Mins = new float[3];
        public int NumBrushes = 0;
        public int NumFaces = 0;
        public static readonly int SizeInBytes = 40;
    }
    public class BSPNode
    {
        public int Back = 0;
        public int Front = 0;
        public Vector3i Max = new Vector3i();
        public Vector3i Min = new Vector3i();
        public int Plane = 0;
        public static readonly int SizeInBytes = 36;
    }
    public class BSPPlane
    {
        public float Distance = 0f;
        public Vector3f Normal = new Vector3f();
        public static readonly int SizeInBytes = 0x10;
    }
    public class BSPShader
    {
        public int BrushIndex = 0;
        public int ContentFlags = 0;
        public string Name = "";
        public static readonly int SizeInBytes = 0x48;
    }
    public class BSPTexture
    {
        public int Contents = 0;
        public int Flags = 0;
        public string Name = "";
        public static readonly int SizeInBytes = 72;
    }
    public class BSPVisData
    {
        public byte[] BitSets = null;
        public int BytesPerCluster = 0;
        public int NumClusters = 0;
    }
}
