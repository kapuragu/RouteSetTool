using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace RouteSetTool
{
    internal static class Program
    {
        private const string RouteIdDictionary = "route_ids.txt";
        private const string RouteEventDictionary = "route_event_names.txt";
        private const string RouteEventMessageDictionary = "route_event_messages.txt";
        private const string RouteEventParamDictionary = "route_event_params.txt";
        private const string RouteIdUserDictionary = "route_ids_user.txt";
        private const string RouteEventParamUserDictionary = "route_event_params_user.txt";
        private const string RouteEventParamConfig = "route_event_param_config.json";

        // -version 3 e20010_area02.frt.xml
        private const string ArgVersion = "version";
        // -convertevent e20010_area02.frt.xml
        private const string ArgEventConvert = "convertevent";
        // -whitelist list.txt f30010.frt
        private const string ArgWhiteList = "whitelist";
        // -combine result.frt afgh_sovietBase_enemy.frt afgh_sovietSouth_enemy.frt
        private const string ArgCombine = "combine";

        private static void Main(string[] args)
        {
            var hashManager = new HashManager();

            // Multi-Dictionary Reading!!
            var dictionaryNames = new List<string>
            {
                RouteIdDictionary,
                RouteEventDictionary,
                RouteEventMessageDictionary,
                RouteEventParamDictionary,
                RouteIdUserDictionary,
                RouteEventParamUserDictionary,
            };
            var strCodeDictionaries = (from dictionaryPath in dictionaryNames 
                where File.Exists(GetPathNearApp(dictionaryPath)) select GetPathNearApp(dictionaryPath)).ToList();

            hashManager.StrCode32LookupTable = MakeStrCode32HashLookupTableFromFiles(strCodeDictionaries);

            var eventParamConfig = GetEventParamConfig();

            var idUserDictionary = new List<string>();
            var eventUserDictionary = new List<string>();
            
            var paths = new List<string>();

            var versionOn = false;
            var version = 0;
            var whiteListOn = false;
            var whiteListName = "";
            var combineOn = false;
            var combineName = "";
            var convertEventOn = false;

            foreach (var arg in args)
            {

                //arguments
                if (versionOn)
                {
                    int.TryParse(arg, out version);
                    versionOn = false;
                    //Console.WriteLine($"Version {arg} detected, use ver {version}");
                }
                else if (combineOn)
                {
                    combineName = arg;
                    //Console.WriteLine($"Combining into {arg}");
                    if (Path.GetExtension(combineName) == null)
                    {
                        combineName += ".frt";
                    }
                    combineOn = false;
                }
                else if (whiteListOn)
                {
                    if (!File.Exists(GetPathNearApp(arg))) continue;
                    
                    //Console.WriteLine($"Whitelist {arg} detected");
                    whiteListName = GetPathNearApp(arg);
                    whiteListOn = false;
                }
                else
                {
                    switch (arg.ToLower())
                    {
                        case "-" + ArgVersion:
                            //Console.WriteLine("Version arg detected");
                            versionOn = true;
                            break;
                        case "-" + ArgWhiteList:
                            //Console.WriteLine("Whitelist arg detected");
                            whiteListOn = true;
                            break;
                        case "-" + ArgCombine:
                            //Console.WriteLine("Combine arg detected");
                            combineOn = true;
                            break;
                        case "-" + ArgEventConvert:
                            //Console.WriteLine("Event convert arg detected");
                            convertEventOn = true;
                            break;
                    }

                    if (!File.Exists(arg)) continue;
                    //paths
                    paths.Add(arg);
                    //Console.WriteLine($"Adding {arg} to read list...");
                }
            }

            var totalRouteNames = new List<uint>();
            var totalRouteList = new List<Route>();

            foreach (var path in paths)
            {
                switch (Path.GetExtension(path))
                {
                    case ".frt":
                    {
                        var frt = ReadFrt(path, hashManager, eventParamConfig);

                        if (convertEventOn)
                            frt.EventTypesGzToTpp();

                        if (whiteListName != "")
                        {
                            //Console.WriteLine("Whitelisting...");
                            frt.WhiteList(GetWhiteList(whiteListName));
                        }

                        if (combineName != "")
                            foreach (var route in frt.Routes.Where(route => !totalRouteNames.Contains(route.Name.HashValue)))
                            {
                                totalRouteNames.Add(route.Name.HashValue);
                                totalRouteList.Add(route);
                            }

                        if (combineName=="")
                            WriteXml(frt, Path.GetFileNameWithoutExtension(path) + ".frt.xml");
                        break;
                    }
                    case ".xml":
                    {
                        var frt = ReadXml(path);

                        CollectUserStrings(frt,hashManager,idUserDictionary,eventUserDictionary);

                        if (convertEventOn)
                            frt.EventTypesGzToTpp();

                        if (version > 0)
                            switch (version)
                            {
                                case (int)RouteSetVersion.GZ:
                                    frt.FileVersion = RouteSetVersion.GZ;
                                    break;
                                case (int)RouteSetVersion.TPP:
                                    frt.FileVersion = RouteSetVersion.TPP;
                                    break;
                            }
                        else
                        {
                            frt.FileVersion = RouteSetVersion.TPP;
                        }

                        if (whiteListName != "")
                            frt.WhiteList(GetWhiteList(whiteListName));

                        if (combineName != "")
                        {
                            foreach (var route in frt.Routes.Where(route => !totalRouteNames.Contains(route.Name.HashValue)))
                            {
                                totalRouteNames.Add(route.Name.HashValue);
                                totalRouteList.Add(route);
                            }
                        }

                        if (combineName == "")
                            WriteFrt(Path.GetFileNameWithoutExtension(path), frt);
                        break;
                    }
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
                var combinedFrt = new RouteSet() { Routes = totalRouteList };
                if (Path.GetExtension(combineName) == ".xml")
                {
                    WriteXml(combinedFrt, combineName);
                }
                else
                {
                    WriteFrt(combineName, combinedFrt);
                }
            }

            WriteUserStringsToFile(RouteIdUserDictionary,idUserDictionary);
            WriteUserStringsToFile(RouteEventParamUserDictionary,eventUserDictionary);
            
            /*foreach(var str in idUserDictionary)
                Console.WriteLine($"{str}");
            foreach(var str in eventUserDictionary)
                Console.WriteLine($"{str}");*/

            //Console.Read();//DEBUG Hold Console
        }

        private static RouteSet ReadFrt(string path, HashManager hashManager, Dictionary<string,string[]> eventParamConfig)
        {
            using (var reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                var frt = new RouteSet();
                frt.Read(reader, hashManager, eventParamConfig);
                return frt;
            }
        }

        private static void WriteFrt(string path, RouteSet frt)
        {
            using (var writer = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                frt.Write(writer);
            }
        }

        private static void WriteXml(RouteSet frt, string path)
        {
            var xmlWriterSettings = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true,
            };
            using (var writer = XmlWriter.Create(path, xmlWriterSettings))
            {
                frt.WriteXml(writer);
            }
        }

        private static RouteSet ReadXml(string path)
        {
            var xmlReaderSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
            };

            var frt = new RouteSet();
            using (var reader = XmlReader.Create(path, xmlReaderSettings))
            {
                frt.ReadXml(reader);
            }
            return frt;
        }
        private static Dictionary<uint, string> MakeStrCode32HashLookupTableFromFiles(List<string> paths)
        {
            var table = new ConcurrentDictionary<uint, string>();

            // Read file
            var stringLiterals = new List<string>();
            foreach (var dictionary in paths)
            {
                using (var file = new StreamReader(dictionary))
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
            Parallel.ForEach(stringLiterals, entry =>
            {
                var hash = HashManager.StrCode32(entry);
                table.TryAdd(hash, entry);
            });

            return new Dictionary<uint, string>(table);
        }

        private static string GetPathNearApp(string name)
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + name;
        }

        private static List<uint> GetWhiteList(string path)
        {
            var whiteList = new List<uint>();
            using (var file = new StreamReader(path))
            {
                // TODO multi-thread
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    var hash = new FoxHash(FoxHash.Type.StrCode32);
                    if (uint.TryParse(line, out var maybeHash))
                    {
                        hash.HashValue = maybeHash;
                        //Console.WriteLine($"Inited {hash.HashValue} to list");
                    }
                    else
                    {
                        hash.StringLiteral = line;

                        hash.HashValue = HashManager.StrCode32(hash.StringLiteral);
                        //Console.WriteLine($"Inited {hash.StringLiteral} to list");
                    }
                    whiteList.Add(hash.HashValue);
                }
            }
            return whiteList;
        }

        private static Dictionary<string, string[]> GetEventParamConfig()
        {
            var configPath = GetPathNearApp(RouteEventParamConfig);
            
            if (!File.Exists(configPath)) return null;
            
            var configJson = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<Dictionary<string, string[]>>(configJson);
        }

        private static void CollectUserStrings(RouteSet frt, HashManager hashManager, List<string> idUserDictionary, List<string> eventUserDictionary)
        {
            foreach (var route in frt.Routes)
            {
                if (IsUserString(route.Name, hashManager))
                {
                    idUserDictionary.Add(route.Name.StringLiteral);
                }

                foreach (var node in route.Nodes)
                {
                    var aimTargetHash = GetAimEventHashParam(node.EdgeEvent.AimTargetTypeParams);
                    if (aimTargetHash!=null)
                        eventUserDictionary.Add(aimTargetHash);
                    idUserDictionary.AddRange(from routeId in node.EdgeEvent.AimTargetTypeParams.GetRouteNames() 
                        where IsUserString(routeId, hashManager) select routeId.StringLiteral);
                    var param0 = GetEventHashParam(node.EdgeEvent.Param0, hashManager);
                    if (param0!=null)
                        eventUserDictionary.Add(param0);
                    var param1 = GetEventHashParam(node.EdgeEvent.Param1, hashManager);
                    if (param1!=null)
                        eventUserDictionary.Add(param1);
                    var param2 = GetEventHashParam(node.EdgeEvent.Param2, hashManager);
                    if (param2!=null)
                        eventUserDictionary.Add(param2);
                    var param3 = GetEventHashParam(node.EdgeEvent.Param3, hashManager);
                    if (param3!=null)
                        eventUserDictionary.Add(param3);

                    foreach (var nodeEvent in node.NodeEvents)
                    {
                        aimTargetHash = GetAimEventHashParam(nodeEvent.AimTargetTypeParams);
                        if (aimTargetHash!=null)
                            eventUserDictionary.Add(aimTargetHash);
                        idUserDictionary.AddRange(from routeId in nodeEvent.AimTargetTypeParams.GetRouteNames() 
                            where IsUserString(routeId, hashManager) select routeId.StringLiteral);
                        param0 = GetEventHashParam(nodeEvent.Param0, hashManager);
                        if (param0!=null)
                            eventUserDictionary.Add(param0);
                        param1 = GetEventHashParam(nodeEvent.Param1, hashManager);
                        if (param1!=null)
                            eventUserDictionary.Add(param1);
                        param2 = GetEventHashParam(nodeEvent.Param2, hashManager);
                        if (param2!=null)
                            eventUserDictionary.Add(param2);
                        param3 = GetEventHashParam(nodeEvent.Param3, hashManager);
                        if (param3!=null)
                            eventUserDictionary.Add(param3);
                    }
                }
            }
        }
        private static bool IsUserString(FoxHash hash, HashManager hashManager)
        {
            return !string.IsNullOrEmpty(hash.StringLiteral) && !hashManager.StrCode32LookupTable.ContainsValue(hash.StringLiteral);
        }

        private static string GetEventHashParam(IEventParam eventParam, HashManager hashManager)
        {
            if (!(eventParam is EventParamStrCode32 param)) return null;
            
            return IsUserString(param.Param, hashManager) ? param.Param.StringLiteral : null;
        }
        private static string GetAimEventHashParam(IAimTargetType aimTargetTypeParams)
        {
            return aimTargetTypeParams is AimCharacter aimCharacter ? aimCharacter.CharacterName.StringLiteral : null;
        }
        private static void WriteUserStringsToFile(string name, List<string> userStrings)
        {
            userStrings.Sort(); //Sort alphabetically for neatness
            var list = userStrings.Distinct().ToList();
            var userDictionary = GetPathNearApp(name);
            foreach (var userString in list)
                using (var file = new StreamWriter(path: userDictionary, append: true))
                    file.WriteLine(userString); //Write them into the user dictionary
        }
    }
}
