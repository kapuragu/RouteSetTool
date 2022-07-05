using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace RouteSetTool
{
    //VehicleBackFast
    //VehicleBackNormal
    //VehicleBackSlow
    //VehicleMoveFast
    //VehicleMoveNormal
    //VehicleMoveSlow
    //41204288
    public class EventTypeParams__common_Vehicle : IEventTypeParams
    {
        public FoxHash RailName; //.frl/.frld rail name hash
        public int Speed; //RPM
        public uint Param2;
        public uint Param3;

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {

            RailName = new FoxHash(FoxHash.Type.StrCode32);
            RailName.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var RailName_printString = RailName.HashValue.ToString();
            if (RailName.IsStringKnown)
                RailName_printString = RailName.StringLiteral;
            Console.WriteLine($"@{reader.BaseStream.Position} RailName: {RailName_printString }");

            Speed = reader.ReadInt32();

            Console.WriteLine($"@{reader.BaseStream.Position} Speed: {Speed }");

            Param2 = reader.ReadUInt32();

            Console.WriteLine($"@{reader.BaseStream.Position} Argument: {Param2 }");

            Param3 = reader.ReadUInt32();

            Console.WriteLine($"@{reader.BaseStream.Position} Argument: {Param3 }");
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("rail");
            RailName = new FoxHash(FoxHash.Type.StrCode32);
            RailName.ReadXmlString(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("speed");
            Speed = 0;
            int.TryParse(reader.ReadString(), out Speed);
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
            writer.Write(RailName.HashValue);
            writer.Write(Speed);
            writer.Write(Param2);
            writer.Write(Param3);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", "common_Vehicle");

            writer.WriteStartElement("rail");
            RailName.WriteXmlString(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("speed");
            writer.WriteString(Speed.ToString());
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
