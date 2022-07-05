using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace RouteSetTool
{
    //DropPoint
    //Hovering
    public class EventTypeParams__common_heli : IEventTypeParams
    {
        public int Param0;
        public uint Param1;
        public uint Param2;
        public uint Param3;

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Param0 = reader.ReadInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Speed: {Param0}");
            Param1 = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param1: {Param1}");
            Param2 = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param2: {Param2}");
            Param3 = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param3: {Param3}");
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("param0");
            Param0 = 0;
            int.TryParse(reader.ReadString(), out Param0);
            reader.ReadEndElement();

            reader.ReadStartElement("param1");
            Param1 = 0;
            uint.TryParse(reader.ReadString(), out Param1);
            reader.ReadEndElement();

            reader.ReadStartElement("param2");
            Param2 = 0;
            uint.TryParse(reader.ReadString(), out Param2);
            reader.ReadEndElement();

            reader.ReadStartElement("param3");
            Param3 = 0;
            uint.TryParse(reader.ReadString(), out Param3);
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Param0);
            writer.Write(Param1);
            writer.Write(Param2);
            writer.Write(Param3);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", "common_heli");

            writer.WriteStartElement("param0");
            writer.WriteString(Param0.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("param1");
            writer.WriteString(Param1.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("param2");
            writer.WriteString(Param2.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("param3");
            writer.WriteString(Param3.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
        public List<FoxHash> GetRouteNames()
        {
            return new List<FoxHash>();
        }
    }
}
