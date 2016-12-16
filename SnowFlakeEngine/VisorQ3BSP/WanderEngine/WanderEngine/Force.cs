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

using Math3D;

namespace SnowflakeEngine.WanderEngine
{
    public class Force
    {
        private readonly Vector3f Velocity = new Vector3f();
        public Vector3f Acceleration = new Vector3f();
        public Vector3f Direction = new Vector3f();
        public Vector3f MaxVelocity = new Vector3f();
        public Vector3f MinVelocity = new Vector3f();

        public void AddVelocityX(float X)
        {
            Velocity.X += X;
            if (Velocity.X > MaxVelocity.X)
            {
                Velocity.X = MaxVelocity.X;
            }
            if (Velocity.X < MinVelocity.X)
            {
                Velocity.X = MinVelocity.X;
            }
        }

        public void AddVelocityY(float Y)
        {
            Velocity.Y += Y;
            if (Velocity.Y > MaxVelocity.Y)
            {
                Velocity.Y = MaxVelocity.Y;
            }
            if (Velocity.Y < MinVelocity.Y)
            {
                Velocity.Y = MinVelocity.Y;
            }
        }

        public void AddVelocityZ(float Z)
        {
            Velocity.Z += Z;
            if (Velocity.Z > MaxVelocity.Z)
            {
                Velocity.Z = MaxVelocity.Z;
            }
            if (Velocity.Z < MinVelocity.Z)
            {
                Velocity.Z = MinVelocity.Z;
            }
        }

        public float GetVelocityX()
        {
            return Velocity.X;
        }

        public float GetVelocityY()
        {
            return Velocity.Y;
        }

        public float GetVelocityZ()
        {
            return Velocity.Z;
        }

        public void SetVelocity(float X, float Y, float Z)
        {
            Velocity.X = X;
            Velocity.Y = Y;
            Velocity.Z = Z;
            if (Velocity.X > MaxVelocity.X)
            {
                Velocity.X = MaxVelocity.X;
            }
            if (Velocity.Y > MaxVelocity.Y)
            {
                Velocity.Y = MaxVelocity.Y;
            }
            if (Velocity.Z > MaxVelocity.Z)
            {
                Velocity.Z = MaxVelocity.Z;
            }
            if (Velocity.X < MinVelocity.X)
            {
                Velocity.X = MinVelocity.X;
            }
            if (Velocity.Y < MinVelocity.Y)
            {
                Velocity.Y = MinVelocity.Y;
            }
            if (Velocity.Z < MinVelocity.Z)
            {
                Velocity.Z = MinVelocity.Z;
            }
        }

        public void Update(Vector3f SourcePoint, float TimeElapsed)
        {
            Velocity.X += Acceleration.X*(TimeElapsed*TimeElapsed);
            Velocity.Y += Acceleration.Y*(TimeElapsed*TimeElapsed);
            Velocity.Z += Acceleration.Z*(TimeElapsed*TimeElapsed);
            if (Velocity.X > MaxVelocity.X)
            {
                Velocity.X = MaxVelocity.X;
            }
            if (Velocity.Y > MaxVelocity.Y)
            {
                Velocity.Y = MaxVelocity.Y;
            }
            if (Velocity.Z > MaxVelocity.Z)
            {
                Velocity.Z = MaxVelocity.Z;
            }
            if (Velocity.X < MinVelocity.X)
            {
                Velocity.X = MinVelocity.X;
            }
            if (Velocity.Y < MinVelocity.Y)
            {
                Velocity.Y = MinVelocity.Y;
            }
            if (Velocity.Z < MinVelocity.Z)
            {
                Velocity.Z = MinVelocity.Z;
            }
            SourcePoint.X += (Direction.X*Velocity.X)*TimeElapsed;
            SourcePoint.Y += (Direction.Y*Velocity.Y)*TimeElapsed;
            SourcePoint.Z += (Direction.Z*Velocity.Z)*TimeElapsed;
        }
    }
}