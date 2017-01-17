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
    public sealed class MathHelp
    {
        public const float Pi = 3.1415926535f;
        private static float[] _seno;
        private static float[] _coseno;
        private static bool _trig;
        private static readonly float Rad2Scale = 4096f/3.14159265f/2f;
        private static readonly float Pad = 256*3.14159265f;
        private static readonly float Deg2Rad = (float) (Math.PI/180); // 0.0175f
        private static readonly float Rad2Deg = (float) (180/Math.PI); // 57.2958f
        private static int[] _fastRandoms;
        private static int _fastRndPointer;
        private static bool _fastRndInit;
        private static readonly Random Rnd = new Random();
        private static readonly Vector3F TempLeft = new Vector3F();
        private static readonly Vector3F TempRight = new Vector3F();

        public static float Interpola(float pA, float pB, float pD)
        {
            var f = (1 - Coseno(pD*Pi))*0.5f;
            return pA + f*(pB - pA);
        }

        public static float Aleatorio()
        {
            var rnd = new Random();
            return (float) (rnd.NextDouble()*2 - 1);
        }

        public static float Aleatorio(float pMinimo, float pMaximo)
        {
            //Random rnd = new Random();
            return (float) (Rnd.NextDouble()*(pMaximo - pMinimo) + pMinimo);
        }

        public static float AleatorioConDelta(float pAveridge, float pDelta)
        {
            return pAveridge + Aleatorio()*pDelta;
        }

        public static int FastRnd(int pBits)
        {
            if (pBits < 1)
            {
                return 0;
            }
            _fastRndPointer = (_fastRndPointer + 1) & 31;
            if (!_fastRndInit)
            {
                _fastRandoms = new int[32];
                for (var i = 0; i < 32; i++)
                {
                    _fastRandoms[i] = (int) Aleatorio(0, 0xFFFFFF);
                }
                _fastRndInit = true;
            }
            return _fastRandoms[_fastRndPointer] & (1 << (pBits - 1));
        }

        public static int FastRndBit()
        {
            return FastRnd(1);
        }

        public static float RadiansFromDegrees(float pDegrees)
        {
            return pDegrees*Deg2Rad;
        }

        public static float DegreesFromRadians(float pRadianes)
        {
            return pRadianes*Rad2Deg;
        }

        public static float Seno(float pAngulo)
        {
            if (!_trig) BuildTrig();
            return _seno[(int) ((pAngulo + Pad)*Rad2Scale) & 0xFFF];
        }

        public static float Coseno(float pAngulo)
        {
            if (!_trig) BuildTrig();
            return _coseno[(int) ((pAngulo + Pad)*Rad2Scale) & 0xFFF];
        }

        private static void BuildTrig()
        {
            Console.WriteLine(">> Building warp_Math LUT");

            _seno = new float[4096];
            _coseno = new float[4096];

            for (var i = 0; i < 4096; i++)
            {
                _seno[i] = (float) Math.Sin(i/Rad2Scale);
                _coseno[i] = (float) Math.Cos(i/Rad2Scale);
            }

            _trig = true;
        }

        public static float Pythagoras(float pA, float pB)
        {
            return (float) Math.Sqrt(pA*pA + pB*pB);
        }

        public static int Pythagoras(int pA, int pB)
        {
            return (int) Math.Sqrt(pA*pA + pB*pB);
        }

        public static int Crop(int pNum, int pMin, int pMax)
        {
            return (pNum < pMin) ? pMin : (pNum > pMax) ? pMax : pNum;
        }

        public static float Crop(float pNum, float pMin, float pMax)
        {
            return (pNum < pMin) ? pMin : (pNum > pMax) ? pMax : pNum;
        }

        public static bool Inrange(int pNum, int pMin, int pMax)
        {
            return ((pNum >= pMin) && (pNum < pMax));
        }

        public static void ClearBuffer(byte[] pBuffer, byte pValue)
        {
            Array.Clear(pBuffer, 0, pBuffer.GetLength(0));
        }

        public static void CropBuffer(int[] pBuffer, int pMin, int pMax)
        {
            for (var i = pBuffer.GetLength(0) - 1; i >= 0; i--) pBuffer[i] = Crop(pBuffer[i], pMin, pMax);
        }

        public static void CopyBuffer(int[] pSource, int[] pTarget)
        {
            Array.Copy(pSource, 0, pTarget, 0, Crop(pSource.GetLength(0), 0, pTarget.GetLength(0)));
        }

        /// <summary>
        ///     Calculate a face normal, no w-information.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static Vector3F CalculateBasicFaceNormal(Vector3F v1, Vector3F v2, Vector3F v3)
        {
            var normal = new Vector3F();
            var left = v2 - v1;
            var right = v3 - v1;

            Vector3F.Cross(left, right, ref normal);
            normal.Normalize();

            return normal;
        }

        public static void CalculateBasicFaceNormal(Vector3F v1, Vector3F v2, Vector3F v3, ref Vector3F result)
        {
            TempLeft.CopyFrom(v2);
            TempRight.CopyFrom(v3);
            TempLeft.Subtract(v1);
            TempRight.Subtract(v1);
            Vector3F.Cross(TempLeft, TempRight, ref result);
            result.Normalize();
        }

        #region Intersecciones

        /*
        public static void Intersects(Ray rayo, Plane plane, ref IntersectResult iResult)
        {
            Intersects(rayo.Origin, rayo.Direction, plane, ref iResult);
        }

        /// <summary>
        ///		Ray/Plane intersection test.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="plane"></param>
        /// <returns>Struct that contains a bool (hit?) and distance.</returns>
        public static void Intersects(Vector3f rayOrigin, Vector3f rayDir, Plane plane, ref IntersectResult iResult)
        {
            float denom = plane.Normal.Dot(rayDir);

            if (Math.Abs(denom) < float.Epsilon)
            {
                // Parellel
                iResult.Hit = false;
                iResult.Distance = 0;
                return;
            }
            else
            {
                float nom = plane.Normal.Dot(rayOrigin) + plane.D;
                float t = -(nom / denom);
                iResult.Hit = t >= 0;
                iResult.Distance = t;
                return;
            }
        }
*/

        /// <summary>
        ///     Simple struct to allow returning a complex intersection result.
        /// </summary>
        public class IntersectResult
        {
            public IntersectResult()
            {
                Hit = false;
                Distance = 0;
            }

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="hit"></param>
            /// <param name="distance"></param>
            public IntersectResult(bool hit, float distance)
            {
                Hit = hit;
                Distance = distance;
            }

            #region Fields

            /// <summary>
            ///     Did the intersection test result in a hit?
            /// </summary>
            public bool Hit;

            /// <summary>
            ///     If Hit was true, this will hold a query specific distance value.
            ///     i.e. for a Ray-Box test, the distance will be the distance from the start point
            ///     of the ray to the point of intersection.
            /// </summary>
            public float Distance;

            #endregion Fields
        }

        #endregion Intersecciones
    }
}