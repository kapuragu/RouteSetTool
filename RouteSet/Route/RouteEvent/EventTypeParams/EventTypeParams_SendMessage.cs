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
    //SendMessage
    //Triggers RoutePoint2 message with messageId hash as argument
    public class EventTypeParams_SendMessage : IEventTypeParams
    {
        //strcode64 leftover uint
        //Message hash
        public FoxHash Message;
        //strcode64 leftover uint
        //Route id for some reason ?
        public FoxHash Param;

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            reader.BaseStream.Position += 4;//strcode64 leftover

            Message = new FoxHash(FoxHash.Type.StrCode32);
            Message.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var ConversationLabel_printString = Message.HashValue.ToString();
            if (Message.IsStringKnown)
                ConversationLabel_printString = Message.StringLiteral;
            Console.WriteLine($"@{reader.BaseStream.Position} Message: {ConversationLabel_printString }");

            Param = new FoxHash(FoxHash.Type.StrCode32);
            Param.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var Friend_printString = Param.HashValue.ToString();
            if (Param.IsStringKnown)
                Friend_printString = Param.StringLiteral;

            Console.WriteLine($"@{reader.BaseStream.Position} RouteName: {Friend_printString }");

            reader.BaseStream.Position += 4;//strcode64 leftover
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("message");
            Message = new FoxHash(FoxHash.Type.StrCode32);
            Message.ReadXmlString(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("param");
            Param = new FoxHash(FoxHash.Type.StrCode32);
            Param.ReadXmlString(reader);
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteZeroes(4);//strcode64 leftover
            writer.Write(Message.HashValue);
            writer.Write(Param.HashValue);
            writer.WriteZeroes(4);//strcode64 leftover
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", "SendMessage");

            writer.WriteStartElement("message");
            Message.WriteXmlString(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("param");
            Param.WriteXmlString(writer);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
        public List<FoxHash> GetRouteNames()
        {
            List<FoxHash> retList = new List<FoxHash>() { Param };
            return retList;
        }
    }
}
