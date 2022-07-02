using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteSetTool
{
    public interface IEventTypeParams
    {
        void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback);
        void Write(BinaryWriter writer);
    }
    public class EventTypeParams_Default : IEventTypeParams
    {
        public uint[] Params = new uint[4];
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            for (int index = 0; index < 4; index++)
            {
                Params[index] = reader.ReadUInt32();
                Console.WriteLine($"@{reader.BaseStream.Position} Event param#{index}: {Params[index]}");
            }
        }
        public void Write(BinaryWriter writer)
        {
            foreach (uint param in Params)
                writer.Write(param);
        }
    }
}
