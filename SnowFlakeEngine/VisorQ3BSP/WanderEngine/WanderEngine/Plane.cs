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

using System;

namespace SnowflakeEngine.WanderEngine
{
    public class Plane
    {
        public float A;
        public float B;
        public float C;
        public float D;

        public Plane()
        {
            A = 0f;
            B = 0f;
            C = 0f;
            D = 0f;
        }

        public Plane(float a, float b, float c, float d)
        {
            A = 0f;
            B = 0f;
            C = 0f;
            D = 0f;
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public void Normalize()
        {
            var num = (float) Math.Sqrt(((A*A) + (B*B)) + (C*C));
            A /= num;
            B /= num;
            C /= num;
            D /= num;
        }
    }

    public enum PlaneName
    {
        Near,
        Far,
        Right,
        Left,
        Top,
        Bottom
    }
}