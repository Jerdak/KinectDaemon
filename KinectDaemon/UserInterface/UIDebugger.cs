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

    }
}
