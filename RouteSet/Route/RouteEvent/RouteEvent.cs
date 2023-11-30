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
        //<-literally never used byte
        public bool IsLoop = false;

        public ushort Time = 0;
        public short Dir = 0;

        public IAimTargetType AimTargetTypeParams = new AimNone();
        public IEventParam Param0 = new EventParamInt();
        public IEventParam Param1 = new EventParamInt();
        public IEventParam Param2 = new EventParamInt();
        public IEventParam Param3 = new EventParamInt();

        //public string Snippet = new char[4]{'\0','\0','\0','\0'}.ToString();

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
            //Console.WriteLine($"@{reader.BaseStream.Position} Event {logName}: Is node event: {IsNodeEvent}, aim type: {AimTargetType}, is loop: {IsLoop}");

            Time = reader.ReadUInt16();
            Dir = reader.ReadInt16();

            if (version == RouteSetVersion.GZ)
            {
                reader.ReadByte();
                //IsNodeEvent = IsEdgeEvent == 0; //-1 = true, 0 = false
                reader.ReadZeroes(3);
            }

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

            SetEventParamTypesByHash();
            Param0.Read(reader, nameLookupTable, hashIdentifiedCallback);
            Param1.Read(reader, nameLookupTable, hashIdentifiedCallback);
            Param2.Read(reader, nameLookupTable, hashIdentifiedCallback);
            Param3.Read(reader, nameLookupTable, hashIdentifiedCallback);

            if (version == RouteSetVersion.TPP)
            {
                //Running a simple ReadChars(4) on f30010 might be giving out this bug:
                //https://stackoverflow.com/questions/1804433/issue-with-binaryreader-readchars
                /*
                Snippet = reader.ReadChars(4);
                */
                //solution: https://stackoverflow.com/a/25354715
                byte[] str = reader.ReadBytes(4);
                var charas = Encoding.Default.GetChars(str, 0, 4);
                Array.Reverse(charas);
                string snippet = charas.ToString();
                //Console.WriteLine($"@{reader.BaseStream.Position} Snippet: {BitConverter.ToUInt32(snippet, 0)}");
            }
        }

        public void ReadXml(XmlReader reader)
        {
            EventType = new FoxHash(FoxHash.Type.StrCode32);
            EventType.ReadXml(reader, "type");

            /*var logName = EventType.HashValue.ToString();
            if (EventType.IsStringKnown)
                logName = EventType.StringLiteral;
            Console.WriteLine($"Event {logName}");*/

            //isnodeevent is set during call
            if (IsNodeEvent)
            {
                IsLoop = bool.Parse(reader.GetAttribute("loop"));
                Time= ushort.Parse(reader.GetAttribute("time"));
                Dir = short.Parse(reader.GetAttribute("dir"));
                //Console.WriteLine($"IsNodeEvent true IsLoop: {IsLoop} Time: {Time} Dir: {Dir}");
            }
            else
            {
                IsLoop = false;
                Time = 0;
                Dir = 0;
                //Console.WriteLine("IsNodeEvent false");
            }

            reader.ReadStartElement("event");

            AimTargetTypeParams = GetAimPointTypeFromXml(reader);
            AimTargetTypeParams.ReadXml(reader);

            Param0 = SetEventParamTypeReadXml(reader);
            reader.ReadStartElement("param0");
            Param0.ReadXml(reader);

            Param1 = SetEventParamTypeReadXml(reader);
            reader.ReadStartElement("param1");
            Param1.ReadXml(reader);

            Param2 = SetEventParamTypeReadXml(reader);
            reader.ReadStartElement("param2");
            Param2.ReadXml(reader);

            Param3 = SetEventParamTypeReadXml(reader);
            reader.ReadStartElement("param3");
            Param3.ReadXml(reader);

            //Console.WriteLine($"Snippet: {Snippet[0]} {Snippet[1]} {Snippet[2]} {Snippet[3]}");
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
            Param0.Write(writer);
            Param1.Write(writer);
            Param2.Write(writer);
            Param3.Write(writer);

            if (version == RouteSetVersion.TPP)
            {
                /* 
                var str = new char[4] { '\0' , '\0' , '\0' , '\0' };
                for (int i = 0; i < 4; i++)
                    if (Snippet.Length<i)
                        str[i] = Snippet.ToCharArray()[i];
                for (int i = 0; i < 4; i++)
                {
                    writer.Write(str[i]);
                }*/
                //Yeah screw it
                writer.WriteZeroes(4);
            }
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
            writer.WriteStartElement("param0");
            Param0.WriteXml(writer);
            writer.WriteStartElement("param1");
            Param1.WriteXml(writer);
            writer.WriteStartElement("param2");
            Param2.WriteXml(writer);
            writer.WriteStartElement("param3");
            Param3.WriteXml(writer);
            writer.WriteEndElement();
        }
        public IAimTargetType GetAimPointTypeFromXml(XmlReader reader)
        {
            switch (reader.Name)
            {
                default:
                    AimTargetType = RouteAimTargetType.ROUTE_AIM_NO_TARGET;
                    return new AimNone();
                case "staticPoint":
                    AimTargetType = RouteAimTargetType.ROUTE_AIM_STATIC_POINT;
                    return new AimStaticPoint();
                case "character":
                    AimTargetType = RouteAimTargetType.ROUTE_AIM_CHARACTER;
                    return new AimCharacter();
                case "routeAsSightMovePath":
                    AimTargetType = RouteAimTargetType.ROUTE_AIM_ROUTE_AS_SIGHT_MOVE_PATH;
                    return new AimRouteAsSightMovePath();
                case "routeAsObject":
                    AimTargetType = RouteAimTargetType.ROUTE_AIM_ROUTE_AS_OBJECT;
                    return new AimRouteAsObject();
            }
        }
        public IEventParam SetEventParamTypeReadXml(XmlReader reader)
        {
            switch (reader["type"])
            {
                default:
                    throw new NotImplementedException();
                case "uint32":
                    return new EventParamUInt();
                case "int32":
                    return new EventParamInt();
                case "float":
                    return new EventParamFloat();
                case "String":
                    return new EventParamStrCode32();
            }
        }
        public void SetEventParamTypesByHash()
        {
            switch (EventType.HashValue)
            {
                default:
                case 804119634: //chase
                case 244962479: //Dash
                case 4126739186: //ForwardChangeSpeed
                case 368586264: //move
                case 4202868537: //Move
                case 1375828191: //SetTargetSpeed
                case 591731182: //Walk
                case 3641577009: //RelaxedStandWalkAct
                case 1472482162: //RelaxedWalkAct
                    Param0 = new EventParamInt();
                    Param1 = new EventParamInt();
                    Param2 = new EventParamInt();
                    Param3 = new EventParamInt();
                    break;
                case 4019510599: //RelaxedIdleAct
                case 2973097149: //CautionIdleAct
                    Param0 = new EventParamStrCode32();
                    //RelaxedIdleAct 4b2d/19245 along with 496717012/StandActNormal in s10030, doesn't seem like a 64 bit leftover
                    Param1 = new EventParamInt();
                    Param2 = new EventParamInt();
                    Param3 = new EventParamInt();
                    break;
                case 3952237029: //Conversation
                    Param0 = new EventParamStrCode32();
                    Param1 = new EventParamInt();//strcode64 leftover
                    Param2 = new EventParamStrCode32();
                    Param3 = new EventParamInt();//strcode64 leftover
                    break;
                case 1536918290: //ConversationIdle
                    Param0 = new EventParamStrCode32();
                    Param1 = new EventParamStrCode32();
                    Param2 = new EventParamInt();//strcode64 leftover
                    Param3 = new EventParamInt();
                    break;
                case 3100429757: //DropPoint
                case 3396619717: //Hovering
                    Param0 = new EventParamInt();
                    Param1 = new EventParamUInt();
                    Param2 = new EventParamUInt();
                    Param3 = new EventParamUInt();
                    break;
                case 2829631605: //PutHostageInVehicle
                case 2481191805: //TakeHostageOutOfVehicle
                case 2265318157: //SendMessage
                    Param0 = new EventParamInt();//strcode64 leftover
                    Param1 = new EventParamStrCode32();
                    Param2 = new EventParamStrCode32();
                    Param3 = new EventParamInt();//strcode64 leftover
                    break;
                case 385624276: //SwitchRoute
                    Param0 = new EventParamStrCode32();//strcode64 leftover
                    Param1 = new EventParamStrCode32();
                    Param2 = new EventParamStrCode32();
                    Param3 = new EventParamInt();//never used; second argument exists in lua, TODO check
                    break;
                case 3369952648: //SyncRoute
                    Param0 = new EventParamStrCode32();//TppEnemy.OnAllocate missionTable.enemy.syncRouteTable SyncRouteManager s10150_enemy02
                    Param1 = new EventParamUInt();//step index
                    Param2 = new EventParamInt();
                    Param3 = new EventParamInt();
                    break;
                case 370178288: //VehicleBackFast
                case 3157073279: //VehicleBackNormal
                case 895529986: //VehicleBackSlow
                case 3487140098: //VehicleMoveFast
                case 4258228081: //VehicleMoveNormal
                case 3297759236: //VehicleMoveSlow
                case 41204288: //VehicleDir
                    Param0 = new EventParamStrCode32();//.frl/.frld rail name hash
                    Param1 = new EventParamInt();//RPM
                    Param2 = new EventParamUInt();
                    Param3 = new EventParamUInt();
                    break;
            }
        }
    }
}
