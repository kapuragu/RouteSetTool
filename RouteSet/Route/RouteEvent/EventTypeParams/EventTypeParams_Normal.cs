using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace RouteSetTool
{
    //Normal
    //Heli edge event, opens the door, sets speed, etc.
    public class EventTypeParams_Normal : IEventTypeParams
    {
        public uint Flags;//512=Open right door, 1024=Open left door. Other unknown flags exist
        public uint Param1;
        public uint Speed;//~KPM
        public uint Param3;

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Flags = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Speed: {Flags}");
            Param1 = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param1: {Param1}");
            Speed = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param2: {Speed}");
            Param3 = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param3: {Param3}");
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("eventParams_Normal");

            reader.ReadStartElement("flags");
            Flags = 0;
            uint.TryParse(reader.ReadString(), out Flags);
            reader.ReadEndElement();

            reader.ReadStartElement("param1");
            Param1 = 0;
            uint.TryParse(reader.ReadString(), out Param1);
            reader.ReadEndElement();

            reader.ReadStartElement("speed");
            Speed = 0;
            uint.TryParse(reader.ReadString(), out Speed);
            reader.ReadEndElement();

            reader.ReadStartElement("param3");
            Param3 = 0;
            uint.TryParse(reader.ReadString(), out Param3);
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Flags);
            writer.Write(Param1);
            writer.Write(Speed);
            writer.Write(Param3);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("eventParams_Normal");

            writer.WriteStartElement("flags");
            writer.WriteString(Flags.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("param1");
            writer.WriteString(Param1.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("speed");
            writer.WriteString(Speed.ToString());
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
