﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RouteSetTool
{
    class Program
    {
        private const string RouteIdDictionary = "route_ids.txt";
        private const string RouteEventDictionary = "route_event_names.txt";
        private const string RouteEventMessageDictionary = "route_event_messages.txt";
        private const string RouteEventParamDictionary = "route_event_params.txt";
        private const string RouteIdUserDictionary = "route_ids_user.txt";
        private const string RouteEventParamUserDictionary = "route_event_messages.txt";

        // -tover 3 e20010_area02.frt
        private const string ArgToVer = "tover";
        // -whitelist afgh_sovietBase_enemy.txt f30010.frt
        private const string ArgWhiteList = "whitelist";
        // -combine sovietBase_and_south.frt afgh_sovietBase_enemy.frt afgh_sovietSouth_enemy.frt
        private const string ArgCombine = "combine"; 
        static void Main(string[] args)
        {
            var hashManager = new HashManager();

            // Multi-Dictionary Reading!!
            List<string> dictionaryNames = new List<string>
            {
                RouteIdDictionary,
                RouteEventDictionary,
                RouteEventMessageDictionary,
                RouteEventParamDictionary,
                RouteIdUserDictionary,
                RouteEventParamUserDictionary,
            };
            List<string> strCodeDictionaries = new List<string>();
            foreach (var dictionaryPath in dictionaryNames)
                if (File.Exists(GetPathNearApp(dictionaryPath)))
                    strCodeDictionaries.Add(GetPathNearApp(dictionaryPath));

            hashManager.StrCode32LookupTable = MakeStrCode32HashLookupTableFromFiles(strCodeDictionaries);

            List<string> paths = new List<string>();

            var toVerOn = false;
            var version = 0;
            var whiteListOn = false;
            var whiteListName = "";
            var combineOn = false;
            var combineName = "";

            foreach (string arg in args)
            {
                if (File.Exists(arg))
                {
                    if (whiteListOn)
                    {
                        if (File.Exists(GetPathNearApp(arg)))
                        {
                            Console.WriteLine($"Whitelist {arg} detected");
                            whiteListName = GetPathNearApp(arg);
                            whiteListOn = false;
                        }
                    }
                    else
                    {
                        //paths
                        paths.Add(arg);
                    }
                }
                else
                {
                    //arguments
                    if (toVerOn)
                    {
                        int.TryParse(arg, out version);
                        toVerOn = false;
                        break;
                    }

                    if (combineOn)
                    {
                        combineName = arg;
                        if (Path.GetExtension(combineName)==null)
                        {
                            combineName += ".frt";
                        }
                        break;
                    }

                    switch (arg.ToLower())
                    {
                        default:
                            break;
                        case "-"+ArgToVer:
                            toVerOn = true;
                            break;
                        case "-" + ArgWhiteList:
                            Console.WriteLine("Whitelist arg detected");
                            whiteListOn = true;
                            break;
                        case "-" + ArgCombine:
                            combineOn = true;
                            break;
                    }
                }
            }

            List<Route> totalRouteList = new List<Route>();

            foreach (string path in paths)
            {
                if (Path.GetExtension(path)==".frt")
                {
                    RouteSet frt = ReadFrt(path, hashManager.StrCode32LookupTable, hashManager.OnHashIdentified);
                    if (whiteListName != "")
                    {
                        Console.WriteLine("Whitelisting...");
                        frt.WhiteList(GetWhiteList(whiteListName));
                    }

                    if (combineName != "")
                    {
                        foreach (Route route in frt.Routes)
                            totalRouteList.Add(route);
                    }

                    if (combineName=="")
                        if (frt.Routes.Count > 0)
                            WriteXml(frt, Path.GetFileNameWithoutExtension(path) + ".frt.xml");
                }
                else if (Path.GetExtension(path)==".xml")
                {
                    RouteSet frt = ReadXml(path);
                    //wip
                    if (version > 0)
                    {
                        switch (version)
                        {
                            case (int)RouteSetVersion.GZ:
                                frt.FileVersion = RouteSetVersion.GZ;
                                break;
                            case (int)RouteSetVersion.TPP:
                                frt.FileVersion = RouteSetVersion.TPP;
                                break;
                        }
                    }

                    if (whiteListName != "")
                    {
                        frt.WhiteList(GetWhiteList(whiteListName));
                    }

                    if (combineName != "")
                    {
                        foreach (Route route in frt.Routes)
                            totalRouteList.Add(route);
                    }

                    if (combineName == "")
                        if (frt.Routes.Count>0)
                            WriteFrt(Path.GetFileNameWithoutExtension(path), frt);
                }

                /*
                 * var newPath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + "_out.frt";
                if (frt.FileVersion == RouteSetVersion.TPP)
                    frt.FileVersion = RouteSetVersion.GZ;
                else
                {
                    frt.FileVersion = RouteSetVersion.TPP;
                    frt.EventTypesGzToTpp();
                }
                WriteFrt(newPath, frt);
                */
            }
            if (combineName != "")
            {
                RouteSet frt = new RouteSet() { Routes = totalRouteList };
                WriteXml(frt, combineName);
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
        public static void WriteXml(RouteSet frt, string path)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };
            using (var writer = XmlWriter.Create(path, xmlWriterSettings))
            {
                frt.WriteXml(writer);
            }
        }

        public static RouteSet ReadXml(string path)
        {
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = true
            };

            RouteSet frt = new RouteSet();
            using (var reader = XmlReader.Create(path, xmlReaderSettings))
            {
                frt.ReadXml(reader);
            }
            return frt;
        }
        private static Dictionary<uint, string> MakeStrCode32HashLookupTableFromFiles(List<string> paths)
        {
            ConcurrentDictionary<uint, string> table = new ConcurrentDictionary<uint, string>();

            // Read file
            List<string> stringLiterals = new List<string>();
            foreach (var dictionary in paths)
            {
                using (StreamReader file = new StreamReader(dictionary))
                {
                    // TODO multi-thread
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        stringLiterals.Add(line);
                    }
                }
            }

            // Hash entries
            Parallel.ForEach(stringLiterals, (string entry) =>
            {
                uint hash = HashManager.StrCode32(entry);
                table.TryAdd(hash, entry);
            });

            return new Dictionary<uint, string>(table);
        }
        public static string GetPathNearApp(string name)
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + name;
        }
        public static List<uint> GetWhiteList(string path)
        {
            List<uint> whiteList = new List<uint>();
            using (StreamReader file = new StreamReader(path))
            {
                // TODO multi-thread
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    FoxHash hash = new FoxHash(FoxHash.Type.StrCode32);
                    if (uint.TryParse(line, out uint maybeHash))
                    {
                        hash.HashValue = maybeHash;
                        Console.WriteLine($"Inited {hash.HashValue} to list");
                    }
                    else
                    {
                        hash.StringLiteral = line;

                        hash.HashValue = HashManager.StrCode32(hash.StringLiteral);
                        Console.WriteLine($"Inited {hash.StringLiteral} to list");
                    }
                    whiteList.Add(hash.HashValue);
                }
            }
            return whiteList;
        }
    }
}
