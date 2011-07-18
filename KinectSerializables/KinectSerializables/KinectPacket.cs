using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Serialized Kinect Packet
/// </summary>
/// <TODO>
/// -  Add contains for depth, color, and 2D joint coordinates in screen space (will require modification of KinectPoint
/// </TODO>
[Serializable]
public class KinectPacket
{
    public Dictionary<string, KinectPoint> Messages = new Dictionary<string, KinectPoint>();
    public KinectPacket() { }
};