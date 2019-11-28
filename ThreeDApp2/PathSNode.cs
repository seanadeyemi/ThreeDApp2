using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDApp2
{
    public class PathSNode
    {
        public enum NodeStatusType
        {
            NotInList,
            WasInList,
            NowInList
        }

        public int Id;
        public Point3D Location;
        public Dictionary<int, PathSLink> Links = new Dictionary<int, PathSLink>();
        public int Dist;                    // Distance from root of path tree.
        public NodeStatusType NodeStatus;   // Path tree status.
        public PathSLink InLink;            // The link into the node.
    }
}
