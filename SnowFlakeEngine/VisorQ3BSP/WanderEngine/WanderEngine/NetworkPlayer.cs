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
    public class NetworkPlayer
    {
        private readonly Vector3F _colorMask = new Vector3F();
        private float _maskTime;
        public int Id = -1;
        public NetworkPlayerState InitialState;
        public string LaserSound = "";
        public bool Marked;
        public string ModelName = "";
        public string Name = "No Name";
        public Md2Model PlayerModel;

        public NetworkPlayer(int id, string name, string modelName, NetworkPlayerState initialState, bool marked)
        {
            Id = id;
            Name = name;
            ModelName = modelName;
            InitialState = initialState;
            Marked = marked;
        }

        public void AddColorMask(float r, float g, float b, float time)
        {
            _colorMask.X = r;
            _colorMask.Y = g;
            _colorMask.Z = b;
            _maskTime = time;
        }

        public void SetState(NetworkPlayerState newState)
        {
            lock (PlayerModel)
            {
                if (newState != null)
                {
                    if (PlayerModel.ModelState != AnimationState.DeathFallFoward)
                    {
                        if ((newState.X != PlayerModel.Position.X) || (newState.Z != PlayerModel.Position.Z))
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
                    PlayerModel.Position.X = newState.X;
                    PlayerModel.Position.Y = (newState.Y - (PlayerModel.BoundMax.Y/2f)) + PlayerModel.Center.Y;
                    PlayerModel.Position.Z = newState.Z;
                    PlayerModel.Yaw = newState.Yaw;
                }
            }
        }

        public void Update(float timeElapsed)
        {
            lock (PlayerModel)
            {
                if (_maskTime > 0f)
                {
                    PlayerModel.Update(timeElapsed, _colorMask.X, _colorMask.Y, _colorMask.Z);
                    _maskTime -= timeElapsed;
                }
                else if (Marked)
                {
                    PlayerModel.Update(timeElapsed, 1f, 1f, 0f);
                }
                else
                {
                    PlayerModel.Update(timeElapsed);
                }
            }
        }
    }
}