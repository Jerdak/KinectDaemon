using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public class KinectPoint
{
    int X { get; set; }
    int Y { get; set; }
    int Z { get; set; }

    ///Screen position, X
    int WX { get; set; }

    ///Screen position, Y
    int WY { get; set; }
    public KinectPoint()
    {
    }
    public KinectPoint(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    public override string ToString()
    {
        return X.ToString() + " " + Y.ToString() + " " + Z.ToString();
    }
};