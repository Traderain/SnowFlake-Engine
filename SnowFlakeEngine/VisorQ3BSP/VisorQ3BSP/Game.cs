using System;
using System.Drawing;
using System.Threading;
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

		#region Constructor

		public Game() : base(640, 480, new GraphicsMode(32, 16, 8, 0))
		{
			try
			{
				Title = "SnowFlake Engine";
				VSync = VSyncMode.Off;
				SEngine = new Engine(this);

				var splashthread = new Thread(SplashScreen.ShowSplashScreen) {IsBackground = true};
				splashthread.Start();
				SplashScreen.UpdatePercentage(10);
				SplashScreen.UdpateStatusTextWithStatus("Loading BSP map: level.bsp", TypeOfMessage.Success);
				SplashScreen.UpdatePercentage(20);
				SEngine.LoadMap(Engine.Quake3FilesPath +
								Utility.AdaptRelativePathToPlatform("maps/"), "outpost.bsp");
				SplashScreen.UpdatePercentage(60);
				SplashScreen.UpdatePercentage(90);
				SplashScreen.CloseSplashScreen();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.InnerException.Message, ex.Message);
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
				MessageBox.Show(ex.InnerException.Message, ex.Message);
			}
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			if (Keyboard[Key.Escape])
				Exit();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			//Title = "FPS: " + (1 / e.Time);
			if (SEngine != null)
				SEngine.UpdateFrame((float) e.Time);

			SwapBuffers();
		}

		protected override void OnUnload(EventArgs e)
		{
			base.OnUnload(e);

			if (SEngine != null)
				SEngine.DestroyAll();
		}

		[STAThread]
		public static void Main(string[] args)
		{
			Engine.Quake3FilesPath = Utility.AdaptRelativePathToPlatform("../../../../media/Quake3/");

			using (var g = new Game())
			{
				g.Run();
			}
		}
	}
}