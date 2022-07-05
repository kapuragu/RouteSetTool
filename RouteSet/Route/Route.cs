using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace RouteSetTool
{
    public class Route : IXmlSerializable
    {
        public FoxHash Name = new FoxHash(FoxHash.Type.StrCode32);
        public List<RouteNode> Nodes = new List<RouteNode>();

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            Name = new FoxHash(FoxHash.Type.StrCode32);
            Name.ReadXml(reader, "id");

            var logName = Name.HashValue.ToString();
            if (Name.IsStringKnown)
                logName = Name.StringLiteral;
            Console.WriteLine($"Id {logName}");

            reader.ReadStartElement("route");
            var loop = true;
            while (loop)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        Console.WriteLine("   NODE START");
                        RouteNode node = new RouteNode();
                        node.ReadXml(reader);
                        Nodes.Add(node);
                        reader.ReadEndElement();
                        continue;
                    case XmlNodeType.EndElement:
                        Console.WriteLine("   NODE END");
                        loop = false;
                        break;
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("route");
            Name.WriteXml(writer, "id");
            foreach (RouteNode node in Nodes)
                node.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
