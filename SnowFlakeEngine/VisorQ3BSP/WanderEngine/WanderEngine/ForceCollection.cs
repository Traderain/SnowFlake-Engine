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
using Math3D;

namespace SnowflakeEngine.WanderEngine
{
    public class ForceCollection : ICollection, IEnumerable
    {
        private readonly Hashtable _disposableForce = new Hashtable();
        private readonly ArrayList _forces = new ArrayList();

        public Force this[int index]
        {
            get { return (Force) _forces[index]; }
            set { _forces[index] = value; }
        }

        public void CopyTo(Array array, int index)
        {
            _forces.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _forces.GetEnumerator();
        }

        public int Count
        {
            get { return _forces.Count; }
        }

        public bool IsSynchronized
        {
            get { return _forces.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return _forces.SyncRoot; }
        }

        public int Add(Force newForce)
        {
            _disposableForce[newForce] = false;
            return _forces.Add(newForce);
        }

        public void AddDispoableForce(Force newForce)
        {
            _disposableForce[newForce] = true;
            _forces.Add(newForce);
        }

        public void ApplyAllForces(Vector3F sourcePoint, float timeElapsed)
        {
            for (var i = 0; i < _forces.Count; i++)
            {
                var force = (Force) _forces[i];
                if (force != null)
                {
                    force.Update(sourcePoint, timeElapsed);
                    if ((((bool) _disposableForce[force]) && (force.GetVelocityX() <= force.MinVelocity.X)) &&
                        ((force.GetVelocityY() <= force.MinVelocity.Y) && (force.GetVelocityZ() <= force.MinVelocity.Z)))
                    {
                        _forces[i] = null;
                    }
                }
            }
        }

        public void ClearDisposableForces()
        {
            for (var i = 0; i < _forces.Count; i++)
            {
                var force = (Force) _forces[i];
                if ((force != null) && ((bool) _disposableForce[force]))
                {
                    force.SetVelocity(0f, 0f, 0f);
                }
            }
        }

        public void Remove(Force oldForce)
        {
            _disposableForce.Remove(oldForce);
            _forces.Remove(oldForce);
        }

        public void RemoveAt(int index)
        {
            var key = _forces[index];
            _disposableForce.Remove(key);
            _forces.RemoveAt(index);
        }
    }
}