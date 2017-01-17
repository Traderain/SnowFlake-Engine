using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Math3D;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace SnowflakeEngine.WanderEngine
{
	internal class DebugWindow
	{
		public enum msgtype
		{
			Engine,
			Game,
			Error
		}

		public static void Log(string msg, msgtype type = msgtype.Engine)
		{
#if DEBUG
			switch (type)
			{
				case msgtype.Engine:
					Console.ForegroundColor = ConsoleColor.DarkGreen;
					break;
				case msgtype.Game:
					Console.ForegroundColor = ConsoleColor.White;
					break;
				case msgtype.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
			}
			Console.WriteLine(@"[" + type.ToString().ToUpper() + @"]: " + msg);
#endif
		}
	}

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
		public static readonly int EveryoneId = 0;
		internal static int TexUnits = 1;
		// miki-play-off private VoiceClient ClientVoice = null;
		private readonly ControlConfig _controlCfg = new ControlConfig();
		private readonly double _farClipDistance = 5000.0f;
		private readonly double _fieldOfView = 45.0f;
		// miki private bool FireEnabled = true;
		private readonly ForceCollection _forces = new ForceCollection();
		private readonly GameWindow _gameWin;
		private readonly Random _generator = new Random();
		private readonly ArrayList _incomingPlayers = new ArrayList();
		private readonly Hashtable _items = new Hashtable();
		private readonly ArrayList _laserBeams = new ArrayList();

		private readonly NetworkPlayerState _lastState = new NetworkPlayerState(float.NaN, float.NaN, float.NaN,
			float.NaN);

		private readonly Camera _mapCamera = new Camera();
		private readonly Frustrum _mapFrustrum = new Frustrum();
		private readonly bool _mProcessInput = true;
		private readonly double _nearClipDistance = 1.0f;
		private readonly Hashtable _networkPlayers = new Hashtable();
		private readonly bool _nextStateReady = true;
		private readonly ArrayList _outgoingPlayerIDs = new ArrayList();
		private readonly ArrayList _spawnPoints = new ArrayList();
		private readonly bool _voiceEnabled = false;
		private bool _acquireMouse = true;
		private double _aspectRatio = 1.0f;
		//private static readonly Guid ApplicationGuid = new Guid("{DCC56EE8-0265-4e9b-91E8-A1210B12E0AC}");
		private int _blastForce = -1;
		private BspFile _currentMap;
		private bool _cursorHide;
		private float _deathTime;
		private int _gravityForce = -1;
		private float _hurtTime;
		private int _jumpForce = -1;
		// miki-input-off private InputDevice KeyboardDevice = null;
		private Texture _knifeTexture;
		private float _laserTime;
		private CommandSet _playerState = new CommandSet();
		private Point _pointerCurrent, _pointerPrevious;
		private Size _pointerDelta;
		private int _runForce = -1;
		private float _stabTime;
		private int _strafeForce = -1;
		// miki-input-off private InputDevice MouseDevice = null;
		private Text2D _textFont;
		private float _timeElapsed;
		private int _winWidth, _winHeight;
		public int Ammo = 10;
		// miki-play-off private PlayClient DPClient = null;
		public bool EnableJump = true;
		public bool HasMarkedItem = false;
		public int Health = 100;
		public bool Marked;
		// miki private static readonly int Port = 0x3104;
		//private Form RenderForm = null;
		//private SimpleOpenGlControl RenderTarget = null;
		public string RoundInfo = "";
		public int Score;
		public int SecondsLeft = 0;
		//private float WalkBias = 0f;
		public WeaponType Weapon = WeaponType.Knife;

		public Engine(GameWindow gameWin)
		{
			if (gameWin == null)
				throw new ArgumentNullException(nameof(gameWin));

			_gameWin = gameWin;
			_winHeight = gameWin.Height;
			_winWidth = gameWin.Width;

			gameWin.Mouse.ButtonDown += gameWin_Mouse_ButtonDown;

			InitializeAll();
		}

		public static string Quake3FilesPath { get; set; }
		public bool Connected { get; private set; }
		public bool IsDead => (_deathTime > 0f);
		public bool IsPaused { get; set; } = false;
		public string ModelName { get; set; } = "";
		public int NetworkId { get; } = -1;
		public string PlayerName { get; set; } = "";

		private float WeaponTime
		{
			get
			{
				switch (Weapon)
				{
					case WeaponType.Knife:
						return _stabTime;
					case WeaponType.Laser:
						return _laserTime;
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
				_acquireMouse = !_acquireMouse;
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
			var num = _generator.Next(_spawnPoints.Count);
			var newPos = (Vector3F) _spawnPoints[num];
			_mapCamera.MoveCameraTo(newPos);
			_mapCamera.MoveCameraUpDown(12f);
		}

		public void ConnectToServer(IPAddress hostIp)
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

		private void CreateItem(int id, ItemType type, Vector3F pos, bool active)
		{
			//this.RenderTarget.Invoke(new CreateItemDelegate(this.CreateItem_Safe), new object[] { ID, IType, Pos, Active });
		}

		private void CreateItem_Safe(int id, ItemType type, Vector3F pos, bool active)
		{
			lock (_items)
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
			_textFont.Dispose();
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

		private void Die(int killerId)
		{
			if (!IsDead)
			{
				_deathTime = 4f;
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
			if (_voiceEnabled)
			{
				// miki-play-off this.ClientVoice.TransmitTargets = new int[0];
			}
		}

		private void DisconnectFromServer()
		{
			if (Connected)
			{
				if (_voiceEnabled)
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
			lock (_incomingPlayers)
			{
				_incomingPlayers.Add(new NetworkPlayer(e.Id, e.Name, e.ModelName, e.InitialState, e.IsMarked));
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
			lock (_outgoingPlayerIDs)
			{
				_outgoingPlayerIDs.Add(e.Id);
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
					_stabTime = 0.3f;
					var a = _mapCamera.ProjectPosition(5f, 0f);
					lock ((hashtable = _networkPlayers))
					{
						foreach (NetworkPlayer player in _networkPlayers.Values)
						{
							if ((Vector3F.Distance(a, player.PlayerModel.Position) < 45.0) &&
								(player.PlayerModel.ModelState != AnimationState.DeathFallFoward))
							{
								// miki-sound-off this.EngineSoundManager.PlaySound("pain");
								if (Connected)
								{
									SendHurt(player.Id);
								}
								player.AddColorMask(1f, 0f, 0f, 0.3f);
							}
						}
						return;
					}
				}
				if (Weapon == WeaponType.Laser)
				{
					_laserTime = 0.9f;
					if (Ammo <= 0)
					{
						// miki-sound-off this.EngineSoundManager.PlaySound("noammo");
					}
					else
					{
						// miki-sound-off this.EngineSoundManager.PlaySound("laser");
						Ammo--;
						AmmoChanged(this, null);
						_currentMap.DetectCollisionRay(_mapCamera.Position, _mapCamera.ProjectPositionWithY(10000f, 0f));
						var start = new Vector3F(_mapCamera.Position.X, _mapCamera.Position.Y, _mapCamera.Position.Z);
						var info = new LaserBeamInfo();
						info.Start = _mapCamera.ProjectPositionWithY(0f, 3f);
						info.Start2 = _mapCamera.ProjectPositionWithY(0f, 5f);
						info.End = new Vector3F(_currentMap.CollisionInfo.EndPoint.X,
							_currentMap.CollisionInfo.EndPoint.Y,
							_currentMap.CollisionInfo.EndPoint.Z);
						info.TimeRemaining = 0.3f;
						lock (_laserBeams)
						{
							_laserBeams.Add(info);
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
						lock ((hashtable = _networkPlayers))
						{
							foreach (NetworkPlayer player2 in _networkPlayers.Values)
							{
								if (player2.PlayerModel.RayCollides(start, info.End))
								{
									// miki-sound-off this.EngineSoundManager.PlaySound("pain");
									if (Connected)
									{
										SendHurt(player2.Id);
									}
									player2.AddColorMask(1f, 0f, 0f, 0.3f);
								}
							}
						}
						var angle = Utility.CapAngle(90f - _mapCamera.Yaw);
						_forces[_blastForce].Direction.X = -Utility.CosDeg(angle);
						_forces[_blastForce].Direction.Y = Utility.SinDeg(_mapCamera.Pitch);
						_forces[_blastForce].Direction.Z = Utility.SinDeg(angle);
						_forces[_blastForce].SetVelocity(_forces[_blastForce].MaxVelocity.X, 0f,
							_forces[_blastForce].MaxVelocity.Z);
					}
				}
			}
		}

		private Item GetItemCollision()
		{
		    return _items.Values.Cast<Item>().FirstOrDefault(item2 => item2.Active && (Vector3F.Distance(_mapCamera.Position, item2.Position) <= 25.0));
		}

	    private void InitializeAll()
		{
			//GL.GetInteger(GetPName.MaxTextureUnits, out texUnits);

			InitializeGraphics();
			InitializeInput();
			InitializeNetwork();
			InitializeSound();
			InitializeForces();
			_knifeTexture = new Texture(Quake3FilesPath +
										Utility.AdaptRelativePathToPlatform("textures/bookstore/knife.jpg"));
			_textFont = new Text2D(Quake3FilesPath + Utility.AdaptRelativePathToPlatform("fonts/NeHe.Lesson17.Font.bmp"));
		}

		private void InitializeForces()
		{
			_runForce = _forces.Add(new Force());
			_forces[_runForce].MinVelocity = new Vector3F(0f, 0f, 0f);
			_forces[_runForce].MaxVelocity = new Vector3F(300f, 300f, 300f);
			_forces[_runForce].Acceleration = new Vector3F(-80000f, -80000f, -80000f);
			_strafeForce = _forces.Add(new Force());
			_forces[_strafeForce].MinVelocity = new Vector3F(0f, 0f, 0f);
			_forces[_strafeForce].MaxVelocity = new Vector3F(300f, 300f, 300f);
			_forces[_strafeForce].Acceleration = new Vector3F(-150000f, -150000f, -150000f);
			_gravityForce = _forces.Add(new Force());
			_forces[_gravityForce].MaxVelocity = new Vector3F(0f, 1500f, 0f);
			_forces[_gravityForce].Direction.Y = -4f;
			_jumpForce = _forces.Add(new Force());
			_forces[_jumpForce].MaxVelocity = new Vector3F(0f, 500f, 0f);
			_forces[_jumpForce].Acceleration.Y = -40000f;
			_forces[_jumpForce].Direction.Y = 1f;
			_blastForce = _forces.Add(new Force());
			_forces[_blastForce].MaxVelocity = new Vector3F(200f, 0f, 200f);
			_forces[_blastForce].Acceleration = new Vector3F(-22000f, 0f, -22000f);
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
		}

		public void LoadMap(string directoryPath, string mapName)
		{
			Directory.SetCurrentDirectory(directoryPath);
			_currentMap = new BspFile(mapName);
			GL.MatrixMode(MatrixMode.Modelview);
			DebugWindow.Log("Matrixmode: " + MatrixMode.Modelview);
			GL.LoadIdentity();
			DebugWindow.Log("Loaded GL Identity");
			GL.MatrixMode(MatrixMode.Projection);
			DebugWindow.Log("Matrixmode: " + MatrixMode.Projection);
			GL.LoadIdentity();
			var entityArray = _currentMap.Entities.SeekEntitiesByClassname("info_player_deathmatch");
			foreach (var strArray in entityArray.Select(entity => entity.SeekValuesByArgument("origin")))
			{
			    if (strArray.Length > 0)
			    {
			        var strArray2 = Regex.Split(strArray[0], " ");
			        if (strArray2.Length == 3)
			        {
			            var vector = new Vector3F();
			            try
			            {
			                vector.X += float.Parse(strArray2[0]);
			                vector.Z += -float.Parse(strArray2[1]);
			                vector.Y += float.Parse(strArray2[2]);
			                _spawnPoints.Add(vector);
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
			            _mapCamera.Yaw = 90f - num;
			        }
			        catch
			        {
			        }
			    }
			}
			DebugWindow.Log("Loaded map entities");
			OnResizeControl();
			ChooseSpawnPoint();
		}

		private void OnResizeControl()
		{
			GL.Viewport(0, 0, _winWidth, _winHeight);
			DebugWindow.Log("Window resize: H:" + _winHeight + " W:" + _winWidth);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			var num = (_winWidth + 0.1f)/_winHeight;
			GluPerspective(70.0, num, 1.0, 5000.0);
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
			if (_currentMap != null)
			{
				Hashtable hashtable;
				PrepareRenderMap();
				_mapCamera.UpdateView();
				_mapFrustrum.UpdateFrustrum();
				_currentMap.RenderLevel(_mapCamera.Position, _mapFrustrum);
				lock ((hashtable = _items))
				{
					GL.Disable(EnableCap.Texture2D);
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha);
					foreach (Item item in _items.Values)
					{
						if (!item.Active)
						{
							continue;
						}
						GL.PushMatrix();
						GL.Translate(item.Position.X, item.Position.Y, item.Position.Z);
						switch (item.Type)
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
							item.Alpha += 0.5f*_timeElapsed;
							if (item.Alpha > 1f)
							{
								item.Alpha = 1f;
								item.AlphaDir = false;
							}
						}
						else
						{
							item.Alpha -= 0.5f*_timeElapsed;
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
				lock ((hashtable = _networkPlayers))
				{
					foreach (NetworkPlayer player in _networkPlayers.Values)
					{
						player.Update(_timeElapsed);
						//this.textFont.glPrint(player.PlayerModel.Position.X - player.PlayerModel.Center.X, player.PlayerModel.BoundMax.Y + player.PlayerModel.Position.Y, player.PlayerModel.Position.Z + player.PlayerModel.Center.Z, player.Name);
					}
				}
				if ((_hurtTime > 0f) || (_deathTime > 0f))
				{
					_hurtTime -= _timeElapsed;
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
				lock (_laserBeams)
				{
					for (var i = 0; i < _laserBeams.Count; i++)
					{
						var info = (LaserBeamInfo) _laserBeams[i];
						if (info.TimeRemaining > 0f)
						{
							info.TimeRemaining -= _timeElapsed;
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
							_laserBeams.RemoveAt(i);
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
					GL.BindTexture(TextureTarget.Texture2D, _knifeTexture.TextureId);
					if (_stabTime > 0f)
					{
						_stabTime -= _timeElapsed;
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
					if (_laserTime > 0f)
					{
						_laserTime -= _timeElapsed;
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
			_forces.ClearDisposableForces();
			ChooseSpawnPoint();
		}

		private void SendHurt(int id)
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

		public void SendMessage(int id, string message, bool isPrivate)
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
			var num = 90f - _mapCamera.Yaw;
			_lastState.X = _mapCamera.Position.X;
			_lastState.Y = _mapCamera.Position.Y;
			_lastState.Z = _mapCamera.Position.Z;
			_lastState.Yaw = _mapCamera.Yaw;
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

		public void SetVoiceTarget(int targetId)
		{
			if (_voiceEnabled)
			{
				// miki-play-off this.ClientVoice.TransmitTargets = new int[] { TargetID };
			}
		}

		private bool TrySlide(Vector3F sourceVector, Vector3F projectedPos)
		{
			var vector = new Vector3F();
			for (var vector2 = new Vector3F(projectedPos.X, projectedPos.Y, projectedPos.Z);
				((Math.Abs(vector2.X) > 0.0001f) || (Math.Abs(vector2.Y) > 0.0001f)) || (Math.Abs(vector2.Z) > 0.0001f);
				vector2 = new Vector3F(vector.X, vector.Y, vector.Z))
			{
				_currentMap.DetectCollisionBox(sourceVector, sourceVector + vector2, _playerState.BoundMin,
					_playerState.BoundMax);
				if (_currentMap.CollisionInfo.Fraction > 0f)
				{
					_mapCamera.MoveCameraTo(_currentMap.CollisionInfo.EndPoint);
					return true;
				}
				var vector3 = new Vector3F();
				vector3.CopyFrom(_currentMap.CollisionInfo.Normal);
				vector3.Normalize();
				vector = vector2 - vector3*vector3.Dot(vector2);
			}
			return false;
		}

		private bool TryStepUpAndSlide(Vector3F projectedPos)
		{
			_currentMap.DetectCollisionBox(_mapCamera.Position, _mapCamera.Position + _playerState.Step,
				_playerState.BoundMin, _playerState.BoundMax);
			if (_currentMap.CollisionInfo.Fraction == 0f)
			{
				return false;
			}
			if (!TrySlide(_currentMap.CollisionInfo.EndPoint, projectedPos))
			{
				return false;
			}
			_currentMap.DetectCollisionBox(_mapCamera.Position, _mapCamera.Position - _playerState.Step,
				_playerState.BoundMin, _playerState.BoundMax);
			if (_currentMap.CollisionInfo.Fraction == 0f)
			{
				return false;
			}
			_mapCamera.MoveCameraTo(_currentMap.CollisionInfo.EndPoint);
			return true;
		}

		public void UpdateFrame(float timeElapsed)
		{
			_timeElapsed = timeElapsed;

			var auxFps = 1/timeElapsed;

			if (!IsPaused)
			{
				if (_currentMap != null)
				{
					ArrayList list;
					Hashtable hashtable;
					lock ((list = _incomingPlayers))
					{
						foreach (NetworkPlayer player in _incomingPlayers)
						{
							Directory.SetCurrentDirectory(Quake3FilesPath + "models");
							player.PlayerModel = new Md2Model(player.ModelName + ".md2", player.ModelName + ".bmp");
							if (player.InitialState == null)
							{
								player.PlayerModel.Position = new Vector3F();
							}
							else
							{
								player.PlayerModel.Position = new Vector3F(player.InitialState.X, 0f,
									player.InitialState.Z);
								player.PlayerModel.Yaw = player.InitialState.Yaw;
							}
							// miki-sound-off player.LaserSound = this.EngineSoundManager.GenerateName();
							// miki-sound-off this.EngineSoundManager.LoadSound3D(player.LaserSound, Quake3FilesPath + @"sounds\laser.wav");
							lock ((hashtable = _networkPlayers))
							{
								_networkPlayers[player.Id] = player;
							}
						}
						_incomingPlayers.Clear();
					}
					lock ((list = _outgoingPlayerIDs))
					{
						foreach (int num in _outgoingPlayerIDs)
						{
							lock ((hashtable = _networkPlayers))
							{
								if (_networkPlayers.ContainsKey(num))
								{
									_networkPlayers.Remove(num);
								}
							}
						}
						_outgoingPlayerIDs.Clear();
					}
					UpdateInput(ref _playerState);
					if (IsDead)
					{
						_deathTime -= timeElapsed;
						if (_deathTime <= 0f)
						{
							Respawn();
						}
					}
					else
					{
						UpdatePlayerState(_playerState);
						var itemCollision = GetItemCollision();
						if (itemCollision != null)
						{
							var flag = false;
							switch (itemCollision.Type)
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
					if (Connected && _nextStateReady)
					{
						SendMovementState();
					}
				}

				//gameWin.Title = "PosX = " + PlayerState.PlayerLook.X + " PosY = " + PlayerState.PlayerLook.Y;

				// miki-sound-off this.EngineSoundManager.SetListenerPosition(this.MapCamera.Position);

				Render();

				GL.Enable(EnableCap.Blend);
				_textFont.GlPrint(10, 20,
					$"Position: X: {(int) _mapCamera.Position.X} Y: {(int) _mapCamera.Position.Y} Z: {(int) _mapCamera.Position.Z}",
					0);
			}
		}

		private void UpdateMouseDelta()
		{
			_pointerCurrent = Cursor.Position;
			_pointerDelta = new Size(_pointerCurrent.X - _pointerPrevious.X,
				_pointerCurrent.Y - _pointerPrevious.Y);
		}

		private void UpdateMousePosition()
		{
			if (_cursorHide == false)
			{
				Cursor.Hide();
				_cursorHide = true;
			}

			Cursor.Position =
				new Point(_gameWin.Bounds.Left + (_gameWin.Bounds.Width/2),
					_gameWin.Bounds.Top + (_gameWin.Bounds.Height/2));

			_pointerPrevious = Cursor.Position;
		}

		private void UpdateInput(ref CommandSet state)
		{
/*
			State.PlayerMovement.X = 0f;
			State.PlayerMovement.Y = 0f;
			State.PlayerMovement.Z = 0f;
			State.PlayerLook.X = 0f;
			State.PlayerLook.Y = 0f;*/
			if (_mProcessInput)
			{
				ProcessKeyboardEvents();
				ProcessMouseEvents(ref state);
			}
		}

		private void ProcessKeyboardEvents()
		{
			try
			{
				// miki-input-off KeyboardState currentKeyboardState = this.KeyboardDevice.GetCurrentKeyboardState();
				if (_gameWin.Keyboard[_controlCfg.MoveForward])
				{
					var angle = Utility.CapAngle(90f - _mapCamera.Yaw);
					_forces[_runForce].Direction.X = Utility.CosDeg(angle);
					_forces[_runForce].Direction.Z = -Utility.SinDeg(angle);
					_forces[_runForce].AddVelocityX(_forces[_runForce].MaxVelocity.X);
					_forces[_runForce].AddVelocityZ(_forces[_runForce].MaxVelocity.Z);
				}
				else if (_gameWin.Keyboard[_controlCfg.MoveBack])
				{
					var num2 = Utility.CapAngle(90f - _mapCamera.Yaw);
					_forces[_runForce].Direction.X = -Utility.CosDeg(num2);
					_forces[_runForce].Direction.Z = Utility.SinDeg(num2);
					_forces[_runForce].AddVelocityX(_forces[_runForce].MaxVelocity.X);
					_forces[_runForce].AddVelocityZ(_forces[_runForce].MaxVelocity.Z);
				}
				if (_gameWin.Keyboard[_controlCfg.LeftStrafe])
				{
					var num3 = Utility.CapAngle(-_mapCamera.Yaw);
					_forces[_strafeForce].Direction.X = -Utility.CosDeg(num3);
					_forces[_strafeForce].Direction.Z = Utility.SinDeg(num3);
					_forces[_strafeForce].AddVelocityX(_forces[_strafeForce].MaxVelocity.X);
					_forces[_strafeForce].AddVelocityZ(_forces[_strafeForce].MaxVelocity.Z);
				}
				else if (_gameWin.Keyboard[_controlCfg.RightStrafe])
				{
					var num4 = Utility.CapAngle(-_mapCamera.Yaw);
					_forces[_strafeForce].Direction.X = Utility.CosDeg(num4);
					_forces[_strafeForce].Direction.Z = -Utility.SinDeg(num4);
					_forces[_strafeForce].AddVelocityX(_forces[_strafeForce].MaxVelocity.X);
					_forces[_strafeForce].AddVelocityZ(_forces[_strafeForce].MaxVelocity.Z);
				}
				if (EnableJump)
				{
					if (_gameWin.Keyboard[_controlCfg.Jump] && (_forces[_gravityForce].GetVelocityY() <= 0f))
					{
						EnableJump = false;
						// miki-sound-off this.EngineSoundManager.PlaySound("jump");
						_forces[_jumpForce].AddVelocityY(_forces[_jumpForce].MaxVelocity.Y);
					}
				}
				else if (!_gameWin.Keyboard[_controlCfg.Jump])
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

		private void ProcessMouseEvents(ref CommandSet state)
		{
			try
			{
				//MouseState currentMouseState = this.MouseDevice.CurrentMouseState;
				//byte[] mouseButtons = currentMouseState.GetMouseButtons();

				if (_acquireMouse)
				{
					UpdateMouseDelta();

					_playerState.PlayerLook.X = -_pointerDelta.Width;

					if (_controlCfg.InvertMouse)
					{
						_playerState.PlayerLook.Y = _pointerDelta.Height;
					}
					else
					{
						_playerState.PlayerLook.Y = -_pointerDelta.Height;
					}

					UpdateMousePosition();
				}
				else if (_cursorHide)
				{
					Cursor.Show();
					_cursorHide = false;
				}
			}
			catch
			{
			}
		}

		private void UpdatePlayerState(CommandSet playerState)
		{
			var num = _timeElapsed*_controlCfg.MouseSpeedHorz;
			var num2 = _timeElapsed*_controlCfg.MouseSpeedVert;
			_mapCamera.SetMouseView(playerState.PlayerLook.X*num, playerState.PlayerLook.Y*num2);
			_currentMap.DetectCollisionBox(_mapCamera.Position, _mapCamera.Position - playerState.Step,
				playerState.BoundMin, playerState.BoundMax);
			if (_currentMap.CollisionInfo.Fraction > 0f)
			{
				if (_forces[_gravityForce].GetVelocityY() <= 0f)
				{
					_forces[_gravityForce].Acceleration.Y = 40000f;
				}
			}
			else
			{
				_forces[_gravityForce].Acceleration.Y = 0f;
				_forces[_gravityForce].SetVelocity(0f, 0f, 0f);
			}
			var sourcePoint = new Vector3F(_mapCamera.Position.X, _mapCamera.Position.Y, _mapCamera.Position.Z);
			lock (_forces)
			{
				_forces.ApplyAllForces(sourcePoint, _timeElapsed);
			}
			var projectedPos = sourcePoint - _mapCamera.Position;
			if (!TryStepUpAndSlide(projectedPos))
			{
				TrySlide(_mapCamera.Position, projectedPos);
			}
		}

		private delegate void CreateItemDelegate(int id, ItemType type, Vector3F pos, bool active);

		#region Utilizados en gluLookAt()

		private static readonly Vector3F Forward = new Vector3F();
		private static Vector3F _side = new Vector3F();
		private static Vector3F _up = new Vector3F();
		private static readonly Matrix4F M = new Matrix4F();

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
		internal static void GluLookAt(float eyex, float eyey, float eyez,
			float centerx, float centery, float centerz,
			float upx, float upy, float upz)
		{
			Forward.X = centerx - eyex;
			Forward.Y = centery - eyey;
			Forward.Z = centerz - eyez;

			_up.X = upx;
			_up.Y = upy;
			_up.Z = upz;

			Forward.Normalize();

			/* Side = forward x up */
			Vector3F.Cross(Forward, _up, ref _side);
			_side.Normalize();

			/* Recompute up as: up = side x forward */
			Vector3F.Cross(_side, Forward, ref _up);

			M.SetRight(_side.X, _up.X, -Forward.X, 0);
			M.SetUp(_side.Y, _up.Y, -Forward.Y, 0);
			M.SetForward(_side.Z, _up.Z, -Forward.Z, 0);
			M.SetTranslation(0, 0, 0, 1);

			GL.MultMatrix(M.Values);
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

		public void SetViewport(int width, int height)
		{
			_winWidth = width;
			_winHeight = height;

			_aspectRatio = width/(double) height;

			GL.Viewport(0, 0, width, height);
		}

		public void SetProjection(ProjectionType proType)
		{
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();

			switch (proType)
			{
				case ProjectionType.Perspective:
					GluPerspective(_fieldOfView, _aspectRatio,
						_nearClipDistance, _farClipDistance);
					break;
				case ProjectionType.Ortho2D:
					GluOrtho2D(0, _winWidth, _winHeight, 0);
					break;
			}
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
		}

		internal void GluOrtho2D(double left, double right, double bottom, double top)
		{
			GL.Ortho(left, right, bottom, top, -1.0, 1.0);
		}

		internal void GluPerspective(double fovy, double aspect, double zNear, double zFar)
		{
		    var ymax = zNear*Math.Tan(fovy*Math.PI/360.0);
			var ymin = -ymax;

			var xmin = ymin*aspect;
			var xmax = ymax*aspect;

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