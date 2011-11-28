using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KinectDaemon.UserInterface
{
    /** UIMainwindow
     *  @descrip
     *      Window frame for running KinectDaemon as a Windows Application.  Useful for showing debugging windows.
     *  @notes
     *      It was too much of hassle trying to disengage the UI thread from Application.Run() and still
     *      use the original CLI.  So the original CLI was ported to a simple WindowsForm that acts
     *      as the entry point for a Windows form based KinectDaemon.
     */
    public partial class UIMainWindow : Form
    {
        Server _server = null;
        Client _client = null;
        UIDebugger _debugger = null;

        public UIMainWindow()
        {
            InitializeComponent();
            StartServer();
        }

        void StartServer()
        {
            _server = new Server();
            if (!_server.IsKinectKinected)
            {
                Console.WriteLine("Kinect must be attached for the server to run, returning.");
                return;
            }
            Console.WriteLine("Daemon running on port 3000, press 'Q' to quit");
            _debugger = new UIDebugger(_server.KinectRaw.KinectRuntime);
            _debugger.Show();
        }
        private void UIMainWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'q':
                    this.Close();
                    break;
            }
        }

        private void UIMainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_server != null) _server.ShutDown();
            if (_client != null) _client.Disconnect();
        }
    }
}
