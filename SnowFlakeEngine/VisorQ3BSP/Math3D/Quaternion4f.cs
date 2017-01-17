#region GPL License

/*
Copyright (c) 2010 Miguel Angel Guirado López

This file is part of Math3D.

    Math3D is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Math3D is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Math3D.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;

namespace Math3D
{
    [Serializable]
    public class Quaternion4F
    {
        private Vector3F _tempVx = new Vector3F();
        private Vector3F _tempVy = new Vector3F();
        private Vector3F _tempVz = new Vector3F();
        private float _w = 1, _x, _y, _z;

        /// <summary>
        ///     Crea un nuevo Quaternion inicializado a la identidad
        /// </summary>
        public Quaternion4F()
        {
        }

        public Quaternion4F(Vector3F v, float w)
        {
            _x = v.X;
            _y = v.X;
            _z = v.X;
            _w = w;
        }

        public Quaternion4F(float w, float x, float y, float z)
        {
            _w = w;
            _x = x;
            _y = y;
            _z = z;
        }

        public float X
        {
            get { return _x; }
            set { _x = value; }
        }

        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public float Z
        {
            get { return _z; }
            set { _z = value; }
        }

        public float W
        {
            get { return _w; }
            set { _w = value; }
        }

        public Vector3F Xyz
        {
            get { return new Vector3F(X, Y, Z); }
        }

        /// <summary>
        ///     Devuelve un nuevo Quaternion igual a la identidad
        /// </summary>
        public static Quaternion4F Identity
        {
            get
            {
                return new Quaternion4F(1.0f, 0.0f, 0.0f, 0.0f);
                ;
            }
        }

        #region public float LengthSquared

        /// <summary>
        ///     Gets the square of the quaternion length (magnitude).
        /// </summary>
        public float LengthSquared
        {
            get
            {
                var v = new Vector3F(X, Y, Z);
                return W*W + v.LengthSquared;
            }
        }

        #endregion

        public void SetIdentity()
        {
            X = 0;
            Y = 0;
            Z = 0;
            W = 1;
        }

        public void Rotation(float radians, Vector3F axis)
        {
            Rotation(radians, axis.X, axis.Y, axis.Z);
        }

        public void Rotation(float radians, float xEje, float yEje, float zEje)
        {
            var num2 = radians*0.5f;
            var num = (float) Math.Sin(num2);
            var num3 = (float) Math.Cos(num2);
            X = xEje*num;
            Y = yEje*num;
            Z = zEje*num;
            W = num3;
        }

        /// <summary>
        ///     this = this * q
        /// </summary>
        /// <param name="q"></param>
        public void Multiply(Quaternion4F q)
        {
            var auxX = _w*q._x + _x*q._w + _y*q._z - _z*q._y;
            var auxY = _w*q._y + _y*q._w + _z*q._x - _x*q._z;
            var auxZ = _w*q._z + _z*q._w + _x*q._y - _y*q._x;
            var auxW = _w*q._w - _x*q._x - _y*q._y - _z*q._z;

            _x = auxX;
            _y = auxY;
            _z = auxZ;
            _w = auxW;
        }

        /// <summary>
        ///     Devuelve un nuevo Quaternion que representa la concatenación de rotaciones de ambos.
        /// </summary>
        /// <param name="tempLeft"></param>
        /// <param name="tempRight"></param>
        /// <returns></returns>
        public static Quaternion4F operator *(Quaternion4F left, Quaternion4F right)
        {
            var q = new Quaternion4F();

            q.X = left.W*right.X + left.X*right.W + left.Y*right.Z - left.Z*right.Y;
            q.Y = left.W*right.Y + left.Y*right.W + left.Z*right.X - left.X*right.Z;
            q.Z = left.W*right.Z + left.Z*right.W + left.X*right.Y - left.Y*right.X;
            q.W = left.W*right.W - left.X*right.X - left.Y*right.Y - left.Z*right.Z;

            return q;
        }

        public static Quaternion4F FromDegreesAxis(float degrees, float x, float y, float z)
        {
            return FromRadiansAxis(MathHelp.RadiansFromDegrees(degrees), x, y, z);
        }

        public static Quaternion4F FromDegreesAxis(float degrees, Vector3F axis)
        {
            return FromRadiansAxis(MathHelp.RadiansFromDegrees(degrees), axis.X, axis.Y, axis.Z);
        }

        public static Quaternion4F FromRadiansAxis(float radianes, Vector3F axis)
        {
            return FromRadiansAxis(radianes, axis.X, axis.Y, axis.Z);
        }

        public static Quaternion4F FromRadiansAxis(float radianes, float x, float y, float z)
        {
            var quaternion = new Quaternion4F();
            var num2 = radianes*0.5f;
            var num = (float) Math.Sin(num2);
            var num3 = (float) Math.Cos(num2);
            quaternion._x = x*num;
            quaternion._y = y*num;
            quaternion._z = z*num;
            quaternion._w = num3;
            quaternion.Normalize();
            return quaternion;
        }

        public void FromMatrix(Matrix4F m)
        {
            m.GetRight(ref _tempVx);
            m.GetUp(ref _tempVy);
            m.GetForward(ref _tempVz);

            var trace = _tempVx.X + _tempVy.Y + _tempVz.Z + 1.0f;

            if (trace > 0.0001f)
            {
                var s = (float) (0.5/Math.Sqrt(trace));
                _w = 0.25f/s;
                _x = (_tempVy.Z - _tempVz.Y)*s;
                _y = (_tempVz.X - _tempVx.Z)*s;
                _z = (_tempVx.Y - _tempVy.X)*s;
            }
            else
            {
                if ((_tempVx.X > _tempVy.Y) && (_tempVx.X > _tempVz.Z))
                {
                    var s = (float) (0.5/Math.Sqrt(1.0f + _tempVx.X - _tempVy.Y - _tempVz.Z));
                    _x = 0.25f/s;
                    _y = (_tempVy.X + _tempVx.Y)*s;
                    _z = (_tempVz.X + _tempVx.Z)*s;
                    _w = (_tempVz.Y - _tempVy.Z)*s;
                }
                else if (_tempVy.Y > _tempVz.Z)
                {
                    var s = (float) (0.5/Math.Sqrt(1.0f + _tempVy.Y - _tempVx.X - _tempVz.Z));
                    _x = (_tempVy.X + _tempVx.Y)*s;
                    _y = 0.25f/s;
                    _z = (_tempVz.Y + _tempVy.Z)*s;
                    _w = (_tempVz.X - _tempVx.Z)*s;
                }
                else
                {
                    var s = (float) (0.5/Math.Sqrt(1.0f + _tempVz.Z - _tempVx.X - _tempVy.Y));
                    _x = (_tempVz.X + _tempVx.Z)*s;
                    _y = (_tempVz.Y + _tempVy.Z)*s;
                    _z = 0.25f/s;
                    _w = (_tempVy.X - _tempVx.Y)*s;
                }
            }
        }

        public void Normalize()
        {
            // Compute magnitude of the quaternion
            var mag = (float) Math.Sqrt((_w*_w) + (_x*_x) + (_y*_y) + (_z*_z));

            // Check for bogus length, to protect against divide by zero
            if (mag > 0.0)
            {
                // Normalize it
                var oneOverMag = 1.0f/mag;

                _w *= oneOverMag;
                _x *= oneOverMag;
                _y *= oneOverMag;
                _z *= oneOverMag;
            }
        }

        public float Dot(Quaternion4F quat)
        {
            return _w*quat._w + _x*quat._x + _y*quat._y + _z*quat._z;
        }

        public static float Dot(Quaternion4F a, Quaternion4F b)
        {
            return ((a._w*b._w) + (a._x*b._x) + (a._y*b._y) + (a._z*b._z));
        }

        public void CopyFrom(Quaternion4F q)
        {
            W = q.W;
            X = q.X;
            Y = q.Y;
            Z = q.Z;
        }

        #region Slerp

        /// <summary>
        ///     Do Spherical linear interpolation between two quaternions
        /// </summary>
        /// <param name="q1">The first quaternion</param>
        /// <param name="q2">The second quaternion</param>
        /// <param name="blend">The blend factor</param>
        /// <returns>A smooth blend between the given quaternions</returns>
        public static Quaternion4F OtkSlerp(Quaternion4F q1, Quaternion4F q2, float blend)
        {
            // if either input is zero, return the other.
            if (q1.LengthSquared == 0.0f)
            {
                if (q2.LengthSquared == 0.0f)
                {
                    return Identity;
                }
                return q2;
            }
            if (q2.LengthSquared == 0.0f)
            {
                return q1;
            }


            var cosHalfAngle = q1.W*q2.W + q1.Xyz.Dot(q2.Xyz);

            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
            {
                // angle = 0.0f, so just return one input.
                return q1;
            }
            if (cosHalfAngle < 0.0f)
            {
                q2.Xyz.Negate();
                q2.W = -q2.W;
                cosHalfAngle = -cosHalfAngle;
            }

            float blendA;
            float blendB;
            if (cosHalfAngle < 0.99f)
            {
                // do proper slerp for big angles
                var halfAngle = (float) Math.Acos(cosHalfAngle);
                var sinHalfAngle = (float) Math.Sin(halfAngle);
                var oneOverSinHalfAngle = 1.0f/sinHalfAngle;
                blendA = (float) Math.Sin(halfAngle*(1.0f - blend))*oneOverSinHalfAngle;
                blendB = (float) Math.Sin(halfAngle*blend)*oneOverSinHalfAngle;
            }
            else
            {
                // do lerp if angle is really small.
                blendA = 1.0f - blend;
                blendB = blend;
            }

            var result = new Quaternion4F(blendA*q1.Xyz + blendB*q2.Xyz, blendA*q1.W + blendB*q2.W);
            if (result.LengthSquared > 0.0f)
            {
                result.Normalize();
                return result;
            }
            return Identity;
        }

        #endregion

        public static Quaternion4F Slerp(Quaternion4F q0, Quaternion4F q1, float t)
        {
            var qresult = new Quaternion4F();

            // Check for out-of range parameter and return edge points if so
            if (t <= 0.0)
            {
                qresult.CopyFrom(q0);
                return qresult;
            }
            if (t >= 1.0)
            {
                qresult.CopyFrom(q1);
                return qresult;
            }

            // Compute "cosine of angle between quaternions" using dot product
            var cosOmega = Dot(q0, q1);

            // If negative dot, use -q1.  Two quaternions q and -q
            // represent the same rotation, but may produce
            // different slerp.  We chose q or -q to rotate using
            // the acute angle.
            var q1W = q1._w;
            var q1X = q1._x;
            var q1Y = q1._y;
            var q1Z = q1._z;

            if (cosOmega < 0.0)
            {
                q1W = -q1W;
                q1X = -q1X;
                q1Y = -q1Y;
                q1Z = -q1Z;
                cosOmega = -cosOmega;
            }

            // We should have two unit quaternions, so dot should be <= 1.0
            if (!(cosOmega < 1.1f))
                throw new Exception("Error en Quaternion.Slerp(), cosOmega > 1.1f");

            // Compute interpolation fraction, checking for quaternions
            // almost exactly the same
            float k0, k1;

            if (cosOmega > 0.9999)
            {
                // Very close - just use linear interpolation,
                // which will protect againt a divide by zero

                k0 = 1.0f - t;
                k1 = t;
            }
            else
            {
                // Compute the sin of the angle using the
                // trig identity sin^2(omega) + cos^2(omega) = 1
                var sinOmega = (float) Math.Sqrt(1.0 - (cosOmega*cosOmega));

                // Compute the angle from its sin and cosine
                var omega = (float) Math.Atan2(sinOmega, cosOmega);

                // Compute inverse of denominator, so we only have
                // to divide once
                var oneOverSinOmega = 1.0f/sinOmega;

                // Compute interpolation parameters
                k0 = (float) (Math.Sin((1.0 - t)*omega)*oneOverSinOmega);
                k1 = (float) (Math.Sin(t*omega)*oneOverSinOmega);
            }

            // Interpolate and return new quaternion
            return new Quaternion4F(
                (k0*q0._w) + (k1*q1W),
                (k0*q0._x) + (k1*q1X),
                (k0*q0._y) + (k1*q1Y),
                (k0*q0._z) + (k1*q1Z)
                );
        }
    }
}