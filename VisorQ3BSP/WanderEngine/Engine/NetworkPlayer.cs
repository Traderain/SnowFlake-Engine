
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
using System;

namespace WanderEngine
{

    public class NetworkPlayer
    {
        private Vector3f ColorMask = new Vector3f();
        public int ID = -1;
        public NetworkPlayerState InitialState = null;
        public string LaserSound = "";
        public bool Marked = false;
        private float MaskTime = 0f;
        public string ModelName = "";
        public string Name = "No Name";
        public MD2Model PlayerModel;

        public NetworkPlayer(int ID, string Name, string ModelName, NetworkPlayerState InitialState, bool Marked)
        {
            this.ID = ID;
            this.Name = Name;
            this.ModelName = ModelName;
            this.InitialState = InitialState;
            this.Marked = Marked;
        }

        public void AddColorMask(float R, float G, float B, float Time)
        {
            this.ColorMask.X = R;
            this.ColorMask.Y = G;
            this.ColorMask.Z = B;
            this.MaskTime = Time;
        }

        public void SetState(NetworkPlayerState NewState)
        {
            lock (this.PlayerModel)
            {
                if (NewState != null)
                {
                    if (this.PlayerModel.ModelState != AnimationState.DeathFallFoward)
                    {
                        if ((NewState.X != this.PlayerModel.Position.X) || (NewState.Z != this.PlayerModel.Position.Z))
                        {
                            if (this.PlayerModel.ModelState != AnimationState.Run)
                            {
                                this.PlayerModel.RepeatAnimation = true;
                                this.PlayerModel.ModelState = AnimationState.Run;
                            }
                        }
                        else if (this.PlayerModel.ModelState != AnimationState.Stand)
                        {
                            this.PlayerModel.RepeatAnimation = true;
                            this.PlayerModel.ModelState = AnimationState.Stand;
                        }
                    }
                    this.PlayerModel.Position.X = NewState.X;
                    this.PlayerModel.Position.Y = (NewState.Y - (this.PlayerModel.BoundMax.Y / 2f)) + this.PlayerModel.Center.Y;
                    this.PlayerModel.Position.Z = NewState.Z;
                    this.PlayerModel.Yaw = NewState.Yaw;
                }
            }
        }

        public void Update(float TimeElapsed)
        {
            lock (this.PlayerModel)
            {
                if (this.MaskTime > 0f)
                {
                    this.PlayerModel.Update(TimeElapsed, this.ColorMask.X, this.ColorMask.Y, this.ColorMask.Z);
                    this.MaskTime -= TimeElapsed;
                }
                else if (this.Marked)
                {
                    this.PlayerModel.Update(TimeElapsed, 1f, 1f, 0f);
                }
                else
                {
                    this.PlayerModel.Update(TimeElapsed);
                }
            }
        }
    }
}

