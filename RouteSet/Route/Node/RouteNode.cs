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
            reader.ReadStartElement("node");
            RouteEvent edgeEvent = new RouteEvent() { IsNodeEvent=false };
            edgeEvent.ReadXml(reader);
            EdgeEvent = edgeEvent;
            bool doNodeLoop = true;
            while (doNodeLoop)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        Console.WriteLine("     EVENT START");
                        RouteEvent nodeEvent = new RouteEvent() { IsNodeEvent = true };
                        nodeEvent.ReadXml(reader);
                        NodeEvents.Add(nodeEvent);
                        continue;
                    case XmlNodeType.EndElement:
                        Console.WriteLine("     EVENT END");
                        doNodeLoop = false;
                        break;
                }
            }
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
