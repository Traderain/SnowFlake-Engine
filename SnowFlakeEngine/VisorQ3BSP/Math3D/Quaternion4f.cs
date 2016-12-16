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
    public class Quaternion4f
    {
        private Vector3f tempVx = new Vector3f();
        private Vector3f tempVy = new Vector3f();
        private Vector3f tempVz = new Vector3f();
        private float w = 1, x, y, z;

        /// <summary>
        ///     Crea un nuevo Quaternion inicializado a la identidad
        /// </summary>
        public Quaternion4f()
        {
        }

        public Quaternion4f(Vector3f v, float w)
        {
            x = v.X;
            y = v.X;
            z = v.X;
            this.w = w;
        }

        public Quaternion4f(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float X
        {
            get { return x; }
            set { x = value; }
        }

        public float Y
        {
            get { return y; }
            set { y = value; }
        }

        public float Z
        {
            get { return z; }
            set { z = value; }
        }

        public float W
        {
            get { return w; }
            set { w = value; }
        }

        public Vector3f Xyz
        {
            get { return new Vector3f(X, Y, Z); }
        }

        /// <summary>
        ///     Devuelve un nuevo Quaternion igual a la identidad
        /// </summary>
        public static Quaternion4f Identity
        {
            get
            {
                return new Quaternion4f(1.0f, 0.0f, 0.0f, 0.0f);
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
                var v = new Vector3f(X, Y, Z);
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

        public void Rotation(float radians, Vector3f axis)
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
        public void Multiply(Quaternion4f q)
        {
            var auxX = w*q.x + x*q.w + y*q.z - z*q.y;
            var auxY = w*q.y + y*q.w + z*q.x - x*q.z;
            var auxZ = w*q.z + z*q.w + x*q.y - y*q.x;
            var auxW = w*q.w - x*q.x - y*q.y - z*q.z;

            x = auxX;
            y = auxY;
            z = auxZ;
            w = auxW;
        }

        /// <summary>
        ///     Devuelve un nuevo Quaternion que representa la concatenación de rotaciones de ambos.
        /// </summary>
        /// <param name="tempLeft"></param>
        /// <param name="tempRight"></param>
        /// <returns></returns>
        public static Quaternion4f operator *(Quaternion4f left, Quaternion4f right)
        {
            var q = new Quaternion4f();

            q.X = left.W*right.X + left.X*right.W + left.Y*right.Z - left.Z*right.Y;
            q.Y = left.W*right.Y + left.Y*right.W + left.Z*right.X - left.X*right.Z;
            q.Z = left.W*right.Z + left.Z*right.W + left.X*right.Y - left.Y*right.X;
            q.W = left.W*right.W - left.X*right.X - left.Y*right.Y - left.Z*right.Z;

            return q;
        }

        public static Quaternion4f FromDegreesAxis(float degrees, float x, float y, float z)
        {
            return FromRadiansAxis(MathHelp.RadiansFromDegrees(degrees), x, y, z);
        }

        public static Quaternion4f FromDegreesAxis(float degrees, Vector3f axis)
        {
            return FromRadiansAxis(MathHelp.RadiansFromDegrees(degrees), axis.X, axis.Y, axis.Z);
        }

        public static Quaternion4f FromRadiansAxis(float radianes, Vector3f axis)
        {
            return FromRadiansAxis(radianes, axis.X, axis.Y, axis.Z);
        }

        public static Quaternion4f FromRadiansAxis(float radianes, float x, float y, float z)
        {
            var quaternion = new Quaternion4f();
            var num2 = radianes*0.5f;
            var num = (float) Math.Sin(num2);
            var num3 = (float) Math.Cos(num2);
            quaternion.x = x*num;
            quaternion.y = y*num;
            quaternion.z = z*num;
            quaternion.w = num3;
            quaternion.Normalize();
            return quaternion;
        }

        public void FromMatrix(Matrix4f m)
        {
            m.GetRight(ref tempVx);
            m.GetUp(ref tempVy);
            m.GetForward(ref tempVz);

            var trace = tempVx.X + tempVy.Y + tempVz.Z + 1.0f;

            if (trace > 0.0001f)
            {
                var s = (float) (0.5/Math.Sqrt(trace));
                w = 0.25f/s;
                x = (tempVy.Z - tempVz.Y)*s;
                y = (tempVz.X - tempVx.Z)*s;
                z = (tempVx.Y - tempVy.X)*s;
            }
            else
            {
                if ((tempVx.X > tempVy.Y) && (tempVx.X > tempVz.Z))
                {
                    var s = (float) (0.5/Math.Sqrt(1.0f + tempVx.X - tempVy.Y - tempVz.Z));
                    x = 0.25f/s;
                    y = (tempVy.X + tempVx.Y)*s;
                    z = (tempVz.X + tempVx.Z)*s;
                    w = (tempVz.Y - tempVy.Z)*s;
                }
                else if (tempVy.Y > tempVz.Z)
                {
                    var s = (float) (0.5/Math.Sqrt(1.0f + tempVy.Y - tempVx.X - tempVz.Z));
                    x = (tempVy.X + tempVx.Y)*s;
                    y = 0.25f/s;
                    z = (tempVz.Y + tempVy.Z)*s;
                    w = (tempVz.X - tempVx.Z)*s;
                }
                else
                {
                    var s = (float) (0.5/Math.Sqrt(1.0f + tempVz.Z - tempVx.X - tempVy.Y));
                    x = (tempVz.X + tempVx.Z)*s;
                    y = (tempVz.Y + tempVy.Z)*s;
                    z = 0.25f/s;
                    w = (tempVy.X - tempVx.Y)*s;
                }
            }
        }

        public void Normalize()
        {
            // Compute magnitude of the quaternion
            var mag = (float) Math.Sqrt((w*w) + (x*x) + (y*y) + (z*z));

            // Check for bogus length, to protect against divide by zero
            if (mag > 0.0)
            {
                // Normalize it
                var oneOverMag = 1.0f/mag;

                w *= oneOverMag;
                x *= oneOverMag;
                y *= oneOverMag;
                z *= oneOverMag;
            }
        }

        public float Dot(Quaternion4f quat)
        {
            return w*quat.w + x*quat.x + y*quat.y + z*quat.z;
        }

        public static float Dot(Quaternion4f a, Quaternion4f b)
        {
            return ((a.w*b.w) + (a.x*b.x) + (a.y*b.y) + (a.z*b.z));
        }

        public void CopyFrom(Quaternion4f q)
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
        public static Quaternion4f OtkSlerp(Quaternion4f q1, Quaternion4f q2, float blend)
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

            var result = new Quaternion4f(blendA*q1.Xyz + blendB*q2.Xyz, blendA*q1.W + blendB*q2.W);
            if (result.LengthSquared > 0.0f)
            {
                result.Normalize();
                return result;
            }
            return Identity;
        }

        #endregion

        public static Quaternion4f Slerp(Quaternion4f q0, Quaternion4f q1, float t)
        {
            var qresult = new Quaternion4f();

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
            var q1w = q1.w;
            var q1x = q1.x;
            var q1y = q1.y;
            var q1z = q1.z;

            if (cosOmega < 0.0)
            {
                q1w = -q1w;
                q1x = -q1x;
                q1y = -q1y;
                q1z = -q1z;
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
            return new Quaternion4f(
                (k0*q0.w) + (k1*q1w),
                (k0*q0.x) + (k1*q1x),
                (k0*q0.y) + (k1*q1y),
                (k0*q0.z) + (k1*q1z)
                );
        }
    }
}