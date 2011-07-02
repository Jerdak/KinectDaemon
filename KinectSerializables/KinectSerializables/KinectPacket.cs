using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

///Serializable Kinect Packet
[Serializable]
public class KinectPacket
{
    public Dictionary<string, KinectPoint> Messages = new Dictionary<string, KinectPoint>();
    public KinectPacket() { }
};