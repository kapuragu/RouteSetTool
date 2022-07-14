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
    public interface IEventParam : IXmlSerializable
    {
        void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback);
        void Write(BinaryWriter writer);
        List<FoxHash> GetRouteNames();
    }
    public class EventParamUInt : IEventParam
    {
        public uint Param = 0;
        public List<FoxHash> GetRouteNames()
        {
            return new List<FoxHash>();
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Param = reader.ReadUInt32();
        }

        public void ReadXml(XmlReader reader)
        {
            uint.TryParse(reader.ReadString(),out Param);
            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Param);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", "uint32");
            writer.WriteString(Param.ToString());
            writer.WriteEndElement();
        }
    }
    public class EventParamInt : IEventParam
    {
        public int Param = 0;
        public List<FoxHash> GetRouteNames()
        {
            return new List<FoxHash>();
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Param = reader.ReadInt32();
        }

        public void ReadXml(XmlReader reader)
        {
            int.TryParse(reader.ReadString(), out Param);
            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Param);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", "int32");
            writer.WriteString(Param.ToString());
            writer.WriteEndElement();
        }
    }
    public class EventParamFloat : IEventParam
    {
        public float Param = 0;
        public List<FoxHash> GetRouteNames()
        {
            return new List<FoxHash>();
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Param = reader.ReadSingle();
        }

        public void ReadXml(XmlReader reader)
        {
            Param = Extensions.ParseFloatRoundtrip(reader.ReadString());
            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Param);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", "float");
            writer.WriteString(Param.ToString());
            writer.WriteEndElement();
        }
    }
    public class EventParamStrCode32 : IEventParam
    {
        public FoxHash Param = new FoxHash(FoxHash.Type.StrCode32);
        public List<FoxHash> GetRouteNames()
        {
            return new List<FoxHash>() { Param };
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Param = new FoxHash(FoxHash.Type.StrCode32);
            Param.Read(reader, nameLookupTable, hashIdentifiedCallback);
        }

        public void ReadXml(XmlReader reader)
        {
            Param = new FoxHash(FoxHash.Type.StrCode32);
            Param.ReadXmlString(reader);
            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Param.HashValue);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", "String");
            Param.WriteXmlString(writer);
            writer.WriteEndElement();
        }
    }
}
