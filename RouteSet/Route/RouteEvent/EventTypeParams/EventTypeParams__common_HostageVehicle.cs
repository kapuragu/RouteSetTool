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
    //PutHostageInVehicle
    //TakeHostageOutOfVehicle
    public class EventTypeParams__common_HostageVehicle : IEventTypeParams
    {
        public uint Param0;
        //Message hash
        public FoxHash Message;
        public uint Param2;
        public uint Param3;

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Param0 = reader.ReadUInt32();

            Message = new FoxHash(FoxHash.Type.StrCode32);
            Message.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var Message_printString = Message.HashValue.ToString();
            if (Message.IsStringKnown)
                Message_printString = Message.StringLiteral;
            Console.WriteLine($"@{reader.BaseStream.Position} Message: {Message_printString }");

            Param2 = reader.ReadUInt32();

            Param3 = reader.ReadUInt32();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("eventParams__common_hostageVehicle");

            reader.ReadStartElement("param0");
            Param0 = 0;
            uint.TryParse(reader.ReadString(), out Param0);
            reader.ReadEndElement();

            reader.ReadStartElement("message");
            Message = new FoxHash(FoxHash.Type.StrCode32);
            Message.ReadXmlString(reader);
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
            writer.Write(Message.HashValue);
            writer.Write(Param2);
            writer.Write(Param3);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("eventParams__common_hostageVehicle");

            writer.WriteStartElement("param0");
            writer.WriteString(Param0.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("message");
            Message.WriteXmlString(writer);
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
