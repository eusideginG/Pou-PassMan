using System.Windows;

namespace Pou_Pass_Man
{
    /// <summary>
    /// Main Application class
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// A bool for initial start of the application
        /// </summary>
        public bool InitialStart { get; set; } = true;
        /// <summary>
        /// A listenner on aplication exit that sets the initial start (bool) to true when exiting the app
        /// </summary>
        /// <param name="sender">Object of the sender of the listener</param>
        /// <param name="e">arguments of the event</param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            InitialStart = true;
        }
    }
}
