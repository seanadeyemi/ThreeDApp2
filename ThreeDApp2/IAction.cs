using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDApp2
{
    public abstract class IAction
    {
        public abstract Polygon Do(Polygon input);
        public abstract Polygon Undo(Polygon input);

    }
}
