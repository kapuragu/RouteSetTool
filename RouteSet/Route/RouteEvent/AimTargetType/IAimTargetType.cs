using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteSetTool
{
    public interface IAimTargetType
    {
        void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback);
        void Write(BinaryWriter writer);
    }
    public class AimNone : IAimTargetType
    {
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            reader.ReadZeroes(4 * 4);
        }
        public void Write(BinaryWriter writer)
        {
            writer.WriteZeroes(4 * 4);
        }
    }
    public class AimStaticPoint : IAimTargetType
    {
        public Vector3 Point = new Vector3(); //ROUTE_AIM_STATIC_POINT
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            var point = new Vector3();
            point.Read(reader);
            Point = point;
            Console.WriteLine($"@{reader.BaseStream.Position} Aim target: | {Point.x} || {Point.y} || {Point.z} |");
            reader.ReadZeroes(4);
        }
        public void Write(BinaryWriter writer)
        {
            Point.Write(writer);
            writer.WriteZeroes(4);
        }
    }
    public class AimCharacter : IAimTargetType
    {
        public FoxHash CharacterName; //ROUTE_AIM_CHARACTER //TODO StrCode64 or CHECK if can be written as str32 (bothersome to add 64)
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            CharacterName = new FoxHash(FoxHash.Type.StrCode32);
            CharacterName.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var CharacterName_printString = CharacterName.HashValue.ToString();
            if (CharacterName.IsStringKnown)
                CharacterName_printString = CharacterName.StringLiteral;
            Console.WriteLine($"@{reader.BaseStream.Position} Conversation label: {CharacterName_printString }");

            reader.BaseStream.Position += 4;//ASSUMING strcode64 leftovers here
            reader.ReadZeroes(4 * 2);
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(CharacterName.HashValue);
            writer.WriteZeroes(4 * 3);
        }
    }
    public class AimRouteAsSightMovePath : IAimTargetType
    {
        public FoxHash[] RouteNames = new FoxHash[4]; //ROUTE_AIM_ROUTE_AS_SIGHT_MOVE_PATH/ROUTE_AIM_ROUTE_AS_OBJECT
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
        public void Write(BinaryWriter writer)
        {
            foreach (FoxHash param in RouteNames)
                param.Write(writer);
        }
    }
    public class AimRouteAsObject : IAimTargetType
    {
        public FoxHash[] RouteNames = new FoxHash[4]; //ROUTE_AIM_ROUTE_AS_SIGHT_MOVE_PATH/ROUTE_AIM_ROUTE_AS_OBJECT
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
        public void Write(BinaryWriter writer)
        {
            foreach (FoxHash param in RouteNames)
                param.Write(writer);
        }
    }
}
