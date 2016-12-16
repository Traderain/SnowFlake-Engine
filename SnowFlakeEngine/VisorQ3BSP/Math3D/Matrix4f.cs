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
    public class Matrix4f
    {
        private static readonly Matrix4f auxMat4 = new Matrix4f();
        private static Matrix4f resultadoMult = new Matrix4f();
        // Quaternion temporal
        private readonly Quaternion4f tempQRot = new Quaternion4f();
        private Vector3f tempV3 = new Vector3f();

        public float[] values = new float[16]
        {
            1, 0, 0, 0, // Right        (values[0]..values[3]
            0, 1, 0, 0, // Up           (values[4]..values[7]
            0, 0, 1, 0, // Forward      (values[8]..values[11]
            0, 0, 0, 1
        }; // Translation  (values[12]..values[15]

        /// <summary>
        ///     Establece los ejes unitarios de esta matriz según la rotación dada por los grados alto ejes indicados.
        /// </summary>
        /// <param name="grados"></param>
        /// <param name="ejeRotacion"></param>
        public virtual void Rotation(float grados, Vector3f ejeRotacion)
        {
            Rotation(grados, ejeRotacion.X, ejeRotacion.Y, ejeRotacion.Z);
        }

        /// <summary>
        ///     Establece los ejes unitarios de esta matriz según la rotación dada por los grados alto ejes indicados.
        /// </summary>
        /// <param name="grados"></param>
        /// <param name="ejeX"></param>
        /// <param name="ejeY"></param>
        /// <param name="ejeZ"></param>
        public virtual void Rotation(float grados, float ejeX, float ejeY, float ejeZ)
        {
            GetTranslation(ref tempV3);
            tempQRot.SetIdentity();
            tempQRot.Rotation(MathHelp.RadiansFromDegrees(grados), ejeX, ejeY, ejeZ);
            //tempQRot.GetMatriz4(ref auxMat4);
            //this.CopiaDesde(auxMat4);
            FromQuaternion(tempQRot);
            Translation3f = tempV3;
        }

        /// <summary>
        ///     Multiplica esta matriz (this) por la representada por el quaternion pasado como parámetro.
        ///     <para>this = this * Quaternion</para>
        /// </summary>
        /// <param name="qRot"></param>
        public virtual void Rotate(Quaternion4f qRot)
        {
            //qRot.GetMatriz4(ref auxMat4);
            auxMat4.FromQuaternion(qRot);
            MultipliesBy(auxMat4);
        }

        /// <summary>
        ///     Multiplica esta matriz por la pasada como parámetro, el mat4result se almacena en esta.
        ///     <para>this = this * mat4</para>
        /// </summary>
        /// <param name="mat"></param>
        public virtual void MultipliesBy(Matrix4f mat4)
        {
            MultipliesBy(mat4, ref resultadoMult);
            CopiaDesde(resultadoMult);
        }

        /// <summary>
        ///     Multiplica esta matriz (this) por la pasada como parámetro (mat4), el parámetro matriz (mat4result) es el
        ///     resultado.
        ///     <para>mat4result = this * mat4</para>
        /// </summary>
        /// <param name="mat4"></param>
        /// <param name="mat4result"></param>
        public virtual void MultipliesBy(Matrix4f mat4, ref Matrix4f mat4result)
        {
            mat4result.SetIdentity();

            mat4result.values[00] = values[00]*mat4.values[00] + values[04]*mat4.values[01] + values[08]*mat4.values[02] +
                                    values[12]*mat4.values[03];
            mat4result.values[01] = values[01]*mat4.values[00] + values[05]*mat4.values[01] + values[09]*mat4.values[02] +
                                    values[13]*mat4.values[03];
            mat4result.values[02] = values[02]*mat4.values[00] + values[06]*mat4.values[01] + values[10]*mat4.values[02] +
                                    values[14]*mat4.values[03];
            mat4result.values[03] = values[03]*mat4.values[00] + values[07]*mat4.values[01] + values[11]*mat4.values[02] +
                                    values[15]*mat4.values[03];

            mat4result.values[04] = values[00]*mat4.values[04] + values[04]*mat4.values[05] + values[08]*mat4.values[06] +
                                    values[12]*mat4.values[07];
            mat4result.values[05] = values[01]*mat4.values[04] + values[05]*mat4.values[05] + values[09]*mat4.values[06] +
                                    values[13]*mat4.values[07];
            mat4result.values[06] = values[02]*mat4.values[04] + values[06]*mat4.values[05] + values[10]*mat4.values[06] +
                                    values[14]*mat4.values[07];
            mat4result.values[07] = values[03]*mat4.values[04] + values[07]*mat4.values[05] + values[11]*mat4.values[06] +
                                    values[15]*mat4.values[07];

            mat4result.values[08] = values[00]*mat4.values[08] + values[04]*mat4.values[09] + values[08]*mat4.values[10] +
                                    values[12]*mat4.values[11];
            mat4result.values[09] = values[01]*mat4.values[08] + values[05]*mat4.values[09] + values[09]*mat4.values[10] +
                                    values[13]*mat4.values[11];
            mat4result.values[10] = values[02]*mat4.values[08] + values[06]*mat4.values[09] + values[10]*mat4.values[10] +
                                    values[14]*mat4.values[11];
            mat4result.values[11] = values[03]*mat4.values[08] + values[07]*mat4.values[09] + values[11]*mat4.values[10] +
                                    values[15]*mat4.values[11];

            mat4result.values[12] = values[00]*mat4.values[12] + values[04]*mat4.values[13] + values[08]*mat4.values[14] +
                                    values[12]*mat4.values[15];
            mat4result.values[13] = values[01]*mat4.values[12] + values[05]*mat4.values[13] + values[09]*mat4.values[14] +
                                    values[13]*mat4.values[15];
            mat4result.values[14] = values[02]*mat4.values[12] + values[06]*mat4.values[13] + values[10]*mat4.values[14] +
                                    values[14]*mat4.values[15];
            mat4result.values[15] = values[03]*mat4.values[12] + values[07]*mat4.values[13] + values[11]*mat4.values[14] +
                                    values[15]*mat4.values[15];
        }

        /// <summary>
        ///     CopiaDesde los valores del array de la matriz pasada como parámetro al array de esta.
        /// </summary>
        /// <param name="mat4"></param>
        public void CopiaDesde(Matrix4f mat)
        {
            values[00] = mat.values[00];
            values[04] = mat.values[04];
            values[08] = mat.values[08];
            values[12] = mat.values[12];
            values[01] = mat.values[01];
            values[05] = mat.values[05];
            values[09] = mat.values[09];
            values[13] = mat.values[13];
            values[02] = mat.values[02];
            values[06] = mat.values[06];
            values[10] = mat.values[10];
            values[14] = mat.values[14];
            values[03] = mat.values[03];
            values[07] = mat.values[07];
            values[11] = mat.values[11];
            values[15] = mat.values[15];
        }

        /// <summary>
        ///     pV = this * pV
        /// </summary>
        /// <param name="pV"></param>
        public virtual void TransformVector(ref Vector3f pV)
        {
            float auxX, auxY, auxZ;

            var inverseW = 1.0f/(values[3] + values[7] + values[11] + values[15]);
            auxX = ((values[0]*pV.X) + (values[4]*pV.Y) + (values[8]*pV.Z) + values[12])*inverseW;
            auxY = ((values[1]*pV.X) + (values[5]*pV.Y) + (values[9]*pV.Z) + values[13])*inverseW;
            auxZ = ((values[2]*pV.X) + (values[6]*pV.Y) + (values[10]*pV.Z) + values[14])*inverseW;

            pV.X = auxX;
            pV.Y = auxY;
            pV.Z = auxZ;
        }

        /// <summary>
        ///     Copia los valores de esta matriz desde la matriz pasada como parámetro
        /// </summary>
        /// <param Name="matrix"></param>
        public void CopyFrom(Matrix4f matrix)
        {
            matrix.values.CopyTo(values, 0);
        }

        public void FromQuaternion(Quaternion4f q)
        {
            SetIdentity();

            // Compute a few values to optimize common subexpressions
            var ww = 2.0f*q.W;
            var xx = 2.0f*q.X;
            var yy = 2.0f*q.Y;
            var zz = 2.0f*q.Z;

            // Set the matrix elements.  There is still a little more
            // opportunity for optimization due to the many common
            // subexpressions.  We'll let the compiler handle that...
            values[0] = 1.0f - (yy*q.Y) - (zz*q.Z);
            values[1] = (xx*q.Y) + (ww*q.Z);
            values[2] = (xx*q.Z) - (ww*q.Y);

            values[4] = (xx*q.Y) - (ww*q.Z);
            values[5] = 1.0f - (xx*q.X) - (zz*q.Z);
            values[6] = (yy*q.Z) + (ww*q.X);

            values[8] = (xx*q.Z) + (ww*q.Y);
            values[9] = (yy*q.Z) - (ww*q.X);
            values[10] = 1.0f - (xx*q.X) - (yy*q.Y);
        }

        #region Constructor

        /// <summary>
        ///     Crea una nueva instancia inicializada a la matriz identidad
        /// </summary>
        public Matrix4f()
        {
        }

        /// <summary>
        ///     Crea una nueva instancia inicializados sus valores a los valores de la matriz parámetro
        /// </summary>
        /// <param Name="matrix"></param>
        public Matrix4f(Matrix4f matrix)
        {
            CopyFrom(matrix);
        }

        #endregion Constructor

        #region SetIdentity() & SetZero()

        public void SetIdentity()
        {
            values[00] = 1;
            values[01] = 0;
            values[02] = 0;
            values[03] = 0;
            values[04] = 0;
            values[05] = 1;
            values[06] = 0;
            values[07] = 0;
            values[08] = 0;
            values[09] = 0;
            values[10] = 1;
            values[11] = 0;
            values[12] = 0;
            values[13] = 0;
            values[14] = 0;
            values[15] = 1;
        }

        public void SetZero()
        {
            values[00] = 0;
            values[01] = 0;
            values[02] = 0;
            values[03] = 0;
            values[04] = 0;
            values[05] = 0;
            values[06] = 0;
            values[07] = 0;
            values[08] = 0;
            values[09] = 0;
            values[10] = 0;
            values[11] = 0;
            values[12] = 0;
            values[13] = 0;
            values[14] = 0;
            values[15] = 0;
        }

        #endregion SetIdentity() & SetZero()

        #region Propiedades

        #region Set-Get Vectores Unitarios (X, Y, Z)

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector3f" /> que corresponde al vector unitario X de esta matriz
        ///     Set: Establece los valores del vector unitario X de esta matriz
        /// </summary>
        public Vector3f Right3f
        {
            get { return new Vector3f(values[0], values[1], values[2]); }
            set
            {
                values[0] = value.X;
                values[1] = value.Y;
                values[2] = value.Z;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector3f" /> que corresponde al vector unitario Y de esta matriz
        ///     Set: Establece los valores del vector unitario Y de esta matriz
        /// </summary>
        public Vector3f Up3f
        {
            get { return new Vector3f(values[4], values[5], values[6]); }
            set
            {
                values[4] = value.X;
                values[5] = value.Y;
                values[6] = value.Z;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector3f" /> que corresponde al vector unitario Z de esta matriz
        ///     Set: Establece los valores del vector unitario Z de esta matriz
        /// </summary>
        public Vector3f Forward3f
        {
            get { return new Vector3f(values[8], values[9], values[10]); }
            set
            {
                values[8] = value.X;
                values[9] = value.Y;
                values[10] = value.Z;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector4f" /> que corresponde al vector unitario X de esta matriz
        ///     Set: Establece los valores del vector unitario X de esta matriz
        /// </summary>
        public Vector4f Right4f
        {
            get { return new Vector4f(values[0], values[1], values[2], values[3]); }
            set
            {
                values[0] = value.X;
                values[1] = value.Y;
                values[2] = value.Z;
                values[3] = value.W;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector4f" /> que corresponde al vector unitario Y de esta matriz
        ///     Set: Establece los valores del vector unitario Y de esta matriz
        /// </summary>
        public Vector4f Up4f
        {
            get { return new Vector4f(values[4], values[5], values[6], values[7]); }
            set
            {
                values[4] = value.X;
                values[5] = value.Y;
                values[6] = value.Z;
                values[7] = value.W;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector4f" /> que corresponde al vector unitario Z de esta matriz
        ///     Set: Establece los valores del vector unitario Z de esta matriz
        /// </summary>
        public Vector4f Forward4f
        {
            get { return new Vector4f(values[8], values[9], values[10], values[11]); }
            set
            {
                values[8] = value.X;
                values[9] = value.Y;
                values[10] = value.Z;
                values[11] = value.W;
            }
        }

        #endregion Set-Get Vectores Unitarios (X, Y, Z)

        #region Set-Get Translation

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector3f" /> que corresponde al vector de traslación de esta matriz
        ///     Set: Establece los valores del vector traslación de esta matriz
        /// </summary>
        public Vector3f Translation3f
        {
            get { return new Vector3f(values[12], values[13], values[14]); }
            set
            {
                values[12] = value.X;
                values[13] = value.Y;
                values[14] = value.Z;
                values[15] = 1f;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector4f" /> que corresponde al vector de traslación de esta matriz
        ///     Set: Establece los valores del vector traslación de esta matriz
        /// </summary>
        public Vector4f Translation4f
        {
            get { return new Vector4f(values[12], values[13], values[14], values[15]); }
            set
            {
                values[12] = value.X;
                values[13] = value.Y;
                values[14] = value.Z;
                values[15] = value.W;
            }
        }

        #endregion Set-Get Translation

        #region Set-Get SetScalation

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector3f" /> que corresponde al vector de escalado de esta matriz
        ///     Set: Establece los valores del vector de escalado de esta matriz
        /// </summary>
        public Vector3f Scalation3f
        {
            get { return new Vector3f(values[0], values[5], values[10]); }
            set
            {
                values[0] = value.X;
                values[5] = value.Y;
                values[10] = value.Z;
            }
        }

        #endregion Set-Get SetScalation

        #endregion Propiedades

        #region Set-Vectores unitarios (X, Y, Z) y translación

        /// <summary>
        ///     Establece la porción de la matriz que corresponde al vector unitario X, segun (x,y,z).
        /// </summary>
        /// <param name="ancho"></param>
        /// <param name="alto"></param>
        /// <param name="Z"></param>
        /// <param name="w"></param>
        public virtual void SetRight(float x, float y, float z)
        {
            SetRight(x, y, z, 0);
        }

        /// <summary>
        ///     Establece la porción de la matriz que corresponde al vector unitario X, segun (x,y,z,w).
        /// </summary>
        /// <param name="ancho"></param>
        /// <param name="alto"></param>
        /// <param name="Z"></param>
        /// <param name="w"></param>
        public virtual void SetRight(float x, float y, float z, float w)
        {
            values[0] = x;
            values[1] = y;
            values[2] = z;
            values[3] = w;
        }

        /// <summary>
        ///     Establece la porción de la matriz que corresponde al vector unitario Y, segun (x,y,z).
        /// </summary>
        /// <param name="ancho"></param>
        /// <param name="alto"></param>
        /// <param name="Z"></param>
        /// <param name="w"></param>
        public virtual void SetUp(float x, float y, float z)
        {
            SetUp(x, y, z, 0);
        }

        /// <summary>
        ///     Establece la porción de la matriz que corresponde al vector unitario Y, segun (x,y,z,w).
        /// </summary>
        /// <param name="ancho"></param>
        /// <param name="alto"></param>
        /// <param name="Z"></param>
        /// <param name="w"></param>
        public virtual void SetUp(float x, float y, float z, float w)
        {
            values[4] = x;
            values[5] = y;
            values[6] = z;
            values[7] = w;
        }

        /// <summary>
        ///     Establece la porción de la matriz que corresponde al vector unitario Z, segun (x,y,z).
        /// </summary>
        /// <param name="ancho"></param>
        /// <param name="alto"></param>
        /// <param name="Z"></param>
        /// <param name="w"></param>
        public virtual void SetForward(float x, float y, float z)
        {
            SetForward(x, y, z, 0);
        }

        /// <summary>
        ///     Establece la porción de la matriz que corresponde al vector unitario Z, segun (x,y,z,w).
        /// </summary>
        /// <param name="ancho"></param>
        /// <param name="alto"></param>
        /// <param name="Z"></param>
        /// <param name="w"></param>
        public virtual void SetForward(float x, float y, float z, float w)
        {
            values[8] = x;
            values[9] = y;
            values[10] = z;
            values[11] = w;
        }

        /// <summary>
        ///     Establece la porción de traslación de esta matriz según los parámetros (x,y,z).
        /// </summary>
        /// <param name="ancho"></param>
        /// <param name="alto"></param>
        /// <param name="Z"></param>
        /// <param name="w"></param>
        public virtual void SetTranslation(float x, float y, float z)
        {
            SetTranslation(x, y, z, 1);
        }

        /// <summary>
        ///     Establece la porción de traslación de esta matriz según los parámetros (x,y,z,w).
        /// </summary>
        /// <param name="ancho"></param>
        /// <param name="alto"></param>
        /// <param name="Z"></param>
        /// <param name="w"></param>
        public virtual void SetTranslation(float x, float y, float z, float w)
        {
            values[12] = x;
            values[13] = y;
            values[14] = z;
            values[15] = w;
        }

        #endregion Set-Vectores unitarios (X, Y, Z) y translación

        #region Get(ref Vector) Vectores unitarios (X, Y, Z)

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario X de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetRight(ref Vector3f right)
        {
            right.X = values[0];
            right.Y = values[1];
            right.Z = values[2];
        }

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario Y de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetUp(ref Vector3f up)
        {
            up.X = values[4];
            up.Y = values[5];
            up.Z = values[6];
        }

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario Z de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetForward(ref Vector3f forward)
        {
            forward.X = values[8];
            forward.Y = values[9];
            forward.Z = values[10];
        }

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario X de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetRight(ref Vector4f right)
        {
            right.X = values[0];
            right.Y = values[1];
            right.Z = values[2];
            right.W = values[3];
        }

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario Y de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetUp(ref Vector4f up)
        {
            up.X = values[4];
            up.Y = values[5];
            up.Z = values[6];
            up.W = values[7];
        }

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario Z de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetForward(ref Vector4f forward)
        {
            forward.X = values[8];
            forward.Y = values[9];
            forward.Z = values[10];
            forward.W = values[11];
        }

        #endregion Get(ref Vector) Vectores unitarios (X, Y, Z)

        #region Get(ref Vector) Translation

        public void GetTranslation(ref Vector3f translation)
        {
            translation.X = values[12];
            translation.Y = values[13];
            translation.Z = values[14];
        }

        public void GetTranslation(ref Vector4f translation)
        {
            translation.X = values[12];
            translation.Y = values[13];
            translation.Z = values[14];
            translation.W = values[15];
        }

        #endregion Get(ref Vector) Translation

        #region Scale - SetScalation

        /// <summary>
        ///     Multiplica esta (this) matriz por la matriz de escalado dada por el vector pasado como parámetro.
        ///     <para>this = this * MatEscalado(Vector3f)</para>
        /// </summary>
        /// <param name="vEscala"></param>
        public virtual void Scale(Vector3f vEscala)
        {
            Scale(vEscala.X, vEscala.Y, vEscala.Z);
        }

        /// <summary>
        ///     Multiplica esta (this) matriz por la matriz de escalado dada por el vector pasado como parámetro.
        ///     <para>this = this * MatEscalado(ancho,alto,Z)</para>
        /// </summary>
        /// <param name="vEscala"></param>
        public virtual void Scale(float x, float y, float z)
        {
            auxMat4.SetIdentity();
            auxMat4.SetScalation(x, y, z);
            MultipliesBy(auxMat4);
        }

        /// <summary>
        ///     Establece la porción de escala de esta matriz.
        /// </summary>
        /// <param name="ancho"></param>
        /// <param name="alto"></param>
        /// <param name="Z"></param>
        public virtual void SetScalation(Vector3f v3Scale)
        {
            SetScalation(v3Scale.X, v3Scale.Y, v3Scale.Z);
        }

        /// <summary>
        ///     Establece la porción de escala de esta matriz.
        /// </summary>
        /// <param name="ancho"></param>
        /// <param name="alto"></param>
        /// <param name="Z"></param>
        public virtual void SetScalation(float x, float y, float z)
        {
            values[0] = x;
            values[5] = y;
            values[10] = z;
        }

        #endregion  Scale - SetScalation
    }
}