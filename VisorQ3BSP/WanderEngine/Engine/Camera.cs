
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
using Math3D;
using OpenTK.Graphics.OpenGL;
    
namespace WanderEngine
{

    public class Camera
    {
        public float Pitch;
        public Vector3f Position;
        public Vector3f Strafe;
        public Vector3f UpVector;
        public Vector3f View;
        public float Yaw;

        public Camera()
        {
            this.Position = new Vector3f(0f, 0f, 0f);
            this.View = new Vector3f(0f, 0f, 1f);
            this.UpVector = new Vector3f(0f, 1f, 0f);
            this.Strafe = new Vector3f();
            this.Pitch = 0f;
            this.Yaw = 0f;
            this.SetMouseView(0f, 0f);
        }

        public Camera(Vector3f Position, Vector3f View, Vector3f UpVector)
        {
            this.Position = new Vector3f(0f, 0f, 0f);
            this.View = new Vector3f(0f, 0f, 1f);
            this.UpVector = new Vector3f(0f, 1f, 0f);
            this.Strafe = new Vector3f();
            this.Pitch = 0f;
            this.Yaw = 0f;
            this.Position = Position;
            this.View = View;
            this.UpVector = UpVector;
        }

        public void MoveCamera(float Speed)
        {
            Vector3f vector = this.View - this.Position;
            vector.Normalize();
            float num = vector.X * Speed;
            float num2 = vector.Z * Speed;
            this.Position.X += num;
            this.Position.Z += num2;
            this.View.X += num;
            this.View.Z += num2;
        }

        public void MoveCameraTo(Vector3f NewPos)
        {
            this.View = (this.View + NewPos) - this.Position;
            this.Position.X = NewPos.X;
            this.Position.Y = NewPos.Y;
            this.Position.Z = NewPos.Z;
        }

        public void MoveCameraUpDown(float Speed)
        {
            this.Position.Y += Speed;
            this.View.Y += Speed;
        }

        public Vector3f ProjectPosition(float FrontBack, float LeftRight)
        {
            Vector3f vector = new Vector3f(this.Position.X, this.Position.Y, this.Position.Z);
            if (FrontBack > 0f)
            {
                float angle = Utility.CapAngle(90f - this.Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X += Utility.CosDeg(angle) * FrontBack;
                vector.Z += -Utility.SinDeg(angle) * FrontBack;
            }
            else if (FrontBack < 0f)
            {
                float num2 = Utility.CapAngle(90f - this.Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X -= Utility.CosDeg(num2) * FrontBack;
                vector.Z += Utility.SinDeg(num2) * FrontBack;
            }
            if (LeftRight > 0f)
            {
                float num3 = Utility.CapAngle(-this.Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X += Utility.CosDeg(num3) * LeftRight;
                vector.Z += -Utility.SinDeg(num3) * LeftRight;
                return vector;
            }
            if (LeftRight < 0f)
            {
                float num4 = Utility.CapAngle(-this.Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X -= Utility.CosDeg(num4) * LeftRight;
                vector.Z += Utility.SinDeg(num4) * LeftRight;
            }
            return vector;
        }

        public Vector3f ProjectPositionWithY(float FrontBack, float LeftRight)
        {
            Vector3f vector = new Vector3f(this.Position.X, this.Position.Y, this.Position.Z);
            if (FrontBack > 0f)
            {
                float angle = Utility.CapAngle(90f - this.Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X += Utility.CosDeg(angle) * FrontBack;
                vector.Y += Utility.SinDeg(this.Pitch) * FrontBack;
                vector.Z += -Utility.SinDeg(angle) * FrontBack;
            }
            else if (FrontBack < 0f)
            {
                float num2 = Utility.CapAngle(90f - this.Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X -= Utility.CosDeg(num2) * FrontBack;
                vector.Y -= Utility.SinDeg(this.Pitch) * FrontBack;
                vector.Z += Utility.SinDeg(num2) * FrontBack;
            }
            if (LeftRight > 0f)
            {
                float num3 = Utility.CapAngle(-this.Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X += Utility.CosDeg(num3) * LeftRight;
                vector.Z += -Utility.SinDeg(num3) * LeftRight;
                return vector;
            }
            if (LeftRight < 0f)
            {
                float num4 = Utility.CapAngle(-this.Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X -= Utility.CosDeg(num4) * LeftRight;
                vector.Z += Utility.SinDeg(num4) * LeftRight;
            }
            return vector;
        }

        public static Vector3f ProjectVector(Vector3f SourceVector, float FrontBack, float LeftRight, float Yaw)
        {
            Vector3f vector = new Vector3f(SourceVector.X, SourceVector.Y, SourceVector.Z);
            if (FrontBack > 0f)
            {
                float angle = Utility.CapAngle(90f - Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X += Utility.CosDeg(angle) * FrontBack;
                vector.Z += -Utility.SinDeg(angle) * FrontBack;
            }
            else if (FrontBack < 0f)
            {
                float num2 = Utility.CapAngle(90f - Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X -= Utility.CosDeg(num2) * FrontBack;
                vector.Z += Utility.SinDeg(num2) * FrontBack;
            }
            if (LeftRight > 0f)
            {
                float num3 = Utility.CapAngle(-Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X += Utility.CosDeg(num3) * LeftRight;
                vector.Z += -Utility.SinDeg(num3) * LeftRight;
                return vector;
            }
            if (LeftRight < 0f)
            {
                float num4 = Utility.CapAngle(-Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X -= Utility.CosDeg(num4) * LeftRight;
                vector.Z += Utility.SinDeg(num4) * LeftRight;
            }
            return vector;
        }

        public void SetMouseView(float Dx, float Dy)
        {
            this.Pitch += Dy;
            this.Yaw -= Dx;
            if (this.Pitch > 90f)
            {
                this.Pitch = 90f;
            }
            if (this.Pitch < -90f)
            {
                this.Pitch = -90f;
            }
            this.View.X = this.Position.X + Utility.SinDeg(this.Yaw);
            this.View.Y = this.Position.Y + Utility.SinDeg(this.Pitch);
            this.View.Z = this.Position.Z - Utility.CosDeg(this.Yaw);
        }

        public void StrafeCamera(float Speed)
        {
            float num = this.Strafe.X * Speed;
            float num2 = this.Strafe.Z * Speed;
            this.Position.X += num;
            this.Position.Z += num2;
            this.View.X += num;
            this.View.Z += num2;
        }

        public void UpdateView()
        {
        	Vector3f v = new Vector3f();
        	Vector3f.Cross(this.View - this.Position, this.UpVector, ref v);
        	v.Normalize();
            this.Strafe = v;
            Engine.gluLookAt(this.Position.X, this.Position.Y, this.Position.Z, 
                             this.View.X, this.View.Y, this.View.Z, 
                             this.UpVector.X, this.UpVector.Y, this.UpVector.Z);
        }
    }
}

