
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
using System.Reflection;
using Math3D;

namespace WanderEngine
{

    public class ForceCollection : ICollection, IEnumerable
    {
        private Hashtable DisposableForce = new Hashtable();
        private ArrayList Forces = new ArrayList();

        public int Add(Force NewForce)
        {
            this.DisposableForce[NewForce] = false;
            return this.Forces.Add(NewForce);
        }

        public void AddDispoableForce(Force NewForce)
        {
            this.DisposableForce[NewForce] = true;
            this.Forces.Add(NewForce);
        }

        public void ApplyAllForces(Vector3f SourcePoint, float TimeElapsed)
        {
            for (int i = 0; i < this.Forces.Count; i++)
            {
                Force force = (Force) this.Forces[i];
                if (force != null)
                {
                    force.Update(SourcePoint, TimeElapsed);
                    if ((((bool) this.DisposableForce[force]) && (force.GetVelocityX() <= force.MinVelocity.X)) && ((force.GetVelocityY() <= force.MinVelocity.Y) && (force.GetVelocityZ() <= force.MinVelocity.Z)))
                    {
                        this.Forces[i] = null;
                    }
                }
            }
        }

        public void ClearDisposableForces()
        {
            for (int i = 0; i < this.Forces.Count; i++)
            {
                Force force = (Force) this.Forces[i];
                if ((force != null) && ((bool) this.DisposableForce[force]))
                {
                    force.SetVelocity(0f, 0f, 0f);
                }
            }
        }

        public void CopyTo(Array array, int index)
        {
            this.Forces.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.Forces.GetEnumerator();
        }

        public void Remove(Force OldForce)
        {
            this.DisposableForce.Remove(OldForce);
            this.Forces.Remove(OldForce);
        }

        public void RemoveAt(int Index)
        {
            object key = this.Forces[Index];
            this.DisposableForce.Remove(key);
            this.Forces.RemoveAt(Index);
        }

        public int Count
        {
            get
            {
                return this.Forces.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this.Forces.IsSynchronized;
            }
        }

        public Force this[int Index]
        {
            get
            {
                return (Force) this.Forces[Index];
            }
            set
            {
                this.Forces[Index] = value;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.Forces.SyncRoot;
            }
        }
    }
}

