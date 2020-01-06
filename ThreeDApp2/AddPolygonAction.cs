using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDApp2
{
    public class AddPolygonAction : IAction
    {
        public AddPolygonAction()
        {
        }

        public AddPolygonAction(MyPolygon polygon)
        {

        }

        

        public override Polygon Do(Polygon input)
        {
            throw new NotImplementedException();
        }

        public override Polygon Undo(Polygon input)
        {
            throw new NotImplementedException();
        }
    }
}
