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

namespace SnowflakeEngine.WanderEngine
{
    public class Camera
    {
        public float Pitch;
        public Vector3F Position;
        public Vector3F Strafe;
        public Vector3F UpVector;
        public Vector3F View;
        public float Yaw;

        public Camera()
        {
            Position = new Vector3F(0f, 0f, 0f);
            View = new Vector3F(0f, 0f, 1f);
            UpVector = new Vector3F(0f, 1f, 0f);
            Strafe = new Vector3F();
            Pitch = 0f;
            Yaw = 0f;
            SetMouseView(0f, 0f);
        }

        public Camera(Vector3F position, Vector3F view, Vector3F upVector)
        {
            Position = new Vector3F(0f, 0f, 0f);
            View = new Vector3F(0f, 0f, 1f);
            UpVector = new Vector3F(0f, 1f, 0f);
            Strafe = new Vector3F();
            Pitch = 0f;
            Yaw = 0f;
            Position = position;
            View = view;
            UpVector = upVector;
        }

        public void MoveCamera(float speed)
        {
            var vector = View - Position;
            vector.Normalize();
            var num = vector.X*speed;
            var num2 = vector.Z*speed;
            Position.X += num;
            Position.Z += num2;
            View.X += num;
            View.Z += num2;
        }

        public void MoveCameraTo(Vector3F newPos)
        {
            View = (View + newPos) - Position;
            Position.X = newPos.X;
            Position.Y = newPos.Y;
            Position.Z = newPos.Z;
        }

        public void MoveCameraUpDown(float speed)
        {
            Position.Y += speed;
            View.Y += speed;
        }

        public Vector3F ProjectPosition(float frontBack, float leftRight)
        {
            var vector = new Vector3F(Position.X, Position.Y, Position.Z);
            if (frontBack > 0f)
            {
                var angle = Utility.CapAngle(90f - Yaw);
                frontBack = Math.Abs(frontBack);
                vector.X += Utility.CosDeg(angle)*frontBack;
                vector.Z += -Utility.SinDeg(angle)*frontBack;
            }
            else if (frontBack < 0f)
            {
                var num2 = Utility.CapAngle(90f - Yaw);
                frontBack = Math.Abs(frontBack);
                vector.X -= Utility.CosDeg(num2)*frontBack;
                vector.Z += Utility.SinDeg(num2)*frontBack;
            }
            if (leftRight > 0f)
            {
                var num3 = Utility.CapAngle(-Yaw);
                leftRight = Math.Abs(leftRight);
                vector.X += Utility.CosDeg(num3)*leftRight;
                vector.Z += -Utility.SinDeg(num3)*leftRight;
                return vector;
            }
            if (leftRight < 0f)
            {
                var num4 = Utility.CapAngle(-Yaw);
                leftRight = Math.Abs(leftRight);
                vector.X -= Utility.CosDeg(num4)*leftRight;
                vector.Z += Utility.SinDeg(num4)*leftRight;
            }
            return vector;
        }

        public Vector3F ProjectPositionWithY(float frontBack, float leftRight)
        {
            var vector = new Vector3F(Position.X, Position.Y, Position.Z);
            if (frontBack > 0f)
            {
                var angle = Utility.CapAngle(90f - Yaw);
                frontBack = Math.Abs(frontBack);
                vector.X += Utility.CosDeg(angle)*frontBack;
                vector.Y += Utility.SinDeg(Pitch)*frontBack;
                vector.Z += -Utility.SinDeg(angle)*frontBack;
            }
            else if (frontBack < 0f)
            {
                var num2 = Utility.CapAngle(90f - Yaw);
                frontBack = Math.Abs(frontBack);
                vector.X -= Utility.CosDeg(num2)*frontBack;
                vector.Y -= Utility.SinDeg(Pitch)*frontBack;
                vector.Z += Utility.SinDeg(num2)*frontBack;
            }
            if (leftRight > 0f)
            {
                var num3 = Utility.CapAngle(-Yaw);
                leftRight = Math.Abs(leftRight);
                vector.X += Utility.CosDeg(num3)*leftRight;
                vector.Z += -Utility.SinDeg(num3)*leftRight;
                return vector;
            }
            if (leftRight < 0f)
            {
                var num4 = Utility.CapAngle(-Yaw);
                leftRight = Math.Abs(leftRight);
                vector.X -= Utility.CosDeg(num4)*leftRight;
                vector.Z += Utility.SinDeg(num4)*leftRight;
            }
            return vector;
        }

        public static Vector3F ProjectVector(Vector3F sourceVector, float frontBack, float leftRight, float yaw)
        {
            var vector = new Vector3F(sourceVector.X, sourceVector.Y, sourceVector.Z);
            if (frontBack > 0f)
            {
                var angle = Utility.CapAngle(90f - yaw);
                frontBack = Math.Abs(frontBack);
                vector.X += Utility.CosDeg(angle)*frontBack;
                vector.Z += -Utility.SinDeg(angle)*frontBack;
            }
            else if (frontBack < 0f)
            {
                var num2 = Utility.CapAngle(90f - yaw);
                frontBack = Math.Abs(frontBack);
                vector.X -= Utility.CosDeg(num2)*frontBack;
                vector.Z += Utility.SinDeg(num2)*frontBack;
            }
            if (leftRight > 0f)
            {
                var num3 = Utility.CapAngle(-yaw);
                leftRight = Math.Abs(leftRight);
                vector.X += Utility.CosDeg(num3)*leftRight;
                vector.Z += -Utility.SinDeg(num3)*leftRight;
                return vector;
            }
            if (leftRight < 0f)
            {
                var num4 = Utility.CapAngle(-yaw);
                leftRight = Math.Abs(leftRight);
                vector.X -= Utility.CosDeg(num4)*leftRight;
                vector.Z += Utility.SinDeg(num4)*leftRight;
            }
            return vector;
        }

        public void SetMouseView(float dx, float dy)
        {
            Pitch += dy;
            Yaw -= dx;
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

        public void StrafeCamera(float speed)
        {
            var num = Strafe.X*speed;
            var num2 = Strafe.Z*speed;
            Position.X += num;
            Position.Z += num2;
            View.X += num;
            View.Z += num2;
        }

        public void UpdateView()
        {
            var v = new Vector3F();
            Vector3F.Cross(View - Position, UpVector, ref v);
            v.Normalize();
            Strafe = v;
            Engine.GluLookAt(Position.X, Position.Y, Position.Z,
                View.X, View.Y, View.Z,
                UpVector.X, UpVector.Y, UpVector.Z);
        }
    }
}