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
    public class Vector4F
    {
        public float[] Values = new float[4];

        public Vector4F()
        {
        }

        public Vector4F(float x, float y, float z, float w)
        {
            Values[0] = x;
            Values[1] = y;
            Values[2] = z;
            Values[3] = w;
        }

        public float X
        {
            get { return Values[0]; }
            set { Values[0] = value; }
        }

        public float Y
        {
            get { return Values[1]; }
            set { Values[1] = value; }
        }

        public float Z
        {
            get { return Values[2]; }
            set { Values[2] = value; }
        }

        public float W
        {
            get { return Values[3]; }
            set { Values[3] = value; }
        }
    }
}