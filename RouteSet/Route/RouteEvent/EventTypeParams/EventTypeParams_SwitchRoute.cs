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
    //SwitchRoute
    public class EventTypeParams_SwitchRoute : IEventTypeParams
    {
        //route to switch to if returns true
        public FoxHash RouteName;
        //TppEnemy.SwitchRouteFunc "IsNotGimmickBroken" "IsGimmickBroken" "CanUseSearchLight" "CanNotUseSearchLight" 
        public FoxHash FunctionName;
        //TppEnemy.SwitchRouteFunc "gntn_srlg001_vrtn002_gim_n0003|srt_gntn_srlg001_gm", gimmickIds from location gimmick table
        public FoxHash Argument;
        //never used; second argument exists in lua, TODO check
        public uint Param3;

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            RouteName = new FoxHash(FoxHash.Type.StrCode32);
            RouteName.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var RouteName_printString = RouteName.HashValue.ToString();
            if (RouteName.IsStringKnown)
                RouteName_printString = RouteName.StringLiteral;
            Console.WriteLine($"@{reader.BaseStream.Position} RouteName: {RouteName_printString }");

            FunctionName = new FoxHash(FoxHash.Type.StrCode32);
            FunctionName.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var FunctionName_printString = FunctionName.HashValue.ToString();
            if (FunctionName.IsStringKnown)
                FunctionName_printString = FunctionName.StringLiteral;

            Console.WriteLine($"@{reader.BaseStream.Position} FunctionName: {FunctionName_printString }");

            Argument = new FoxHash(FoxHash.Type.StrCode32);
            Argument.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var Argument_printString = Argument.HashValue.ToString();
            if (FunctionName.IsStringKnown)
                Argument_printString = Argument.StringLiteral;

            Console.WriteLine($"@{reader.BaseStream.Position} Argument: {Argument_printString }");

            Param3 = reader.ReadUInt32();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("eventParams_SwitchRoute");

            reader.ReadStartElement("routeName");
            RouteName = new FoxHash(FoxHash.Type.StrCode32);
            RouteName.ReadXmlString(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("functionName");
            FunctionName = new FoxHash(FoxHash.Type.StrCode32);
            FunctionName.ReadXmlString(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("argument");
            Argument = new FoxHash(FoxHash.Type.StrCode32);
            Argument.ReadXmlString(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("param3");
            Param3 = 0;
            uint.TryParse(reader.ReadString(), out Param3);
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(RouteName.HashValue);
            writer.Write(FunctionName.HashValue);
            writer.Write(Argument.HashValue);
            writer.Write(Param3);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("eventParams_SwitchRoute");

            writer.WriteStartElement("routeName");
            RouteName.WriteXmlString(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("functionName");
            FunctionName.WriteXmlString(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("argument");
            Argument.WriteXmlString(writer);
            writer.WriteEndElement();

            writer.WriteStartElement("param3");
            writer.WriteString(Param3.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
        public List<FoxHash> GetRouteNames()
        {
            List<FoxHash> retList = new List<FoxHash>() { RouteName };
            return retList;
        }
    }
}
