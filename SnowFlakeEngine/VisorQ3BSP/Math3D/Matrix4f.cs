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
    public class Matrix4F
    {
        private static readonly Matrix4F AuxMat4 = new Matrix4F();
        private static Matrix4F _resultadoMult = new Matrix4F();
        // Quaternion temporal
        private readonly Quaternion4F _tempQRot = new Quaternion4F();
        private Vector3F _tempV3 = new Vector3F();

        public float[] Values = new float[16]
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
        public virtual void Rotation(float grados, Vector3F ejeRotacion)
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
            GetTranslation(ref _tempV3);
            _tempQRot.SetIdentity();
            _tempQRot.Rotation(MathHelp.RadiansFromDegrees(grados), ejeX, ejeY, ejeZ);
            //tempQRot.GetMatriz4(ref auxMat4);
            //this.CopiaDesde(auxMat4);
            FromQuaternion(_tempQRot);
            Translation3F = _tempV3;
        }

        /// <summary>
        ///     Multiplica esta matriz (this) por la representada por el quaternion pasado como parámetro.
        ///     <para>this = this * Quaternion</para>
        /// </summary>
        /// <param name="qRot"></param>
        public virtual void Rotate(Quaternion4F qRot)
        {
            //qRot.GetMatriz4(ref auxMat4);
            AuxMat4.FromQuaternion(qRot);
            MultipliesBy(AuxMat4);
        }

        /// <summary>
        ///     Multiplica esta matriz por la pasada como parámetro, el mat4result se almacena en esta.
        ///     <para>this = this * mat4</para>
        /// </summary>
        /// <param name="mat"></param>
        public virtual void MultipliesBy(Matrix4F mat4)
        {
            MultipliesBy(mat4, ref _resultadoMult);
            CopiaDesde(_resultadoMult);
        }

        /// <summary>
        ///     Multiplica esta matriz (this) por la pasada como parámetro (mat4), el parámetro matriz (mat4result) es el
        ///     resultado.
        ///     <para>mat4result = this * mat4</para>
        /// </summary>
        /// <param name="mat4"></param>
        /// <param name="mat4Result"></param>
        public virtual void MultipliesBy(Matrix4F mat4, ref Matrix4F mat4Result)
        {
            mat4Result.SetIdentity();

            mat4Result.Values[00] = Values[00]*mat4.Values[00] + Values[04]*mat4.Values[01] + Values[08]*mat4.Values[02] +
                                    Values[12]*mat4.Values[03];
            mat4Result.Values[01] = Values[01]*mat4.Values[00] + Values[05]*mat4.Values[01] + Values[09]*mat4.Values[02] +
                                    Values[13]*mat4.Values[03];
            mat4Result.Values[02] = Values[02]*mat4.Values[00] + Values[06]*mat4.Values[01] + Values[10]*mat4.Values[02] +
                                    Values[14]*mat4.Values[03];
            mat4Result.Values[03] = Values[03]*mat4.Values[00] + Values[07]*mat4.Values[01] + Values[11]*mat4.Values[02] +
                                    Values[15]*mat4.Values[03];

            mat4Result.Values[04] = Values[00]*mat4.Values[04] + Values[04]*mat4.Values[05] + Values[08]*mat4.Values[06] +
                                    Values[12]*mat4.Values[07];
            mat4Result.Values[05] = Values[01]*mat4.Values[04] + Values[05]*mat4.Values[05] + Values[09]*mat4.Values[06] +
                                    Values[13]*mat4.Values[07];
            mat4Result.Values[06] = Values[02]*mat4.Values[04] + Values[06]*mat4.Values[05] + Values[10]*mat4.Values[06] +
                                    Values[14]*mat4.Values[07];
            mat4Result.Values[07] = Values[03]*mat4.Values[04] + Values[07]*mat4.Values[05] + Values[11]*mat4.Values[06] +
                                    Values[15]*mat4.Values[07];

            mat4Result.Values[08] = Values[00]*mat4.Values[08] + Values[04]*mat4.Values[09] + Values[08]*mat4.Values[10] +
                                    Values[12]*mat4.Values[11];
            mat4Result.Values[09] = Values[01]*mat4.Values[08] + Values[05]*mat4.Values[09] + Values[09]*mat4.Values[10] +
                                    Values[13]*mat4.Values[11];
            mat4Result.Values[10] = Values[02]*mat4.Values[08] + Values[06]*mat4.Values[09] + Values[10]*mat4.Values[10] +
                                    Values[14]*mat4.Values[11];
            mat4Result.Values[11] = Values[03]*mat4.Values[08] + Values[07]*mat4.Values[09] + Values[11]*mat4.Values[10] +
                                    Values[15]*mat4.Values[11];

            mat4Result.Values[12] = Values[00]*mat4.Values[12] + Values[04]*mat4.Values[13] + Values[08]*mat4.Values[14] +
                                    Values[12]*mat4.Values[15];
            mat4Result.Values[13] = Values[01]*mat4.Values[12] + Values[05]*mat4.Values[13] + Values[09]*mat4.Values[14] +
                                    Values[13]*mat4.Values[15];
            mat4Result.Values[14] = Values[02]*mat4.Values[12] + Values[06]*mat4.Values[13] + Values[10]*mat4.Values[14] +
                                    Values[14]*mat4.Values[15];
            mat4Result.Values[15] = Values[03]*mat4.Values[12] + Values[07]*mat4.Values[13] + Values[11]*mat4.Values[14] +
                                    Values[15]*mat4.Values[15];
        }

        /// <summary>
        ///     CopiaDesde los valores del array de la matriz pasada como parámetro al array de esta.
        /// </summary>
        /// <param name="mat4"></param>
        public void CopiaDesde(Matrix4F mat)
        {
            Values[00] = mat.Values[00];
            Values[04] = mat.Values[04];
            Values[08] = mat.Values[08];
            Values[12] = mat.Values[12];
            Values[01] = mat.Values[01];
            Values[05] = mat.Values[05];
            Values[09] = mat.Values[09];
            Values[13] = mat.Values[13];
            Values[02] = mat.Values[02];
            Values[06] = mat.Values[06];
            Values[10] = mat.Values[10];
            Values[14] = mat.Values[14];
            Values[03] = mat.Values[03];
            Values[07] = mat.Values[07];
            Values[11] = mat.Values[11];
            Values[15] = mat.Values[15];
        }

        /// <summary>
        ///     pV = this * pV
        /// </summary>
        /// <param name="pV"></param>
        public virtual void TransformVector(ref Vector3F pV)
        {
            float auxX, auxY, auxZ;

            var inverseW = 1.0f/(Values[3] + Values[7] + Values[11] + Values[15]);
            auxX = ((Values[0]*pV.X) + (Values[4]*pV.Y) + (Values[8]*pV.Z) + Values[12])*inverseW;
            auxY = ((Values[1]*pV.X) + (Values[5]*pV.Y) + (Values[9]*pV.Z) + Values[13])*inverseW;
            auxZ = ((Values[2]*pV.X) + (Values[6]*pV.Y) + (Values[10]*pV.Z) + Values[14])*inverseW;

            pV.X = auxX;
            pV.Y = auxY;
            pV.Z = auxZ;
        }

        /// <summary>
        ///     Copia los valores de esta matriz desde la matriz pasada como parámetro
        /// </summary>
        /// <param Name="matrix"></param>
        public void CopyFrom(Matrix4F matrix)
        {
            matrix.Values.CopyTo(Values, 0);
        }

        public void FromQuaternion(Quaternion4F q)
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
            Values[0] = 1.0f - (yy*q.Y) - (zz*q.Z);
            Values[1] = (xx*q.Y) + (ww*q.Z);
            Values[2] = (xx*q.Z) - (ww*q.Y);

            Values[4] = (xx*q.Y) - (ww*q.Z);
            Values[5] = 1.0f - (xx*q.X) - (zz*q.Z);
            Values[6] = (yy*q.Z) + (ww*q.X);

            Values[8] = (xx*q.Z) + (ww*q.Y);
            Values[9] = (yy*q.Z) - (ww*q.X);
            Values[10] = 1.0f - (xx*q.X) - (yy*q.Y);
        }

        #region Constructor

        /// <summary>
        ///     Crea una nueva instancia inicializada a la matriz identidad
        /// </summary>
        public Matrix4F()
        {
        }

        /// <summary>
        ///     Crea una nueva instancia inicializados sus valores a los valores de la matriz parámetro
        /// </summary>
        /// <param Name="matrix"></param>
        public Matrix4F(Matrix4F matrix)
        {
            CopyFrom(matrix);
        }

        #endregion Constructor

        #region SetIdentity() & SetZero()

        public void SetIdentity()
        {
            Values[00] = 1;
            Values[01] = 0;
            Values[02] = 0;
            Values[03] = 0;
            Values[04] = 0;
            Values[05] = 1;
            Values[06] = 0;
            Values[07] = 0;
            Values[08] = 0;
            Values[09] = 0;
            Values[10] = 1;
            Values[11] = 0;
            Values[12] = 0;
            Values[13] = 0;
            Values[14] = 0;
            Values[15] = 1;
        }

        public void SetZero()
        {
            Values[00] = 0;
            Values[01] = 0;
            Values[02] = 0;
            Values[03] = 0;
            Values[04] = 0;
            Values[05] = 0;
            Values[06] = 0;
            Values[07] = 0;
            Values[08] = 0;
            Values[09] = 0;
            Values[10] = 0;
            Values[11] = 0;
            Values[12] = 0;
            Values[13] = 0;
            Values[14] = 0;
            Values[15] = 0;
        }

        #endregion SetIdentity() & SetZero()

        #region Propiedades

        #region Set-Get Vectores Unitarios (X, Y, Z)

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector3F" /> que corresponde al vector unitario X de esta matriz
        ///     Set: Establece los valores del vector unitario X de esta matriz
        /// </summary>
        public Vector3F Right3F
        {
            get { return new Vector3F(Values[0], Values[1], Values[2]); }
            set
            {
                Values[0] = value.X;
                Values[1] = value.Y;
                Values[2] = value.Z;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector3F" /> que corresponde al vector unitario Y de esta matriz
        ///     Set: Establece los valores del vector unitario Y de esta matriz
        /// </summary>
        public Vector3F Up3F
        {
            get { return new Vector3F(Values[4], Values[5], Values[6]); }
            set
            {
                Values[4] = value.X;
                Values[5] = value.Y;
                Values[6] = value.Z;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector3F" /> que corresponde al vector unitario Z de esta matriz
        ///     Set: Establece los valores del vector unitario Z de esta matriz
        /// </summary>
        public Vector3F Forward3F
        {
            get { return new Vector3F(Values[8], Values[9], Values[10]); }
            set
            {
                Values[8] = value.X;
                Values[9] = value.Y;
                Values[10] = value.Z;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector4F" /> que corresponde al vector unitario X de esta matriz
        ///     Set: Establece los valores del vector unitario X de esta matriz
        /// </summary>
        public Vector4F Right4F
        {
            get { return new Vector4F(Values[0], Values[1], Values[2], Values[3]); }
            set
            {
                Values[0] = value.X;
                Values[1] = value.Y;
                Values[2] = value.Z;
                Values[3] = value.W;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector4F" /> que corresponde al vector unitario Y de esta matriz
        ///     Set: Establece los valores del vector unitario Y de esta matriz
        /// </summary>
        public Vector4F Up4F
        {
            get { return new Vector4F(Values[4], Values[5], Values[6], Values[7]); }
            set
            {
                Values[4] = value.X;
                Values[5] = value.Y;
                Values[6] = value.Z;
                Values[7] = value.W;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector4F" /> que corresponde al vector unitario Z de esta matriz
        ///     Set: Establece los valores del vector unitario Z de esta matriz
        /// </summary>
        public Vector4F Forward4F
        {
            get { return new Vector4F(Values[8], Values[9], Values[10], Values[11]); }
            set
            {
                Values[8] = value.X;
                Values[9] = value.Y;
                Values[10] = value.Z;
                Values[11] = value.W;
            }
        }

        #endregion Set-Get Vectores Unitarios (X, Y, Z)

        #region Set-Get Translation

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector3F" /> que corresponde al vector de traslación de esta matriz
        ///     Set: Establece los valores del vector traslación de esta matriz
        /// </summary>
        public Vector3F Translation3F
        {
            get { return new Vector3F(Values[12], Values[13], Values[14]); }
            set
            {
                Values[12] = value.X;
                Values[13] = value.Y;
                Values[14] = value.Z;
                Values[15] = 1f;
            }
        }

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector4F" /> que corresponde al vector de traslación de esta matriz
        ///     Set: Establece los valores del vector traslación de esta matriz
        /// </summary>
        public Vector4F Translation4F
        {
            get { return new Vector4F(Values[12], Values[13], Values[14], Values[15]); }
            set
            {
                Values[12] = value.X;
                Values[13] = value.Y;
                Values[14] = value.Z;
                Values[15] = value.W;
            }
        }

        #endregion Set-Get Translation

        #region Set-Get SetScalation

        /// <summary>
        ///     Get: Devuelve un nuevo <see cref="Vector3F" /> que corresponde al vector de escalado de esta matriz
        ///     Set: Establece los valores del vector de escalado de esta matriz
        /// </summary>
        public Vector3F Scalation3F
        {
            get { return new Vector3F(Values[0], Values[5], Values[10]); }
            set
            {
                Values[0] = value.X;
                Values[5] = value.Y;
                Values[10] = value.Z;
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
            Values[0] = x;
            Values[1] = y;
            Values[2] = z;
            Values[3] = w;
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
            Values[4] = x;
            Values[5] = y;
            Values[6] = z;
            Values[7] = w;
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
            Values[8] = x;
            Values[9] = y;
            Values[10] = z;
            Values[11] = w;
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
            Values[12] = x;
            Values[13] = y;
            Values[14] = z;
            Values[15] = w;
        }

        #endregion Set-Vectores unitarios (X, Y, Z) y translación

        #region Get(ref Vector) Vectores unitarios (X, Y, Z)

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario X de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetRight(ref Vector3F right)
        {
            right.X = Values[0];
            right.Y = Values[1];
            right.Z = Values[2];
        }

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario Y de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetUp(ref Vector3F up)
        {
            up.X = Values[4];
            up.Y = Values[5];
            up.Z = Values[6];
        }

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario Z de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetForward(ref Vector3F forward)
        {
            forward.X = Values[8];
            forward.Y = Values[9];
            forward.Z = Values[10];
        }

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario X de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetRight(ref Vector4F right)
        {
            right.X = Values[0];
            right.Y = Values[1];
            right.Z = Values[2];
            right.W = Values[3];
        }

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario Y de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetUp(ref Vector4F up)
        {
            up.X = Values[4];
            up.Y = Values[5];
            up.Z = Values[6];
            up.W = Values[7];
        }

        /// <summary>
        ///     Establece las componentes del vector parámetro a los valores del vector unitario Z de esta matriz.
        /// </summary>
        /// <param Name="up"></param>
        public void GetForward(ref Vector4F forward)
        {
            forward.X = Values[8];
            forward.Y = Values[9];
            forward.Z = Values[10];
            forward.W = Values[11];
        }

        #endregion Get(ref Vector) Vectores unitarios (X, Y, Z)

        #region Get(ref Vector) Translation

        public void GetTranslation(ref Vector3F translation)
        {
            translation.X = Values[12];
            translation.Y = Values[13];
            translation.Z = Values[14];
        }

        public void GetTranslation(ref Vector4F translation)
        {
            translation.X = Values[12];
            translation.Y = Values[13];
            translation.Z = Values[14];
            translation.W = Values[15];
        }

        #endregion Get(ref Vector) Translation

        #region Scale - SetScalation

        /// <summary>
        ///     Multiplica esta (this) matriz por la matriz de escalado dada por el vector pasado como parámetro.
        ///     <para>this = this * MatEscalado(Vector3f)</para>
        /// </summary>
        /// <param name="vEscala"></param>
        public virtual void Scale(Vector3F vEscala)
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
            AuxMat4.SetIdentity();
            AuxMat4.SetScalation(x, y, z);
            MultipliesBy(AuxMat4);
        }

        /// <summary>
        ///     Establece la porción de escala de esta matriz.
        /// </summary>
        /// <param name="ancho"></param>
        /// <param name="alto"></param>
        /// <param name="Z"></param>
        public virtual void SetScalation(Vector3F v3Scale)
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
            Values[0] = x;
            Values[5] = y;
            Values[10] = z;
        }

        #endregion  Scale - SetScalation
    }
}