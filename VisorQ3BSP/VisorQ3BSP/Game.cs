
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
*/
#endregion

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using VisorQ3BSP.Splash;
using WanderEngine;

namespace VisorQ3BSP
{
    public class Game : GameWindow
    {
        SplashScreenForm sf = new SplashScreenForm();

        //protected ProjectionType typeProjection = ProjectionType.Perspective;
        protected ClearBufferMask maskClearBuffer = ClearBufferMask.ColorBufferBit;
        protected Color clearColor = Color.Black;
        private Engine WEngine = null;

        #region Constructor

        public Game()
            : base(1024, 768, new GraphicsMode(32, 16, 8, 0))
        {
            try
            {
            	Title = "Q3BSP Loader-viewer using OpenTK";
                VSync = VSyncMode.Off;
                WEngine = new Engine(this);
 
                Thread splashthread = new Thread(new ThreadStart(SplashScreen.ShowSplashScreen));
            	splashthread.IsBackground = true;
            	splashthread.Start();
            	
            	SplashScreen.UdpateStatusTextWithStatus("Loading BSP map: level.bsp", TypeOfMessage.Success);
   	            Thread.Sleep(5000);

             	//WEngine.LoadMap(Engine.Quake3FilesPath +
                //                Utility.AdaptRelativePathToPlatform("maps/"), "level.bsp");
               	WEngine.LoadMap(Engine.Quake3FilesPath +
                	            Utility.AdaptRelativePathToPlatform("maps/"), "outpost.bsp");

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
                WEngine.SetViewport(Width, Height);
                WEngine.SetProjection(ProjectionType.Perspective);
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

            if (Keyboard[OpenTK.Input.Key.Escape])
                this.Exit();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            //Title = "FPS: " + (1 / e.Time);
            if (WEngine != null)
                WEngine.UpdateFrame((float)e.Time);

            SwapBuffers();
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            if (WEngine != null)
                WEngine.DestroyAll();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Engine.Quake3FilesPath = Utility.AdaptRelativePathToPlatform("../../../../media/Quake3/");

            using (Game g = new Game())
            {
            	g.Run();
            }
        }
    }
}

