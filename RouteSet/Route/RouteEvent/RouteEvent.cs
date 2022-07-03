using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class RouteEvent
    {
        public FoxHash EventType;

        public bool IsNodeEvent;
        public RouteAimTargetType AimTargetType;
        //literally never used byte
        public bool IsLoop;

        public ushort Time;
        public short Dir;

        public IAimTargetType AimTargetTypeParams;
        public IEventTypeParams EventTypeParams;

        public char[] Snippet = new char[4];
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

            switch (EventType.HashValue)
            {
                default:
                    EventTypeParams = new EventTypeParams_Default();
                    break;
                case 4019510599: //RelaxedIdleAct
                case 2973097149: //CautionIdleAct
                    EventTypeParams = new EventTypeParams_IdleAct();
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
            }
            EventTypeParams.Read(reader, nameLookupTable, hashIdentifiedCallback);

            if (version == RouteSetVersion.TPP)
            {
                Snippet = reader.ReadChars(4);
                Console.WriteLine($"@{reader.BaseStream.Position} Snippet: {new string(Snippet)}");
            }
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
    }
}
