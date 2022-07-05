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
    public interface IAimTargetType : IXmlSerializable
    {
        void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback);
        void Write(BinaryWriter writer);
        List<FoxHash> GetRouteNames();
    }
    public class AimNone : IAimTargetType
    {
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            reader.ReadZeroes(4 * 4);
        }

        public void ReadXml(XmlReader reader) {}

        public void Write(BinaryWriter writer)
        {
            writer.WriteZeroes(4 * 4);
        }

        public void WriteXml(XmlWriter writer)
        {
            //don't need to B)
        }
        public List<FoxHash> GetRouteNames()
        {
            return new List<FoxHash>();
        }
    }
    public class AimStaticPoint : IAimTargetType
    {
        public Vector3 StaticPoint = new Vector3(); //ROUTE_AIM_STATIC_POINT

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            var point = new Vector3();
            point.Read(reader);
            StaticPoint = point;
            Console.WriteLine($"@{reader.BaseStream.Position} Aim target: | {StaticPoint.x} || {StaticPoint.y} || {StaticPoint.z} |");
            reader.ReadZeroes(4);
        }

        public void ReadXml(XmlReader reader)
        {
            StaticPoint = new Vector3();
            StaticPoint.ReadXml(reader);
            reader.ReadStartElement("staticPoint");
        }

        public void Write(BinaryWriter writer)
        {
            StaticPoint.Write(writer);
            writer.WriteZeroes(4);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("staticPoint");
            StaticPoint.WriteXml(writer);
            writer.WriteEndElement();
        }
        public List<FoxHash> GetRouteNames()
        {
            return new List<FoxHash>();
        }
    }
    public class AimCharacter : IAimTargetType
    {
        public FoxHash CharacterName; //ROUTE_AIM_CHARACTER //TODO StrCode64 or CHECK if can be written as str32 (bothersome to add 64)

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            CharacterName = new FoxHash(FoxHash.Type.StrCode32);
            CharacterName.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var CharacterName_printString = CharacterName.HashValue.ToString();
            if (CharacterName.IsStringKnown)
                CharacterName_printString = CharacterName.StringLiteral;
            Console.WriteLine($"@{reader.BaseStream.Position} Character aim target: {CharacterName_printString }");

            reader.BaseStream.Position += 4;//ASSUMING strcode64 leftovers here
            reader.ReadZeroes(4 * 2);
        }

        public void ReadXml(XmlReader reader)
        {
            CharacterName = new FoxHash(FoxHash.Type.StrCode32);
            CharacterName.ReadXmlString(reader);
            reader.ReadStartElement("character");
            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(CharacterName.HashValue);
            writer.WriteZeroes(4 * 3);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("character");
            CharacterName.WriteXmlString(writer);
            writer.WriteEndElement();
        }
        public List<FoxHash> GetRouteNames()
        {
            return new List<FoxHash>();
        }
    }
    public class AimRouteAsSightMovePath : IAimTargetType
    {
        public FoxHash[] RouteNames = new FoxHash[4]; //ROUTE_AIM_ROUTE_AS_SIGHT_MOVE_PATH/ROUTE_AIM_ROUTE_AS_OBJECT

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            for (int index = 0; index < 4; index++)
            {
                RouteNames[index] = new FoxHash(FoxHash.Type.StrCode32);
                RouteNames[index].Read(reader, nameLookupTable, hashIdentifiedCallback);
                if (RouteNames[index].IsStringKnown)
                    Console.WriteLine($"@{reader.BaseStream.Position} Route Target#{index} Name: {RouteNames[index].StringLiteral}");
                else
                    Console.WriteLine($"@{reader.BaseStream.Position} Route Target#{index} Name: {RouteNames[index].HashValue}");
            }
        }

        public void ReadXml(XmlReader reader)
        {
            for (int index = 0; index < 4; index++)
            {
                RouteNames[index] = new FoxHash(FoxHash.Type.StrCode32);
                RouteNames[index].ReadXmlString(reader);
            }
            reader.ReadStartElement("routeAsSightMovePath");
            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            foreach (FoxHash param in RouteNames)
                param.Write(writer);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("routeAsSightMovePath");
            for (int index = 0; index < 4; index++)
            {
                writer.WriteStartElement("routeAsSightMovePath" + index);
                RouteNames[index].WriteXmlString(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        public List<FoxHash> GetRouteNames()
        {
            return RouteNames.ToList();
        }
    }
    public class AimRouteAsObject : IAimTargetType
    {
        public FoxHash[] RouteNames = new FoxHash[4]; //ROUTE_AIM_ROUTE_AS_SIGHT_MOVE_PATH/ROUTE_AIM_ROUTE_AS_OBJECT


        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            for (int index = 0; index < 4; index++)
            {
                RouteNames[index] = new FoxHash(FoxHash.Type.StrCode32);
                RouteNames[index].Read(reader, nameLookupTable, hashIdentifiedCallback);
                if (RouteNames[index].IsStringKnown)
                    Console.WriteLine($"@{reader.BaseStream.Position} Route Target#{index} Name: {RouteNames[index].StringLiteral}");
                else
                    Console.WriteLine($"@{reader.BaseStream.Position} Route Target#{index} Name: {RouteNames[index].HashValue}");
            }
        }

        public void ReadXml(XmlReader reader)
        {
            for (int index = 0; index < 4; index++)
            {
                RouteNames[index] = new FoxHash(FoxHash.Type.StrCode32);
                RouteNames[index].ReadXmlString(reader);
            }
            reader.ReadStartElement("routeAsSightMovePath");
            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            foreach (FoxHash param in RouteNames)
                param.Write(writer);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("routeAsObject");
            for (int index = 0; index < 4; index++)
            {
                writer.WriteStartElement("routeAsObject" + index);
                RouteNames[index].WriteXmlString(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        public List<FoxHash> GetRouteNames()
        {
            return RouteNames.ToList();
        }
    }
}
