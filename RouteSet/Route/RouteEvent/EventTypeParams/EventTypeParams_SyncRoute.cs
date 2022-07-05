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
    //SyncRoute
    public class EventTypeParams_SyncRoute : IEventTypeParams
    {
        //TppEnemy.OnAllocate missionTable.enemy.syncRouteTable SyncRouteManager s10150_enemy02
        public FoxHash SyncTableName;
        public int StepIndex;
        public uint Param2;
        public uint Param3;

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {

            SyncTableName = new FoxHash(FoxHash.Type.StrCode32);
            SyncTableName.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var SyncTableName_printString = SyncTableName.HashValue.ToString();
            if (SyncTableName.IsStringKnown)
                SyncTableName_printString = SyncTableName.StringLiteral;
            Console.WriteLine($"@{reader.BaseStream.Position} SyncTableName: {SyncTableName_printString }");

            StepIndex = reader.ReadInt32();

            Console.WriteLine($"@{reader.BaseStream.Position} StepIndex: {StepIndex }");

            Param2 = reader.ReadUInt32();

            Console.WriteLine($"@{reader.BaseStream.Position} Argument: {Param2 }");

            Param3 = reader.ReadUInt32();

            Console.WriteLine($"@{reader.BaseStream.Position} Argument: {Param3 }");
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("syncTableName");
            SyncTableName = new FoxHash(FoxHash.Type.StrCode32);
            SyncTableName.ReadXmlString(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("stepIndex");
            StepIndex = 0;
            int.TryParse(reader.ReadString(), out StepIndex);
            reader.ReadEndElement();

            reader.ReadStartElement("param2");
            Param2 = 0;
            int.TryParse(reader.ReadString(), out StepIndex);
            reader.ReadEndElement();

            reader.ReadStartElement("param3");
            Param3 = 0;
            int.TryParse(reader.ReadString(), out StepIndex);
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(SyncTableName.HashValue);
            writer.Write(StepIndex);
            writer.Write(Param2);
            writer.Write(Param3);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", "SyncRoute");

            writer.WriteStartElement("syncTableName");
            SyncTableName.WriteXmlString(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("stepIndex");
            writer.WriteString(StepIndex.ToString());
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
