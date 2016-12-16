using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Math3D;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace SnowflakeEngine.WanderEngine
{
    // miki-play-off using VoiceClient = Microsoft.DirectX.DirectPlay.Voice.Client;
    // miki-play-off using PlayClient = Microsoft.DirectX.DirectPlay.Client;
    // miki-input-off using InputDevice = Microsoft.DirectX.DirectInput.Device;

    public class Engine
    {
        public delegate void AddPlayerHandler(object sender, PlayerEventArgs e);

        public delegate void ChatMessageRecievedHandler(object sender, ChatMessageEventArgs e);

        public delegate void ConnectedToServerHandler(object sender, EventArgs e);

        public delegate void DisconnectedFromServerHandler(object sender, EventArgs e);

        public delegate void GameOverHandler(object sender, GameOverEventArgs e);

        public delegate void RemovePlayerHandler(object sender, PlayerEventArgs e);

        public delegate void TriggerActivatedHandler(object sender, TriggerEventArgs e);

        // miki-sound-off private SoundManager EngineSoundManager = null;
        public static readonly int EveryoneID = 0;
        internal static int texUnits = 1;
        // miki-play-off private VoiceClient ClientVoice = null;
        private readonly ControlConfig ControlCfg = new ControlConfig();
        private readonly double farClipDistance = 5000.0f;
        private readonly double fieldOfView = 45.0f;
        // miki private bool FireEnabled = true;
        private readonly ForceCollection Forces = new ForceCollection();
        private readonly GameWindow gameWin;
        private readonly Random Generator = new Random();
        private readonly ArrayList IncomingPlayers = new ArrayList();
        private readonly Hashtable Items = new Hashtable();
        private readonly ArrayList LaserBeams = new ArrayList();

        private readonly NetworkPlayerState LastState = new NetworkPlayerState(float.NaN, float.NaN, float.NaN,
            float.NaN);

        private readonly bool m_ProcessInput = true;
        private readonly Camera MapCamera = new Camera();
        private readonly Frustrum MapFrustrum = new Frustrum();
        private readonly double nearClipDistance = 1.0f;
        private readonly Hashtable NetworkPlayers = new Hashtable();
        private readonly bool NextStateReady = true;
        private readonly ArrayList OutgoingPlayerIDs = new ArrayList();
        private readonly ArrayList SpawnPoints = new ArrayList();
        private readonly bool VoiceEnabled = false;
        private bool acquireMouse = true;
        public int Ammo = 10;
        private double aspectRatio = 1.0f;
        //private static readonly Guid ApplicationGuid = new Guid("{DCC56EE8-0265-4e9b-91E8-A1210B12E0AC}");
        private int BlastForce = -1;
        private BSPFile CurrentMap;
        private bool CursorHide;
        private float DeathTime;
        // miki-play-off private PlayClient DPClient = null;
        public bool EnableJump = true;
        private int GravityForce = -1;
        public bool HasMarkedItem = false;
        public int Health = 100;
        private float HurtTime;
        private int JumpForce = -1;
        // miki-input-off private InputDevice KeyboardDevice = null;
        private Texture KnifeTexture;
        private float LaserTime;
        public bool Marked;
        private CommandSet PlayerState = new CommandSet();
        private Point pointer_current, pointer_previous;
        private Size pointer_delta;
        // miki private static readonly int Port = 0x3104;
        //private Form RenderForm = null;
        //private SimpleOpenGlControl RenderTarget = null;
        public string RoundInfo = "";
        private int RunForce = -1;
        public int Score;
        public int SecondsLeft = 0;
        private float StabTime;
        private int StrafeForce = -1;
        // miki-input-off private InputDevice MouseDevice = null;
        private Text2D textFont;
        private float TimeElapsed;
        //private float WalkBias = 0f;
        public WeaponType Weapon = WeaponType.Knife;
        private int winWidth = 1, winHeight = 1;

        public Engine(GameWindow gameWin)
        {
            if (gameWin == null)
                throw new ArgumentNullException("Argumento nulo en el constructor de Engine");

            this.gameWin = gameWin;
            winHeight = gameWin.Height;
            winWidth = gameWin.Width;

            gameWin.Mouse.ButtonDown += gameWin_Mouse_ButtonDown;

            InitializeAll();
        }

        public static string Quake3FilesPath { get; set; }

        public bool Connected { get; private set; }

        public bool IsDead
        {
            get { return (DeathTime > 0f); }
        }

        public bool IsPaused { get; set; } = false;
        public string ModelName { get; set; } = "";
        public int NetworkID { get; } = -1;
        public string PlayerName { get; set; } = "";

        private float WeaponTime
        {
            get
            {
                if (Weapon == WeaponType.Knife)
                {
                    return StabTime;
                }
                if (Weapon == WeaponType.Laser)
                {
                    return LaserTime;
                }
                return 0f;
            }
        }

        public event AddPlayerHandler AddPlayer;
        public event EventHandler AmmoChanged;
        public event ChatMessageRecievedHandler ChatMessageRecieved;
        public event ConnectedToServerHandler ConnectedToServer;
        public event DisconnectedFromServerHandler DisconnectedFromServer;
        public event GameOverHandler GameOver;
        public event EventHandler HealthChanged;
        public event EventHandler QuitGame;
        public event RemovePlayerHandler RemovePlayer;
        public event EventHandler RoundInfoChanged;
        public event EventHandler ScoreChanged;
        public event EventHandler ServerTimeChanged;
        public event TriggerActivatedHandler TriggerActivated;
        public event EventHandler WeaponChanged;

        private void gameWin_Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Right)
                acquireMouse = !acquireMouse;
            /*
			if (e.Button == MouseButton.Left)
			{
				if (this.FireEnabled)
				{
					this.FireEnabled = false;
					this.Fire();
				}
			}
			else
			{
				this.FireEnabled = true;
			}*/
        }

        private void ChooseSpawnPoint()
        {
            var num = Generator.Next(SpawnPoints.Count);
            var newPos = (Vector3f) SpawnPoints[num];
            MapCamera.MoveCameraTo(newPos);
            MapCamera.MoveCameraUpDown(12f);
        }

        public void ConnectToServer(IPAddress HostIP)
        {
            Connected = false;
            // miki-play-off Address deviceInformation = new Address();
            // miki-play-off deviceInformation.ServiceProvider = Address.ServiceProviderTcpIp;
            // miki-play-off Address hostAddress = new Address(new IPEndPoint(HostIP, Port));
            // miki-play-off PlayerInformation playerInformation = new PlayerInformation();
            // miki-play-off playerInformation.Name = this.m_PlayerName;
            // miki-play-off this.DPClient.SetClientInformation(playerInformation, SyncFlags.PeerInformation);
            // miki-play-off ApplicationDescription applicationDescription = new ApplicationDescription();
            // miki-play-off applicationDescription.GuidApplication = ApplicationGuid;
            // miki-play-off this.DPClient.FindHosts(applicationDescription, hostAddress, deviceInformation, null, 0, 0, 0, FindHostsFlags.None);
        }

        private void CreateItem(int ID, ItemType IType, Vector3f Pos, bool Active)
        {
            //this.RenderTarget.Invoke(new CreateItemDelegate(this.CreateItem_Safe), new object[] { ID, IType, Pos, Active });
        }

        private void CreateItem_Safe(int ID, ItemType IType, Vector3f Pos, bool Active)
        {
            lock (Items)
            {
                /*Item item = new Item(Pos, IType, Glu.gluNewQuadric());
				item.Active = Active;
				item.ID = ID;
				this.Items[ID] = item;*/
            }
        }

        public void DestroyAll()
        {
            DestroyInput();
            DestroyNetwork();
            textFont.Dispose();
        }

        private void DestroyInput()
        {
            /* miki-input-off 
			if (this.KeyboardDevice != null)
			{
				this.KeyboardDevice.Unacquire();
				this.KeyboardDevice.Dispose();
				this.KeyboardDevice = null;
			}
			if (this.MouseDevice != null)
			{
				this.MouseDevice.Unacquire();
				this.MouseDevice.Dispose();
				this.MouseDevice = null;
			}
			*/
        }

        private void DestroyNetwork()
        {
            DisconnectFromServer();
        }

        private void Die(int KillerID)
        {
            if (!IsDead)
            {
                DeathTime = 4f;
                Marked = false;
                if (Connected)
                {
                    // miki-play-off NetworkPacket sendData = new NetworkPacket();
                    // miki-play-off sendData.Write(MessageType.Kill);
                    // miki-play-off sendData.Write(this.m_NetworkID);
                    // miki-play-off sendData.Write(KillerID);
                    // miki-play-off this.DPClient.Send(sendData, 0, SendFlags.Guaranteed);
                }
                // miki-sound-off this.EngineSoundManager.PlaySound("death");
            }
        }

        public void DisableVoice()
        {
            if (VoiceEnabled)
            {
                // miki-play-off this.ClientVoice.TransmitTargets = new int[0];
            }
        }

        private void DisconnectFromServer()
        {
            if (Connected)
            {
                if (VoiceEnabled)
                {
                    // miki-play-off this.ClientVoice.Dispose();
                    // miki-play-off this.ClientVoice = null;
                }
                // miki-play-off NetworkPacket sendData = new NetworkPacket();
                // miki-play-off sendData.Write(MessageType.RemovePlayer);
                // miki-play-off sendData.Write(this.m_NetworkID);
                // miki-play-off sendData.Write(this.m_PlayerName);
                // miki-play-off this.DPClient.Send(sendData, 0, SendFlags.Guaranteed | SendFlags.Sync);
                // miki-play-off if (this.DPClient != null)
                // miki-play-off {
                // miki-play-off     this.DPClient = null;
                // miki-play-off }
                Connected = false;
            }
        }

        /* miki-play-off 
		private void DPClient_ConnectComplete(object sender, ConnectCompleteEventArgs e)
		{
			if (e.Message.ResultCode == Microsoft.DirectX.DirectPlay.ResultCode.Success)
			{
				this.m_Connected = true;
				this.m_NetworkID = e.Message.LocalPlayerId;
				NetworkPacket sendData = new NetworkPacket();
				sendData.Write(MessageType.AddPlayer);
				sendData.Write(this.m_NetworkID);
				sendData.Write(this.m_PlayerName);
				sendData.Write(this.m_ModelName);
				this.DPClient.Send(sendData, 0, SendFlags.Guaranteed);
				this.ConnectedToServer(this, null);
			}
		}

		private void DPClient_FindHostResponse(object sender, FindHostResponseEventArgs e)
		{
			this.DPClient.Connect(e.Message.ApplicationDescription, e.Message.AddressSender, e.Message.AddressDevice, null, ConnectFlags.OkToQueryForAddressing);
		}

		private void DPClient_Receive(object sender, ReceiveEventArgs e)
		{
			try
			{
				Hashtable hashtable;
				NetworkPacket receiveData = e.Message.ReceiveData;
				MessageType type = (MessageType) receiveData.Read(typeof(MessageType));
				int num = -1;
				string name = "";
				string modelName = "";
				float x = 0f;
				float y = 0f;
				float z = 0f;
				float yaw = 0f;
				switch (type)
				{
					case MessageType.AddPlayer:
						num = (int) receiveData.Read(typeof(int));
						name = receiveData.ReadString();
						modelName = receiveData.ReadString();
						if (num != this.m_NetworkID)
						{
							NetworkPacket sendData = new NetworkPacket();
							sendData.Write(MessageType.AddCurrentPlayer);
							sendData.Write(num);
							sendData.Write(this.m_PlayerName);
							sendData.Write(this.m_ModelName);
							sendData.Write(this.MapCamera.Position.X);
							sendData.Write(this.MapCamera.Position.Y);
							sendData.Write(this.MapCamera.Position.Z);
							sendData.Write(90f - this.MapCamera.Yaw);
							sendData.Write(this.Marked);
							this.DPClient.Send(sendData, 0, SendFlags.Guaranteed);
							this.AddPlayer(this, new PlayerEventArgs(num, name, modelName, null, false));
						}
						return;

					case MessageType.RemovePlayer:
						num = (int) receiveData.Read(typeof(int));
						name = receiveData.ReadString();
						if (num != this.m_NetworkID)
						{
							this.RemovePlayer(this, new PlayerEventArgs(num, name, "", null, false));
						}
						return;

					case MessageType.AddCurrentPlayer:
					{
						num = (int) receiveData.Read(typeof(int));
						name = receiveData.ReadString();
						modelName = receiveData.ReadString();
						x = (float) receiveData.Read(typeof(float));
						y = (float) receiveData.Read(typeof(float));
						z = (float) receiveData.Read(typeof(float));
						yaw = (float) receiveData.Read(typeof(float));
						bool isMarked = (bool) receiveData.Read(typeof(bool));
						if (num != this.m_NetworkID)
						{
							this.AddPlayer(this, new PlayerEventArgs(num, name, modelName, new NetworkPlayerState(x, y, z, yaw), isMarked));
						}
						return;
					}
					case MessageType.UpdateState:
						num = (int) receiveData.Read(typeof(int));
						x = (float) receiveData.Read(typeof(float));
						y = (float) receiveData.Read(typeof(float));
						z = (float) receiveData.Read(typeof(float));
						yaw = (float) receiveData.Read(typeof(float));
						if (num != this.m_NetworkID)
						{
							lock ((hashtable = this.NetworkPlayers))
							{
								if (this.NetworkPlayers.ContainsKey(num))
								{
									((NetworkPlayer) this.NetworkPlayers[num]).SetState(new NetworkPlayerState(x, y, z, yaw));
								}
								return;
							}
						}
						this.NextStateReady = true;
						return;

					case MessageType.PrivateMessage:
					case MessageType.ChatMessage:
					{
						num = (int) receiveData.Read(typeof(int));
						string chatMessage = receiveData.ReadString();
						this.ChatMessageRecieved(this, new ChatMessageEventArgs(num, chatMessage, type == MessageType.PrivateMessage));
						return;
					}
					case MessageType.Hurt:
					{
						num = (int) receiveData.Read(typeof(int));
						float num6 = (float) receiveData.Read(typeof(float));
						lock ((hashtable = this.NetworkPlayers))
						{
							if (!this.IsDead && (this.Health > 0))
							{
								this.HurtTime = 0.1f;
								// miki-sound-off this.EngineSoundManager.PlaySound("hurt");
								this.Health -= (int) num6;
								if (this.Health < 0)
								{
									this.Health = 0;
								}
								this.HealthChanged(this, null);
								if (this.Health == 0)
								{
									this.Die(num);
								}
								if (num6 >= 20f)
								{
									float num7 = (float) receiveData.Read(typeof(float));
									float num8 = (float) receiveData.Read(typeof(float));
									float num9 = (float) receiveData.Read(typeof(float));
									Vector3 vector = new Vector3(num7, num8, num9);
									Force newForce = new Force();
									newForce.MaxVelocity = new Vector3(400f, 400f, 400f);
									newForce.Acceleration = new Vector3(-10000f, -10000f, -10000f);
									newForce.Direction = this.MapCamera.Position - vector;
									newForce.Direction.Normalize();
									newForce.SetVelocity(400f, 0f, 400f);
									lock (this.Forces)
									{
										this.Forces.AddDispoableForce(newForce);
									}
								}
							}
							return;
						}
					}
					case MessageType.Kill:
						break;

					case MessageType.ServerTime:
						goto Label_06E1;

					case MessageType.GameOver:
					{
						num = (int) receiveData.Read(typeof(int));
						name = receiveData.ReadString();
						int score = (int) receiveData.Read(typeof(int));
						this.GameOver(this, new GameOverEventArgs(num, name, score));
						return;
					}
					case MessageType.Respawn:
						goto Label_066A;

					case MessageType.ItemInfo:
					{
						int iD = (int) receiveData.Read(typeof(int));
						ItemType iType = (ItemType) receiveData.Read(typeof(ItemType));
						float num13 = (float) receiveData.Read(typeof(float));
						float num14 = (float) receiveData.Read(typeof(float));
						float num15 = (float) receiveData.Read(typeof(float));
						bool active = (bool) receiveData.Read(typeof(bool));
						this.CreateItem(iD, iType, new Vector3(num13, num14, num15), active);
						return;
					}
					case MessageType.ItemState:
					{
						int key = (int) receiveData.Read(typeof(int));
						bool flag3 = (bool) receiveData.Read(typeof(bool));
						lock ((hashtable = this.Items))
						{
							if (this.Items.Contains(key))
							{
								((Item) this.Items[key]).Active = flag3;
							}
							return;
						}
						//goto Label_0888;
					}
					case MessageType.MarkPlayer:
						num = (int) receiveData.Read(typeof(int));
						if (num != this.m_NetworkID)
						{
							goto Label_08FA;
						}
						this.Marked = true;
						this.RoundInfo = "You're the marked man!";
						this.RoundInfoChanged(this, null);
						// miki-sound-off this.EngineSoundManager.PlaySound("marked");
						return;

					case MessageType.UnmarkPlayer:
						goto Label_0948;

					case MessageType.RoundInfo:
						goto Label_0888;

					case MessageType.LaserBeam:
						goto Label_09C2;

					default:
						return;
				}
				num = (int) receiveData.Read(typeof(int));
				int num10 = (int) receiveData.Read(typeof(int));
				if ((num10 == this.m_NetworkID) && (num != num10))
				{
					this.Score++;
					this.ScoreChanged(this, null);
				}
				lock ((hashtable = this.NetworkPlayers))
				{
					if (this.NetworkPlayers.ContainsKey(num))
					{
						NetworkPlayer player2 = (NetworkPlayer) this.NetworkPlayers[num];
						player2.PlayerModel.RepeatAnimation = false;
						player2.PlayerModel.ModelState = AnimationState.DeathFallFoward;
					}
					return;
				}
			Label_066A:
				num = (int) receiveData.Read(typeof(int));
				lock ((hashtable = this.NetworkPlayers))
				{
					if (this.NetworkPlayers.ContainsKey(num))
					{
						NetworkPlayer player3 = (NetworkPlayer) this.NetworkPlayers[num];
						player3.PlayerModel.RepeatAnimation = true;
						player3.PlayerModel.ModelState = AnimationState.Stand;
					}
					return;
				}
			Label_06E1:
				this.SecondsLeft = (int) receiveData.Read(typeof(int));
				this.ServerTimeChanged(this, null);
				return;
			Label_0888:
				this.RoundInfo = receiveData.ReadString();
				this.RoundInfoChanged(this, null);
				return;
			Label_08FA:
				Monitor.Enter(hashtable = this.NetworkPlayers);
				try
				{
					if (this.NetworkPlayers.ContainsKey(num))
					{
						NetworkPlayer player4 = (NetworkPlayer) this.NetworkPlayers[num];
						player4.Marked = true;
					}
					return;
				}
				finally
				{
					Monitor.Exit(hashtable);
				}
			Label_0948:
				num = (int) receiveData.Read(typeof(int));
				if (num == this.m_NetworkID)
				{
					this.Marked = false;
					return;
				}
				lock ((hashtable = this.NetworkPlayers))
				{
					if (this.NetworkPlayers.ContainsKey(num))
					{
						NetworkPlayer player5 = (NetworkPlayer) this.NetworkPlayers[num];
						player5.Marked = false;
					}
					return;
				}
			Label_09C2:
				num = (int) receiveData.Read(typeof(int));
				if (num != this.m_NetworkID)
				{
					LaserBeamInfo info = new LaserBeamInfo();
					info.Start.X = (float) receiveData.Read(typeof(float));
					info.Start.Y = (float) receiveData.Read(typeof(float));
					info.Start.Z = (float) receiveData.Read(typeof(float));
					info.Start2.X = (float) receiveData.Read(typeof(float));
					info.Start2.Y = (float) receiveData.Read(typeof(float));
					info.Start2.Z = (float) receiveData.Read(typeof(float));
					info.End.X = (float) receiveData.Read(typeof(float));
					info.End.Y = (float) receiveData.Read(typeof(float));
					info.End.Z = (float) receiveData.Read(typeof(float));
					info.TimeRemaining = 0.3f;
					lock ((hashtable = this.NetworkPlayers))
					{
						if (this.NetworkPlayers.ContainsKey(num))
						{
							NetworkPlayer player6 = (NetworkPlayer) this.NetworkPlayers[num];
							// miki-sound-off this.EngineSoundManager.SetBufferPosition(player6.LaserSound, player6.PlayerModel.Position);
							// miki-sound-off this.EngineSoundManager.PlaySound(player6.LaserSound);
						}
					}
					lock (this.LaserBeams)
					{
						this.LaserBeams.Add(info);
					}
				}
			}
			catch
			{
			}
		}

		private void DPClient_SessionTerminated(object sender, SessionTerminatedEventArgs e)
		{
			this.DisconnectFromServer();
			this.DisconnectedFromServer(this, null);
		}
		
		public void EnableVoice()
		{
			if (this.m_Connected)
			{
				this.ClientVoice = new VoiceClient(this.DPClient);
				SoundDeviceConfig soundConfig = new SoundDeviceConfig();
				soundConfig.Flags = SoundConfigFlags.AutoSelect;
				// miki-sound-off soundConfig.GuidPlaybackDevice = DSoundHelper.DefaultPlaybackDevice;
				// miki-sound-off soundConfig.GuidCaptureDevice = DSoundHelper.DefaultCaptureDevice;
				soundConfig.Window = this.RenderForm;
				ClientConfig clientConfig = new ClientConfig();
				clientConfig.Flags = ClientConfigFlags.AutoVoiceActivated | ClientConfigFlags.AutoRecordVolume;
				clientConfig.RecordVolume = 1;
				clientConfig.PlaybackVolume = 0;
				clientConfig.Threshold = Threshold.Unused;
				clientConfig.BufferQuality = BufferQuality.Default;
				clientConfig.BufferAggressiveness = BufferAggressiveness.Default;
				try
				{
					this.ClientVoice.Connect(soundConfig, clientConfig, VoiceFlags.Sync);
				}
				catch (RunSetupException)
				{
					new Test().CheckAudioSetup();
					this.ClientVoice.Connect(soundConfig, clientConfig, VoiceFlags.Sync);
				}
				this.ClientVoice.TransmitTargets = new int[] { 0 };
				this.VoiceEnabled = true;
			}
		}
		*/

        private void Engine_AddPlayer(object sender, PlayerEventArgs e)
        {
            lock (IncomingPlayers)
            {
                IncomingPlayers.Add(new NetworkPlayer(e.ID, e.Name, e.ModelName, e.InitialState, e.IsMarked));
            }
        }

        private void Engine_AmmoChanged(object sender, EventArgs e)
        {
        }

        private void Engine_ChatMessageRecieved(object sender, ChatMessageEventArgs e)
        {
        }

        private void Engine_ConnectedToServer(object sender, EventArgs e)
        {
        }

        private void Engine_DisconnectedFromServer(object sender, EventArgs e)
        {
        }

        private void Engine_GameOver(object sender, GameOverEventArgs e)
        {
        }

        private void Engine_HealthChanged(object sender, EventArgs e)
        {
        }

        private void Engine_QuitGame(object sender, EventArgs e)
        {
        }

        private void Engine_RemovePlayer(object sender, PlayerEventArgs e)
        {
            lock (OutgoingPlayerIDs)
            {
                OutgoingPlayerIDs.Add(e.ID);
            }
        }

        private void Engine_RoundInfoChanged(object sender, EventArgs e)
        {
        }

        private void Engine_ScoreChanged(object sender, EventArgs e)
        {
        }

        private void Engine_ServerTimeChanged(object sender, EventArgs e)
        {
        }

        private void Engine_TriggerActivated(object sender, TriggerEventArgs e)
        {
        }

        private void Engine_WeaponChanged(object sender, EventArgs e)
        {
        }

        public void Fire()
        {
            if (!IsDead && (WeaponTime <= 0f))
            {
                Hashtable hashtable;
                if (Weapon == WeaponType.Knife)
                {
                    // miki-sound-off this.EngineSoundManager.PlaySound("swing");
                    StabTime = 0.3f;
                    var a = MapCamera.ProjectPosition(5f, 0f);
                    lock ((hashtable = NetworkPlayers))
                    {
                        foreach (NetworkPlayer player in NetworkPlayers.Values)
                        {
                            if ((Vector3f.Distance(a, player.PlayerModel.Position) < 45.0) &&
                                (player.PlayerModel.ModelState != AnimationState.DeathFallFoward))
                            {
                                // miki-sound-off this.EngineSoundManager.PlaySound("pain");
                                if (Connected)
                                {
                                    SendHurt(player.ID);
                                }
                                player.AddColorMask(1f, 0f, 0f, 0.3f);
                            }
                        }
                        return;
                    }
                }
                if (Weapon == WeaponType.Laser)
                {
                    LaserTime = 0.9f;
                    if (Ammo <= 0)
                    {
                        // miki-sound-off this.EngineSoundManager.PlaySound("noammo");
                    }
                    else
                    {
                        // miki-sound-off this.EngineSoundManager.PlaySound("laser");
                        Ammo--;
                        AmmoChanged(this, null);
                        CurrentMap.DetectCollisionRay(MapCamera.Position, MapCamera.ProjectPositionWithY(10000f, 0f));
                        var start = new Vector3f(MapCamera.Position.X, MapCamera.Position.Y, MapCamera.Position.Z);
                        var info = new LaserBeamInfo();
                        info.Start = MapCamera.ProjectPositionWithY(0f, 3f);
                        info.Start2 = MapCamera.ProjectPositionWithY(0f, 5f);
                        info.End = new Vector3f(CurrentMap.CollisionInfo.EndPoint.X, CurrentMap.CollisionInfo.EndPoint.Y,
                            CurrentMap.CollisionInfo.EndPoint.Z);
                        info.TimeRemaining = 0.3f;
                        lock (LaserBeams)
                        {
                            LaserBeams.Add(info);
                        }
                        if (Connected)
                        {
                            /* miki-play-off 
							NetworkPacket sendData = new NetworkPacket();
							sendData.Write(MessageType.LaserBeam);
							sendData.Write(this.m_NetworkID);
							sendData.Write(info.Start.X);
							sendData.Write(info.Start.Y);
							sendData.Write(info.Start.Z);
							sendData.Write(info.Start2.X);
							sendData.Write(info.Start2.Y);
							sendData.Write(info.Start2.Z);
							sendData.Write(info.End.X);
							sendData.Write(info.End.Y);
							sendData.Write(info.End.Z);
							this.DPClient.Send(sendData, 0, SendFlags.Guaranteed | SendFlags.Sync);
							*/
                        }
                        lock ((hashtable = NetworkPlayers))
                        {
                            foreach (NetworkPlayer player2 in NetworkPlayers.Values)
                            {
                                if (player2.PlayerModel.RayCollides(start, info.End))
                                {
                                    // miki-sound-off this.EngineSoundManager.PlaySound("pain");
                                    if (Connected)
                                    {
                                        SendHurt(player2.ID);
                                    }
                                    player2.AddColorMask(1f, 0f, 0f, 0.3f);
                                }
                            }
                        }
                        var angle = Utility.CapAngle(90f - MapCamera.Yaw);
                        Forces[BlastForce].Direction.X = -Utility.CosDeg(angle);
                        Forces[BlastForce].Direction.Y = Utility.SinDeg(MapCamera.Pitch);
                        Forces[BlastForce].Direction.Z = Utility.SinDeg(angle);
                        Forces[BlastForce].SetVelocity(Forces[BlastForce].MaxVelocity.X, 0f,
                            Forces[BlastForce].MaxVelocity.Z);
                    }
                }
            }
        }

        private Item GetItemCollision()
        {
            foreach (Item item2 in Items.Values)
            {
                if (item2.Active && (Vector3f.Distance(MapCamera.Position, item2.Position) <= 25.0))
                {
                    return item2;
                }
            }
            return null;
        }

        private void InitializeAll()
        {
            //GL.GetInteger(GetPName.MaxTextureUnits, out texUnits);

            InitializeGraphics();
            InitializeInput();
            InitializeNetwork();
            InitializeSound();
            InitializeForces();
            KnifeTexture = new Texture(Quake3FilesPath +
                                       Utility.AdaptRelativePathToPlatform("textures/bookstore/knife.jpg"));
            textFont = new Text2D(Quake3FilesPath + Utility.AdaptRelativePathToPlatform("fonts/NeHe.Lesson17.Font.bmp"));
        }

        private void InitializeForces()
        {
            RunForce = Forces.Add(new Force());
            Forces[RunForce].MinVelocity = new Vector3f(0f, 0f, 0f);
            Forces[RunForce].MaxVelocity = new Vector3f(300f, 300f, 300f);
            Forces[RunForce].Acceleration = new Vector3f(-80000f, -80000f, -80000f);
            StrafeForce = Forces.Add(new Force());
            Forces[StrafeForce].MinVelocity = new Vector3f(0f, 0f, 0f);
            Forces[StrafeForce].MaxVelocity = new Vector3f(300f, 300f, 300f);
            Forces[StrafeForce].Acceleration = new Vector3f(-150000f, -150000f, -150000f);
            GravityForce = Forces.Add(new Force());
            Forces[GravityForce].MaxVelocity = new Vector3f(0f, 1500f, 0f);
            Forces[GravityForce].Direction.Y = -4.5f;
            JumpForce = Forces.Add(new Force());
            Forces[JumpForce].MaxVelocity = new Vector3f(0f, 500f, 0f);
            Forces[JumpForce].Acceleration.Y = -40000f;
            Forces[JumpForce].Direction.Y = 1f;
            BlastForce = Forces.Add(new Force());
            Forces[BlastForce].MaxVelocity = new Vector3f(200f, 0f, 200f);
            Forces[BlastForce].Acceleration = new Vector3f(-22000f, 0f, -22000f);
        }

        private void InitializeGraphics()
        {
            try
            {
                GL.ClearColor(0f, 0f, 0f, 0f);
                GL.ShadeModel(ShadingModel.Smooth);
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.DepthTest);
                GL.ClearDepth(1.0);
                GL.DepthFunc(DepthFunction.Lequal);
                GL.DepthRange(0.0, 1.0);
                GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
                GL.CullFace(CullFaceMode.Front);
                GL.Enable(EnableCap.CullFace);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                OnResizeControl();
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to initialize OpenGL", exception);
            }
        }

        private void InitializeInput()
        {
            /* miki-input-off 
			try
			{
				this.KeyboardDevice = new InputDevice(SystemGuid.Keyboard);
				this.KeyboardDevice.SetCooperativeLevel(this.RenderForm, CooperativeLevelFlags.Foreground | CooperativeLevelFlags.NonExclusive);
				this.KeyboardDevice.Acquire();
			}
			catch
			{
			}
			try
			{
				this.MouseDevice = new InputDevice(SystemGuid.Mouse);
				this.MouseDevice.SetCooperativeLevel(this.RenderForm, CooperativeLevelFlags.Foreground | CooperativeLevelFlags.Exclusive);
				this.MouseDevice.Acquire();
			}
			catch
			{
			}
			*/
        }

        private void InitializeNetwork()
        {
            /* miki-play-off 
			if (this.DPClient != null)
			{
				this.DPClient.Dispose();
				this.DPClient = null;
			}
			this.DPClient = new PlayClient();
			//Hook events
			DPClient.FindHostResponse += new FindHostResponseEventHandler(DPClient_FindHostResponse);
			DPClient.ConnectComplete += new ConnectCompleteEventHandler(DPClient_ConnectComplete);
			DPClient.SessionTerminated += new SessionTerminatedEventHandler(DPClient_SessionTerminated);
			DPClient.Receive += new ReceiveEventHandler(DPClient_Receive);
			*/
            //Hook default engine events
            ConnectedToServer += Engine_ConnectedToServer;
            DisconnectedFromServer += Engine_DisconnectedFromServer;
            ChatMessageRecieved += Engine_ChatMessageRecieved;
            AddPlayer += Engine_AddPlayer;
            RemovePlayer += Engine_RemovePlayer;
            TriggerActivated += Engine_TriggerActivated;

            HealthChanged = (EventHandler) Delegate.Combine(HealthChanged, new EventHandler(Engine_HealthChanged));
            ServerTimeChanged =
                (EventHandler) Delegate.Combine(ServerTimeChanged, new EventHandler(Engine_ServerTimeChanged));
            ScoreChanged = (EventHandler) Delegate.Combine(ScoreChanged, new EventHandler(Engine_ScoreChanged));
            GameOver = (GameOverHandler) Delegate.Combine(GameOver, new GameOverHandler(Engine_GameOver));
            RoundInfoChanged =
                (EventHandler) Delegate.Combine(RoundInfoChanged, new EventHandler(Engine_RoundInfoChanged));
            AmmoChanged = (EventHandler) Delegate.Combine(AmmoChanged, new EventHandler(Engine_AmmoChanged));
            WeaponChanged = (EventHandler) Delegate.Combine(WeaponChanged, new EventHandler(Engine_WeaponChanged));
            QuitGame = (EventHandler) Delegate.Combine(QuitGame, new EventHandler(Engine_QuitGame));
        }

        private void InitializeSound()
        {
            // miki-sound-off this.EngineSoundManager = new SoundManager(this.RenderForm);
            // miki-sound-off string str = Quake3FilesPath + @"sounds\";
            // miki-sound-off this.EngineSoundManager.LoadSound("swing", str + "swing.wav");
            // miki-sound-off this.EngineSoundManager.LoadSound("laser", str + "laser.wav");
            // miki-sound-off this.EngineSoundManager.LoadSound("pain", str + "pain.wav");
            // miki-sound-off this.EngineSoundManager.LoadSound("death", str + "death.wav");
            // miki-sound-off this.EngineSoundManager.LoadSound("respawn", str + "respawn.wav");
            // miki-sound-off this.EngineSoundManager.LoadSound("item", str + "powerup.wav");
            // miki-sound-off this.EngineSoundManager.LoadSound("marked", str + "marked.wav");
            // miki-sound-off this.EngineSoundManager.LoadSound("noammo", str + "noammo.wav");
            // miki-sound-off this.EngineSoundManager.LoadSound("hurt", str + "hurt.wav");
            // miki-sound-off this.EngineSoundManager.LoadSound("jump", str + "jump.wav");
        }

        public void LoadMap(string DirectoryPath, string MapName)
        {
            Directory.SetCurrentDirectory(DirectoryPath);
            CurrentMap = new BSPFile(MapName);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            var entityArray = CurrentMap.Entities.SeekEntitiesByClassname("info_player_deathmatch");
            foreach (var entity in entityArray)
            {
                var strArray = entity.SeekValuesByArgument("origin");
                if (strArray.Length > 0)
                {
                    var strArray2 = Regex.Split(strArray[0], " ");
                    if (strArray2.Length == 3)
                    {
                        var vector = new Vector3f();
                        try
                        {
                            vector.X += float.Parse(strArray2[0]);
                            vector.Z += -float.Parse(strArray2[1]);
                            vector.Y += float.Parse(strArray2[2]);
                            SpawnPoints.Add(vector);
                        }
                        catch
                        {
                        }
                    }
                }
                var strArray3 = entityArray[0].SeekValuesByArgument("angle");
                if (strArray3.Length > 0)
                {
                    try
                    {
                        var num = float.Parse(strArray3[0]);
                        MapCamera.Yaw = 90f - num;
                    }
                    catch
                    {
                    }
                }
            }
            OnResizeControl();
            ChooseSpawnPoint();
        }

        private void OnResizeControl()
        {
            GL.Viewport(0, 0, winWidth, winHeight);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            var num = (winWidth + 0.1f)/winHeight;
            gluPerspective(70.0, num, 1.0, 5000.0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void PrepareRenderMap()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.LoadIdentity();
        }

        public void ReAcquireInput()
        {
            /*
			if (this.MouseDevice != null)
			{
				try
				{
					this.MouseDevice.Acquire();
				}
				catch
				{
				}
			}
			if (this.KeyboardDevice != null)
			{
				try
				{
					this.KeyboardDevice.Acquire();
				}
				catch
				{
				}
			}
			*/
        }

        public void Render()
        {
            if (CurrentMap != null)
            {
                Hashtable hashtable;
                PrepareRenderMap();
                MapCamera.UpdateView();
                MapFrustrum.UpdateFrustrum();
                CurrentMap.RenderLevel(MapCamera.Position, MapFrustrum);
                lock ((hashtable = Items))
                {
                    GL.Disable(EnableCap.Texture2D);
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha);
                    foreach (Item item in Items.Values)
                    {
                        if (!item.Active)
                        {
                            continue;
                        }
                        GL.PushMatrix();
                        GL.Translate(item.Position.X, item.Position.Y, item.Position.Z);
                        switch (item.IType)
                        {
                            case ItemType.Health:
                                GL.Color4(0f, 0f, 1f, item.Alpha);
                                break;

                            case ItemType.Special:
                                GL.Color4(0f, 1f, 0f, item.Alpha);
                                break;

                            case ItemType.Ammo:
                                GL.Color4(1f, 0f, 0f, item.Alpha);
                                break;
                        }
                        // miki Glu.gluSphere(item.Quad, 10.0, 15, 15);
                        if (item.AlphaDir)
                        {
                            item.Alpha += 0.5f*TimeElapsed;
                            if (item.Alpha > 1f)
                            {
                                item.Alpha = 1f;
                                item.AlphaDir = false;
                            }
                        }
                        else
                        {
                            item.Alpha -= 0.5f*TimeElapsed;
                            if (item.Alpha < 0.3f)
                            {
                                item.Alpha = 0.3f;
                                item.AlphaDir = true;
                            }
                        }
                        GL.PopMatrix();
                    }
                    GL.Color4(1f, 1f, 1f, 1f);
                    GL.Enable(EnableCap.Texture2D);
                    GL.Disable(EnableCap.Blend);
                }
                lock ((hashtable = NetworkPlayers))
                {
                    foreach (NetworkPlayer player in NetworkPlayers.Values)
                    {
                        player.Update(TimeElapsed);
                        //this.textFont.glPrint(player.PlayerModel.Position.X - player.PlayerModel.Center.X, player.PlayerModel.BoundMax.Y + player.PlayerModel.Position.Y, player.PlayerModel.Position.Z + player.PlayerModel.Center.Z, player.Name);
                    }
                }
                if ((HurtTime > 0f) || (DeathTime > 0f))
                {
                    HurtTime -= TimeElapsed;
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    GL.Enable(EnableCap.Blend);
                    GL.Disable(EnableCap.DepthTest);
                    GL.CullFace(CullFaceMode.Back);
                    GL.DepthMask(false);
                    GL.Disable(EnableCap.Texture2D);
                    //GL.BlendFunc(0x300, 0x306); 
                    //GL.BlendFunc(BlendingFactorSrc.?, BlendingFactorDest.DstColor);
                    GL.Color4(0.5f, 0f, 0f, 0.8f);
                    GL.Begin(BeginMode.Quads);
                    GL.Vertex3(-10f, -10f, -10f);
                    GL.Vertex3(10f, -10f, -10f);
                    GL.Vertex3(10f, 10f, -10f);
                    GL.Vertex3(-10f, 10f, -10f);
                    GL.End();
                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthMask(true);
                    GL.CullFace(CullFaceMode.Front);
                    GL.Disable(EnableCap.Blend);
                    GL.Enable(EnableCap.Texture2D);
                    GL.PopMatrix();
                }
                else if (Marked)
                {
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    GL.Enable(EnableCap.Blend);
                    GL.Disable(EnableCap.DepthTest);
                    GL.CullFace(CullFaceMode.Back);
                    GL.DepthMask(false);
                    GL.Disable(EnableCap.Texture2D);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha);
                    GL.Color4(1f, 1f, 0f, 0.2f);
                    GL.Begin(BeginMode.Quads);
                    GL.Vertex3(-10f, -10f, -10f);
                    GL.Vertex3(10f, -10f, -10f);
                    GL.Vertex3(10f, 10f, -10f);
                    GL.Vertex3(-10f, 10f, -10f);
                    GL.End();
                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthMask(true);
                    GL.CullFace(CullFaceMode.Front);
                    GL.Disable(EnableCap.Blend);
                    GL.Enable(EnableCap.Texture2D);
                    GL.PopMatrix();
                }
                lock (LaserBeams)
                {
                    for (var i = 0; i < LaserBeams.Count; i++)
                    {
                        var info = (LaserBeamInfo) LaserBeams[i];
                        if (info.TimeRemaining > 0f)
                        {
                            info.TimeRemaining -= TimeElapsed;
                            GL.Disable(EnableCap.CullFace);
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha);
                            GL.Disable(EnableCap.Texture2D);
                            GL.Begin(BeginMode.Triangles);
                            GL.Color4(0f, 0f, 1f, 0.8f);
                            GL.Vertex3(info.End.X, info.End.Y, info.End.Z);
                            GL.Color4(0f, 0.2f, 0.8f, 0.9f);
                            GL.Vertex3(info.Start.X, info.Start.Y - 5f, info.Start.Z);
                            GL.Color4(0f, 0.8f, 0.3f, 0.9f);
                            GL.Vertex3(info.Start2.X, info.Start2.Y - 5f, info.Start2.Z);
                            GL.Color4(0f, 0f, 1f, 0.8f);
                            GL.Vertex3(info.End.X, info.End.Y, info.End.Z);
                            GL.Color4(0f, 0.2f, 0.8f, 0.9f);
                            GL.Vertex3(info.Start.X, info.Start.Y - 6f, info.Start.Z);
                            GL.Color4(0f, 0.8f, 0.3f, 0.9f);
                            GL.Vertex3(info.Start2.X, info.Start2.Y - 6f, info.Start2.Z);
                            GL.Color4(0f, 0f, 1f, 0.8f);
                            GL.Vertex3(info.End.X, info.End.Y, info.End.Z);
                            GL.Color4(0f, 0.2f, 0.8f, 0.9f);
                            GL.Vertex3(info.Start.X, info.Start.Y - 5f, info.Start.Z);
                            GL.Color4(0f, 0.8f, 0.3f, 0.9f);
                            GL.Vertex3(info.Start.X, info.Start.Y - 6f, info.Start.Z);
                            GL.Color4(0f, 0f, 1f, 0.8f);
                            GL.Vertex3(info.End.X, info.End.Y, info.End.Z);
                            GL.Color4(0f, 0.2f, 0.8f, 0.9f);
                            GL.Vertex3(info.Start2.X, info.Start.Y - 5f, info.Start.Z);
                            GL.Color4(0f, 0.8f, 0.3f, 0.9f);
                            GL.Vertex3(info.Start2.X, info.Start2.Y - 6f, info.Start2.Z);
                            GL.End();
                            GL.Begin(BeginMode.Quads);
                            GL.Color4(0f, 0f, 1f, 0.8f);
                            GL.Vertex3(info.Start.X, info.Start.Y - 5f, info.Start.Z);
                            GL.Color4(0f, 0.2f, 0.8f, 0.9f);
                            GL.Vertex3(info.Start.X, info.Start.Y - 6f, info.Start.Z);
                            GL.Color4(0f, 0.8f, 0.3f, 0.9f);
                            GL.Vertex3(info.Start2.X, info.Start2.Y - 6f, info.Start2.Z);
                            GL.Color4(0f, 0f, 1f, 0.8f);
                            GL.Vertex3(info.Start2.X, info.Start2.Y - 5f, info.Start2.Z);
                            GL.End();
                            GL.Disable(EnableCap.Blend);
                            GL.Enable(EnableCap.Texture2D);
                            GL.Enable(EnableCap.CullFace);
                        }
                        else
                        {
                            LaserBeams.RemoveAt(i);
                            i--;
                        }
                    }
                }
                if (Weapon == WeaponType.Knife)
                {
                    GL.PushMatrix();
                    GL.LoadIdentity();
                    GL.Disable(EnableCap.DepthTest);
                    GL.Disable(EnableCap.CullFace);
                    GL.DepthMask(false);
                    GL.BindTexture(TextureTarget.Texture2D, KnifeTexture.TextureID);
                    if (StabTime > 0f)
                    {
                        StabTime -= TimeElapsed;
                        GL.Begin(BeginMode.Triangles);
                        GL.TexCoord2(0f, 0f);
                        GL.Vertex3(5f, -10f, -10f);
                        GL.TexCoord2(0f, 1f);
                        GL.Vertex3(9f, -10f, -10f);
                        GL.TexCoord2(1f, 0f);
                        GL.Vertex3(0f, -2f, -10f);
                        GL.End();
                    }
                    else
                    {
                        GL.Begin(BeginMode.Triangles);
                        GL.TexCoord2(0f, 0f);
                        GL.Vertex3(5f, -10f, -10f);
                        GL.TexCoord2(0f, 1f);
                        GL.Vertex3(9f, -10f, -10f);
                        GL.TexCoord2(1f, 0f);
                        GL.Vertex3(2f, -4f, -10f);
                        GL.End();
                    }
                    GL.Enable(EnableCap.DepthTest);
                    GL.Enable(EnableCap.CullFace);
                    GL.DepthMask(true);
                    GL.CullFace(CullFaceMode.Front);
                    GL.PopMatrix();
                }
                else if (Weapon == WeaponType.Laser)
                {
                    if (LaserTime > 0f)
                    {
                        LaserTime -= TimeElapsed;
                    }
                    GL.PushMatrix();
                    GL.Disable(EnableCap.DepthTest);
                    GL.Disable(EnableCap.CullFace);
                    GL.LoadIdentity();
                    GL.Disable(EnableCap.Texture2D);
                    GL.Begin(BeginMode.Quads);
                    GL.Color3(1f, 0f, 0f);
                    GL.Vertex3(5f, -10f, -5f);
                    GL.Color3(0.7f, 0f, 0f);
                    GL.Vertex3(12f, -10f, -5f);
                    GL.Color3(0.7f, 0f, 0f);
                    GL.Vertex3(12f, -10f, -25f);
                    GL.Color3(1f, 0f, 0f);
                    GL.Vertex3(5f, -10f, -25f);
                    GL.Color3(0.7f, 0.7f, 0.7f);
                    GL.Vertex3(5f, -10f, -5f);
                    GL.Color3(0.7f, 0.7f, 0.7f);
                    GL.Vertex3(5f, -10f, -25f);
                    GL.Color3(0.3f, 0.3f, 0.3f);
                    GL.Vertex3(5f, -12f, -25f);
                    GL.Color3(0.3f, 0.3f, 0.3f);
                    GL.Vertex3(5f, -12f, -5f);
                    GL.End();
                    GL.Enable(EnableCap.Texture2D);
                    GL.Enable(EnableCap.CullFace);
                    GL.Enable(EnableCap.DepthTest);
                    GL.PopMatrix();
                }
            }
            else
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
        }

        public void Respawn()
        {
            // miki-sound-off this.EngineSoundManager.PlaySound("respawn");
            if (Connected)
            {
                // miki-play-off NetworkPacket sendData = new NetworkPacket();
                // miki-play-off sendData.Write(MessageType.Respawn);
                // miki-play-off sendData.Write(this.m_NetworkID);
                // miki-play-off this.DPClient.Send(sendData, 0, SendFlags.Guaranteed);
            }
            Health = 100;
            HealthChanged(this, null);
            Ammo = 10;
            AmmoChanged(this, null);
            Forces.ClearDisposableForces();
            ChooseSpawnPoint();
        }

        private void SendHurt(int ID)
        {
            /* miki-play-off NetworkPacket sendData = new NetworkPacket();
			sendData.Write(MessageType.Hurt);
			sendData.Write(this.m_NetworkID);
			sendData.Write(ID);
			if (this.Weapon == WeaponType.Knife)
			{
				sendData.Write(10f);
			}
			else if (this.Weapon == WeaponType.Laser)
			{
				sendData.Write(20f);
			}
			sendData.Write(this.MapCamera.Position.X);
			sendData.Write(this.MapCamera.Position.Y);
			sendData.Write(this.MapCamera.Position.Z);
			this.DPClient.Send(sendData, 0, SendFlags.Guaranteed);
			*/
        }

        public void SendMessage(int ID, string Message, bool IsPrivate)
        {
            /* miki-play-off 
			NetworkPacket sendData = new NetworkPacket();
			if (IsPrivate)
			{
				sendData.Write(MessageType.PrivateMessage);
			}
			else
			{
				sendData.Write(MessageType.ChatMessage);
			}
			sendData.Write(ID);
			sendData.Write(Message);
			this.DPClient.Send(sendData, 0, SendFlags.Guaranteed);
			*/
        }

        private void SendMovementState()
        {
            var num = 90f - MapCamera.Yaw;
            LastState.X = MapCamera.Position.X;
            LastState.Y = MapCamera.Position.Y;
            LastState.Z = MapCamera.Position.Z;
            LastState.Yaw = MapCamera.Yaw;
            /* miki-play-off 
			NetworkPacket sendData = new NetworkPacket();
			sendData.Write(3);
			sendData.Write(this.m_NetworkID);
			sendData.Write(this.MapCamera.Position.X);
			sendData.Write(this.MapCamera.Position.Y);
			sendData.Write(this.MapCamera.Position.Z);
			sendData.Write(num);
			this.DPClient.Send(sendData, 0, SendFlags.Guaranteed | SendFlags.Sync);
			*/
        }

        public void SetVoiceTarget(int TargetID)
        {
            if (VoiceEnabled)
            {
                // miki-play-off this.ClientVoice.TransmitTargets = new int[] { TargetID };
            }
        }

        private bool TrySlide(Vector3f SourceVector, Vector3f ProjectedPos)
        {
            var vector = new Vector3f();
            for (var vector2 = new Vector3f(ProjectedPos.X, ProjectedPos.Y, ProjectedPos.Z);
                ((Math.Abs(vector2.X) > 0.0001f) || (Math.Abs(vector2.Y) > 0.0001f)) || (Math.Abs(vector2.Z) > 0.0001f);
                vector2 = new Vector3f(vector.X, vector.Y, vector.Z))
            {
                CurrentMap.DetectCollisionBox(SourceVector, SourceVector + vector2, PlayerState.BoundMin,
                    PlayerState.BoundMax);
                if (CurrentMap.CollisionInfo.Fraction > 0f)
                {
                    MapCamera.MoveCameraTo(CurrentMap.CollisionInfo.EndPoint);
                    return true;
                }
                var vector3 = new Vector3f();
                vector3.CopyFrom(CurrentMap.CollisionInfo.Normal);
                vector3.Normalize();
                vector = vector2 - vector3*vector3.Dot(vector2);
            }
            return false;
        }

        private bool TryStepUpAndSlide(Vector3f ProjectedPos)
        {
            CurrentMap.DetectCollisionBox(MapCamera.Position, MapCamera.Position + PlayerState.Step,
                PlayerState.BoundMin, PlayerState.BoundMax);
            if (CurrentMap.CollisionInfo.Fraction == 0f)
            {
                return false;
            }
            if (!TrySlide(CurrentMap.CollisionInfo.EndPoint, ProjectedPos))
            {
                return false;
            }
            CurrentMap.DetectCollisionBox(MapCamera.Position, MapCamera.Position - PlayerState.Step,
                PlayerState.BoundMin, PlayerState.BoundMax);
            if (CurrentMap.CollisionInfo.Fraction == 0f)
            {
                return false;
            }
            MapCamera.MoveCameraTo(CurrentMap.CollisionInfo.EndPoint);
            return true;
        }

        public void UpdateFrame(float TimeElapsed)
        {
            this.TimeElapsed = TimeElapsed;

            var auxFps = 1/TimeElapsed;

            if (!IsPaused)
            {
                if (CurrentMap != null)
                {
                    ArrayList list;
                    Hashtable hashtable;
                    lock ((list = IncomingPlayers))
                    {
                        foreach (NetworkPlayer player in IncomingPlayers)
                        {
                            Directory.SetCurrentDirectory(Quake3FilesPath + "models");
                            player.PlayerModel = new MD2Model(player.ModelName + ".md2", player.ModelName + ".bmp");
                            if (player.InitialState == null)
                            {
                                player.PlayerModel.Position = new Vector3f();
                            }
                            else
                            {
                                player.PlayerModel.Position = new Vector3f(player.InitialState.X, 0f,
                                    player.InitialState.Z);
                                player.PlayerModel.Yaw = player.InitialState.Yaw;
                            }
                            // miki-sound-off player.LaserSound = this.EngineSoundManager.GenerateName();
                            // miki-sound-off this.EngineSoundManager.LoadSound3D(player.LaserSound, Quake3FilesPath + @"sounds\laser.wav");
                            lock ((hashtable = NetworkPlayers))
                            {
                                NetworkPlayers[player.ID] = player;
                            }
                        }
                        IncomingPlayers.Clear();
                    }
                    lock ((list = OutgoingPlayerIDs))
                    {
                        foreach (int num in OutgoingPlayerIDs)
                        {
                            lock ((hashtable = NetworkPlayers))
                            {
                                if (NetworkPlayers.ContainsKey(num))
                                {
                                    NetworkPlayers.Remove(num);
                                }
                            }
                        }
                        OutgoingPlayerIDs.Clear();
                    }
                    UpdateInput(ref PlayerState);
                    if (IsDead)
                    {
                        DeathTime -= TimeElapsed;
                        if (DeathTime <= 0f)
                        {
                            Respawn();
                        }
                    }
                    else
                    {
                        UpdatePlayerState(PlayerState);
                        var itemCollision = GetItemCollision();
                        if (itemCollision != null)
                        {
                            var flag = false;
                            switch (itemCollision.IType)
                            {
                                case ItemType.Health:
                                    if (Health < 100)
                                    {
                                        flag = true;
                                        Health += 0x19;
                                        if (Health > 100)
                                        {
                                            Health = 100;
                                        }
                                        HealthChanged(this, null);
                                    }
                                    break;

                                case ItemType.Special:
                                    flag = true;
                                    Score++;
                                    ScoreChanged(this, null);
                                    break;

                                case ItemType.Ammo:
                                    if (Ammo < 20)
                                    {
                                        flag = true;
                                        Ammo += 5;
                                        if (Ammo > 20)
                                        {
                                            Ammo = 20;
                                        }
                                        AmmoChanged(this, null);
                                    }
                                    break;
                            }
                            if (flag)
                            {
                                itemCollision.Active = false;
                                // miki-sound-off this.EngineSoundManager.PlaySound("item");
                                /* miki-play-off 
								NetworkPacket sendData = new NetworkPacket();
								sendData.Write(MessageType.ItemState);
								sendData.Write(this.m_NetworkID);
								sendData.Write(itemCollision.ID);
								sendData.Write(false);
								this.DPClient.Send(sendData, 0, SendFlags.Guaranteed);
								*/
                            }
                        }
                    }
                    if (Connected && NextStateReady)
                    {
                        SendMovementState();
                    }
                }

                //gameWin.Title = "PosX = " + PlayerState.PlayerLook.X + " PosY = " + PlayerState.PlayerLook.Y;

                // miki-sound-off this.EngineSoundManager.SetListenerPosition(this.MapCamera.Position);

                Render();

                GL.Enable(EnableCap.Blend);
                textFont.glPrint(10, 20,
                    $"Position: X: {MapCamera.Position.X} Y: {MapCamera.Position.Y} Z: {MapCamera.Position.Z}", 0);
            }
        }

        private void UpdateMouseDelta()
        {
            pointer_current = Cursor.Position;
            pointer_delta = new Size(pointer_current.X - pointer_previous.X,
                pointer_current.Y - pointer_previous.Y);
        }

        private void UpdateMousePosition()
        {
            if (CursorHide == false)
            {
                Cursor.Hide();
                CursorHide = true;
            }

            Cursor.Position =
                new Point(gameWin.Bounds.Left + (gameWin.Bounds.Width/2),
                    gameWin.Bounds.Top + (gameWin.Bounds.Height/2));

            pointer_previous = Cursor.Position;
        }

        private void UpdateInput(ref CommandSet State)
        {
/*
			State.PlayerMovement.X = 0f;
			State.PlayerMovement.Y = 0f;
			State.PlayerMovement.Z = 0f;
			State.PlayerLook.X = 0f;
			State.PlayerLook.Y = 0f;*/
            if (m_ProcessInput)
            {
                ProcessKeyboardEvents();
                ProcessMouseEvents(ref State);
            }
        }

        private void ProcessKeyboardEvents()
        {
            try
            {
                // miki-input-off KeyboardState currentKeyboardState = this.KeyboardDevice.GetCurrentKeyboardState();
                if (gameWin.Keyboard[ControlCfg.MoveForward])
                {
                    var angle = Utility.CapAngle(90f - MapCamera.Yaw);
                    Forces[RunForce].Direction.X = Utility.CosDeg(angle);
                    Forces[RunForce].Direction.Z = -Utility.SinDeg(angle);
                    Forces[RunForce].AddVelocityX(Forces[RunForce].MaxVelocity.X);
                    Forces[RunForce].AddVelocityZ(Forces[RunForce].MaxVelocity.Z);
                }
                else if (gameWin.Keyboard[ControlCfg.MoveBack])
                {
                    var num2 = Utility.CapAngle(90f - MapCamera.Yaw);
                    Forces[RunForce].Direction.X = -Utility.CosDeg(num2);
                    Forces[RunForce].Direction.Z = Utility.SinDeg(num2);
                    Forces[RunForce].AddVelocityX(Forces[RunForce].MaxVelocity.X);
                    Forces[RunForce].AddVelocityZ(Forces[RunForce].MaxVelocity.Z);
                }
                if (gameWin.Keyboard[ControlCfg.LeftStrafe])
                {
                    var num3 = Utility.CapAngle(-MapCamera.Yaw);
                    Forces[StrafeForce].Direction.X = -Utility.CosDeg(num3);
                    Forces[StrafeForce].Direction.Z = Utility.SinDeg(num3);
                    Forces[StrafeForce].AddVelocityX(Forces[StrafeForce].MaxVelocity.X);
                    Forces[StrafeForce].AddVelocityZ(Forces[StrafeForce].MaxVelocity.Z);
                }
                else if (gameWin.Keyboard[ControlCfg.RightStrafe])
                {
                    var num4 = Utility.CapAngle(-MapCamera.Yaw);
                    Forces[StrafeForce].Direction.X = Utility.CosDeg(num4);
                    Forces[StrafeForce].Direction.Z = -Utility.SinDeg(num4);
                    Forces[StrafeForce].AddVelocityX(Forces[StrafeForce].MaxVelocity.X);
                    Forces[StrafeForce].AddVelocityZ(Forces[StrafeForce].MaxVelocity.Z);
                }
                if (EnableJump)
                {
                    if (gameWin.Keyboard[ControlCfg.Jump] && (Forces[GravityForce].GetVelocityY() <= 0f))
                    {
                        EnableJump = false;
                        // miki-sound-off this.EngineSoundManager.PlaySound("jump");
                        Forces[JumpForce].AddVelocityY(Forces[JumpForce].MaxVelocity.Y);
                    }
                }
                else if (!gameWin.Keyboard[ControlCfg.Jump])
                {
                    EnableJump = true;
                }
                /*       if (currentKeyboardState[(int)Keys.K])
					   {
						   this.Die(this.m_NetworkID);
					   }
					   if (currentKeyboardState[(int)Keys.Escape])
					   {
						   this.QuitGame(this, null);
					   }
					   if ((this.Weapon != WeaponType.Knife) && currentKeyboardState[(int)Keys.D1])
					   {
						   this.Weapon = WeaponType.Knife;
						   this.WeaponChanged(this, null);
					   }
					   if ((this.Weapon != WeaponType.Laser) && currentKeyboardState[(int)Keys.D2])
					   {
						   this.Weapon = WeaponType.Laser;
						   this.WeaponChanged(this, null);
					   }*/
            }
            catch
            {
            }
        }

        private void ProcessMouseEvents(ref CommandSet State)
        {
            try
            {
                //MouseState currentMouseState = this.MouseDevice.CurrentMouseState;
                //byte[] mouseButtons = currentMouseState.GetMouseButtons();

                if (acquireMouse)
                {
                    UpdateMouseDelta();

                    PlayerState.PlayerLook.X = -pointer_delta.Width;

                    if (ControlCfg.InvertMouse)
                    {
                        PlayerState.PlayerLook.Y = pointer_delta.Height;
                    }
                    else
                    {
                        PlayerState.PlayerLook.Y = -pointer_delta.Height;
                    }

                    UpdateMousePosition();
                }
                else if (CursorHide)
                {
                    Cursor.Show();
                    CursorHide = false;
                }
            }
            catch
            {
            }
        }

        private void UpdatePlayerState(CommandSet PlayerState)
        {
            var num = TimeElapsed*ControlCfg.MouseSpeedHorz;
            var num2 = TimeElapsed*ControlCfg.MouseSpeedVert;
            MapCamera.SetMouseView(PlayerState.PlayerLook.X*num, PlayerState.PlayerLook.Y*num2);
            CurrentMap.DetectCollisionBox(MapCamera.Position, MapCamera.Position - PlayerState.Step,
                PlayerState.BoundMin, PlayerState.BoundMax);
            if (CurrentMap.CollisionInfo.Fraction > 0f)
            {
                if (Forces[GravityForce].GetVelocityY() <= 0f)
                {
                    Forces[GravityForce].Acceleration.Y = 20000f;
                }
            }
            else
            {
                Forces[GravityForce].Acceleration.Y = 0f;
                Forces[GravityForce].SetVelocity(0f, 0f, 0f);
            }
            var sourcePoint = new Vector3f(MapCamera.Position.X, MapCamera.Position.Y, MapCamera.Position.Z);
            lock (Forces)
            {
                Forces.ApplyAllForces(sourcePoint, TimeElapsed);
            }
            var projectedPos = sourcePoint - MapCamera.Position;
            if (!TryStepUpAndSlide(projectedPos))
            {
                TrySlide(MapCamera.Position, projectedPos);
            }
        }

        private delegate void CreateItemDelegate(int ID, ItemType IType, Vector3f Pos, bool Active);

        #region Utilizados en gluLookAt()

        private static readonly Vector3f forward = new Vector3f();
        private static Vector3f side = new Vector3f();
        private static Vector3f up = new Vector3f();
        private static readonly Matrix4f m = new Matrix4f();

        #endregion Utilizados en gluLookAt()

        #region Util gluLookAt() - CalculateAverageTime_FPS() - SetColor()

        /// <summary>
        ///     From Mesa Lib
        /// </summary>
        /// <param name="eyex"></param>
        /// <param name="eyey"></param>
        /// <param name="eyez"></param>
        /// <param name="centerx"></param>
        /// <param name="centery"></param>
        /// <param name="centerz"></param>
        /// <param name="upx"></param>
        /// <param name="upy"></param>
        /// <param name="upz"></param>
        internal static void gluLookAt(float eyex, float eyey, float eyez,
            float centerx, float centery, float centerz,
            float upx, float upy, float upz)
        {
            forward.X = centerx - eyex;
            forward.Y = centery - eyey;
            forward.Z = centerz - eyez;

            up.X = upx;
            up.Y = upy;
            up.Z = upz;

            forward.Normalize();

            /* Side = forward x up */
            Vector3f.Cross(forward, up, ref side);
            side.Normalize();

            /* Recompute up as: up = side x forward */
            Vector3f.Cross(side, forward, ref up);

            m.SetRight(side.X, up.X, -forward.X, 0);
            m.SetUp(side.Y, up.Y, -forward.Y, 0);
            m.SetForward(side.Z, up.Z, -forward.Z, 0);
            m.SetTranslation(0, 0, 0, 1);

            GL.MultMatrix(m.values);
            GL.Translate(-eyex, -eyey, -eyez);
        }

/*
		void CalculateAverageTime_FPS(double renderTime)
		{
			numPromedio++;
			acumTiempoPro += renderTime;
			if (numPromedio >= AverageFactor_)
			{
				AverageFrameTime = acumTiempoPro / numPromedio;
				acumTiempoPro = 0;
				numPromedio = 0;
			}
		}*/

        public void SetDefaultModelColor()
        {
            GL.Color4(0.8f, 0.8f, 0.8f, 1f);
        }

        public void SetColor(TColor pColor)
        {
            GL.Color4(pColor.R, pColor.G, pColor.B, pColor.A);
        }

        #endregion Util gluLookAt() - CalculateAverageTime_FPS() - SetColor()

        #region projection-Perspective-Viewport

        public void SetViewport(int Width, int Height)
        {
            winWidth = Width;
            winHeight = Height;

            aspectRatio = Width/(double) Height;

            GL.Viewport(0, 0, Width, Height);
        }

        public void SetProjection(ProjectionType proType)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            switch (proType)
            {
                case ProjectionType.Perspective:
                    gluPerspective(fieldOfView, aspectRatio,
                        nearClipDistance, farClipDistance);
                    break;
                case ProjectionType.Ortho2D:
                    gluOrtho2D(0, winWidth, winHeight, 0);
                    break;
            }
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        internal void gluOrtho2D(double left, double right, double bottom, double top)
        {
            GL.Ortho(left, right, bottom, top, -1.0, 1.0);
        }

        internal void gluPerspective(double fovy, double aspect, double zNear, double zFar)
        {
            double xmin, xmax, ymin, ymax;

            ymax = zNear*Math.Tan(fovy*Math.PI/360.0);
            ymin = -ymax;

            xmin = ymin*aspect;
            xmax = ymax*aspect;

            GL.Frustum(xmin, xmax, ymin, ymax, zNear, zFar);
        }

        #endregion projection-Perspective-Viewport
    }

    public enum ProjectionType
    {
        Perspective,
        Ortho2D
    }
}