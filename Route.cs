using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteSetTool
{
    public class Route
    {
        public FoxHash Name;
        public List<RouteNode> Nodes = new List<RouteNode>();
    }
}
