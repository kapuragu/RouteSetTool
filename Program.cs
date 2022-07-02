using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteSetTool
{
    class Program
    {
        private const string RouteIdDictionary = "route_ids.txt";
        private const string RouteEventDictionary = "route_event_names.txt";
        private const string RouteEventParamDictionary = "route_event_messages.txt";
        static void Main(string[] args)
        {
            var hashManager = new HashManager();
            if (File.Exists(RouteIdDictionary))
                hashManager.StrCode32LookupTable = MakeHashLookupTableFromFile(RouteIdDictionary, FoxHash.Type.StrCode32);

            List<string> paths = new List<string>();

            foreach (string arg in args)
            {
                if (File.Exists(arg))
                {
                    paths.Add(arg);
                }
            }

            foreach (string path in paths)
            {
                RouteSet frt = ReadFrt(path, hashManager.StrCode32LookupTable, hashManager.OnHashIdentified);
                var newPath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + "_out.frt";
                if (frt.FileVersion == RouteSetVersion.TPP)
                    frt.FileVersion = RouteSetVersion.GZ;
                else
                {
                    frt.FileVersion = RouteSetVersion.TPP;
                    frt.EventTypesGzToTpp();
                }
                WriteFrt(newPath, frt);
            }

            Console.Read();//DEBUG Hold Console
        }
        public static RouteSet ReadFrt(string path, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                RouteSet frt = new RouteSet();
                frt.Read(reader, nameLookupTable, hashIdentifiedCallback);
                return frt;
            }
        }
        public static void WriteFrt(string path, RouteSet frt)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                frt.Write(writer);
            }
        }
        private static Dictionary<uint, string> MakeHashLookupTableFromFile(string path, FoxHash.Type hashType)
        {
            ConcurrentDictionary<uint, string> table = new ConcurrentDictionary<uint, string>();

            // Read file
            List<string> stringLiterals = new List<string>();
            using (StreamReader file = new StreamReader(path))
            {
                // TODO multi-thread
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    stringLiterals.Add(line);
                }
            }

            // Hash entries
            Parallel.ForEach(stringLiterals, (string entry) =>
            {
                if (hashType == FoxHash.Type.StrCode32)
                {
                    uint hash = HashManager.StrCode32(entry);
                    table.TryAdd(hash, entry);
                }
            });

            return new Dictionary<uint, string>(table);
        }
    }
}
