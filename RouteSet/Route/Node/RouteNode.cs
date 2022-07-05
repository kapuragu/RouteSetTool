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
    public class RouteNode : IXmlSerializable
    {
        public Vector3 Translation = new Vector3();
        public RouteEvent EdgeEvent = new RouteEvent();
        public List<RouteEvent> NodeEvents = new List<RouteEvent>();

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            Translation = new Vector3();
            Translation.ReadXml(reader);
            Console.WriteLine($"Translation x: {Translation.x} y: {Translation.y} z: {Translation.z}");
            reader.ReadStartElement("node");
            RouteEvent edgeEvent = new RouteEvent() { IsNodeEvent=false };
            edgeEvent.ReadXml(reader);
            EdgeEvent = edgeEvent;

            reader.ReadStartElement("nodeEvents");
            while (reader.Read())
            {
                if (reader.Name.Equals("event") && reader.NodeType == XmlNodeType.Element)
                {
                    Console.WriteLine("      EVENT START");
                    RouteEvent nodeEvent = new RouteEvent() { IsNodeEvent = true };
                    nodeEvent.ReadXml(reader);
                    NodeEvents.Add(nodeEvent);
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals("event"))
                        {
                            break;
                        }
                    }
                }
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("node");
            Translation.WriteXml(writer);
            EdgeEvent.WriteXml(writer);
            writer.WriteStartElement("nodeEvents");
            foreach (RouteEvent nodeEvent in NodeEvents)
                nodeEvent.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}
