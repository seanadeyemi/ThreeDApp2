using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDApp2
{
   public class PathSLink
    {
        public enum LinkUseageType
        {
            Unused,
            InTree,
            InPath
        }

        public PathSNode Node1, Node2;
        public int Cost;
        public LinkUseageType LinkUsage = LinkUseageType.Unused;
    }
}
