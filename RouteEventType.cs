using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteSetTool
{
    public class RouteEventType
    {
        public FoxHash Name;
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Name = new FoxHash(FoxHash.Type.StrCode32);
            Name.Read(reader, nameLookupTable, hashIdentifiedCallback);
        }
        public void Write(BinaryWriter writer)
        {
            Name.Write(writer);
        }
    }
}
