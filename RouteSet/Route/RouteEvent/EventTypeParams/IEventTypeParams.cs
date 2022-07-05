using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace RouteSetTool
{
    public interface IEventTypeParams : IXmlSerializable
    {
        void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback);
        void Write(BinaryWriter writer);
        List<FoxHash> GetRouteNames();
    }
    public class EventTypeParams_Default : IEventTypeParams
    {
        public uint[] Params = new uint[4];

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            for (int index = 0; index < 4; index++)
            {
                Params[index] = reader.ReadUInt32();
                Console.WriteLine($"@{reader.BaseStream.Position} Event param#{index}: {Params[index]}");
            }
        }

        public void ReadXml(XmlReader reader)
        {
            for (int index = 0; index < 4; index++)
            {
                reader.ReadStartElement("param" + index);
                Params[index] = 0;
                uint.TryParse(reader.ReadString(), out Params[index]);
                reader.ReadEndElement();
            }
        }

        public void Write(BinaryWriter writer)
        {
            foreach (uint param in Params)
                writer.Write(param);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", "default");
            for (int index = 0; index < 4; index++)
            {
                writer.WriteStartElement("param" + index);
                writer.WriteString(Params[index].ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        public List<FoxHash> GetRouteNames()
        {
            return new List<FoxHash>();
        }
    }
}
