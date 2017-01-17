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

using OpenTK.Graphics.OpenGL;

namespace SnowflakeEngine.WanderEngine
{
    public class Frustrum
    {
        public static readonly int NumPlanes = 6;
        public Plane[] Planes = new Plane[NumPlanes];

        public Frustrum()
        {
            for (var i = 0; i < NumPlanes; i++)
            {
                Planes[i] = new Plane();
            }
        }

        public bool BoxInFrustrum(float x, float y, float z, float x2, float y2, float z2)
        {
            var flag = true;
            for (var i = 0; i < 6; i++)
            {
                var plane = Planes[i];
                if ((((((((plane.A*x) + (plane.B*y)) + (plane.C*z)) + plane.D) <= 0f) &&
                      (((((plane.A*x2) + (plane.B*y)) + (plane.C*z)) + plane.D) <= 0f)) &&
                     ((((((plane.A*x) + (plane.B*y2)) + (plane.C*z)) + plane.D) <= 0f) &&
                      (((((plane.A*x2) + (plane.B*y2)) + (plane.C*z)) + plane.D) <= 0f))) &&
                    (((((((plane.A*x) + (plane.B*y)) + (plane.C*z2)) + plane.D) <= 0f) &&
                      (((((plane.A*x2) + (plane.B*y)) + (plane.C*z2)) + plane.D) <= 0f)) &&
                     ((((((plane.A*x) + (plane.B*y2)) + (plane.C*z2)) + plane.D) <= 0f) &&
                      (((((plane.A*x2) + (plane.B*y2)) + (plane.C*z2)) + plane.D) <= 0f))))
                {
                    flag = false;
                }
            }
            return flag;
        }

        public void UpdateFrustrum()
        {
            var projection = new float[0x10];
            var modelView = new float[0x10];
            var clip = new float[0x10];
            GL.GetFloat(GetPName.ProjectionMatrix, projection);
            GL.GetFloat(GetPName.ModelviewMatrix, modelView);
            clip[0] = (((modelView[0]*projection[0]) + (modelView[1]*projection[4])) + (modelView[2]*projection[8])) +
                      (modelView[3]*projection[12]);
            clip[1] = (((modelView[0]*projection[1]) + (modelView[1]*projection[5])) + (modelView[2]*projection[9])) +
                      (modelView[3]*projection[13]);
            clip[2] = (((modelView[0]*projection[2]) + (modelView[1]*projection[6])) + (modelView[2]*projection[10])) +
                      (modelView[3]*projection[14]);
            clip[3] = (((modelView[0]*projection[3]) + (modelView[1]*projection[7])) + (modelView[2]*projection[11])) +
                      (modelView[3]*projection[15]);
            clip[4] = (((modelView[4]*projection[0]) + (modelView[5]*projection[4])) + (modelView[6]*projection[8])) +
                      (modelView[7]*projection[12]);
            clip[5] = (((modelView[4]*projection[1]) + (modelView[5]*projection[5])) + (modelView[6]*projection[9])) +
                      (modelView[7]*projection[13]);
            clip[6] = (((modelView[4]*projection[2]) + (modelView[5]*projection[6])) + (modelView[6]*projection[10])) +
                      (modelView[7]*projection[14]);
            clip[7] = (((modelView[4]*projection[3]) + (modelView[5]*projection[7])) + (modelView[6]*projection[11])) +
                      (modelView[7]*projection[15]);
            clip[8] = (((modelView[8]*projection[0]) + (modelView[9]*projection[4])) + (modelView[10]*projection[8])) +
                      (modelView[11]*projection[12]);
            clip[9] = (((modelView[8]*projection[1]) + (modelView[9]*projection[5])) + (modelView[10]*projection[9])) +
                      (modelView[11]*projection[13]);
            clip[10] = (((modelView[8]*projection[2]) + (modelView[9]*projection[6])) + (modelView[10]*projection[10])) +
                       (modelView[11]*projection[14]);
            clip[11] = (((modelView[8]*projection[3]) + (modelView[9]*projection[7])) + (modelView[10]*projection[11])) +
                       (modelView[11]*projection[15]);
            clip[12] = (((modelView[12]*projection[0]) + (modelView[13]*projection[4])) + (modelView[14]*projection[8])) +
                       (modelView[15]*projection[12]);
            clip[13] = (((modelView[12]*projection[1]) + (modelView[13]*projection[5])) + (modelView[14]*projection[9])) +
                       (modelView[15]*projection[13]);
            clip[14] = (((modelView[12]*projection[2]) + (modelView[13]*projection[6])) + (modelView[14]*projection[10])) +
                       (modelView[15]*projection[14]);
            clip[15] = (((modelView[12]*projection[3]) + (modelView[13]*projection[7])) + (modelView[14]*projection[11])) +
                       (modelView[15]*projection[15]);
            Planes[2].A = clip[3] - clip[0];
            Planes[2].B = clip[7] - clip[4];
            Planes[2].C = clip[11] - clip[8];
            Planes[2].D = clip[15] - clip[12];
            Planes[3].A = clip[3] + clip[0];
            Planes[3].B = clip[7] + clip[4];
            Planes[3].C = clip[11] + clip[8];
            Planes[3].D = clip[15] + clip[12];
            Planes[5].A = clip[3] + clip[1];
            Planes[5].B = clip[7] + clip[5];
            Planes[5].C = clip[11] + clip[9];
            Planes[5].D = clip[15] + clip[13];
            Planes[4].A = clip[3] - clip[1];
            Planes[4].B = clip[7] - clip[5];
            Planes[4].C = clip[11] - clip[9];
            Planes[4].D = clip[15] - clip[13];
            Planes[1].A = clip[3] - clip[2];
            Planes[1].B = clip[7] - clip[6];
            Planes[1].C = clip[11] - clip[10];
            Planes[1].D = clip[15] - clip[14];
            Planes[0].A = clip[3] + clip[2];
            Planes[0].B = clip[7] + clip[6];
            Planes[0].C = clip[11] + clip[10];
            Planes[0].D = clip[15] + clip[14];
            for (var i = 0; i < NumPlanes; i++)
            {
                Planes[i].Normalize();
            }
        }
    }
}