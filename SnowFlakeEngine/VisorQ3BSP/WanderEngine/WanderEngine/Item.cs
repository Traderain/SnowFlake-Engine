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

using Math3D;

namespace SnowflakeEngine.WanderEngine
{
    public class Item
    {
        public bool Active = true;
        public float Alpha = 1f;
        public bool AlphaDir = true;
        public int Id = -1;
        public Vector3F Position = new Vector3F();
        public ItemType Type = ItemType.Health;
        //public Glu.GLUquadric Quad;

        //public Item(Vector3f Position, ItemType IType, Glu.GLUquadric Quad)
        public Item(Vector3F position, ItemType type)
        {
            Position = position;
            Type = type;
            //this.Quad = Quad;
        }
    }

    public enum ItemType
    {
        Health,
        Special,
        Ammo
    }
}