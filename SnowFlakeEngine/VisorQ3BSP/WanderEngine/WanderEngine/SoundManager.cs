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

namespace SnowflakeEngine.WanderEngine
{
    // miki-sound-off using SoundBuffer = Microsoft.DirectX.DirectSound.Buffer;

    public class SoundManager
    {
        private readonly Hashtable _buffer3DTable = new Hashtable();
        private readonly Hashtable _bufferTable = new Hashtable();
        // miki-sound-off private Device DSoundDevice = null;
        private readonly Random _generator = new Random();
        // miki-sound-off private Listener3D Listener = null;
        private Control _owner;
        // miki-sound-off private SoundBuffer Primary = null;

        public SoundManager(Control owner)
        {
            _owner = owner;
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
            var key = "";
            while (_bufferTable.ContainsKey(key))
            {
                var num = _generator.Next(1, 20);
                for (var i = 0; i < num; i++)
                {
                    var ch = Convert.ToChar(_generator.Next(0, 0xff));
                    key = key + ch;
                }
            }
            return key;
        }

        public void LoadSound(string name, string fileName)
        {
            // miki-sound-off SecondaryBuffer projection = new SecondaryBuffer(FileName, this.DSoundDevice);
            // miki-sound-off this.BufferTable[Name] = projection;
        }

        public void LoadSound3D(string name, string fileName)
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

        public void PlaySound(string name)
        {
            if (_bufferTable.ContainsKey(name))
            {
                // miki-sound-off SecondaryBuffer projection = (SecondaryBuffer) this.BufferTable[Name];
                // miki-sound-off if (projection.Status.Playing)
                // miki-sound-off {
                // miki-sound-off     projection.SetCurrentPosition(0);
                // miki-sound-off }
                // miki-sound-off projection.Play(0, BufferPlayFlags.Default);
            }
        }

        public void SetBufferPosition(string name, Vector3F position)
        {
            if (_buffer3DTable.ContainsKey(name))
            {
                // miki-sound-off Buffer3D bufferd = (Buffer3D) this.Buffer3DTable[Name];
                // miki-sound-off bufferd.Position = new Microsoft.DirectX.Vector3(Position.X, Position.Y, Position.Z);
            }
        }

        public void SetListenerPosition(Vector3F position)
        {
            // miki-sound-off this.Listener.Position = new Microsoft.DirectX.Vector3(Position.X, Position.Y, Position.Z);
        }
    }
}