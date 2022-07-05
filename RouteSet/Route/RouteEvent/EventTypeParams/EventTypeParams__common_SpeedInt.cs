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
    //Dash
    //ForwardChangeSpeed
    //move
    //Move
    //SetTargetSpeed
    //Walk
    //RelaxedStandWalkAct
    //RelaxedWalkAct
    public class EventTypeParams__common_SpeedInt : IEventTypeParams
    {
        //first is speed
        public int Speed;
        public int Param1;
        public int Param2;
        public int Param3;

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Speed = reader.ReadInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Speed: {Speed}");
            Param1 = reader.ReadInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param1: {Param1}");
            Param2 = reader.ReadInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param2: {Param2}");
            Param3 = reader.ReadInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param3: {Param3}");
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("speed");
            Speed = 0;
            int.TryParse(reader.ReadString(), out Speed);
            reader.ReadEndElement();

            reader.ReadStartElement("param1");
            Param1 = 0;
            int.TryParse(reader.ReadString(), out Param1);
            reader.ReadEndElement();

            reader.ReadStartElement("param2");
            Param2 = 0;
            int.TryParse(reader.ReadString(), out Param2);
            reader.ReadEndElement();

            reader.ReadStartElement("param3");
            Param3 = 0;
            int.TryParse(reader.ReadString(), out Param3);
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Speed);
            writer.Write(Param1);
            writer.Write(Param2);
            writer.Write(Param3);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", "common_SpeedInt");

            writer.WriteStartElement("speed");
            writer.WriteString(Speed.ToString());
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
