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
				Console.WriteLine("[ENGINE] - VSync: " + VSync);
				SEngine = new Engine(this);

				var splashthread = new Thread(SplashScreen.ShowSplashScreen) {IsBackground = true};
				splashthread.Start();
				SplashScreen.UpdatePercentage(10);
				SplashScreen.UdpateStatusTextWithStatus("Loading BSP map: level.bsp", TypeOfMessage.Success);
				Console.WriteLine(@"[ENGINE] - Loading level outpost.bsp");
				SplashScreen.UpdatePercentage(20);
				SEngine.LoadMap(Engine.Quake3FilesPath + Utility.AdaptRelativePathToPlatform("maps/"), "outpost.bsp");
				Console.WriteLine(@"[ENGINE] - Loaded map outpost.bsp");
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
				Console.WriteLine(@"Are you sure you would like to exit? (Y/N)");
				Console.WriteLine();
				Console.Write(@"> ");
				var input = Console.ReadLine();
			    if (input.ToUpper().Contains("Y"))
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

			Title = "FPS: " + (1 / e.Time).ToString("F1");
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
			Console.WriteLine(@"[ENGINE] - Starting");
			Engine.Quake3FilesPath = Utility.AdaptRelativePathToPlatform("../../../../media/Quake3/");
			Console.WriteLine(@"[ENGINE] - Strating new game!");
			using (var g = new Game())
			{
				g.Run();
			}
		}

		public async void DebugConsole()
		{

		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();
	}
}