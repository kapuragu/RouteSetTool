using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteSetTool
{
    public class RouteNode
    {
        public Vector3 Translation;
        public RouteEvent EdgeEvent;
        public List<RouteEvent> NodeEvents = new List<RouteEvent>();
    }
}
