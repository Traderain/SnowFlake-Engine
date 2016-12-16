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

namespace WanderEngine.WanderEngine
{
    public class NetworkPlayer
    {
        private readonly Vector3f ColorMask = new Vector3f();
        public int ID = -1;
        public NetworkPlayerState InitialState;
        public string LaserSound = "";
        public bool Marked;
        private float MaskTime;
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
            ColorMask.X = R;
            ColorMask.Y = G;
            ColorMask.Z = B;
            MaskTime = Time;
        }

        public void SetState(NetworkPlayerState NewState)
        {
            lock (PlayerModel)
            {
                if (NewState != null)
                {
                    if (PlayerModel.ModelState != AnimationState.DeathFallFoward)
                    {
                        if ((NewState.X != PlayerModel.Position.X) || (NewState.Z != PlayerModel.Position.Z))
                        {
                            if (PlayerModel.ModelState != AnimationState.Run)
                            {
                                PlayerModel.RepeatAnimation = true;
                                PlayerModel.ModelState = AnimationState.Run;
                            }
                        }
                        else if (PlayerModel.ModelState != AnimationState.Stand)
                        {
                            PlayerModel.RepeatAnimation = true;
                            PlayerModel.ModelState = AnimationState.Stand;
                        }
                    }
                    PlayerModel.Position.X = NewState.X;
                    PlayerModel.Position.Y = (NewState.Y - (PlayerModel.BoundMax.Y/2f)) + PlayerModel.Center.Y;
                    PlayerModel.Position.Z = NewState.Z;
                    PlayerModel.Yaw = NewState.Yaw;
                }
            }
        }

        public void Update(float TimeElapsed)
        {
            lock (PlayerModel)
            {
                if (MaskTime > 0f)
                {
                    PlayerModel.Update(TimeElapsed, ColorMask.X, ColorMask.Y, ColorMask.Z);
                    MaskTime -= TimeElapsed;
                }
                else if (Marked)
                {
                    PlayerModel.Update(TimeElapsed, 1f, 1f, 0f);
                }
                else
                {
                    PlayerModel.Update(TimeElapsed);
                }
            }
        }
    }
}