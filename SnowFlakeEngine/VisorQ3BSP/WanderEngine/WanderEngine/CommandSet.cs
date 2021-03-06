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

using Math3D;

namespace SnowflakeEngine.WanderEngine
{
    public class CommandSet
    {
        public Vector3F BoundMax = new Vector3F(20f, 5f, 20f);
        public Vector3F BoundMin = new Vector3F(-20f, -40f, -20f);
        public float[] Max = {20f, 5f, 20f};
        public Vector3F MaxHeight = new Vector3F(0f, 10000f, 0f);
        public float[] Min = {-20f, 40f, -20f};
        public Vector2F PlayerLook = new Vector2F();
        public Vector3F PlayerMovement = new Vector3F();
        public Vector3F Step = new Vector3F(0f, 20f, 0f);
    }
}