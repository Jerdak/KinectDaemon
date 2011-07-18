/*
    License Notes:
    KinectSDK and example code are licensed by Microsoft under a limited non-commercial license.
    Rather than isolating KinectSDK code in another file I include both my additions to the code
    and the Microsoft example code in a single file.
 
    So basically the code I've written falls under GPL.  If you wish to release an entire app
    under GPL you'll need to remove the Kinect crap. 
*/

/////////////////////////////////////////////////////////////////////////
// Code pertaining directly to the Kinect SDK is licensed under the Microsoft Lincense
// agreement below.
// Microsoft Kinect for Windows SDK (Beta) from Microsoft Research 
// License Agreement: http://research.microsoft.com/KinectSDK-ToU
//
/////////////////////////////////////////////////////////////////////////

/* 
 * Code pertaining to anything other than KinectSDK falls under GPL; 
 * you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation; either version 2 of the License, or 
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
 * or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
 * for more details.
 * 
 * You should have received a copy of the GNU General Public License along 
 * with this program; if not, write to the Free Software Foundation, Inc., 
 * 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace KinectDaemon
{
    /// <summary>
    /// Microsoft KinectSDK wrapper
    /// </summary>
    /// <author>Jeremy Carson</author>
    /// <original_source>http://www.switchonthecode.com/tutorials/csharp-tutorial-simple-threaded-tcp-server</original_source>
    /// <related_source>http://www.seethroughskin.com/blog/?p=1159</related_source>
    public class Kinect
    {
        ///Joint data hashed on joint name (string)
        public Dictionary<string, KinectPoint> Joints = new Dictionary<string, KinectPoint>();
        
        ///Stream recorder.
        public Recorder Record { get; set; }

        ///Is Skeleton being tracked?
        private bool _isTrackingSkeleton = false;
        
        ///KinectSDK NUI Runtime 
        Runtime _nuiRunTime;

        ///Total number of depth frames
        int _totalFrames = 0;

        ///Last total number of depth frames
        int _lastFrames = 0;

        ///Last time in frame.
        DateTime _lastTime = DateTime.MaxValue;

        // We want to control how depth data gets converted into false-color data
        // for more intuitive visualization, so we keep 32-bit color frame buffer versions of
        // these, to be updated whenever we receive and process a 16-bit frame.
        const int RED_IDX = 2;
        const int GREEN_IDX = 1;
        const int BLUE_IDX = 0;
        byte[] depthFrame32 = new byte[320 * 240 * 4];


        public Kinect()
        {
            Record = new Recorder();
        }

        ///Be certain not to leave a dangling file stream or recorder data will be lost.
        ~Kinect()
        {
            Record.StopRecording();
        }

        public bool IsTrackingSkeleton
        {
            get
            {
                return _isTrackingSkeleton;
            }
            set
            {
                if (_isTrackingSkeleton && !value)
                {
                    Console.WriteLine("Lost tracking for all skeletons.");
                }
                if (!_isTrackingSkeleton && value)
                {
                    Console.WriteLine("Tracking new skeleton.");
                }

                _isTrackingSkeleton = value;
            }

        }
      
        public void Start()
        {
            _nuiRunTime = new Runtime();

            try
            {
                _nuiRunTime.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Runtime initialization failed. Please make sure Kinect device is plugged in.");
                return;
            }


            try
            {
                _nuiRunTime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                _nuiRunTime.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Failed to open stream. Please make sure to specify a supported image type and resolution.");
                return;
            }

            _lastTime = DateTime.Now;

            _nuiRunTime.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
            _nuiRunTime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            _nuiRunTime.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_ColorFrameReady);

            _nuiRunTime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(Record.nui_SkeletonFrameReady);
        }

        // Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame
        // that displays different players in different colors
        byte[] convertDepthFrame(byte[] depthFrame16)
        {
            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < depthFrame32.Length; i16 += 2, i32 += 4)
            {
                int player = depthFrame16[i16] & 0x07;
                int realDepth = (depthFrame16[i16 + 1] << 5) | (depthFrame16[i16] >> 3);
                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(255 - (255 * realDepth / 0x0fff));

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

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            /*PlanarImage Image = e.ImageFrame.Image;
            byte[] convertedDepthFrame = convertDepthFrame(Image.Bits);

            depth.Source = BitmapSource.Create(
                Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, convertedDepthFrame, Image.Width * 4);

            ++_totalFrames;

            DateTime cur = DateTime.Now;
            if (cur.Subtract(_lastTime) > TimeSpan.FromSeconds(1))
            {
                int frameDiff = _totalFrames - _lastFrames;
                _lastFrames = _totalFrames;
                _lastTime = cur;
                frameRate.Text = frameDiff.ToString() + " fps";
            }*/
        }

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            int iSkeleton = 0;
     
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    lock (Joints)
                    {
                        //drop joint data in to outward facing lookup table
                        foreach (Joint joint in data.Joints)
                        {
                            
                            //store joint position here.
                            Joints[joint.ID.ToString()] = new KinectPoint((int)(joint.Position.X * 1000), (int)(joint.Position.Y * 1000), (int)(joint.Position.Z * 1000));
                        }
                    }
                    IsTrackingSkeleton = true;
                }
              
                iSkeleton++;
            } // for each skeleton
        }

        void nui_ColorFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            // 32-bit per pixel, RGBA image
            PlanarImage Image = e.ImageFrame.Image;
           // video.Source = BitmapSource.Create(
               // Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, Image.Bits, Image.Width * Image.BytesPerPixel);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _nuiRunTime.Uninitialize();
            Environment.Exit(0);
        }

    }
}
