using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteSetTool
{
    //chase
    public class EventTypeParams_chase : IEventTypeParams
    {
        //first three params are used, fourth one never used
        public int Param0;
        public int Param1;
        public int Param2;
        public uint Param3;
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Param0 = reader.ReadInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param1: {Param0}");
            Param1 = reader.ReadInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param1: {Param1}");
            Param2 = reader.ReadInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param2: {Param2}");
            Param3 = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param3: {Param3}");
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(Param0);
            writer.Write(Param1);
            writer.Write(Param2);
            writer.Write(Param3);
        }
    }
}
