using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace RouteSetTool
{
    public class Vector3 : IXmlSerializable
    {
        public float x;
        public float y;
        public float z;
        public void Read(BinaryReader reader)
        {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
        }
        public void ReadUnpack(long nodePacked)
        {
            //Vector3 Bit Unpacking by Joey:
            //https://discord.com/channels/364177293133873153/364178190588968970/894215344099573760
            //https://cdn.discordapp.com/attachments/364178190588968970/894215342069538907/unknown.png
            //Console.WriteLine($"Translation packed #{nodePacked}");

            uint packed_x = (uint)(nodePacked & 0x003FFFFF);
            if ((packed_x & (1 << 21))!=0)
                packed_x |= 0xFFC00000;
            x = (float)(int)packed_x / 1024;

            uint packed_y = (uint)((nodePacked >> 22) & 0x000FFFFF);
            if ((packed_y & (1 << 19)) != 0)
                packed_y |= 0xFFF00000;
            y = (float)(int)packed_y / 1024;

            uint packed_z = (uint)((nodePacked >> 42) & 0x003FFFFF);
            if ((packed_z & (1 << 21)) != 0)
                packed_z |= 0xFFC00000;
            z = (float)(int)packed_z / 1024;
        }
        public void WritePack(BinaryWriter writer)
        {
            ulong nodePacked = 0;
            float xDiv = x * 1024;
            int packed_x = (int)xDiv;
            if (x < 0)
                packed_x |= (1 << 21);
            nodePacked |= (uint)packed_x & 0x003FFFFF;

            float yDiv = y * 1024;
            int packed_y = (int)yDiv;
            if (y < 0)
                packed_y |= (1 << 19);
            nodePacked |= (ulong)((uint)packed_y & 0x000FFFFF) << 22;

            float zDiv = z * 1024;
            int packed_z = (int)zDiv;
            if (z < 0)
                packed_z |= (1 << 21);
            nodePacked |= (ulong)((uint)packed_z & 0x003FFFFF) << 42;

            writer.Write(nodePacked);
        }
        public virtual void ReadXml(XmlReader reader)
        {
            x = Extensions.ParseFloatRoundtrip(reader["x"]);
            y = Extensions.ParseFloatRoundtrip(reader["y"]);
            z = Extensions.ParseFloatRoundtrip(reader["z"]);
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("x", x.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("y", y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("z", z.ToString(CultureInfo.InvariantCulture));
        }
        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
