using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;

namespace KinectDaemon.UserInterface
{
    /// <summary>
    /// Interaction logic for UIDepthViewer.xaml
    /// </summary>
    public partial class UIDepthViewer : UserControl
    {
        Runtime _kinectRuntime = null;

        int depthWidth = 0;
        int depthHeight = 0;
        int[] depthPixels = new int[320 * 240];
        byte[] depthFrame32 = new byte[320 * 240 * 4];
   
        InteropBitmapHelper imageHelper = null;
        const int RED_IDX = 2;
        const int GREEN_IDX = 1;
        const int BLUE_IDX = 0;

        public UIDepthViewer()
        {
            InitializeComponent();
        }
        public Runtime KinectRuntime
        {
            get { return _kinectRuntime; }
            set
            {
                if (_kinectRuntime != null)
                {
                    _kinectRuntime.DepthFrameReady -= new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
                }
                _kinectRuntime = value;
                _kinectRuntime.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
            }
        }
        // Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame
        // that displays different players in different colors
        byte[] convertDepthFrame(ImageFrame Image)
        {
            byte[] depthFrame16 = Image.Image.Bits;
            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < depthFrame32.Length; i16 += 2, i32 += 4)
            {
                int player = depthFrame16[i16] & 0x07;
                int realDepth = (depthFrame16[i16 + 1] << 5) | (depthFrame16[i16] >> 3);
                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(255 - (255 * realDepth / 0x0fff));

                switch (Image.Type)
                {
                    case ImageType.DepthAndPlayerIndex:
                        depthPixels[i16 / 2] = (depthFrame16[i16 + 1] << 5) | (depthFrame16[i16] >> 3);
                        break;
                    case ImageType.Depth:
                        //Mirror depth
                        depthPixels[i16 / 2] = (depthFrame16[i16 + 1] << 8) | depthFrame16[i16];
                        break;
                }
               
                depthFrame32[i32 + RED_IDX] = 0;
                depthFrame32[i32 + GREEN_IDX] = 0;
                depthFrame32[i32 + BLUE_IDX] = 0;

               
                // choose different display colors based on player
                switch (player)
                {
                    case 0:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity / 2);
                        break;
                    case 1:
                        depthFrame32[i32 + RED_IDX] = intensity;
                        break;
                    case 2:
                        depthFrame32[i32 + GREEN_IDX] = intensity;
                        break;
                    case 3:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 4);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 4:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity / 4);
                        break;
                    case 5:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 4);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 6:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 7:
                        depthFrame32[i32 + RED_IDX] = (byte)(255 - intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(255 - intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(255 - intensity);
                        break;
                }
            }
            return depthFrame32;
        }
        List<Microsoft.Research.Kinect.Nui.Vector> convertRealDepth()
        {
            List<Microsoft.Research.Kinect.Nui.Vector> ret = new List<Microsoft.Research.Kinect.Nui.Vector>();
            for (int i = 0; i < depthPixels.Length; i+=2)
            {
                int depthPixel = depthPixels[i/2];

                // The x and y positions can be calculated using modulus
                // division from the array index

                int x = (i / 2) % depthWidth;
                int y = (i / 2) / depthWidth;

                // The x and y we pass into DepthImageToSkeleton() need to
                // be normalised (between 0 and 1), so we divide by the
                // width and height of the depth image, respectively

                // As we're using UseDepth (not UseDepthAndPlayerIndex) in
                // the depth sensor settings, we also need to shift the
                // depth pixel by 3 bits

               Microsoft.Research.Kinect.Nui.Vector v = _kinectRuntime.SkeletonEngine.DepthImageToSkeleton(
                    ((float)x) / ((float)depthWidth),
                    ((float)y) / ((float)depthHeight),
                    (short)(depthPixel << 3)
                  );
               if(v.Z>0)ret.Add(v);
            }
            return ret;
        }
        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage Image = e.ImageFrame.Image;

            byte[] convertedDepthFrame = convertDepthFrame(e.ImageFrame);
            depthWidth = Image.Width;
            depthHeight = Image.Height;
           
            //An interopBitmap is a WPF construct that enables resetting the Bits of the image.
            //This is more efficient than doing a BitmapSource.Create call every frame.
            if (imageHelper == null)
            {
                imageHelper = new InteropBitmapHelper(Image.Width, Image.Height, convertedDepthFrame);
                kinectDepthImage.Source = imageHelper.InteropBitmap;
            }
            else
            {
                imageHelper.UpdateBits(convertedDepthFrame);
            }
        }

        public void DumpData()
        {
            List<Microsoft.Research.Kinect.Nui.Vector> verts = convertRealDepth();
            DirectoryInfo root = new DirectoryInfo("./Dump");
            if(!root.Exists){
                root.Create();
            }

            int ct = 1;
            FileInfo file = new FileInfo(root.FullName + "/" + "dump_0.obj");
            while(file.Exists){
                file = new FileInfo(root.FullName + "/" + "dump_" + ct.ToString() + ".obj");
                ct++;
            }

            using (StreamWriter sw = new StreamWriter(file.FullName))
            {
                foreach (Microsoft.Research.Kinect.Nui.Vector v in verts)
                {
                    sw.WriteLine("v " + v.X.ToString() + " " + v.Y.ToString() + " " + v.Z.ToString());
                }
               
            }
        }
    }
}
