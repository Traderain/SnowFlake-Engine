﻿#region GPL License

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

namespace SnowflakeEngine.WanderEngine
{
    public class NetworkPlayerState
    {
        public float X;
        public float Y;
        public float Yaw;
        public float Z;

        public NetworkPlayerState()
        {
            X = 0f;
            Y = 0f;
            Z = 0f;
            Yaw = 0f;
        }

        public NetworkPlayerState(float x, float y, float z, float yaw)
        {
            X = 0f;
            Y = 0f;
            Z = 0f;
            Yaw = 0f;
            X = x;
            Y = y;
            Z = z;
            Yaw = yaw;
        }
    }
}