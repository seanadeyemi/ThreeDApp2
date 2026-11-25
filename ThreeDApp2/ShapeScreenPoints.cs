using System;
using System.Collections.Generic;
using System.Drawing;

namespace ThreeDApp2
{
    public class ShapeScreenPoints : IComparable<ShapeScreenPoints>
    {
        public List<PointF> PtList = new List<PointF>();
        public float Z;          // Average distance from the camera
        public int LayerIndex;   // Which layer these points belong to
        public float Shade = 1;  // 0..1 lighting factor
        public bool Closed;      // Filled face (true) or open polyline (false)

        public int CompareTo(ShapeScreenPoints other)
        {
            // Compare the floats directly: casting the difference to int treated any two
            // layers less than 1 unit apart as equal depth.
            return Z.CompareTo(other.Z);
        }
    }
}
