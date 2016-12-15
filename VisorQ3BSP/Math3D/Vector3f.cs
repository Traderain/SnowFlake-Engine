
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
using System.Collections.Generic;

namespace Math3D
{
    [Serializable]
    public class Vector3f
    {
        public float[] values = new float[3];
        
        #region Constructores
        
        public Vector3f()
        {
        }
        public Vector3f(float x, float y, float z)
        {
            values[0] = x;
            values[1] = y;
            values[2] = z;
        }
		#endregion Constructores
        
        #region Propiedades
        
        public float X
        {
            get { return values[0]; }
            set { values[0] = value; }
        }

        public float Y
        {
            get { return values[1]; }
            set { values[1] = value; }
        }

        public float Z
        {
            get { return values[2]; }
            set { values[2] = value; }
        }
        #endregion Propiedades
 		
        #region Operadores
        
        public static Vector3f operator +(Vector3f left, Vector3f right)
        {
            return new Vector3f(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static Vector3f operator -(Vector3f left, Vector3f right)
        {
            return new Vector3f(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static Vector3f operator *(float scalar, Vector3f right)
        {
            return new Vector3f(right.X * scalar, right.Y * scalar, right.Z * scalar);
        }

        public static Vector3f operator +(float scalar, Vector3f right)
        {
            return new Vector3f(right.X + scalar, right.Y + scalar, right.Z + scalar);
        }

        public static Vector3f operator *(Vector3f left, float scalar)
        {
            return new Vector3f(left.X * scalar, left.Y * scalar, left.Z * scalar);
        }
        public static Vector3f operator -(Vector3f vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            vec.Z = -vec.Z;
            return vec;
        }
        #endregion Operadores

        #region Methods

        /// <summary>
        /// Devuelve el ángulo entre este vector y el vector parámetro. Ambos vectores deben estar normalizados.
        /// <para>Si Dot > 0, el ángulo es menor de 90º</para>
        /// <para>Si Dot < 0, el ángulo es mayor de 90º</para>
        /// <para>Si Dot == 0, el ángulo es igual a 90º (son perpendiculares)</para>
        /// <para>Si Dot == 1, el ángulo es 0º (son paralelos)</para>
        /// <para>Si Dot == 1, el ángulo es de 180º (son paralelos y opuestos)</para>
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public float Dot(Vector3f vector)
        {
            double dotProduct = 0.0f;

            dotProduct += this.X * vector.X;
            dotProduct += this.Y * vector.Y;
            dotProduct += this.Z * vector.Z;

            return (float)dotProduct;
        }
        /// <summary>
        /// Caclulate the cross (vector) product of two vectors
        /// </summary>
        /// <param name="tempLeft">First operand</param>
        /// <param name="tempRight">Second operand</param>
        /// <returns>The cross product of the two inputs</returns>
        /// <param name="result">The cross product of the two inputs</param>
        public static void Cross(Vector3f left, Vector3f right, ref Vector3f result)
        {
            result.X = left.Y * right.Z - left.Z * right.Y;
            result.Y = left.Z * right.X - left.X * right.Z;
            result.Z = left.X * right.Y - left.Y * right.X;
        }
        /// <summary>
        /// Producto cruz entre este vector y v3param, el resultado se almacena en este mismo vector.
        /// <para>this = this (cross) v3param</para>
        /// </summary>
        /// <param name="v3param"></param>
        public void Cross(Vector3f v3param)
        {
            Vector3f cross = new Vector3f();

            float auxX = (this.Y * v3param.Z) - (this.Z * v3param.Y);
            float auxY = (this.Z * v3param.X) - (this.X * v3param.Z);
            float auxZ = (this.X * v3param.Y) - (this.Y * v3param.X);

            this.X = auxX;
            this.Y = auxY;
            this.Z = auxZ;
        }
        public void CopyFrom(Vector3f v3source)
        {
            this.X = v3source.X;
            this.Y = v3source.Y;
            this.Z = v3source.Z;
        }
        public void Normalize()
        {
            float length = (float)System.Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);

            // Will also work for zero-sized vectors, but will change nothing
            if (length > float.Epsilon)
            {
                float inverseLength = 1.0f / length;

                this.X *= inverseLength;
                this.Y *= inverseLength;
                this.Z *= inverseLength;
            }
        }
        /// <summary>
        ///    Calculates a reflection vector to the plane with the given normal.
        /// </summary>
        /// <remarks>
        ///    Assumes this vector is pointing AWAY from the plane, invert if not.
        /// </remarks>
        /// <param name="normal">Normal vector on which this vector will be reflected.</param>
        /// <returns></returns>
        public Vector3f Reflect(Vector3f normal)
        {
            return this - (2 * this.Dot(normal) * normal);
        }

        public float Length
        {
            get
            {
                return (float)System.Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
            }
        }

        public float LengthSquared
        {
            get
            {
                return (this.X * this.X + this.Y * this.Y + this.Z * this.Z);
            }
        }

        public override string ToString()
        {
            return string.Format("Vector3({0}, {1}, {2})", this.X, this.Y, this.Z);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3f)
                return (this == (Vector3f)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() ^ (~Z.GetHashCode()));
        }
        /// <summary>
        /// Suma este vector con el vector argumento, el resultado es este mismo vector.
        /// this += v3
        /// </summary>
        /// <param name="v3"></param>
        public void Sum(Vector3f v3)
        {
            this.X += v3.X;
            this.Y += v3.Y;
            this.Z += v3.Z;
        }
        /// <summary>
        /// Resta este vector con el vector argumento, el resultado es este mismo vector.
        /// this -= v3
        /// </summary>
        /// <param name="v3"></param>
        public void Subtract(Vector3f v3)
        {
            this.X -= v3.X;
            this.Y -= v3.Y;
            this.Z -= v3.Z;
        }
        public void Subtract(float vx, float vy, float vz)
        {
            this.X -= vx;
            this.Y -= vy;
            this.Z -= vz;
        }
        /// <summary>
        /// this = -this
        /// </summary>
        public void Negate()
        {
            this.X = -this.X;
            this.Y = -this.Y;
            this.Z = -this.Z;
        }

        public static double Distance(Vector3f v1, Vector3f v2)
        {
            return
            (
               Math.Sqrt
               (
                   (v1.X - v2.X) * (v1.X - v2.X) +
                   (v1.Y - v2.Y) * (v1.Y - v2.Y) +
                   (v1.Z - v2.Z) * (v1.Z - v2.Z)
               )
            );
        }

        public double Distance(Vector3f other)
        {
            return Distance(this, other);
        }
        /// <summary>
        /// Devuelve el ángulo (en grados) entre v1 y v2. 
        /// <para>Ambos vectores se normalizan y quedan modificados.</para>
        /// <para>Si uno de los vectores o los dos valen cero, devuelve 90º.</para>
        /// <para>El ángulo devuelto varia entre 0 y 180 grados</para>
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double Angle(Vector3f v1, Vector3f v2)
        {
            v1.Normalize();
            v2.Normalize();
            float rdot = v1.Dot(v2);
            double result = Math.Acos(rdot);
            return MathHelp.DegreesFromRadians((float)result);
        }
        /// <summary>
        /// Devuelve el ángulo (en grados) entre este vector y el vector parámetro.
        /// <para>Ambos vectores se normalizan y quedan modificados.</para>
        /// <para>Si uno de los vectores o los dos valen cero, devuelve 90º.</para>
        /// <para>El ángulo devuelto varia entre 0 y 180 grados</para>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double Angle(Vector3f other)
        {
            return Angle(this, other);
        }        
        #endregion Methods
    }
}
