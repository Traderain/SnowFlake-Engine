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
using System.IO;

namespace SnowflakeEngine.WanderEngine
{
    public abstract class Utility
    {
        public const float DegreeToRadian = 0.01745329f;

        public static float CapAngle(float Angle)
        {
            if (Angle > 360f)
            {
                Angle -= 360f;
            }
            if (Angle < 0f)
            {
                Angle += 360f;
            }
            return Angle;
        }

        public static float CosDeg(float Angle)
        {
            return (float) Math.Cos(Angle*0.01745329f);
        }

        public static float SinDeg(float Angle)
        {
            return (float) Math.Sin(Angle*0.01745329f);
        }

        public static string AdaptRelativePathToPlatform(string relativePath)
        {
            var result = relativePath;

            result = relativePath.Replace('\\', Path.DirectorySeparatorChar);
            result = relativePath.Replace('/', Path.DirectorySeparatorChar);

            return result;
        }
    }
}