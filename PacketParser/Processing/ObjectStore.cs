using System;
using Guid = PacketParser.DataStructures.Guid;
using PacketParser.DataStructures;
using PacketParser.Misc;
using System.Collections.Generic;
using PacketParser.Enums;
using System.Diagnostics;

namespace PacketParser.Processing
{
    public class ObjectStore : IPacketProcessor
    {
        public bool LoadOnDepend { get { return false; } }
        public Type[] DependsOn { get { return null; } }

        public ProcessPacketEventHandler ProcessAnyPacketHandler { get { return null; } }
        public ProcessedPacketEventHandler ProcessedAnyPacketHandler { get { return null; } }
        public ProcessDataEventHandler ProcessAnyDataHandler { get { return ProcessData; } }
        public ProcessedDataNodeEventHandler ProcessedAnyDataNodeHandler { get { return null; } }

        public readonly Dictionary<Guid, WoWObject> Objects = new Dictionary<Guid, WoWObject>();

        public bool Init(PacketFileProcessor p) { return true; }
        public void Finish() { }

        public WoWObject GetObjectIfFound(Guid guid)
        {
            if (Objects.ContainsKey(guid))
                return Objects[guid];
            return null;
        }

        public WoWObject GetObjectOrCreate(Guid guid)
        {
            if (Objects.ContainsKey(guid))
                return Objects[guid];
            CreateObjectWithType(guid, guid.GetObjectType());
            return Objects[guid];
        }

        public WoWObject GetObjectWithTypeOrCreate(Guid guid, ObjectType type)
        {
            WoWObject w = GetObjectIfFound(guid);
            if (w != null)
            {
                if (w.Type != type)
                {
                    if (!w.Created)
                        Trace.WriteLine(String.Format("Object store: different requested object type ({0}) than current type of object created from GUID ({1}), probably wrong HighGuid type define", type, w.Type));
                    CreateObjectWithType(guid, type, w);
                    return Objects[guid];
                }
                return w;
            }
            CreateObjectWithType(guid, type);
            return Objects[guid];
        }

        public void CreateObjectWithType(Guid guid, ObjectType type, WoWObject obj = null)
        {
            WoWObject wobj;
            switch (type)
            {
                case ObjectType.Unit:
                    wobj = new Unit(obj);
                    break;
                case ObjectType.GameObject:
                    wobj = new GameObject(obj);
                    break;
                case ObjectType.Item:
                    wobj = new Item(obj);
                    break;
                case ObjectType.Player:
                    wobj = new Player(obj);
                    break;
                default:
                    wobj = new WoWObject(obj);
                    break;
            }
            wobj.Type = type;
            Objects[guid] = wobj;
        }

        public void ProcessData(string name, int? index, Object obj, Type t, Packet packet)
        {
            if (obj.GetType() == typeof(Guid))
            {
                var guid = (Guid)obj;
                if (guid.Full != 0 && !Objects.ContainsKey(guid))
                    CreateObjectWithType(guid, guid.GetObjectType());
            }
        }
    }
}
