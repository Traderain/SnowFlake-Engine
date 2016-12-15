
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
        float w = 1, x = 0, y = 0, z = 0;

        Vector3f tempVx = new Vector3f();
        Vector3f tempVy = new Vector3f();
        Vector3f tempVz = new Vector3f();
        /// <summary>
        /// Crea un nuevo Quaternion inicializado a la identidad
        /// </summary>
        public Quaternion4f()
        {
        }
        public Quaternion4f(Vector3f v, float w)
        {
            this.x = v.X;
            this.y = v.X;
            this.z = v.X;
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

        public void SetIdentity()
        {
            this.X = 0; this.Y = 0; this.Z = 0; this.W = 1;
        }
        /// <summary>
        /// Devuelve un nuevo Quaternion igual a la identidad
        /// </summary>
        public static Quaternion4f Identity
        {
            get
            {
                return new Quaternion4f(1.0f, 0.0f, 0.0f, 0.0f); ;
            }
        }

        public void Rotation(float radians, Vector3f axis)
        {
            Rotation(radians, axis.X, axis.Y, axis.Z);
        }

        public void Rotation(float radians, float xEje, float yEje, float zEje)
        {
            float num2 = radians * 0.5f;
            float num = (float)System.Math.Sin((double)num2);
            float num3 = (float)System.Math.Cos((double)num2);
            this.X = xEje * num;
            this.Y = yEje * num;
            this.Z = zEje * num;
            this.W = num3;
        }
        /// <summary>
        /// this = this * q
        /// </summary>
        /// <param name="q"></param>
        public void Multiply(Quaternion4f q)
        {
            float auxX = this.w * q.x + this.x * q.w + this.y * q.z - this.z * q.y;
            float auxY = this.w * q.y + this.y * q.w + this.z * q.x - this.x * q.z;
            float auxZ = this.w * q.z + this.z * q.w + this.x * q.y - this.y * q.x;
            float auxW = this.w * q.w - this.x * q.x - this.y * q.y - this.z * q.z;

            this.x = auxX;
            this.y = auxY;
            this.z = auxZ;
            this.w = auxW;
        }
        /// <summary>
        /// Devuelve un nuevo Quaternion que representa la concatenación de rotaciones de ambos.
        /// </summary>
        /// <param name="tempLeft"></param>
        /// <param name="tempRight"></param>
        /// <returns></returns>
        public static Quaternion4f operator *(Quaternion4f left, Quaternion4f right)
        {
            Quaternion4f q = new Quaternion4f();

            q.X = left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y;
            q.Y = left.W * right.Y + left.Y * right.W + left.Z * right.X - left.X * right.Z;
            q.Z = left.W * right.Z + left.Z * right.W + left.X * right.Y - left.Y * right.X;
            q.W = left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z;

            return q;
        }

        public static Quaternion4f FromDegreesAxis(float degrees, float x, float y, float z)
        {
            return Quaternion4f.FromRadiansAxis(MathHelp.RadiansFromDegrees(degrees), x, y, z);
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
            Quaternion4f quaternion = new Quaternion4f();
            float num2 = radianes * 0.5f;
            float num = (float)System.Math.Sin((double)num2);
            float num3 = (float)System.Math.Cos((double)num2);
            quaternion.x = x * num;
            quaternion.y = y * num;
            quaternion.z = z * num;
            quaternion.w = num3;
            quaternion.Normalize();
            return quaternion;
        }

        public void FromMatrix(Matrix4f m)
        {
            m.GetRight(ref tempVx);
            m.GetUp(ref tempVy);
            m.GetForward(ref tempVz);

            float trace = tempVx.X + tempVy.Y + tempVz.Z + 1.0f;

            if (trace > 0.0001f)
            {
                float s = (float)(0.5 / Math.Sqrt(trace));
                w = 0.25f / s;
                x = (tempVy.Z - tempVz.Y) * s;
                y = (tempVz.X - tempVx.Z) * s;
                z = (tempVx.Y - tempVy.X) * s;
            }
            else
            {
                if ((tempVx.X > tempVy.Y) && (tempVx.X > tempVz.Z))
                {
                    float s = (float)(0.5 / Math.Sqrt(1.0f + tempVx.X - tempVy.Y - tempVz.Z));
                    x = 0.25f / s;
                    y = (tempVy.X + tempVx.Y) * s;
                    z = (tempVz.X + tempVx.Z) * s;
                    w = (tempVz.Y - tempVy.Z) * s;
                }
                else if (tempVy.Y > tempVz.Z)
                {
                    float s = (float)(0.5 / Math.Sqrt(1.0f + tempVy.Y - tempVx.X - tempVz.Z));
                    x = (tempVy.X + tempVx.Y) * s;
                    y = 0.25f / s;
                    z = (tempVz.Y + tempVy.Z) * s;
                    w = (tempVz.X - tempVx.Z) * s;
                }
                else
                {
                    float s = (float)(0.5 / Math.Sqrt(1.0f + tempVz.Z - tempVx.X - tempVy.Y));
                    x = (tempVz.X + tempVx.Z) * s;
                    y = (tempVz.Y + tempVy.Z) * s;
                    z = 0.25f / s;
                    w = (tempVy.X - tempVx.Y) * s;
                }
            }
        }

        public void Normalize()
        {
            // Compute magnitude of the quaternion
            float mag = (float)System.Math.Sqrt((w * w) + (x * x) + (y * y) + (z * z));

            // Check for bogus length, to protect against divide by zero
            if (mag > 0.0)
            {
                // Normalize it
                float oneOverMag = 1.0f / mag;

                w *= oneOverMag;
                x *= oneOverMag;
                y *= oneOverMag;
                z *= oneOverMag;
            }
        }

        public float Dot(Quaternion4f quat)
        {
            return this.w * quat.w + this.x * quat.x + this.y * quat.y + this.z * quat.z;
        }

        public static float Dot(Quaternion4f a, Quaternion4f b)
        {
            return ((a.w * b.w) + (a.x * b.x) + (a.y * b.y) + (a.z * b.z));
        }

        public void CopyFrom(Quaternion4f q)
        {
            this.W = q.W;
            this.X = q.X;
            this.Y = q.Y;
            this.Z = q.Z;
        }

        #region public float LengthSquared

        /// <summary>
        /// Gets the square of the quaternion length (magnitude).
        /// </summary>
        public float LengthSquared
        {
            get
            {
                Vector3f v = new Vector3f(X, Y, Z);
                return W * W + v.LengthSquared;
            }
        }

        #endregion

        #region Slerp

        /// <summary>
        /// Do Spherical linear interpolation between two quaternions 
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
            else if (q2.LengthSquared == 0.0f)
            {
                return q1;
            }


            float cosHalfAngle = q1.W * q2.W + q1.Xyz.Dot(q2.Xyz);

            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
            {
                // angle = 0.0f, so just return one input.
                return q1;
            }
            else if (cosHalfAngle < 0.0f)
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
                float halfAngle = (float)System.Math.Acos(cosHalfAngle);
                float sinHalfAngle = (float)System.Math.Sin(halfAngle);
                float oneOverSinHalfAngle = 1.0f / sinHalfAngle;
                blendA = (float)System.Math.Sin(halfAngle * (1.0f - blend)) * oneOverSinHalfAngle;
                blendB = (float)System.Math.Sin(halfAngle * blend) * oneOverSinHalfAngle;
            }
            else
            {
                // do lerp if angle is really small.
                blendA = 1.0f - blend;
                blendB = blend;
            }

            Quaternion4f result = new Quaternion4f(blendA * q1.Xyz + blendB * q2.Xyz, blendA * q1.W + blendB * q2.W);
            if (result.LengthSquared > 0.0f)
            {
                result.Normalize();
                return result;
            }
            else
                return Identity;
        }

        #endregion

        public static Quaternion4f Slerp(Quaternion4f q0, Quaternion4f q1, float t)
        {
            Quaternion4f qresult = new Quaternion4f();

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
            float cosOmega = Quaternion4f.Dot(q0, q1);

            // If negative dot, use -q1.  Two quaternions q and -q
            // represent the same rotation, but may produce
            // different slerp.  We chose q or -q to rotate using
            // the acute angle.
            float q1w = q1.w;
            float q1x = q1.x;
            float q1y = q1.y;
            float q1z = q1.z;

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
                float sinOmega = (float)System.Math.Sqrt(1.0 - (cosOmega * cosOmega));

                // Compute the angle from its sin and cosine
                float omega = (float)System.Math.Atan2(sinOmega, cosOmega);

                // Compute inverse of denominator, so we only have
                // to divide once
                float oneOverSinOmega = 1.0f / sinOmega;

                // Compute interpolation parameters
                k0 = (float)(System.Math.Sin((1.0 - t) * omega) * oneOverSinOmega);
                k1 = (float)(System.Math.Sin(t * omega) * oneOverSinOmega);
            }

            // Interpolate and return new quaternion
            return new Quaternion4f(
              (k0 * q0.w) + (k1 * q1w),
              (k0 * q0.x) + (k1 * q1x),
              (k0 * q0.y) + (k1 * q1y),
              (k0 * q0.z) + (k1 * q1z)
             );
        }
    }
}

