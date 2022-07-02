﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteSetTool
{
    public enum RouteSetVersion : short
    {
        GZ = 2,
        TPP = 3,
    }
    public class RouteSet
    {
        
        public RouteSetVersion FileVersion;
        public List<Route> Routes = new List<Route>();
        public void Read(BinaryReader reader, Dictionary<uint, string> nameLookupTable, HashIdentifiedDelegate hashIdentifiedCallback)
        {
            string ROUT = new string (reader.ReadChars(4)); if (ROUT != "ROUT") { throw new Exception($"ROUT isn't in header!!! {ROUT}"); }
            FileVersion = (RouteSetVersion)reader.ReadInt16(); if (FileVersion != RouteSetVersion.TPP & FileVersion != RouteSetVersion.GZ) { throw new Exception("version isn't supported!!!"); }
            ushort routeCount = reader.ReadUInt16();
            Console.WriteLine($"{ROUT} {FileVersion}: {routeCount} route(s)");

            Vector3 origin = new Vector3();
            if (FileVersion == RouteSetVersion.GZ)
            {
                reader.ReadBytes(4*2); //Irrelevant memory leak bytes
                origin.Read(reader);
                reader.ReadBytes(4); //Irrelevant memory leak bytes
                Console.WriteLine($"Origin: | {origin.x} || {origin.y} || {origin.z} | ");
            }

            uint offsetToRouteNames = reader.ReadUInt32();
            uint offsetToRouteDefinitions = reader.ReadUInt32();
            uint offsetToRouteTranslations = reader.ReadUInt32();
            uint offsetToRouteEventCounts = reader.ReadUInt32();
            uint offsetToRouteEvents = reader.ReadUInt32();

            if (FileVersion == RouteSetVersion.GZ)
            {
                reader.ReadBytes(4 * 2); //Irrelevant memory leak bytes
            }
            Console.WriteLine($"Names @{offsetToRouteNames}, defintions @{offsetToRouteDefinitions}, translations @{offsetToRouteTranslations}, event counts @{offsetToRouteEventCounts}, events @{offsetToRouteEvents}");

            for (int routeIndex = 0; routeIndex < routeCount; routeIndex++)
            {
                Route route = new Route();

                reader.BaseStream.Position = offsetToRouteNames + routeIndex * 4;
                route.Name = new FoxHash(FoxHash.Type.StrCode32);
                route.Name.Read(reader, nameLookupTable, hashIdentifiedCallback);
                var logName = route.Name.HashValue.ToString();
                if (route.Name.IsStringKnown)
                    logName = route.Name.StringLiteral;
                Console.WriteLine($"@{reader.BaseStream.Position} Route#{routeIndex} Name: {logName}");

                uint routeDefinitonsPosition = (uint)(offsetToRouteDefinitions + (routeIndex * 16));
                Console.WriteLine($"@{reader.BaseStream.Position} routeDefinitonsPosition: {routeDefinitonsPosition} ");
                reader.BaseStream.Position = routeDefinitonsPosition;
                uint nodeOffset = reader.ReadUInt32()+ routeDefinitonsPosition;
                uint eventTableOffset = reader.ReadUInt32()+ routeDefinitonsPosition;
                uint eventsOffset = reader.ReadUInt32()+ routeDefinitonsPosition;
                ushort nodeCount = reader.ReadUInt16();
                ushort eventCount = reader.ReadUInt16();
                Console.WriteLine($"@{reader.BaseStream.Position} Node count: {nodeCount} Event count: {eventCount}");

                var globalEventIndex = 0;

                for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                {
                    RouteNode node = new RouteNode();

                    reader.BaseStream.Position = eventTableOffset + nodeIndex*4;
                    ushort eventNodeCount = reader.ReadUInt16();
                    ushort eventStartIndex = reader.ReadUInt16();
                    Console.WriteLine($"@{reader.BaseStream.Position} Node #{nodeIndex} event count: {eventNodeCount}");

                    int lengthOfTranslation;
                    if (FileVersion == RouteSetVersion.GZ)
                        lengthOfTranslation = 8;
                    else
                        lengthOfTranslation = 12;

                    reader.BaseStream.Position = nodeOffset+nodeIndex * lengthOfTranslation;
                    Vector3 translation = new Vector3();
                    if (FileVersion == RouteSetVersion.GZ)
                    {
                        translation.ReadUnpack(reader.ReadInt64());
                        translation.x = origin.x+ translation.x; 
                        translation.y = origin.y+ translation.y; 
                        translation.z = origin.z+ translation.z;
                    }
                    else
                    {
                        translation.Read(reader);
                    }

                    node.Translation=translation;
                    Console.WriteLine($"@{reader.BaseStream.Position} translation: | {node.Translation.x} || {node.Translation.y} || {node.Translation.z} |");

                    var eventLength = 48;
                    reader.BaseStream.Position = eventsOffset + (globalEventIndex * eventLength);
                    Console.WriteLine($"@{reader.BaseStream.Position} Edge event");
                    RouteEvent edgeEvent = new RouteEvent();
                    edgeEvent.Read(reader, nameLookupTable, hashIdentifiedCallback, FileVersion);
                    if (edgeEvent.IsNodeEvent)
                    {
                        throw new Exception($"@{reader.BaseStream.Position} Edge event isn't edge event!!!");
                    }
                    node.EdgeEvent = edgeEvent;
                    globalEventIndex++;

                    int nodeEventCount = eventNodeCount - 1;
                    for (int eventIndex = 0; eventIndex < nodeEventCount; eventIndex++)
                    {
                        Console.WriteLine($"@{reader.BaseStream.Position} Node event #{eventIndex}");
                        RouteEvent nodeEvent = new RouteEvent();
                        nodeEvent.Read(reader, nameLookupTable, hashIdentifiedCallback, FileVersion);
                        if (!nodeEvent.IsNodeEvent)
                        {
                            throw new Exception($"@{reader.BaseStream.Position} Node event isn't node event!!!");
                        }
                        node.NodeEvents.Add(nodeEvent);
                        globalEventIndex++;
                    }
                    route.Nodes.Add(node);
                }
                Routes.Add(route);
            }
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write("ROUT".ToArray());
            writer.Write((short)FileVersion);
            writer.Write((ushort)Routes.Count);


            Vector3 origin = new Vector3();
            if (FileVersion == RouteSetVersion.GZ)
            {
                writer.WriteZeroes(4 * 2); //Irrelevant memory leak bytes
                origin = GetOrigin();
                origin.Write(writer);
                writer.WriteZeroes(4); //Irrelevant memory leak bytes
            }

            //Write positons of offsets to sections to write in later
            uint toWrite_offsetToRouteNames = (uint)writer.BaseStream.Position; writer.WriteZeroes(4);
            uint toWrite_offsetToRouteDefinitions = (uint)writer.BaseStream.Position; writer.WriteZeroes(4);
            uint toWrite_offsetToRouteTranslations = (uint)writer.BaseStream.Position; writer.WriteZeroes(4);
            uint toWrite_offsetToRouteEventCounts = (uint)writer.BaseStream.Position; writer.WriteZeroes(4);
            uint toWrite_offsetToRouteEvents = (uint)writer.BaseStream.Position; writer.WriteZeroes(4);

            uint toWrite_fileSize = (uint)writer.BaseStream.Position; //GZ only
            if (FileVersion == RouteSetVersion.GZ)
            {
                writer.WriteZeroes(4);
                writer.WriteZeroes(4 * 2); //Irrelevant memory leak bytes
            }

            // ROUTE NAMES --V
            uint offsetToRouteNames = (uint)writer.BaseStream.Position;
            Routes.OrderBy(route => route.Name.HashValue);
            foreach (Route route in Routes)
                route.Name.Write(writer);
            // ROUTE NAMES --^

            // ROUTE DEFINITIONS --V
            uint offsetToRouteDefinitions = (uint)writer.BaseStream.Position;
            uint[] toWrite_nodeOffset = new uint[Routes.Count]; uint[] offsetToRouteDefinitions_nodeOffset = new uint[Routes.Count];
            uint[] toWrite_eventTableOffset = new uint[Routes.Count];
            uint[] toWrite_eventsOffset = new uint[Routes.Count];

            uint[] nodeOffset = new uint[Routes.Count];
            uint[] eventTableOffset = new uint[Routes.Count];
            uint[] eventsOffset = new uint[Routes.Count];

            for (int routeIndex = 0; routeIndex < Routes.Count; routeIndex++)
            {
                offsetToRouteDefinitions_nodeOffset[routeIndex] = (uint)writer.BaseStream.Position;
                toWrite_nodeOffset[routeIndex] = (uint)writer.BaseStream.Position - offsetToRouteDefinitions_nodeOffset[routeIndex]; writer.WriteZeroes(4);
                toWrite_eventTableOffset[routeIndex] = (uint)writer.BaseStream.Position - offsetToRouteDefinitions_nodeOffset[routeIndex]; writer.WriteZeroes(4);
                toWrite_eventsOffset[routeIndex] = (uint)writer.BaseStream.Position - offsetToRouteDefinitions_nodeOffset[routeIndex]; writer.WriteZeroes(4);
                writer.Write((ushort)Routes[routeIndex].Nodes.Count);
                uint eventCount = (uint)(1 *Routes[routeIndex].Nodes.Count); //edge events
                for (int nodeIndex = 0; nodeIndex < Routes[routeIndex].Nodes.Count; nodeIndex++)
                    eventCount += (uint)Routes[routeIndex].Nodes[nodeIndex].NodeEvents.Count;
                writer.Write((ushort)eventCount);
            }
            // ROUTE DEFINITIONS --^

            // ROUTE TRANSLATIONS --V
            uint offsetToRouteTranslations = (uint)writer.BaseStream.Position;
            for (int routeIndex = 0; routeIndex < Routes.Count; routeIndex++)
            {
                nodeOffset[routeIndex] = (uint)writer.BaseStream.Position - (toWrite_nodeOffset[routeIndex] + offsetToRouteDefinitions_nodeOffset[routeIndex]);
                foreach (RouteNode node in Routes[routeIndex].Nodes)
                    if (FileVersion == RouteSetVersion.GZ)
                    {
                        Vector3 packedPoint = new Vector3();
                        packedPoint = node.Translation;
                        packedPoint.x -= origin.x;
                        packedPoint.y -= origin.y;
                        packedPoint.z -= origin.z;
                        packedPoint.WritePack(writer);
                    }
                    else
                        node.Translation.Write(writer);
            }
            // ROUTE TRANSLATIONS --^

            // ROUTE EVENT COUNTS --V
            uint offsetToRouteEventCounts = (uint)writer.BaseStream.Position;
            for (int routeIndex = 0; routeIndex < Routes.Count; routeIndex++)
            {
                uint totalEventIndex = 0;
                eventTableOffset[routeIndex] = (uint)writer.BaseStream.Position - (toWrite_nodeOffset[routeIndex] + offsetToRouteDefinitions_nodeOffset[routeIndex]);
                for (int nodeIndex = 0; nodeIndex < Routes[routeIndex].Nodes.Count; nodeIndex++)
                {
                    uint eventCount = (uint)(1 + Routes[routeIndex].Nodes[nodeIndex].NodeEvents.Count);
                    writer.Write((ushort)eventCount);
                    writer.Write((ushort)totalEventIndex);
                    totalEventIndex += eventCount;
                }
            }
            // ROUTE EVENT COUNTS --^

            // ROUTE EVENTS --V
            uint offsetToRouteEvents = (uint)writer.BaseStream.Position;
            for (int routeIndex = 0; routeIndex < Routes.Count; routeIndex++)
            {
                eventsOffset[routeIndex] = (uint)writer.BaseStream.Position - (toWrite_nodeOffset[routeIndex] + offsetToRouteDefinitions_nodeOffset[routeIndex]);
                foreach (RouteNode node in Routes[routeIndex].Nodes)
                {
                    node.EdgeEvent.Write(writer, FileVersion);
                    foreach (RouteEvent nodeEvent in node.NodeEvents)
                    {
                        nodeEvent.Write(writer, FileVersion);
                    }
                }
            }
            // ROUTE EVENTS --^

            //END

            //Writing toWrite offsets

            if (FileVersion==RouteSetVersion.GZ)
            {
                uint fileSize = (uint)writer.BaseStream.Position;
                writer.BaseStream.Position = toWrite_fileSize;
                writer.Write(fileSize);
            }

            //  nodeOffsets - v
            for (int routeIndex = 0; routeIndex < Routes.Count; routeIndex++)
            {
                writer.BaseStream.Position = toWrite_nodeOffset[routeIndex] + offsetToRouteDefinitions_nodeOffset[routeIndex];
                writer.Write(nodeOffset[routeIndex]);
            }
            //  nodeOffsets - ^

            //  eventTableOffsets - v
            for (int routeIndex = 0; routeIndex < Routes.Count; routeIndex++)
            {
                writer.BaseStream.Position = toWrite_eventTableOffset[routeIndex] + offsetToRouteDefinitions_nodeOffset[routeIndex];
                writer.Write(eventTableOffset[routeIndex]);
            }
            //  eventTableOffsets - ^

            //  eventsOffsets - v
            for (int routeIndex = 0; routeIndex < Routes.Count; routeIndex++)
            {
                writer.BaseStream.Position = toWrite_eventsOffset[routeIndex] + offsetToRouteDefinitions_nodeOffset[routeIndex];
                writer.Write(eventsOffset[routeIndex]);
            }
            //  eventsOffsets - ^

            //Write offsets to sections
            writer.BaseStream.Position = toWrite_offsetToRouteNames;
            writer.Write(offsetToRouteNames);
            writer.BaseStream.Position = toWrite_offsetToRouteDefinitions;
            writer.Write(offsetToRouteDefinitions);
            writer.BaseStream.Position = toWrite_offsetToRouteTranslations;
            writer.Write(offsetToRouteTranslations);
            writer.BaseStream.Position = toWrite_offsetToRouteEventCounts;
            writer.Write(offsetToRouteEventCounts);
            writer.BaseStream.Position = toWrite_offsetToRouteEvents;
            writer.Write(offsetToRouteEvents);
        }
        public Vector3 GetOrigin()
        {
            Vector3 ave = new Vector3();
            List<float> xList = new List<float>();
            List<float> yList = new List<float>();
            List<float> zList = new List<float>();
            foreach (Route route in Routes)
                foreach (RouteNode node in route.Nodes)
                {
                    xList.Add(node.Translation.x);
                    yList.Add(node.Translation.y);
                    zList.Add(node.Translation.z);
                }
            ave.x = xList.Min(); ave.y = yList.Min(); ave.z = zList.Min();
            return ave;
        }
        public void EventTypesGzToTpp()
        {
            foreach (Route route in Routes)
            {
                foreach(RouteNode node in route.Nodes)
                {
                    RouteEventType moveEventType = new RouteEventType();

                    FoxHash newEdgeEvent = new FoxHash(FoxHash.Type.StrCode32);

                    foreach(var entry in EventTypeGZtoTPP)
                    {
                        if (entry.Key.Item1== node.EdgeEvent.EventType.Name.HashValue)
                        {
                            newEdgeEvent.HashValue = entry.Value.Item1;
                            newEdgeEvent.StringLiteral = entry.Value.Item2;
                            Console.WriteLine($"Edge event {node.EdgeEvent.EventType.Name.HashValue} to {entry.Value.Item1}");

                            moveEventType.Name = newEdgeEvent;

                            node.EdgeEvent.EventType = moveEventType;
                        }
                    }
                    foreach(RouteEvent nodeEvent in node.NodeEvents)
                    {
                        RouteEventType waitEventType = new RouteEventType();

                        FoxHash newNodeEvent = new FoxHash(FoxHash.Type.StrCode32);

                        foreach (var entry in EventTypeGZtoTPP)
                        {
                            if (entry.Key.Item1 == nodeEvent.EventType.Name.HashValue)
                            {
                                newNodeEvent.HashValue = entry.Value.Item1;
                                newNodeEvent.StringLiteral = entry.Value.Item2;
                                Console.WriteLine($"Node event {nodeEvent.EventType.Name.HashValue} to {entry.Value.Item1}");

                                waitEventType.Name = newNodeEvent;

                                nodeEvent.EventType = waitEventType;
                            }
                        }
                    }
                }
            }
        }
        public Dictionary<Tuple<uint, string>, Tuple<uint, string>> EventTypeGZtoTPP = new Dictionary<Tuple<uint, string>, Tuple<uint, string>>()
        {
            //EDGE:
            //{ new Tuple<uint,string>(null, null), new Tuple<uint,string>(4202868537, "Move") },//DEFAULT Unknown Edge Event
            { new Tuple<uint,string>(32956284, null), new Tuple<uint,string>(2801188700, "RelaxedRun") },
            { new Tuple<uint,string>(710958087, null), new Tuple<uint,string>(1500257626, "RelaxedWalk") },
            { new Tuple<uint,string>(895949676, "Back"), new Tuple<uint,string>(3157073279 , "VehicleBackNormal") },
            { new Tuple<uint,string>(1264107696, "Forward"), new Tuple<uint,string>(4258228081, "VehicleMoveNormal") },
            { new Tuple<uint,string>(1969779497, "StandDash"), new Tuple<uint,string>(4103551892, "CautionDash") },
            { new Tuple<uint,string>(2089664649, null), new Tuple<uint,string>(1530489467, "CautionWalk") },//edge, caution soldier route in e20020
            { new Tuple<uint,string>(2913611970, "RouteMoveRun"), new Tuple<uint,string>(195789850, "MoveFast") },//rat run route
            { new Tuple<uint,string>(3132068007, "CautionSearchStandWalkReady"), new Tuple<uint,string>(3962304554, "CautionWalkSearch") },
            { new Tuple<uint,string>(3459700982, null), new Tuple<uint,string>(1318242912, "CautionWalkFire") },//edge 20040, aimtarget is character, fire?

            //NODE:
            //{ new Tuple<uint,string>(null, null), new Tuple<uint,string>(561913624, "Wait") },//DEFAULT Unknown Node Event
            { new Tuple<uint,string>(175901962, null), new Tuple<uint,string>(1446254949, "Fire") },//using AA gun, dunno if tpp has one
            { new Tuple<uint,string>(515872494, null), new Tuple<uint,string>(1974185602, "VehicleIdle") },//some vehicle node event, not idle!
            { new Tuple<uint,string>(1014362962, null), new Tuple<uint,string>(3777183860, "UseFlashLight") },//use flashlight
            { new Tuple<uint,string>(1053377933, null), new Tuple<uint,string>(3031653810, "CautionStandIdleAim") },//caution search node, aimpoint
            { new Tuple<uint,string>(1179698897, null), new Tuple<uint,string>(3443665058, "CautionStandFire") },//20040/20070 caution loop node with char aim
            { new Tuple<uint,string>(1377873783, null), new Tuple<uint,string>(2091872133, "CautionStandIdleReady") },//20070 only use 585050408 HeliEnemyRoute02, non-loop end of long caution walk aim/ready, 
            { new Tuple<uint,string>(1521517928, null), new Tuple<uint,string>(4019510599, "RelaxedIdleAct") },//relaxed node
            { new Tuple<uint,string>(1597064337, null), new Tuple<uint,string>(100777367, "VehicleGetIn") },//enter vehicle
            { new Tuple<uint,string>(1630004328, null), new Tuple<uint,string>(518500859, "Vanish") },//20020 target/20040 once, escape from mission area, does vanish even work in tpp?
            { new Tuple<uint,string>(1847252103, null), new Tuple<uint,string>(895026164, "UseSearchLight") },//use searchlight
            { new Tuple<uint,string>(2156064302, "SquatIdle"), new Tuple<uint,string>(2951130021, "CautionSquatIdle") },//rip relaxed squat idle
            { new Tuple<uint,string>(2686021750, "Stop"), new Tuple<uint,string>(1974185602, "VehicleIdle") },//vehicle stop
            { new Tuple<uint,string>(2716485425, null), new Tuple<uint,string>(3443665058, "CautionStandFire") },//shoot stand, aimtarget is xyz, 20010 kill hostage and 20040 fire
            { new Tuple<uint,string>(2953726856, null), new Tuple<uint,string>(2951130021, "VehicleIdle") },//vehicle end node - go off route and go combat
            { new Tuple<uint,string>(3129891228, null), new Tuple<uint,string>(2450663153, "CautionStandIdleSearch") },//caution idle search
            { new Tuple<uint,string>(3240040728, null), new Tuple<uint,string>(199044653, "CautionSquatIdleAim") },//only use is 20040, end of route, FriendManRoute03, args is two str64: 89365857539635 81784608717711 no idea what kojima does there
            { new Tuple<uint,string>(3952237029, "Conversation"), new Tuple<uint,string>(1536918290, "ConversationIdle") },
        };
    }
}