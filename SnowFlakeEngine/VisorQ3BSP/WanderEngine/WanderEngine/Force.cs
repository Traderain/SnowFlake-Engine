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
        private readonly Vector3F _velocity = new Vector3F();
        public Vector3F Acceleration = new Vector3F();
        public Vector3F Direction = new Vector3F();
        public Vector3F MaxVelocity = new Vector3F();
        public Vector3F MinVelocity = new Vector3F();

        public void AddVelocityX(float x)
        {
            _velocity.X += x;
            if (_velocity.X > MaxVelocity.X)
            {
                _velocity.X = MaxVelocity.X;
            }
            if (_velocity.X < MinVelocity.X)
            {
                _velocity.X = MinVelocity.X;
            }
        }

        public void AddVelocityY(float y)
        {
            _velocity.Y += y;
            if (_velocity.Y > MaxVelocity.Y)
            {
                _velocity.Y = MaxVelocity.Y;
            }
            if (_velocity.Y < MinVelocity.Y)
            {
                _velocity.Y = MinVelocity.Y;
            }
        }

        public void AddVelocityZ(float z)
        {
            _velocity.Z += z;
            if (_velocity.Z > MaxVelocity.Z)
            {
                _velocity.Z = MaxVelocity.Z;
            }
            if (_velocity.Z < MinVelocity.Z)
            {
                _velocity.Z = MinVelocity.Z;
            }
        }

        public float GetVelocityX()
        {
            return _velocity.X;
        }

        public float GetVelocityY()
        {
            return _velocity.Y;
        }

        public float GetVelocityZ()
        {
            return _velocity.Z;
        }

        public void SetVelocity(float x, float y, float z)
        {
            _velocity.X = x;
            _velocity.Y = y;
            _velocity.Z = z;
            if (_velocity.X > MaxVelocity.X)
            {
                _velocity.X = MaxVelocity.X;
            }
            if (_velocity.Y > MaxVelocity.Y)
            {
                _velocity.Y = MaxVelocity.Y;
            }
            if (_velocity.Z > MaxVelocity.Z)
            {
                _velocity.Z = MaxVelocity.Z;
            }
            if (_velocity.X < MinVelocity.X)
            {
                _velocity.X = MinVelocity.X;
            }
            if (_velocity.Y < MinVelocity.Y)
            {
                _velocity.Y = MinVelocity.Y;
            }
            if (_velocity.Z < MinVelocity.Z)
            {
                _velocity.Z = MinVelocity.Z;
            }
        }

        public void Update(Vector3F sourcePoint, float timeElapsed)
        {
            _velocity.X += Acceleration.X*(timeElapsed*timeElapsed);
            _velocity.Y += Acceleration.Y*(timeElapsed*timeElapsed);
            _velocity.Z += Acceleration.Z*(timeElapsed*timeElapsed);
            if (_velocity.X > MaxVelocity.X)
            {
                _velocity.X = MaxVelocity.X;
            }
            if (_velocity.Y > MaxVelocity.Y)
            {
                _velocity.Y = MaxVelocity.Y;
            }
            if (_velocity.Z > MaxVelocity.Z)
            {
                _velocity.Z = MaxVelocity.Z;
            }
            if (_velocity.X < MinVelocity.X)
            {
                _velocity.X = MinVelocity.X;
            }
            if (_velocity.Y < MinVelocity.Y)
            {
                _velocity.Y = MinVelocity.Y;
            }
            if (_velocity.Z < MinVelocity.Z)
            {
                _velocity.Z = MinVelocity.Z;
            }
            sourcePoint.X += (Direction.X*_velocity.X)*timeElapsed;
            sourcePoint.Y += (Direction.Y*_velocity.Y)*timeElapsed;
            sourcePoint.Z += (Direction.Z*_velocity.Z)*timeElapsed;
        }
    }
}