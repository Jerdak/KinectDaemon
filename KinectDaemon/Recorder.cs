using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Research.Kinect.Nui;

namespace KinectDaemon
{
    /// <summary>
    /// Recorder class
    /// @descrip
    ///     'Recorder' attaches to the kinect event framework to record skeletal joint data for playback.
    /// </summary>
    public class Recorder
    {
        ///Output file name
        public string           FileName        { get; set; }
        
        ///Is Recorder Recording
        public bool             IsRecording     { get; set; }
       
        ///Recording Stream
        private StreamWriter    RecordStream    { get; set; }
        
        ///Date recording was started
        private DateTime        StartTime       { get; set; }

        public Recorder()
        {
            IsRecording = false;
            FileName = "KinectDaemon.dat";
            RecordStream = null;

        }
        public void StartRecording()
        {
            RecordStream = new StreamWriter(FileName);
            IsRecording = true;
            StartTime = DateTime.Now;
        }
        public void StopRecording()
        {
            if (!IsRecording) return;

            IsRecording = false;
            if (RecordStream != null)
            {
                lock (RecordStream) RecordStream.Close();
            }
        }
        public void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (!IsRecording) return;
            
            lock (RecordStream)
            {
                SkeletonFrame skeletonFrame = e.SkeletonFrame;
                int iSkeleton = 0;

                foreach (SkeletonData data in skeletonFrame.Skeletons)
                {
                    if (SkeletonTrackingState.Tracked == data.TrackingState)
                    {
                        //Recording for longer than a day will cause the timer to roll over.  Who would record 24+ hours of Kinect data anyways?
                        TimeSpan elapsedTime = DateTime.Now - StartTime;
                        RecordStream.Write(elapsedTime.Hours.ToString() + ":" + elapsedTime.Minutes.ToString() + ":" + elapsedTime.Seconds.ToString() + ":" + elapsedTime.Milliseconds.ToString() + ",");
                        foreach (Joint joint in data.Joints)
                        {
                            RecordStream.Write(joint.ID.ToString() + "," + (joint.Position.X * 1000).ToString() + "," + (joint.Position.Y * 1000) + "," + (joint.Position.Z * 1000)+",");
                        }
                        RecordStream.WriteLine("");
                     }
                    iSkeleton++;
                } // for each skeleton
            }
        }
    }
}
