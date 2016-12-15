
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

namespace WanderEngine
{
    using System;

    public class Force
    {
        public Vector3f Acceleration = new Vector3f();
        public Vector3f Direction = new Vector3f();
        public Vector3f MaxVelocity = new Vector3f();
        public Vector3f MinVelocity = new Vector3f();
        private Vector3f Velocity = new Vector3f();

        public void AddVelocityX(float X)
        {
            this.Velocity.X += X;
            if (this.Velocity.X > this.MaxVelocity.X)
            {
                this.Velocity.X = this.MaxVelocity.X;
            }
            if (this.Velocity.X < this.MinVelocity.X)
            {
                this.Velocity.X = this.MinVelocity.X;
            }
        }

        public void AddVelocityY(float Y)
        {
            this.Velocity.Y += Y;
            if (this.Velocity.Y > this.MaxVelocity.Y)
            {
                this.Velocity.Y = this.MaxVelocity.Y;
            }
            if (this.Velocity.Y < this.MinVelocity.Y)
            {
                this.Velocity.Y = this.MinVelocity.Y;
            }
        }

        public void AddVelocityZ(float Z)
        {
            this.Velocity.Z += Z;
            if (this.Velocity.Z > this.MaxVelocity.Z)
            {
                this.Velocity.Z = this.MaxVelocity.Z;
            }
            if (this.Velocity.Z < this.MinVelocity.Z)
            {
                this.Velocity.Z = this.MinVelocity.Z;
            }
        }

        public float GetVelocityX()
        {
            return this.Velocity.X;
        }

        public float GetVelocityY()
        {
            return this.Velocity.Y;
        }

        public float GetVelocityZ()
        {
            return this.Velocity.Z;
        }

        public void SetVelocity(float X, float Y, float Z)
        {
            this.Velocity.X = X;
            this.Velocity.Y = Y;
            this.Velocity.Z = Z;
            if (this.Velocity.X > this.MaxVelocity.X)
            {
                this.Velocity.X = this.MaxVelocity.X;
            }
            if (this.Velocity.Y > this.MaxVelocity.Y)
            {
                this.Velocity.Y = this.MaxVelocity.Y;
            }
            if (this.Velocity.Z > this.MaxVelocity.Z)
            {
                this.Velocity.Z = this.MaxVelocity.Z;
            }
            if (this.Velocity.X < this.MinVelocity.X)
            {
                this.Velocity.X = this.MinVelocity.X;
            }
            if (this.Velocity.Y < this.MinVelocity.Y)
            {
                this.Velocity.Y = this.MinVelocity.Y;
            }
            if (this.Velocity.Z < this.MinVelocity.Z)
            {
                this.Velocity.Z = this.MinVelocity.Z;
            }
        }

        public void Update(Vector3f SourcePoint, float TimeElapsed)
        {
            this.Velocity.X += this.Acceleration.X * (TimeElapsed * TimeElapsed);
            this.Velocity.Y += this.Acceleration.Y * (TimeElapsed * TimeElapsed);
            this.Velocity.Z += this.Acceleration.Z * (TimeElapsed * TimeElapsed);
            if (this.Velocity.X > this.MaxVelocity.X)
            {
                this.Velocity.X = this.MaxVelocity.X;
            }
            if (this.Velocity.Y > this.MaxVelocity.Y)
            {
                this.Velocity.Y = this.MaxVelocity.Y;
            }
            if (this.Velocity.Z > this.MaxVelocity.Z)
            {
                this.Velocity.Z = this.MaxVelocity.Z;
            }
            if (this.Velocity.X < this.MinVelocity.X)
            {
                this.Velocity.X = this.MinVelocity.X;
            }
            if (this.Velocity.Y < this.MinVelocity.Y)
            {
                this.Velocity.Y = this.MinVelocity.Y;
            }
            if (this.Velocity.Z < this.MinVelocity.Z)
            {
                this.Velocity.Z = this.MinVelocity.Z;
            }
            SourcePoint.X += (this.Direction.X * this.Velocity.X) * TimeElapsed;
            SourcePoint.Y += (this.Direction.Y * this.Velocity.Y) * TimeElapsed;
            SourcePoint.Z += (this.Direction.Z * this.Velocity.Z) * TimeElapsed;
        }
    }
}

