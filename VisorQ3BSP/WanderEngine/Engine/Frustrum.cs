
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
    using OpenTK.Graphics.OpenGL;

    public class Frustrum
    {
        public static readonly int NumPlanes = 6;
        public Plane[] Planes = new Plane[NumPlanes];

        public Frustrum()
        {
            for (int i = 0; i < NumPlanes; i++)
            {
                this.Planes[i] = new Plane();
            }
        }

        public bool BoxInFrustrum(float X, float Y, float Z, float X2, float Y2, float Z2)
        {
            bool flag = true;
            for (int i = 0; i < 6; i++)
            {
                Plane plane = this.Planes[i];
                if ((((((((plane.A * X) + (plane.B * Y)) + (plane.C * Z)) + plane.D) <= 0f) && (((((plane.A * X2) + (plane.B * Y)) + (plane.C * Z)) + plane.D) <= 0f)) && ((((((plane.A * X) + (plane.B * Y2)) + (plane.C * Z)) + plane.D) <= 0f) && (((((plane.A * X2) + (plane.B * Y2)) + (plane.C * Z)) + plane.D) <= 0f))) && (((((((plane.A * X) + (plane.B * Y)) + (plane.C * Z2)) + plane.D) <= 0f) && (((((plane.A * X2) + (plane.B * Y)) + (plane.C * Z2)) + plane.D) <= 0f)) && ((((((plane.A * X) + (plane.B * Y2)) + (plane.C * Z2)) + plane.D) <= 0f) && (((((plane.A * X2) + (plane.B * Y2)) + (plane.C * Z2)) + plane.D) <= 0f))))
                {
                    flag = false;
                }
            }
            return flag;
        }

        public void UpdateFrustrum()
        {
            float[] projection = new float[0x10];
            float[] modelView = new float[0x10];
            float[] clip = new float[0x10];
            GL.GetFloat(GetPName.ProjectionMatrix, projection);
            GL.GetFloat(GetPName.ModelviewMatrix, modelView);
            clip[0] = (((modelView[0] * projection[0]) + (modelView[1] * projection[4])) + (modelView[2] * projection[8])) + (modelView[3] * projection[12]);
            clip[1] = (((modelView[0] * projection[1]) + (modelView[1] * projection[5])) + (modelView[2] * projection[9])) + (modelView[3] * projection[13]);
            clip[2] = (((modelView[0] * projection[2]) + (modelView[1] * projection[6])) + (modelView[2] * projection[10])) + (modelView[3] * projection[14]);
            clip[3] = (((modelView[0] * projection[3]) + (modelView[1] * projection[7])) + (modelView[2] * projection[11])) + (modelView[3] * projection[15]);
            clip[4] = (((modelView[4] * projection[0]) + (modelView[5] * projection[4])) + (modelView[6] * projection[8])) + (modelView[7] * projection[12]);
            clip[5] = (((modelView[4] * projection[1]) + (modelView[5] * projection[5])) + (modelView[6] * projection[9])) + (modelView[7] * projection[13]);
            clip[6] = (((modelView[4] * projection[2]) + (modelView[5] * projection[6])) + (modelView[6] * projection[10])) + (modelView[7] * projection[14]);
            clip[7] = (((modelView[4] * projection[3]) + (modelView[5] * projection[7])) + (modelView[6] * projection[11])) + (modelView[7] * projection[15]);
            clip[8] = (((modelView[8] * projection[0]) + (modelView[9] * projection[4])) + (modelView[10] * projection[8])) + (modelView[11] * projection[12]);
            clip[9] = (((modelView[8] * projection[1]) + (modelView[9] * projection[5])) + (modelView[10] * projection[9])) + (modelView[11] * projection[13]);
            clip[10] = (((modelView[8] * projection[2]) + (modelView[9] * projection[6])) + (modelView[10] * projection[10])) + (modelView[11] * projection[14]);
            clip[11] = (((modelView[8] * projection[3]) + (modelView[9] * projection[7])) + (modelView[10] * projection[11])) + (modelView[11] * projection[15]);
            clip[12] = (((modelView[12] * projection[0]) + (modelView[13] * projection[4])) + (modelView[14] * projection[8])) + (modelView[15] * projection[12]);
            clip[13] = (((modelView[12] * projection[1]) + (modelView[13] * projection[5])) + (modelView[14] * projection[9])) + (modelView[15] * projection[13]);
            clip[14] = (((modelView[12] * projection[2]) + (modelView[13] * projection[6])) + (modelView[14] * projection[10])) + (modelView[15] * projection[14]);
            clip[15] = (((modelView[12] * projection[3]) + (modelView[13] * projection[7])) + (modelView[14] * projection[11])) + (modelView[15] * projection[15]);
            this.Planes[2].A = clip[3] - clip[0];
            this.Planes[2].B = clip[7] - clip[4];
            this.Planes[2].C = clip[11] - clip[8];
            this.Planes[2].D = clip[15] - clip[12];
            this.Planes[3].A = clip[3] + clip[0];
            this.Planes[3].B = clip[7] + clip[4];
            this.Planes[3].C = clip[11] + clip[8];
            this.Planes[3].D = clip[15] + clip[12];
            this.Planes[5].A = clip[3] + clip[1];
            this.Planes[5].B = clip[7] + clip[5];
            this.Planes[5].C = clip[11] + clip[9];
            this.Planes[5].D = clip[15] + clip[13];
            this.Planes[4].A = clip[3] - clip[1];
            this.Planes[4].B = clip[7] - clip[5];
            this.Planes[4].C = clip[11] - clip[9];
            this.Planes[4].D = clip[15] - clip[13];
            this.Planes[1].A = clip[3] - clip[2];
            this.Planes[1].B = clip[7] - clip[6];
            this.Planes[1].C = clip[11] - clip[10];
            this.Planes[1].D = clip[15] - clip[14];
            this.Planes[0].A = clip[3] + clip[2];
            this.Planes[0].B = clip[7] + clip[6];
            this.Planes[0].C = clip[11] + clip[10];
            this.Planes[0].D = clip[15] + clip[14];
            for (int i = 0; i < NumPlanes; i++)
            {
                this.Planes[i].Normalize();
            }
        }
    }
}

