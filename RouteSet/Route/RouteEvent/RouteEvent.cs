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
    public enum RouteAimTargetType : byte
    {
        ROUTE_AIM_NO_TARGET = 0,
        ROUTE_AIM_STATIC_POINT = 1,
        ROUTE_AIM_CHARACTER = 2,
        ROUTE_AIM_ROUTE_AS_SIGHT_MOVE_PATH = 3,
        ROUTE_AIM_ROUTE_AS_OBJECT = 4,
    }
    public class RouteEvent : IXmlSerializable
    {
        public FoxHash EventType = new FoxHash(FoxHash.Type.StrCode32);

        public bool IsNodeEvent = false;
        public RouteAimTargetType AimTargetType = RouteAimTargetType.ROUTE_AIM_NO_TARGET;
        //literally never used byte
        public bool IsLoop = false;

        public ushort Time = 0;
        public short Dir = 0;

        public IAimTargetType AimTargetTypeParams = new AimNone();
        public IEventTypeParams EventTypeParams = new EventTypeParams_Default();

        public char[] Snippet = new char[4];

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback, RouteSetVersion version)
        {
            EventType = new FoxHash(FoxHash.Type.StrCode32);
            EventType.Read(reader, nameLookupTable, hashIdentifiedCallback);
            IsNodeEvent = reader.ReadBoolean();
            AimTargetType = (RouteAimTargetType)reader.ReadByte();
            reader.ReadZeroes(1);
            IsLoop = reader.ReadBoolean();

            var logName = EventType.HashValue.ToString();
            if (EventType.IsStringKnown)
                logName = EventType.StringLiteral;
            Console.WriteLine($"@{reader.BaseStream.Position} Event {logName}: Is node event: {IsNodeEvent}, aim type: {AimTargetType}, is loop: {IsLoop}");

            Time = reader.ReadUInt16();
            Dir = reader.ReadInt16();

            if (version == RouteSetVersion.GZ)
            {
                reader.ReadByte();
                //IsNodeEvent = IsEdgeEvent == 0; //-1 = true, 0 = false
                reader.ReadZeroes(3);
            }

            Console.WriteLine($"@{reader.BaseStream.Position} Time: {Time}, direction: {Dir}");

            switch (AimTargetType)
            {
                default:
                case RouteAimTargetType.ROUTE_AIM_NO_TARGET:
                    AimTargetTypeParams = new AimNone();
                    break;
                case RouteAimTargetType.ROUTE_AIM_STATIC_POINT:
                    AimTargetTypeParams = new AimStaticPoint();
                    break;
                case RouteAimTargetType.ROUTE_AIM_CHARACTER:
                    AimTargetTypeParams = new AimCharacter();
                    break;
                case RouteAimTargetType.ROUTE_AIM_ROUTE_AS_SIGHT_MOVE_PATH:
                    AimTargetTypeParams = new AimRouteAsSightMovePath();
                    break;
                case RouteAimTargetType.ROUTE_AIM_ROUTE_AS_OBJECT:
                    AimTargetTypeParams = new AimRouteAsObject();
                    break;
            }
            AimTargetTypeParams.Read(reader, nameLookupTable, hashIdentifiedCallback);

            SetEventTypeClass();
            EventTypeParams.Read(reader, nameLookupTable, hashIdentifiedCallback);

            if (version == RouteSetVersion.TPP)
            {
                //Running a simple ReadChars(4) on f30010 might be giving out this bug:
                //https://stackoverflow.com/questions/1804433/issue-with-binaryreader-readchars
                /*
                Snippet = reader.ReadChars(4);
                */
                //solution: https://stackoverflow.com/a/25354715
                byte[] str = reader.ReadBytes(4);
                Snippet = Encoding.Default.GetChars(str, 0, 4);
                Array.Reverse(str);
                Console.WriteLine($"@{reader.BaseStream.Position} Snippet: {BitConverter.ToUInt32(str, 0)}");
            }
        }

        public void ReadXml(XmlReader reader)
        {
            EventType = new FoxHash(FoxHash.Type.StrCode32);
            EventType.ReadXml(reader, "type");

            var logName = EventType.HashValue.ToString();
            if (EventType.IsStringKnown)
                logName = EventType.StringLiteral;
            Console.WriteLine($"Event {logName}");

            //isnodeevent is set during call
            if (IsNodeEvent)
            {
                IsLoop = bool.Parse(reader.GetAttribute("loop"));
                Time= ushort.Parse(reader.GetAttribute("time"));
                Dir = short.Parse(reader.GetAttribute("dir"));
                Console.WriteLine($"IsNodeEvent true IsLoop: {IsLoop} Time: {Time} Dir: {Dir}");
            }
            else
            {
                IsLoop = false;
                Time = 0;
                Dir = 0;
                Console.WriteLine("IsNodeEvent false");
            }

            reader.ReadStartElement("event");

            AimTargetTypeParams = GetAimPointTypeFromXml(reader);
            AimTargetTypeParams.ReadXml(reader);

            SetEventTypeClass();
            //var readType = reader["type"];
            reader.ReadStartElement("params");
            EventTypeParams.ReadXml(reader);

            reader.ReadEndElement();

            Console.WriteLine($"Snippet: {Snippet[0]} {Snippet[1]} {Snippet[2]} {Snippet[3]}");
        }

        public void Write(BinaryWriter writer, RouteSetVersion version)
        {
            EventType.Write(writer);

            writer.Write(IsNodeEvent);
            writer.Write((byte)AimTargetType);
            writer.WriteZeroes(1);
            writer.Write(IsLoop);

            writer.Write(Time);
            writer.Write(Dir);

            if (version == RouteSetVersion.GZ)
            {
                writer.Write((byte)(-Convert.ToByte(IsNodeEvent)));
                //IsNodeEvent = IsEdgeEvent == 0; //-1 = true, 0 = false
                writer.WriteZeroes(3);
            }

            AimTargetTypeParams.Write(writer);
            EventTypeParams.Write(writer);

            if (version == RouteSetVersion.TPP)
                writer.Write(Snippet); //writer.WriteZeroes(4);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("event");
            EventType.WriteXml(writer, "type");
            if (IsNodeEvent)
            {
                writer.WriteAttributeString("loop", IsLoop.ToString());
                writer.WriteAttributeString("time", Time.ToString());
                writer.WriteAttributeString("dir", Dir.ToString());
            }
            AimTargetTypeParams.WriteXml(writer);
            writer.WriteStartElement("params");
            EventTypeParams.WriteXml(writer);
            writer.WriteEndElement();
        }
        public IAimTargetType GetAimPointTypeFromXml(XmlReader reader)
        {
            if (reader.Name=="staticPoint")
                return new AimStaticPoint();
            else if (reader.Name == "character")
                return new AimCharacter();
            else if (reader.Name == "routeAsSightMovePath")
                return new AimRouteAsSightMovePath();
            else if (reader.Name == "routeAsObject")
                return new AimRouteAsObject();
            return new AimNone();
        }
        public void SetEventTypeClass()
        {
            switch (EventType.HashValue)
            {
                default:
                    EventTypeParams = new EventTypeParams_Default();
                    break;
                case 4019510599: //RelaxedIdleAct
                case 2973097149: //CautionIdleAct
                    EventTypeParams = new EventTypeParams__common_IdleAct();
                    break;
                case 804119634: //chase
                    EventTypeParams = new EventTypeParams_chase();
                    break;
                case 3952237029: //Conversation
                    EventTypeParams = new EventTypeParams_Conversation();
                    break;
                case 1536918290: //ConversationIdle
                    EventTypeParams = new EventTypeParams_ConversationIdle();
                    break;
                case 244962479: //Dash
                case 4126739186: //ForwardChangeSpeed
                case 368586264: //move
                case 4202868537: //Move
                case 1375828191: //SetTargetSpeed
                case 591731182: //Walk
                case 3641577009: //RelaxedStandWalkAct
                case 1472482162: //RelaxedWalkAct
                    EventTypeParams = new EventTypeParams__common_SpeedInt();
                    break;
                case 3100429757: //DropPoint
                case 3396619717: //Hovering
                    EventTypeParams = new EventTypeParams__common_heli();
                    break;
                case 2829631605: //PutHostageInVehicle
                case 2481191805: //TakeHostageOutOfVehicle
                case 2265318157: //SendMessage
                    EventTypeParams = new EventTypeParams_SendMessage();
                    break;
                case 385624276: //SwitchRoute
                    EventTypeParams = new EventTypeParams_SwitchRoute();
                    break;
                case 3369952648: //SyncRoute
                    EventTypeParams = new EventTypeParams_SyncRoute();
                    break;
                case 370178288: //VehicleBackFast
                case 3157073279: //VehicleBackNormal
                case 895529986: //VehicleBackSlow
                case 3487140098: //VehicleMoveFast
                case 4258228081: //VehicleMoveNormal
                case 3297759236: //VehicleMoveSlow
                case 41204288: //
                    EventTypeParams = new EventTypeParams__common_Vehicle();
                    break;
                    //41204288
            }
        }
    }
}
