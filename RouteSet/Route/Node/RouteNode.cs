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
            //Console.WriteLine($"Translation x: {Translation.x} y: {Translation.y} z: {Translation.z}");
            reader.ReadStartElement("node");
            RouteEvent edgeEvent = new RouteEvent() { IsNodeEvent=false };
            var readingEdge = true;
            edgeEvent.ReadXml(reader);
            reader.ReadEndElement();
            readingEdge = false;
            EdgeEvent = edgeEvent;

            while ((2 > 1)&!readingEdge)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "nodeEvents")
                        {
                            reader.ReadStartElement("nodeEvents");
                            //Console.WriteLine("      NODEEVENTS START");
                        }
                        else if (reader.Name == "event")
                        {
                            //Console.WriteLine("         EVENT START");
                            RouteEvent nodeEvent = new RouteEvent() { IsNodeEvent = true };
                            nodeEvent.ReadXml(reader);
                            reader.ReadEndElement();
                            NodeEvents.Add(nodeEvent);
                        }
                        continue;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "event")
                        {
                            //Console.WriteLine("         EVENT END");
                            reader.ReadEndElement();
                            continue;
                        }
                        else if (reader.Name== "nodeEvents")
                        {
                            //Console.WriteLine("      NODEEVENTS END");
                            reader.ReadEndElement();
                            return;
                        }
                        else
                        {
                            return;
                        }

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
