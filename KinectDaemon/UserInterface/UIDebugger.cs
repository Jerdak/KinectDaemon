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
            int elapsed = 0;
            double value = 0;
            DateTime start = DateTime.Now;
            TimeSpan ts = DateTime.Now - start;
            TimeSpan waitSpan = new TimeSpan(0, 0, 0, 0, wait);
            while (DateTime.Now - start < waitSpan)
            {
                value = (DateTime.Now - start).TotalMilliseconds * 100.0 / (double)wait;// (float)pbTimer.Value + 100.0f / wait;

                if (value > 100) value = 100;
                pbTimer.Value = (int)value;
                ts =  DateTime.Now - start;
            }
          
            uiDepthViewer.DumpData();
        }
        private void btDump5_Click(object sender, EventArgs e)
        {
            pbTimer.Value = 0;
            
            System.Threading.Thread wtThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(DumpWait));
            wtThread.SetApartmentState(System.Threading.ApartmentState.STA);
            wtThread.Start(5000);
        }

        private void btDump10_Click(object sender, EventArgs e)
        {
            pbTimer.Value = 0;

            System.Threading.Thread wtThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(DumpWait));
            wtThread.SetApartmentState(System.Threading.ApartmentState.STA);
            wtThread.Start(10000); 
        }

    }
}
