
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
using System.Collections;
using System.Windows.Forms;
using Math3D;

namespace WanderEngine
{

    // miki-sound-off using SoundBuffer = Microsoft.DirectX.DirectSound.Buffer;

    public class SoundManager
    {
        private Hashtable Buffer3DTable = new Hashtable();
        private Hashtable BufferTable = new Hashtable();
        // miki-sound-off private Device DSoundDevice = null;
        private Random Generator = new Random();
        // miki-sound-off private Listener3D Listener = null;
        private Control Owner = null;
        // miki-sound-off private SoundBuffer Primary = null;

        public SoundManager(Control Owner)
        {
            this.Owner = Owner;
            // miki-sound-off this.DSoundDevice = new Device();
            // miki-sound-off this.DSoundDevice.SetCooperativeLevel(Owner, CooperativeLevel.Normal);
            // miki-sound-off BufferDescription desc = new BufferDescription();
            // miki-sound-off desc.Control3D = true;
            // miki-sound-off desc.PrimaryBuffer = true;
            // miki-sound-off this.Primary = new SoundBuffer(desc, this.DSoundDevice);
            // miki-sound-off this.Listener = new Listener3D(this.Primary);
        }

        public string GenerateName()
        {
            string key = "";
            while (this.BufferTable.ContainsKey(key))
            {
                int num = this.Generator.Next(1, 20);
                for (int i = 0; i < num; i++)
                {
                    char ch = Convert.ToChar(this.Generator.Next(0, 0xff));
                    key = key + ch;
                }
            }
            return key;
        }

        public void LoadSound(string Name, string FileName)
        {
            // miki-sound-off SecondaryBuffer projection = new SecondaryBuffer(FileName, this.DSoundDevice);
            // miki-sound-off this.BufferTable[Name] = projection;
        }

        public void LoadSound3D(string Name, string FileName)
        {
            // miki-sound-off BufferDescription desc = new BufferDescription();
            // miki-sound-off desc.Control3D = true;
            // miki-sound-off desc.GlobalFocus = true;
            // miki-sound-off SecondaryBuffer lp = new SecondaryBuffer(FileName, desc, this.DSoundDevice);
            // miki-sound-off Buffer3D bufferd = new Buffer3D(lp);
            // miki-sound-off bufferd.MaxDistance = 20000f;
            // miki-sound-off bufferd.MinDistance = 4000f;
            // miki-sound-off this.BufferTable[Name] = lp;
            // miki-sound-off this.Buffer3DTable[Name] = bufferd;
        }

        public void PlaySound(string Name)
        {
            if (this.BufferTable.ContainsKey(Name))
            {
                // miki-sound-off SecondaryBuffer projection = (SecondaryBuffer) this.BufferTable[Name];
                // miki-sound-off if (projection.Status.Playing)
                // miki-sound-off {
                // miki-sound-off     projection.SetCurrentPosition(0);
                // miki-sound-off }
                // miki-sound-off projection.Play(0, BufferPlayFlags.Default);
            }
        }

        public void SetBufferPosition(string Name, Vector3f Position)
        {
            if (this.Buffer3DTable.ContainsKey(Name))
            {
                // miki-sound-off Buffer3D bufferd = (Buffer3D) this.Buffer3DTable[Name];
                // miki-sound-off bufferd.Position = new Microsoft.DirectX.Vector3(Position.X, Position.Y, Position.Z);
            }
        }

        public void SetListenerPosition(Vector3f Position)
        {
            // miki-sound-off this.Listener.Position = new Microsoft.DirectX.Vector3(Position.X, Position.Y, Position.Z);
        }
    }
}

