using System.Drawing;
using System.Windows.Forms;

namespace BSP.Splash
{
    public partial class SplashForm : Form
    {
        private bool CloseSplashScreenFlag;

        public SplashForm()
        {
            InitializeComponent();
            label1.BackColor = Color.Transparent;
            label1.ForeColor = Color.Green;
        }

        public void ShowSplashScreen()
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new SplashShowCloseDelegate(ShowSplashScreen));
                return;
            }
            Show();
            Application.Run(this);
        }

        public void CloseSplashScreen()
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new SplashShowCloseDelegate(CloseSplashScreen));
                return;
            }
            CloseSplashScreenFlag = true;
            Close();
        }

        public void UpdateProgress(int percentage)
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new StringParameterDelegate(UdpateStatusText), Text);
                return;
            }
            // Must be on the UI thread if we've got this far
            progressBar1.Value = percentage;
        }

        public void UdpateStatusText(string Text)
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new StringParameterDelegate(UdpateStatusText), Text);
                return;
            }
            // Must be on the UI thread if we've got this far
            label2.ForeColor = Color.Green;
            label2.Text = Text;
        }

        public void UdpateStatusTextWithStatus(string Text, TypeOfMessage tom)
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new StringParameterWithStatusDelegate(UdpateStatusTextWithStatus), Text, tom);
                return;
            }
            // Must be on the UI thread if we've got this far
            switch (tom)
            {
                case TypeOfMessage.Error:
                    label2.ForeColor = Color.Red;
                    break;
                case TypeOfMessage.Warning:
                    label2.ForeColor = Color.Yellow;
                    break;
                case TypeOfMessage.Success:
                    label2.ForeColor = Color.Green;
                    break;
            }
            label2.Text = Text;
        }

        private void SplashForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CloseSplashScreenFlag == false)
                e.Cancel = true;
        }

        private delegate void StringParameterDelegate(string Text);

        private delegate void StringParameterWithStatusDelegate(string Text, TypeOfMessage tom);

        private delegate void SplashShowCloseDelegate();
    }
}