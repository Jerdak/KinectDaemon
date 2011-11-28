using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Research.Kinect.Nui;

namespace KinectDaemon.UserInterface
{
    public partial class UIDebugger : Form
    {
        Runtime _kinectRuntime;
        public UIDebugger(Runtime rt)
        {
            InitializeComponent();
            KinectRuntime = rt;
        }

        public Runtime KinectRuntime {
            get { return _kinectRuntime; }
            set
            {
                _kinectRuntime = value;
                uiDepthViewer.KinectRuntime = _kinectRuntime;
            }
        }

        private void btDumpDepth_Click(object sender, EventArgs e)
        {
            uiDepthViewer.DumpData();
        }

        private void DumpWait(object param)
        {
            int wait = (int)param;
            System.Threading.Thread.Sleep(wait);
            uiDepthViewer.DumpData();
        }
        private void btDump5_Click(object sender, EventArgs e)
        {
            System.Threading.Thread wtThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(DumpWait));
            wtThread.SetApartmentState(System.Threading.ApartmentState.STA);
            wtThread.Start(5000);
        }

    }
}
