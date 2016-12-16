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

namespace WanderEngine.WanderEngine
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
            Position = new Vector3f(0f, 0f, 0f);
            View = new Vector3f(0f, 0f, 1f);
            UpVector = new Vector3f(0f, 1f, 0f);
            Strafe = new Vector3f();
            Pitch = 0f;
            Yaw = 0f;
            SetMouseView(0f, 0f);
        }

        public Camera(Vector3f Position, Vector3f View, Vector3f UpVector)
        {
            this.Position = new Vector3f(0f, 0f, 0f);
            this.View = new Vector3f(0f, 0f, 1f);
            this.UpVector = new Vector3f(0f, 1f, 0f);
            Strafe = new Vector3f();
            Pitch = 0f;
            Yaw = 0f;
            this.Position = Position;
            this.View = View;
            this.UpVector = UpVector;
        }

        public void MoveCamera(float Speed)
        {
            var vector = View - Position;
            vector.Normalize();
            var num = vector.X*Speed;
            var num2 = vector.Z*Speed;
            Position.X += num;
            Position.Z += num2;
            View.X += num;
            View.Z += num2;
        }

        public void MoveCameraTo(Vector3f NewPos)
        {
            View = (View + NewPos) - Position;
            Position.X = NewPos.X;
            Position.Y = NewPos.Y;
            Position.Z = NewPos.Z;
        }

        public void MoveCameraUpDown(float Speed)
        {
            Position.Y += Speed;
            View.Y += Speed;
        }

        public Vector3f ProjectPosition(float FrontBack, float LeftRight)
        {
            var vector = new Vector3f(Position.X, Position.Y, Position.Z);
            if (FrontBack > 0f)
            {
                var angle = Utility.CapAngle(90f - Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X += Utility.CosDeg(angle)*FrontBack;
                vector.Z += -Utility.SinDeg(angle)*FrontBack;
            }
            else if (FrontBack < 0f)
            {
                var num2 = Utility.CapAngle(90f - Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X -= Utility.CosDeg(num2)*FrontBack;
                vector.Z += Utility.SinDeg(num2)*FrontBack;
            }
            if (LeftRight > 0f)
            {
                var num3 = Utility.CapAngle(-Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X += Utility.CosDeg(num3)*LeftRight;
                vector.Z += -Utility.SinDeg(num3)*LeftRight;
                return vector;
            }
            if (LeftRight < 0f)
            {
                var num4 = Utility.CapAngle(-Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X -= Utility.CosDeg(num4)*LeftRight;
                vector.Z += Utility.SinDeg(num4)*LeftRight;
            }
            return vector;
        }

        public Vector3f ProjectPositionWithY(float FrontBack, float LeftRight)
        {
            var vector = new Vector3f(Position.X, Position.Y, Position.Z);
            if (FrontBack > 0f)
            {
                var angle = Utility.CapAngle(90f - Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X += Utility.CosDeg(angle)*FrontBack;
                vector.Y += Utility.SinDeg(Pitch)*FrontBack;
                vector.Z += -Utility.SinDeg(angle)*FrontBack;
            }
            else if (FrontBack < 0f)
            {
                var num2 = Utility.CapAngle(90f - Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X -= Utility.CosDeg(num2)*FrontBack;
                vector.Y -= Utility.SinDeg(Pitch)*FrontBack;
                vector.Z += Utility.SinDeg(num2)*FrontBack;
            }
            if (LeftRight > 0f)
            {
                var num3 = Utility.CapAngle(-Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X += Utility.CosDeg(num3)*LeftRight;
                vector.Z += -Utility.SinDeg(num3)*LeftRight;
                return vector;
            }
            if (LeftRight < 0f)
            {
                var num4 = Utility.CapAngle(-Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X -= Utility.CosDeg(num4)*LeftRight;
                vector.Z += Utility.SinDeg(num4)*LeftRight;
            }
            return vector;
        }

        public static Vector3f ProjectVector(Vector3f SourceVector, float FrontBack, float LeftRight, float Yaw)
        {
            var vector = new Vector3f(SourceVector.X, SourceVector.Y, SourceVector.Z);
            if (FrontBack > 0f)
            {
                var angle = Utility.CapAngle(90f - Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X += Utility.CosDeg(angle)*FrontBack;
                vector.Z += -Utility.SinDeg(angle)*FrontBack;
            }
            else if (FrontBack < 0f)
            {
                var num2 = Utility.CapAngle(90f - Yaw);
                FrontBack = Math.Abs(FrontBack);
                vector.X -= Utility.CosDeg(num2)*FrontBack;
                vector.Z += Utility.SinDeg(num2)*FrontBack;
            }
            if (LeftRight > 0f)
            {
                var num3 = Utility.CapAngle(-Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X += Utility.CosDeg(num3)*LeftRight;
                vector.Z += -Utility.SinDeg(num3)*LeftRight;
                return vector;
            }
            if (LeftRight < 0f)
            {
                var num4 = Utility.CapAngle(-Yaw);
                LeftRight = Math.Abs(LeftRight);
                vector.X -= Utility.CosDeg(num4)*LeftRight;
                vector.Z += Utility.SinDeg(num4)*LeftRight;
            }
            return vector;
        }

        public void SetMouseView(float Dx, float Dy)
        {
            Pitch += Dy;
            Yaw -= Dx;
            if (Pitch > 90f)
            {
                Pitch = 90f;
            }
            if (Pitch < -90f)
            {
                Pitch = -90f;
            }
            View.X = Position.X + Utility.SinDeg(Yaw);
            View.Y = Position.Y + Utility.SinDeg(Pitch);
            View.Z = Position.Z - Utility.CosDeg(Yaw);
        }

        public void StrafeCamera(float Speed)
        {
            var num = Strafe.X*Speed;
            var num2 = Strafe.Z*Speed;
            Position.X += num;
            Position.Z += num2;
            View.X += num;
            View.Z += num2;
        }

        public void UpdateView()
        {
            var v = new Vector3f();
            Vector3f.Cross(View - Position, UpVector, ref v);
            v.Normalize();
            Strafe = v;
            Engine.gluLookAt(Position.X, Position.Y, Position.Z,
                View.X, View.Y, View.Z,
                UpVector.X, UpVector.Y, UpVector.Z);
        }
    }
}