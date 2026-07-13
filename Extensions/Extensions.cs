using System;
using System.Globalization;
using System.IO;

namespace RouteSetTool
{
    public static class Extensions
    {
        public static float ParseFloatRoundtrip(string text)
        {
            if (text == "-0")
            {
                return -0f;
            }

            return float.Parse(text, CultureInfo.InvariantCulture);
        }
        public static void ReadZeroes(this BinaryReader reader, int count)
        {
            byte[] zeroes = reader.ReadBytes(count);
            foreach (byte zero in zeroes)
            {
                if (zero != 0)
                {
#if DEBUG
                    Console.WriteLine($"Padding @{reader.BaseStream.Position} isn't zero!!!");
                    Console.WriteLine($"@{reader.BaseStream.Position}!=0");
#endif
                    //throw new Exception();
                }
            }
        } //WriteZeroes
        public static void WriteZeroes(this BinaryWriter writer, int count)
        {
            byte[] array = new byte[count];

            writer.Write(array);
        } //WriteZeroes
    }
}
