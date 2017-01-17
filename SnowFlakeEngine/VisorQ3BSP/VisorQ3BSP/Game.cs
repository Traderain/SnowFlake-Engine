using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BSP.Splash;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SnowflakeEngine.WanderEngine;

namespace BSP
{
	internal class DebugWindow
	{
		public enum msgtype
		{
			engine,
			game,
			error
		}

		public static void Log(string msg,msgtype type = msgtype.engine)
		{
#if DEBUG
			switch (type)
			{
				case msgtype.engine:
					Console.ForegroundColor = ConsoleColor.DarkGreen;
					break;
				case msgtype.game:
					Console.ForegroundColor = ConsoleColor.White;
					break;
				case msgtype.error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
			}
			Console.WriteLine(@"[" + type.ToString().ToUpper() + @"]: " + msg);
#endif
		}
	}
	public class Game : GameWindow
	{
		private readonly Engine SEngine;
		protected Color clearColor = Color.Black;
		//protected ProjectionType typeProjection = ProjectionType.Perspective;
		protected ClearBufferMask maskClearBuffer = ClearBufferMask.ColorBufferBit;
		private SplashForm sf = new SplashForm();
		public static List<DebugMessage> DebugMessages = new List<DebugMessage>();

		public struct DebugMessage
		{
			public ConsoleColor color;
			public string msg;
		}

		#region Constructor

		public Game() : base(640, 480, new GraphicsMode(32, 16, 8, 0))
		{
			try
			{
				Title = "SnowFlake Engine";
				VSync = VSyncMode.Off;
				DebugWindow.Log("VSync: " + VSync);
				SEngine = new Engine(this);

				var splashthread = new Thread(SplashScreen.ShowSplashScreen) {IsBackground = true};
				splashthread.Start();
				SplashScreen.UpdatePercentage(10);
				SplashScreen.UdpateStatusTextWithStatus("Loading BSP map: level.bsp", TypeOfMessage.Success);
				DebugWindow.Log("Loading level outpost.bsp");
				SplashScreen.UpdatePercentage(20);
				SEngine.LoadMap(Engine.Quake3FilesPath + Utility.AdaptRelativePathToPlatform("maps/"), "outpost.bsp");
				DebugWindow.Log("Loaded map outpost.bsp");
				SplashScreen.UpdatePercentage(60);
				SplashScreen.UpdatePercentage(90);
				SplashScreen.CloseSplashScreen();
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null) MessageBox.Show(ex.InnerException.Message, ex.Message);
			}
		}

		#endregion Constructor

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			if (Width != 0 && Height != 0)
			{
				SEngine.SetViewport(Width, Height);
				SEngine.SetProjection(ProjectionType.Perspective);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null) MessageBox.Show(ex.InnerException.Message, ex.Message);
			}
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			if (Keyboard[Key.Escape])
			{
#if DEBUG
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(@"Are you sure you would like to exit? (Y/N)");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine();
				Console.Write(@"> ");
				var input = Console.ReadLine();
				if (input != null && input.ToUpper().Contains("Y"))
				{
					Exit();
				}
#endif
#if !DEBUG
			   this.Exit();
#endif
			}
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
#if DEBUG
			Title = "FPS: " + (1 / e.Time).ToString("F1");
#endif
			SEngine?.UpdateFrame((float) e.Time);

			SwapBuffers();
		}

		protected override void OnUnload(EventArgs e)
		{
			base.OnUnload(e);

			SEngine?.DestroyAll();
		}

		[STAThread]
		public static void Main(string[] args)
		{
			AllocConsole();
			Console.Title = (@"SNOWFLAKE ENGINE DEBUG CONSOLE");
			DebugWindow.Log("Starting");
			Engine.Quake3FilesPath = Utility.AdaptRelativePathToPlatform("../../../../media/Quake3/");
			DebugWindow.Log("Starting new game!");
			using (var g = new Game())
			{
				g.Run();
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();
	}
}