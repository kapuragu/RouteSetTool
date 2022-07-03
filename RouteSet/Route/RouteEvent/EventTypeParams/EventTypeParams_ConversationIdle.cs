using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteSetTool
{
    //ConversationIdle
    public class EventTypeParams_ConversationIdle : IEventTypeParams
    {
        //spch label name
        public FoxHash ConversationLabel;
        //locator name of friend
        public FoxHash FriendName;
        //strcode64 leftover uint

        //range in meters as activation zone
        public uint Range;
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            ConversationLabel = new FoxHash(FoxHash.Type.StrCode32);
            ConversationLabel.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var ConversationLabel_printString = ConversationLabel.HashValue.ToString();
            if (ConversationLabel.IsStringKnown)
                ConversationLabel_printString = ConversationLabel.StringLiteral;
            Console.WriteLine($"@{reader.BaseStream.Position} Conversation label: {ConversationLabel_printString }");

            FriendName = new FoxHash(FoxHash.Type.StrCode32);
            FriendName.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var Friend_printString = FriendName.HashValue.ToString();
            if (FriendName.IsStringKnown)
                Friend_printString = FriendName.StringLiteral;

            Console.WriteLine($"@{reader.BaseStream.Position} Friend name: {Friend_printString }");

            reader.BaseStream.Position += 4;//strcode64 leftover

            Range = reader.ReadUInt32();
            Console.WriteLine($"@{reader.BaseStream.Position} Range: {Range}");
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(ConversationLabel.HashValue);
            writer.Write(FriendName.HashValue);
            writer.WriteZeroes(4);//strcode64 leftover
            writer.Write(Range);
        }
    }
}
