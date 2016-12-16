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
        public int SenderID = -1;

        public ChatMessageEventArgs(int SenderID, string ChatMessage, bool IsPrivate)
        {
            this.SenderID = SenderID;
            this.ChatMessage = ChatMessage;
            this.IsPrivate = IsPrivate;
        }
    }

    public class GameOverEventArgs : EventArgs
    {
        public int ID = -1;
        public string Name = "";
        public int Score = -1;

        public GameOverEventArgs(int ID, string Name, int Score)
        {
            this.ID = ID;
            this.Name = Name;
            this.Score = Score;
        }
    }

    public class PlayerEventArgs : EventArgs
    {
        public int ID = -1;
        public NetworkPlayerState InitialState;
        public bool IsMarked;
        public string ModelName = "";
        public string Name = "";

        public PlayerEventArgs(int ID, string Name, string ModelName, NetworkPlayerState InitialState, bool IsMarked)
        {
            this.ID = ID;
            this.Name = Name;
            this.ModelName = ModelName;
            this.InitialState = InitialState;
            this.IsMarked = IsMarked;
        }
    }

    public class TriggerEventArgs : EventArgs
    {
        public string Name = "";

        public TriggerEventArgs(string Name)
        {
            this.Name = Name;
        }
    }
}