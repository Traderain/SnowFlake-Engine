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

namespace SnowflakeEngine.WanderEngine
{
    public class ChatMessageEventArgs : EventArgs
    {
        public string ChatMessage = "";
        public bool IsPrivate;
        public int SenderId = -1;

        public ChatMessageEventArgs(int senderId, string chatMessage, bool isPrivate)
        {
            SenderId = senderId;
            ChatMessage = chatMessage;
            IsPrivate = isPrivate;
        }
    }

    public class GameOverEventArgs : EventArgs
    {
        public int Id = -1;
        public string Name = "";
        public int Score = -1;

        public GameOverEventArgs(int id, string name, int score)
        {
            Id = id;
            Name = name;
            Score = score;
        }
    }

    public class PlayerEventArgs : EventArgs
    {
        public int Id = -1;
        public NetworkPlayerState InitialState;
        public bool IsMarked;
        public string ModelName = "";
        public string Name = "";

        public PlayerEventArgs(int id, string name, string modelName, NetworkPlayerState initialState, bool isMarked)
        {
            Id = id;
            Name = name;
            ModelName = modelName;
            InitialState = initialState;
            IsMarked = isMarked;
        }
    }

    public class TriggerEventArgs : EventArgs
    {
        public string Name = "";

        public TriggerEventArgs(string name)
        {
            Name = name;
        }
    }
}