using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteSetTool
{
    //RelaxedIdleAct
    //CautionIdleAct
    public class EventTypeParams_IdleAct : IEventTypeParams
    {
        //Animation
        //Known strings:
        //  StandActNormal Looking forward, mix of anims
        //  SoldierFootStep Stepping around
        //  StandActSmoking Smoking
        //Known hashes:
        //  911690047 Sleeping in a bed
        //  4113090604 Sleeping sitting?
        //  3641544758 Looking left and right, caution-like
        //  2135364818 Relaxed kick with a sound effect
        //  1049359963
        //  3641544758 Slow start, sneak into caution, left and right
        //  4293802043
        public FoxHash Animation;
        //RelaxedIdleAct 4b2d/19245 along with 496717012/StandActNormal in s10030, doesn't seem like a 64 bit leftover
        public uint Param1;
        //For safety
        public uint Param2;
        public uint Param3;
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            Animation = new FoxHash(FoxHash.Type.StrCode32);
            Animation.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var Animation_printString = Animation.HashValue.ToString();
            if (Animation.IsStringKnown)
                Animation_printString  = Animation.StringLiteral;
            Console.WriteLine($"@{reader.BaseStream.Position} Animation: {Animation_printString }");

            Param1 = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param1: {Param1}");

            Param2 = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param2: {Param2}");

            Param3 = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Event param3: {Param3}");
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(Animation.HashValue);
            writer.Write(Param1);
            writer.Write(Param2);
            writer.Write(Param3);
        }
    }
}
