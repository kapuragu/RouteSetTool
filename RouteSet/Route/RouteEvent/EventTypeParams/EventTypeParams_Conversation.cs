using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteSetTool
{
    //Conversation
    public class EventTypeParams_Conversation : IEventTypeParams
    {
        //spch label name
        public FoxHash ConversationSetLabel;
        //strcode64 leftover uint

        //locator name of friend
        public FoxHash FriendName;
        //strcode64 leftover uint
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            ConversationSetLabel = new FoxHash(FoxHash.Type.StrCode32);
            ConversationSetLabel.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var ConversationLabel_printString = ConversationSetLabel.HashValue.ToString();
            if (ConversationSetLabel.IsStringKnown)
                ConversationLabel_printString = ConversationSetLabel.StringLiteral;
            Console.WriteLine($"@{reader.BaseStream.Position} Conversation label: {ConversationLabel_printString }");

            reader.BaseStream.Position += 4;//strcode64 leftover

            FriendName = new FoxHash(FoxHash.Type.StrCode32);
            FriendName.Read(reader, nameLookupTable, hashIdentifiedCallback);
            var Friend_printString = FriendName.HashValue.ToString();
            if (FriendName.IsStringKnown)
                Friend_printString = FriendName.StringLiteral;

            Console.WriteLine($"@{reader.BaseStream.Position} Friend name: {Friend_printString }");

            reader.BaseStream.Position += 4;//strcode64 leftover
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(ConversationSetLabel.HashValue);
            writer.WriteZeroes(4);//strcode64 leftover
            writer.Write(FriendName.HashValue);
            writer.WriteZeroes(4);//strcode64 leftover
        }
    }
}
