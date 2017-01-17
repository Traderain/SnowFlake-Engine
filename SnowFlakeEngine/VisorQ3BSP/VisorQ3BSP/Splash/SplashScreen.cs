namespace BSP.Splash
{
    /// <summary>
    ///     Defined types of messages: Success/Warning/Error.
    /// </summary>
    public enum TypeOfMessage
    {
        Success,
        Warning,
        Error
    }

    /// <summary>
    ///     Initiate instance of SplashScreen
    /// </summary>
    public static class SplashScreen
    {
        private static SplashForm _sf;

        /// <summary>
        ///     Displays the splashscreen
        /// </summary>
        public static void ShowSplashScreen()
        {
            if (_sf == null)
            {
                _sf = new SplashForm();
                _sf.ShowSplashScreen();
            }
        }

        /// <summary>
        ///     Closes the SplashScreen
        /// </summary>
        public static void CloseSplashScreen()
        {
            if (_sf != null)
            {
                _sf.CloseSplashScreen();
                _sf = null;
            }
        }

        /// <summary>
        ///     Update text in default green color of success message
        /// </summary>
        /// <param name="text">Message</param>
        public static void UdpateStatusText(string text)
        {
            if (_sf != null)
                _sf.UdpateStatusText(text);
        }

        public static void UpdatePercentage(int percentage)
        {
            if (_sf != null)
                _sf.UpdateProgress(percentage);
        }

        /// <summary>
        ///     Update text with message color defined as green/yellow/red/ for success/warning/failure
        /// </summary>
        /// <param name="text">Message</param>
        /// <param name="tom">Type of Message</param>
        public static void UdpateStatusTextWithStatus(string text, TypeOfMessage tom)
        {
            if (_sf != null)
                _sf.UdpateStatusTextWithStatus(text, tom);
        }
    }
}