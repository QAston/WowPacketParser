using System;
using System.Collections;
using System.Collections.Generic;
using PacketParser.Enums;
using PacketParser.Enums.Version;
using PacketParser.Misc;
using Guid = PacketParser.DataStructures.Guid;
using PacketParser.Processing;
using PacketParser.DataStructures;
using System.Text;

namespace PacketParser.Parsing.Parsers
{
    public static class UpdateHandler
    {
        [Parser(Opcode.SMSG_UPDATE_OBJECT)]
        public static void HandleUpdateObject(Packet packet)
        {
            uint map = PacketFileProcessor.Current.GetProcessor<SessionStore>().CurrentMapId;
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_0_1_13164))
                map = packet.ReadUInt16("Map");

            var count = packet.ReadUInt32("Count");

            if (ClientVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.ReadBoolean("Has Transport");
            var ObjectStore = PacketFileProcessor.Current.GetProcessor<ObjectStore>();
            packet.StoreBeginList("Updates");
            for (var i = 0; i < count; i++)
            {
                var type = packet.ReadByte();
                string typeObj;
                if (ClientVersion.AddedInVersion(ClientType.Cataclysm))
                    typeObj = Enum<UpdateTypeCataclysm>.ToString(type);
                else
                    typeObj = Enum<UpdateType>.ToString(type);

                packet.Store("UpdateType", typeObj, i);
                switch (typeObj)
                {
                    case "Values":
                    {
                        var guid = packet.ReadPackedGuid("GUID", i);

                        ReadValuesUpdateBlock(ref packet, guid, map, i, false);

                        break;
                    }
                    case "Movement":
                    {
                        var guid = ClientVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901) ? packet.ReadPackedGuid("GUID", i) : packet.ReadGuid("GUID", i);
                        var obj = PacketFileProcessor.Current.GetProcessor<ObjectStore>().GetObjectOrCreate(guid);
                        obj.Map = map;
                        obj.Area = PacketFileProcessor.Current.GetProcessor<SessionStore>().CurrentAreaId;
                        obj.PhaseMask |= (uint)PacketFileProcessor.Current.GetProcessor<SessionStore>().CurrentPhaseMask; // additional phase mask if we see obj again
                        obj.Movement = ReadMovementUpdateBlock(ref packet, guid, i);
                        // Should we update Storage.Object?
                        break;
                    }
                    case "CreateObject1":
                    case "CreateObject2": // Might != CreateObject1 on Cata
                    {
                        var guid = packet.ReadPackedGuid("GUID", i);
                        ReadCreateObjectBlock(ref packet, guid, map, i);
                        break;
                    }
                    case "FarObjects":
                    {
                        ReadObjectsBlock(ref packet, i, ObjectState.Far);
                        break;
                    }
                    case "NearObjects":
                    {
                        ReadObjectsBlock(ref packet, i, ObjectState.Near);
                        break;
                    }
                    case "DestroyObjects":
                    {
                        ReadObjectsBlock(ref packet, i, ObjectState.Destroyed);
                        break;
                    }
                }
            }
            packet.StoreEndList();
        }

        private static void ReadCreateObjectBlock(ref Packet packet, Guid guid, uint map, int index)
        {
            ObjectType objType = packet.ReadEnum<ObjectType>("Object Type", TypeCode.Byte, index);
            var obj = PacketFileProcessor.Current.GetProcessor<ObjectStore>().GetObjectWithTypeOrCreate(guid, objType);
            obj.Created = true;
            obj.Map = map;
            obj.Area = PacketFileProcessor.Current.GetProcessor<SessionStore>().CurrentAreaId;
            obj.PhaseMask |= (uint)PacketFileProcessor.Current.GetProcessor<SessionStore>().CurrentPhaseMask; // additional phase mask if we see obj again

            obj.Movement = ReadMovementUpdateBlock(ref packet, guid, index);
            obj.SpawnMovement = new MovementInfo(obj.Movement);
            ReadValuesUpdateBlock(ref packet, guid, map, index, true);
            obj.SpawnUpdateFields = new UpdateFieldDictionary(obj.UpdateFields);

            obj.LoadValuesFromUpdateFields();
        }

        private static void ReadObjectsBlock(ref Packet packet, int index, ObjectState state)
        {
            var ObjectStore = PacketFileProcessor.Current.GetProcessor<ObjectStore>();
            var objCount = packet.ReadInt32("Object Count", index);
            packet.StoreBeginList("Objects", index);
            for (var j = 0; j < objCount; j++)
            {
                var guid = packet.ReadPackedGuid("GUID", index, j);
                var obj = ObjectStore.GetObjectOrCreate(guid); // must be found - was read
                obj.State = state;
            }
            packet.StoreEndList();
        }

        private static void ReadValuesUpdateBlock(ref Packet packet, Guid guid, uint map, int index, bool isCreating)
        {
            var objects = PacketFileProcessor.Current.GetProcessor<ObjectStore>();
            var obj = objects.GetObjectOrCreate(guid);

            // update obj info
            if (!isCreating)
            {
                obj.Map = map;
                obj.Area = PacketFileProcessor.Current.GetProcessor<SessionStore>().CurrentAreaId;
                obj.PhaseMask |= (uint)PacketFileProcessor.Current.GetProcessor<SessionStore>().CurrentPhaseMask; // additional phase mask if we see obj again
            }

            var maskSize = packet.ReadByte();

            var updateMask = new int[maskSize];
            for (var i = 0; i < maskSize; i++)
                updateMask[i] = packet.ReadInt32();

            var mask = new BitArray(updateMask);
            bool[] m = new bool[mask.Count];
            mask.CopyTo(m, 0);

            int size = 0;

            for (var i = 0; i < m.Length; i++)
            {
                if (!m[i])
                    continue;

                size = i;
                var blockVal = packet.ReadUpdateField();

                obj.UpdateFields[i] = blockVal;
            }

            packet.StoreBeginList("UpdateFields", index);
            for (var i = 0; i < size; )
            {
                var name = UpdateFields.GetUpdateFieldNameByOffset(i, obj.Type);

                Type fieldType = UpdateFields.GetUpdateFieldType(name);
                int fieldSize = UpdateFieldDictionary.GetFieldsCount(fieldType);
                // check if any part of field was modified and needs display
                int j;
                for (j = i; j < i + fieldSize; ++j)
                {
                    if (m[i])
                        break;
                }
                // nothing found
                if (j == i + fieldSize)
                {
                    i = j;
                    continue;
                }

                StringBuilder keyBuilder = new StringBuilder(30);

                if (name == null)
                {
                    keyBuilder.Append("Update field ");
                    keyBuilder.Append(i);
                }
                else
                {
                    keyBuilder.Append(name);
                    keyBuilder.Append(" (");
                    keyBuilder.Append(i);
                    keyBuilder.Append(")");
                }

                keyBuilder.Append(UpdateFields.GetUpdateFieldBytesInfo(name));
                Object val = obj.UpdateFields.GetValue(i, fieldType);
                packet.Store(keyBuilder.ToString(), val, index, i);
                i = i + fieldSize;
            }
            packet.StoreEndList();

            // Dynamic fields - NYI
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V5_0_4_16016))
            {
                maskSize = packet.ReadByte();
                updateMask = new int[maskSize];
                for (var i = 0; i < maskSize; i++)
                    updateMask[i] = packet.ReadInt32();

                mask = new BitArray(updateMask);
                for (var i = 0; i < mask.Count; ++i)
                {
                    if (!mask[i])
                        continue;

                    var flag = packet.ReadByte();

                    if ((flag & 0x80) != 0)
                        packet.ReadUInt16();

                    var cnt = flag & 0x7F;
                    var vals = new uint[cnt];
                    for (var j = 0; j < cnt; ++j)
                        vals[j] = packet.ReadUInt32();

                    for (var j = 0; j < cnt; ++j)
                        if (vals[j] != 0)
                            for (var k = 0; k < 32; ++k)
                                if (((1 << k) & vals[j]) != 0)
                                    packet.ReadUInt32();
                }
            }
        }

        private static MovementInfo ReadMovementUpdateBlock510(ref Packet packet, Guid guid, int index)
        {
            var moveInfo = new MovementInfo();

            var bit654 = packet.ReadBit("Has bit654", index);
            packet.ReadBit();
            var hasGameObjectRotation = packet.ReadBit("Has GameObject Rotation", index);
            var hasAttackingTarget = packet.ReadBit("Has Attacking Target", index);
            /*var bit2 = */
            packet.ReadBit();
            var bit520 = packet.ReadBit("Has bit520", index);
            var unkLoopCounter = packet.ReadBits(24);
            var transport = packet.ReadBit("Transport", index);
            var hasGameObjectPosition = packet.ReadBit("Has GameObject Position", index);
            /*var bit653 = */ packet.ReadBit();
            var bit784 = packet.ReadBit("Has bit784", index);
            /*var isSelf = */ packet.ReadBit("Self", index);
            /*var bit1 = */ packet.ReadBit();
            var living = packet.ReadBit("Living", index);
            /*var bit3 = */ packet.ReadBit();
            var bit644 = packet.ReadBit("Has bit644", index);
            var hasStationaryPosition = packet.ReadBit("Has Stationary Position", index);
            var hasVehicleData = packet.ReadBit("Has Vehicle Data", index);
            var bits360 = packet.ReadBits(21);
            var hasAnimKits = packet.ReadBit("Has AnimKits", index);
            for (var i = 0; i < bits360; ++i)
                packet.ReadBits(2);

            var guid2 = new byte[8];
            var facingTargetGuid = new byte[8];
            var unkSplineCounter = 0u;
            var attackingTargetGuid = new byte[8];
            var transportGuid = new byte[8];
            var goTransportGuid = new byte[8];
            var hasFallData = false;
            var hasFallDirection = false;
            var hasTimestamp = false;
            var hasOrientation = false;
            var hasMoveFlagsExtra = false;
            var hasMovementFlags = false;
            var hasPitch = false;
            var hasSplineElevation = false;
            var hasTransportData = false;
            var hasTransportTime2 = false;
            var hasTransportTime3 = false;
            var hasFullSpline = false;
            var hasSplineVerticalAcceleration = false;
            var hasUnkSplineCounter = false;
            var hasSplineStartTime = false;
            var hasGOTransportTime3 = false;
            var hasGOTransportTime2 = false;
            var hasAnimKit1 = false;
            var hasAnimKit2 = false;
            var hasAnimKit3 = false;
            var splineType = SplineType.Stop;
            var unkLoopCounter2 = 0u;
            var splineCount = 0u;

            var field8 = false;
            var bit540 = false;
            var bit552 = false;
            var bit580 = false;
            var bit624 = false;
            var bit147 = 0u;
            var bit151 = 0u;
            var bit158 = 0u;
            var bit198 = 0u;

            if (living)
            {
                guid2[3] = packet.ReadBit();
                hasFallData = packet.ReadBit("Has Fall Data", index);
                hasTimestamp = !packet.ReadBit("Lacks Timestamp", index);
                packet.ReadBit(); // bit172
                guid2[2] = packet.ReadBit();
                packet.ReadBit(); // bit149
                hasPitch = !packet.ReadBit("Lacks Pitch", index);
                hasMoveFlagsExtra = !packet.ReadBit();
                guid2[4] = packet.ReadBit();
                guid2[5] = packet.ReadBit();
                unkLoopCounter2 = packet.ReadBits(24);
                hasSplineElevation = !packet.ReadBit();
                field8 = !packet.ReadBit();
                packet.ReadBit(); // bit148
                guid2[0] = packet.ReadBit();
                guid2[6] = packet.ReadBit();
                guid2[7] = packet.ReadBit();
                hasTransportData = packet.ReadBit("Has Transport Data", index);
                hasOrientation = !packet.ReadBit();

                if (hasTransportData)
                {
                    transportGuid[3] = packet.ReadBit();
                    transportGuid[0] = packet.ReadBit();
                    transportGuid[4] = packet.ReadBit();
                    transportGuid[5] = packet.ReadBit();
                    transportGuid[2] = packet.ReadBit();
                    transportGuid[7] = packet.ReadBit();
                    transportGuid[1] = packet.ReadBit();
                    hasTransportTime2 = packet.ReadBit();
                    transportGuid[6] = packet.ReadBit();
                    hasTransportTime3 = packet.ReadBit();
                }

                if (hasMoveFlagsExtra)
                    moveInfo.FlagsExtra = packet.ReadEnum<MovementFlagExtra>("Extra Movement Flags", 13, index);

                hasMovementFlags = !packet.ReadBit();
                guid2[1] = packet.ReadBit();
                if (hasFallData)
                    hasFallDirection = packet.ReadBit("Has Fall Direction", index);

                moveInfo.HasSplineData = packet.ReadBit("Has Spline Data", index);
                if (hasMovementFlags)
                    moveInfo.Flags = packet.ReadEnum<MovementFlag>("Movement Flags", 30, index);

                if (moveInfo.HasSplineData)
                {
                    hasFullSpline = packet.ReadBit("Has extended spline data", index);
                    if (hasFullSpline)
                    {
                        hasSplineStartTime = packet.ReadBit();
                        splineCount = packet.ReadBits("Spline Waypoints", 22, index);
                        /*var splineFlags = */ packet.ReadEnum<SplineFlag434>("Spline flags", 25, index);
                        var bits57 = packet.ReadBits(2);
                        switch (bits57)
                        {
                            case 1:
                                splineType = SplineType.FacingTarget;
                                break;
                            case 0:
                                splineType = SplineType.FacingAngle;
                                break;
                            case 2:
                                splineType = SplineType.Normal;
                                break;
                            case 3:
                                splineType = SplineType.FacingSpot;
                                break;
                        }

                        if (splineType == SplineType.FacingTarget)
                            facingTargetGuid = packet.StartBitStream(0, 1, 6, 5, 2, 3, 4, 7);

                        hasUnkSplineCounter = packet.ReadBit();
                        if (hasUnkSplineCounter)
                        {
                            unkSplineCounter = packet.ReadBits(23);
                            packet.ReadBits(2);
                        }

                        /*var splineMode = */ packet.ReadEnum<SplineMode>("Spline Mode", 2, index);
                        hasSplineVerticalAcceleration = packet.ReadBit();
                    }
                }
            }

            if (hasGameObjectPosition)
            {
                hasGOTransportTime3 = packet.ReadBit();
                goTransportGuid[3] = packet.ReadBit();
                goTransportGuid[1] = packet.ReadBit();
                goTransportGuid[4] = packet.ReadBit();
                goTransportGuid[7] = packet.ReadBit();
                goTransportGuid[2] = packet.ReadBit();
                goTransportGuid[5] = packet.ReadBit();
                goTransportGuid[0] = packet.ReadBit();
                goTransportGuid[6] = packet.ReadBit();
                hasGOTransportTime2 = packet.ReadBit();
            }

            if (bit654)
                packet.ReadBits(9);

            if (bit520)
            {
                bit540 = packet.ReadBit("bit540", index);
                packet.ReadBit("bit536", index);
                bit552 = packet.ReadBit("bit552", index);
                packet.ReadBit("bit539", index);
                bit624 = packet.ReadBit("bit624", index);
                bit580 = packet.ReadBit("bit580", index);
                packet.ReadBit("bit537", index);

                if (bit580)
                {
                    bit147 = packet.ReadBits(23);
                    bit151 = packet.ReadBits(23);
                }

                if (bit624)
                    bit158 = packet.ReadBits(22);

                packet.ReadBit("bit538", index);
            }

            if (hasAttackingTarget)
                attackingTargetGuid = packet.StartBitStream(2, 6, 7, 1, 0, 3, 4, 5);

            if (bit784)
                bit198 = packet.ReadBits(24);

            if (hasAnimKits)
            {
                hasAnimKit3 = !packet.ReadBit();
                hasAnimKit1 = !packet.ReadBit();
                hasAnimKit2 = !packet.ReadBit();
            }

            packet.ResetBitReader();

            // Reading data
            packet.StoreBeginList("UnknownList1", index);
            for (var i = 0; i < bits360; ++i)
            {
                packet.ReadSingle();
                packet.ReadUInt32();
                packet.ReadSingle();
                packet.ReadUInt32();
                packet.ReadSingle();
                packet.ReadSingle();
            }
            packet.StoreEndList();

            packet.StoreBeginList("UnknownList2", index);
            for (var i = 0u; i < unkLoopCounter; ++i)
                packet.ReadUInt32("Unk UInt32", index, (int)i);
            packet.StoreEndList();

            if (living)
            {
                packet.ReadSingle("FlyBack Speed", index);
                if (moveInfo.HasSplineData)
                {
                    if (hasFullSpline)
                    {
                        if (hasUnkSplineCounter)
                        {
                            packet.StoreBeginList("UnknownSplineList", index);
                            for (var i = 0; i < unkSplineCounter; ++i)
                            {
                                packet.ReadSingle("Unk Spline Float1", index, (int)i);
                                packet.ReadSingle("Unk Spline Float2", index, (int)i);
                            }
                            packet.StoreEndList();
                        }

                        if (splineType == SplineType.FacingTarget)
                        {
                            packet.ParseBitStream(facingTargetGuid, 3, 2, 0, 5, 6, 7, 4, 1);
                            packet.StoreBitstreamGuid("Facing Target GUID", facingTargetGuid, index);
                        }

                        packet.ReadUInt32("Spline Time", index);
                        packet.ReadUInt32("Spline Full Time", index);

                        if (hasSplineVerticalAcceleration)
                            packet.ReadSingle("Spline Vertical Acceleration", index);

                        packet.ReadSingle("Spline Duration Multiplier Next", index);
                        packet.ReadSingle("Spline Duration Multiplier", index);

                        if (splineType == SplineType.FacingSpot)
                        {
                            var point = new Vector3
                            {
                                X = packet.ReadSingle(),
                                Z = packet.ReadSingle(),
                                Y = packet.ReadSingle(),
                            };

                            packet.Store("Facing Spot", point, index);
                        }

                        if (hasSplineStartTime)
                            packet.ReadUInt32("Spline Start Time", index);

                        packet.StoreBeginList("SplineWaypoints", index);
                        for (var i = 0; i < splineCount; ++i)
                        {
                            var wp = new Vector3
                            {
                                Y = packet.ReadSingle(),
                                Z = packet.ReadSingle(),
                                X = packet.ReadSingle(),
                            };

                            packet.Store("Spline Waypoint", wp, index, i);
                        }
                        packet.StoreEndList();

                        if (splineType == SplineType.FacingAngle)
                            packet.ReadSingle("Facing Angle", index);
                    }

                    var endPoint = new Vector3
                    {
                        Y = packet.ReadSingle(),
                        X = packet.ReadSingle(),
                        Z = packet.ReadSingle(),
                    };

                    packet.ReadUInt32("Spline Id", index);
                    packet.Store("Spline Endpoint", endPoint, index);
                }

                packet.ReadSingle("Swim Speed", index);

                if (hasFallData)
                {
                    if (hasFallDirection)
                    {
                        packet.ReadSingle("Jump Velocity", index);
                        packet.ReadSingle("Jump Cos", index);
                        packet.ReadSingle("Jump Sin", index);
                    }

                    packet.ReadSingle("Fall Start Velocity", index);
                    packet.ReadInt32("Time Fallen", index);
                }

                if (hasTransportData)
                {
                    moveInfo.TransportOffset.Z = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 4);
                    moveInfo.TransportOffset.X = packet.ReadSingle();
                    if (hasTransportTime3)
                        packet.ReadUInt32("Transport Time 3", index);

                    packet.ReadXORByte(transportGuid, 6);
                    packet.ReadXORByte(transportGuid, 5);
                    packet.ReadXORByte(transportGuid, 1);
                    moveInfo.TransportOffset.O = packet.ReadSingle();
                    moveInfo.TransportOffset.Y = packet.ReadSingle();
                    packet.ReadSByte("Transport Seat", index);
                    packet.ReadXORByte(transportGuid, 7);
                    if (hasTransportTime2)
                        packet.ReadUInt32("Transport Time 2", index);

                    packet.ReadUInt32("Transport Time", index);
                    packet.ReadXORByte(transportGuid, 0);
                    packet.ReadXORByte(transportGuid, 2);
                    packet.ReadXORByte(transportGuid, 3);

                    moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(transportGuid, 0));
                    packet.Store("Transport GUID", moveInfo.TransportGuid, index);
                    packet.Store("Transport Position", moveInfo.TransportOffset, index);
                }

                packet.ReadXORByte(guid2, 1);
                packet.ReadSingle("Turn Speed", index);
                moveInfo.Position.Y = packet.ReadSingle();
                packet.ReadXORByte(guid2, 3);
                moveInfo.Position.Z = packet.ReadSingle();
                if (hasOrientation)
                    moveInfo.Orientation = packet.ReadSingle();

                packet.ReadSingle("Run Back Speed", index);
                if (hasSplineElevation)
                    packet.ReadSingle("Spline Elevation", index);

                packet.ReadXORByte(guid2, 0);
                packet.ReadXORByte(guid2, 6);
                packet.StoreBeginList("UnknownList3", index);
                for (var i = 0u; i < unkLoopCounter2; ++i)
                    packet.ReadUInt32("Unk2 UInt32", index, (int)i);
                packet.StoreEndList();

                moveInfo.Position.X = packet.ReadSingle();
                if (hasTimestamp)
                    packet.ReadUInt32("Time", index);

                moveInfo.WalkSpeed = packet.ReadSingle("Walk Speed", index) / 2.5f;
                if (hasPitch)
                    packet.ReadSingle("Pitch", index);

                packet.ReadXORByte(guid2, 5);
                if (field8)
                    packet.ReadUInt32("Unk UInt32", index);

                packet.ReadSingle("Pitch Speed", index);
                packet.ReadXORByte(guid2, 2);
                moveInfo.RunSpeed = packet.ReadSingle("Run Speed", index) / 7.0f;
                packet.ReadXORByte(guid2, 7);
                packet.ReadSingle("SwimBack Speed", index);
                packet.ReadXORByte(guid2, 4);
                packet.ReadSingle("Fly Speed", index);

                packet.Store("GUID 2", new Guid(BitConverter.ToUInt64(guid2, 0)), index);
                packet.Store("Position", moveInfo.Position, index);
                packet.Store("Orientation", moveInfo.Orientation, index);
            }

            if (bit520)
            {
                if (bit580)
                {
                    packet.ReadSingle("field154", index);
                    packet.ReadSingle("field155", index);

                    for (var i = 0; i < bit147; ++i)
                    {
                        packet.ReadSingle();
                        packet.ReadSingle();
                    }

                    for (var i = 0; i < bit151; ++i)
                    {
                        packet.ReadSingle();
                        packet.ReadSingle();
                    }
                }

                if (bit540)
                {
                    packet.ReadSingle("field136", index);
                    packet.ReadSingle("field134", index);
                }

                if (bit552)
                {
                    packet.ReadSingle("field143", index);
                    packet.ReadSingle("field141", index);
                    packet.ReadSingle("field142", index);
                    packet.ReadSingle("field140", index);
                    packet.ReadSingle("field139", index);
                    packet.ReadSingle("field144", index);
                }

                packet.ReadSingle("field132", index);
                if (bit624)
                {
                    for (var i = 0; i < bit158; ++i)
                    {
                        packet.ReadSingle();
                        packet.ReadSingle();
                        packet.ReadSingle();
                    }
                }

                packet.ReadSingle("field133", index);
                packet.ReadSingle("field131", index);
            }

            if (hasAttackingTarget)
            {
                packet.ParseBitStream(attackingTargetGuid, 3, 4, 2, 5, 1, 6, 7, 0);
                packet.StoreBitstreamGuid("Attacking Target GUID", attackingTargetGuid, index);
            }

            if (hasStationaryPosition)
            {
                moveInfo.Position.X = packet.ReadSingle();
                moveInfo.Orientation = packet.ReadSingle("Stationary Orientation", index);
                moveInfo.Position.Y = packet.ReadSingle();
                moveInfo.Position.Z = packet.ReadSingle();
                packet.Store("Stationary Position", moveInfo.Position, index);
            }

            if (hasGameObjectPosition)
            {
                packet.ReadXORByte(goTransportGuid, 3);
                packet.ReadXORByte(goTransportGuid, 1);
                packet.ReadSByte("GO Transport Seat", index);
                moveInfo.TransportOffset.Z = packet.ReadSingle();
                packet.ReadXORByte(goTransportGuid, 2);
                packet.ReadXORByte(goTransportGuid, 7);
                if (hasGOTransportTime3)
                    packet.ReadUInt32("GO Transport Time 3", index);

                packet.ReadXORByte(goTransportGuid, 6);
                if (hasGOTransportTime2)
                    packet.ReadUInt32("GO Transport Time 2", index);

                packet.ReadUInt32("GO Transport Time", index);
                moveInfo.TransportOffset.Y = packet.ReadSingle();
                moveInfo.TransportOffset.X = packet.ReadSingle();
                packet.ReadXORByte(goTransportGuid, 0);
                packet.ReadXORByte(goTransportGuid, 4);
                packet.ReadXORByte(goTransportGuid, 5);
                moveInfo.TransportOffset.O = packet.ReadSingle();

                moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(goTransportGuid, 0));
                packet.Store("GO Transport GUID", moveInfo.TransportGuid, index);
                packet.Store("GO Transport Position", moveInfo.TransportOffset, index);
            }

            if (hasAnimKits)
            {
                if (hasAnimKit3)
                    packet.ReadUInt16("Anim Kit 3", index);
                if (hasAnimKit1)
                    packet.ReadUInt16("Anim Kit 1", index);
                if (hasAnimKit2)
                    packet.ReadUInt16("Anim Kit 2", index);
            }

            if (hasVehicleData)
            {
                packet.ReadSingle("Vehicle Orientation", index);
                moveInfo.VehicleId = packet.ReadUInt32("Vehicle Id", index);
            }

            if (transport)
                packet.ReadUInt32("Transport Path Timer", index);

            if (bit644)
                packet.ReadUInt32("field162", index);

            if (bit784)
            {
                for (var i = 0; i < bit198; ++i)
                    packet.ReadUInt32();
            }

            if (hasGameObjectRotation)
                packet.ReadPackedQuaternion("GameObject Rotation", index);

            return moveInfo;
        }

        private static MovementInfo ReadMovementUpdateBlock504(ref Packet packet, Guid guid, int index)
        {
            var moveInfo = new MovementInfo();

            // bits
            var hasAttackingTarget = packet.ReadBit("Has Attacking Target", index);
            var hasVehicleData = packet.ReadBit("Has Vehicle Data", index);
            var unkLoopCounter = packet.ReadBits(24);
            var bit284 = packet.ReadBit();
            var hasGameObjectPosition = packet.ReadBit("Has GameObject Position", index);
            var hasStationaryPosition = packet.ReadBit("Has Stationary Position", index);
            var bits16C = packet.ReadBits(21);
            var transport = packet.ReadBit("Transport", index);
            var bit208 = packet.ReadBit();
            /*var bit 28C =*/ packet.ReadBit();
            var living = packet.ReadBit("Living", index);
            /*var bit1 =*/ packet.ReadBit();
            var bit28D = packet.ReadBit();
            /*var bit2 =*/ packet.ReadBit();
            var hasGameObjectRotation = packet.ReadBit("Has GameObject Rotation", index);
            var hasAnimKits = packet.ReadBit("Has AnimKits", index);
            /*var bit3 =*/ packet.ReadBit();
            packet.ReadBit("Self", index);
            for (var i = 0; i < bits16C; ++i)
                packet.ReadBits(2);

            var hasOrientation = false;
            var guid2 = new byte[8];
            var hasPitch = false;
            var hasFallData = false;
            var hasSplineElevation = false;
            var hasTransportData = false;
            var hasTimestamp = false;
            var transportGuid = new byte[8];
            var hasTransportTime2 = false;
            var hasTransportTime3 = false;
            var hasFullSpline = false;
            var hasSplineStartTime = false;
            var splineCount = 0u;
            var splineType = SplineType.Stop;
            var facingTargetGuid = new byte[8];
            var hasSplineVerticalAcceleration = false;
            var hasFallDirection = false;
            var goTransportGuid = new byte[8];
            var hasGOTransportTime2 = false;
            var hasGOTransportTime3 = false;
            var attackingTargetGuid = new byte[8];
            var hasAnimKit1 = false;
            var hasAnimKit2 = false;
            var hasAnimKit3 = false;
            var bit228 = false;
            var bit21C = false;
            var bit278 = 0u;
            var bit244 = false;
            var bit24C = 0u;
            var bit25C = 0u;
            var field9C = 0u;
            var hasFieldA8 = false;
            var unkSplineCounter = 0u;

            if (hasGameObjectPosition)
            {
                goTransportGuid[4] = packet.ReadBit();
                goTransportGuid[3] = packet.ReadBit();
                goTransportGuid[6] = packet.ReadBit();
                goTransportGuid[0] = packet.ReadBit();
                goTransportGuid[5] = packet.ReadBit();
                goTransportGuid[1] = packet.ReadBit();
                hasGOTransportTime2 = packet.ReadBit();
                hasGOTransportTime3 = packet.ReadBit();
                goTransportGuid[2] = packet.ReadBit();
                goTransportGuid[7] = packet.ReadBit();
            }

            if (bit208)
            {
                bit228 = packet.ReadBit();
                var bit270 = packet.ReadBit();
                packet.ReadBit();   // bit219
                packet.ReadBit();   // bit21A
                bit21C = packet.ReadBit();
                if (bit270)
                    bit278 = packet.ReadBits(22);

                bit244 = packet.ReadBit();
                if (bit244)
                {
                    bit24C = packet.ReadBits(23);
                    bit25C = packet.ReadBits(23);
                }

                packet.ReadBit();   // bit218
            }

            if (living)
            {
                guid2[3] = packet.ReadBit();
                moveInfo.HasSplineData = packet.ReadBit("Has Spline Data", index);
                field9C = packet.ReadBits(24);
                guid2[4] = packet.ReadBit();
                hasPitch = !packet.ReadBit("Lacks Pitch", index);
                hasTransportData = packet.ReadBit("Has Transport Data", index);
                hasFallData = packet.ReadBit("Has Fall Data", index);
                hasTimestamp = !packet.ReadBit("Lacks Timestamp", index);
                if (hasTransportData)
                {
                    transportGuid[3] = packet.ReadBit();
                    hasTransportTime3 = packet.ReadBit();
                    transportGuid[7] = packet.ReadBit();
                    transportGuid[0] = packet.ReadBit();
                    transportGuid[6] = packet.ReadBit();
                    hasTransportTime2 = packet.ReadBit();
                    transportGuid[4] = packet.ReadBit();
                    transportGuid[1] = packet.ReadBit();
                    transportGuid[2] = packet.ReadBit();
                    transportGuid[5] = packet.ReadBit();
                }

                hasFieldA8 = !packet.ReadBit();
                guid2[7] = packet.ReadBit();
                var hasMoveFlagsExtra = !packet.ReadBit();
                guid2[0] = packet.ReadBit();
                packet.ReadBit();
                guid2[5] = packet.ReadBit();
                if (hasMoveFlagsExtra)
                    moveInfo.FlagsExtra = packet.ReadEnum<MovementFlagExtra>("Extra Movement Flags", 13, index);

                guid2[2] = packet.ReadBit();
                guid2[6] = packet.ReadBit();
                var hasMovementFlags = !packet.ReadBit();
                if (hasFallData)
                    hasFallDirection = packet.ReadBit("Has Fall Direction", index);

                if (hasMovementFlags)
                    moveInfo.Flags = packet.ReadEnum<MovementFlag>("Movement Flags", 30, index);

                hasOrientation = !packet.ReadBit();
                packet.ReadBit();
                packet.ReadBit();

                if (moveInfo.HasSplineData)
                {
                    hasFullSpline = packet.ReadBit("Has extended spline data", index);
                    if (hasFullSpline)
                    {
                        hasSplineVerticalAcceleration = packet.ReadBit();
                        /*var splineMode =*/ packet.ReadEnum<SplineMode>("Spline Mode", 2, index);
                        var bit134 = packet.ReadBit();
                        if (bit134)
                        {
                            unkSplineCounter = packet.ReadBits(23);
                            packet.ReadBits(2);
                        }

                        /*splineFlags =*/
                        packet.Store("Spline flags", packet.ReadBits(25).ToString("X8"), index);
                        hasSplineStartTime = packet.ReadBit();
                        splineCount = packet.ReadBits("Spline Waypoints", 22, index);
                        var bits57 = packet.ReadBits(2);
                        switch (bits57)
                        {
                            case 0:
                                splineType = SplineType.FacingTarget;
                                break;
                            case 1:
                                splineType = SplineType.FacingSpot;
                                break;
                            case 2:
                                splineType = SplineType.Normal;
                                break;
                            case 3:
                                splineType = SplineType.FacingAngle;
                                break;
                        }


                        if (splineType == SplineType.FacingTarget)
                            facingTargetGuid = packet.StartBitStream(4, 5, 0, 7, 1, 3, 2, 6);

                        packet.Store("Spline type", splineType, index);
                    }
                }

                guid2[1] = packet.ReadBit();
                hasSplineElevation = !packet.ReadBit();
            }

            if (hasAttackingTarget)
                attackingTargetGuid = packet.StartBitStream(2, 6, 5, 1, 7, 3, 4, 0);

            if (hasAnimKits)
            {
                hasAnimKit2 = !packet.ReadBit();
                hasAnimKit3 = !packet.ReadBit();
                hasAnimKit1 = !packet.ReadBit();
            }

            if (bit28D)
                packet.ReadBits(9);

            packet.ResetBitReader();

            // Reading data
            for (var i = 0; i < bits16C; ++i)
            {
                packet.ReadUInt32();
                packet.ReadSingle();
                packet.ReadSingle();
                packet.ReadUInt32();
                packet.ReadSingle();
                packet.ReadSingle();
            }

            packet.StoreBeginList("UnknownList1", index);
            for (var i = 0u; i < unkLoopCounter; ++i)
                packet.ReadUInt32("Unk UInt32", index, (int)i);
            packet.StoreEndList();

            if (living)
            {
                if (moveInfo.HasSplineData)
                {
                    if (hasFullSpline)
                    {
                        if (splineType == SplineType.FacingSpot)
                        {
                            var point = new Vector3
                            {
                                X = packet.ReadSingle(),
                                Z = packet.ReadSingle(),
                                Y = packet.ReadSingle(),
                            };

                            packet.Store("Facing Spot", point, index);
                        }
                        else if (splineType == SplineType.FacingTarget)
                        {
                            packet.ParseBitStream(facingTargetGuid, 5, 6, 0, 1, 2, 4, 7, 3);
                            packet.StoreBitstreamGuid("Facing Target GUID", facingTargetGuid, index);
                        }

                        packet.ReadUInt32("Spline Time", index);
                        if (hasSplineVerticalAcceleration)
                            packet.ReadSingle("Spline Vertical Acceleration", index);

                        if (hasSplineStartTime)
                            packet.ReadUInt32("Spline Start time", index);

                        for (var i = 0; i < unkSplineCounter; ++i)
                        {
                            packet.ReadSingle();
                            packet.ReadSingle();
                        }

                        if (splineType == SplineType.FacingAngle)
                            packet.ReadSingle("Facing Angle", index);

                        packet.StoreBeginList("SplineWaypoints", index);
                        for (var i = 0; i < splineCount; ++i)
                        {
                            var wp = new Vector3
                            {
                                X = packet.ReadSingle(),
                                Y = packet.ReadSingle(),
                                Z = packet.ReadSingle(),
                            };

                            packet.Store("Spline Waypoint", wp, index, i);
                        }
                        packet.StoreEndList();

                        packet.ReadSingle("Spline Duration Multiplier", index);
                        packet.ReadUInt32("Spline Full Time", index);
                        packet.ReadSingle("Spline Duration Multiplier Next", index);
                    }

                    var endPoint = new Vector3();
                    endPoint.Z = packet.ReadSingle();
                    packet.ReadUInt32("Spline Id", index);
                    endPoint.X = packet.ReadSingle();
                    endPoint.Y = packet.ReadSingle();

                    packet.Store("Spline Endpoint", endPoint, index);
                }

                for (var i = 0; i < field9C; ++i)
                    packet.ReadUInt32();

                moveInfo.WalkSpeed = packet.ReadSingle("Walk Speed", index) / 2.5f;
                if (hasTransportData)
                {
                    packet.ReadXORByte(transportGuid, 4);
                    packet.ReadXORByte(transportGuid, 0);
                    moveInfo.TransportOffset.Y = packet.ReadSingle();
                    moveInfo.TransportOffset.X = packet.ReadSingle();
                    packet.ReadSByte("Transport Seat", index);
                    packet.ReadXORByte(transportGuid, 7);
                    packet.ReadXORByte(transportGuid, 3);
                    if (hasTransportTime3)
                        packet.ReadUInt32("Transport Time 3", index);

                    packet.ReadXORByte(transportGuid, 6);
                    moveInfo.TransportOffset.O = packet.ReadSingle();
                    packet.ReadUInt32("Transport Time", index);
                    packet.ReadXORByte(transportGuid, 2);
                    packet.ReadXORByte(transportGuid, 1);
                    moveInfo.TransportOffset.Z = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 5);
                    if (hasTransportTime2)
                        packet.ReadUInt32("Transport Time 2", index);

                    moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(transportGuid, 0));
                    packet.Store("Transport GUID", moveInfo.TransportGuid, index);
                    packet.Store("Transport Position", moveInfo.TransportOffset, index);
                }

                packet.ReadXORByte(guid2, 2);
                if (hasFallData)
                {
                    packet.ReadInt32("Time Fallen", index);
                    if (hasFallDirection)
                    {
                        packet.ReadSingle("Jump Sin", index);
                        packet.ReadSingle("Jump Cos", index);
                        packet.ReadSingle("Jump Velocity", index);
                    }

                    packet.ReadSingle("Fall Start Velocity", index);
                }

                packet.ReadXORByte(guid2, 7);
                if (hasTimestamp)
                    packet.ReadUInt32("Time", index);

                packet.ReadSingle("Fly Speed", index);
                moveInfo.Position.X = packet.ReadSingle();
                if (hasFieldA8)
                    packet.ReadUInt32();

                moveInfo.Position.Y = packet.ReadSingle();
                packet.ReadXORByte(guid2, 5);
                moveInfo.Position.Z = packet.ReadSingle();
                if (hasPitch)
                    packet.ReadSingle("Pitch", index);

                packet.ReadXORByte(guid2, 3);
                packet.ReadXORByte(guid2, 6);
                packet.ReadXORByte(guid2, 1);
                if (hasSplineElevation)
                    packet.ReadSingle("Spline Elevation", index);

                packet.ReadSingle("Turn Speed", index);
                packet.ReadSingle("Pitch Speed", index);
                moveInfo.RunSpeed = packet.ReadSingle("Run Speed", index) / 7.0f;
                if (hasOrientation)
                    moveInfo.Orientation = packet.ReadSingle();

                packet.ReadXORByte(guid2, 4);
                packet.ReadSingle("Swim Speed", index);
                packet.ReadSingle("SwimBack Speed", index);
                packet.ReadSingle("FlyBack Speed", index);
                packet.ReadSingle("RunBack Speed", index);
                packet.ReadXORByte(guid2, 0);

                packet.Store("GUID 2", new Guid(BitConverter.ToUInt64(guid2, 0)), index);
                packet.Store("Position", moveInfo.Position, index);
                packet.Store("Orientation", moveInfo.Orientation, index);
            }

            if (bit208)
            {
                if (bit228)
                {
                    packet.ReadSingle();
                    packet.ReadSingle();
                    packet.ReadSingle();
                    packet.ReadSingle();
                    packet.ReadSingle();
                    packet.ReadSingle();
                }

                if (bit21C)
                {
                    packet.ReadSingle();
                    packet.ReadSingle();
                }

                if (bit244)
                {
                    for (var i = 0; i < bit24C; ++i)
                    {
                        packet.ReadSingle();
                        packet.ReadSingle();
                    }

                    packet.ReadSingle();
                    for (var i = 0; i < bit25C; ++i)
                    {
                        packet.ReadSingle();
                        packet.ReadSingle();
                    }

                    packet.ReadSingle();
                }

                packet.ReadUInt32();
                for (var i = 0; i < bit278; ++i)
                {
                    packet.ReadSingle();
                    packet.ReadSingle();
                    packet.ReadSingle();
                }

                packet.ReadSingle();
                packet.ReadSingle();
            }

            if (hasGameObjectPosition)
            {
                packet.ReadXORByte(goTransportGuid, 7);
                packet.ReadXORByte(goTransportGuid, 3);
                packet.ReadXORByte(goTransportGuid, 5);
                moveInfo.TransportOffset.O = packet.ReadSingle();
                packet.ReadXORByte(goTransportGuid, 6);
                packet.ReadXORByte(goTransportGuid, 0);
                packet.ReadXORByte(goTransportGuid, 2);
                packet.ReadUInt32("GO Transport Time", index);
                if (hasGOTransportTime3)
                    packet.ReadUInt32("GO Transport Time 3", index);

                packet.ReadXORByte(goTransportGuid, 1);
                moveInfo.TransportOffset.Z = packet.ReadSingle();
                packet.ReadSByte("GO Transport Seat", index);
                if (hasGOTransportTime2)
                    packet.ReadUInt32("GO Transport Time 2", index);

                moveInfo.TransportOffset.X = packet.ReadSingle();
                packet.ReadXORByte(goTransportGuid, 4);
                moveInfo.TransportOffset.Y = packet.ReadSingle();

                moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(goTransportGuid, 0));
                packet.Store("GO Transport GUID", moveInfo.TransportGuid, index);
                packet.Store("GO Transport Position", moveInfo.TransportOffset, index);
            }

            if (hasStationaryPosition)
            {
                moveInfo.Position.Y = packet.ReadSingle();
                moveInfo.Position.Z = packet.ReadSingle();
                moveInfo.Position.X = packet.ReadSingle();
                packet.Store("Stationary Position", moveInfo.Position, index);
                moveInfo.Orientation = packet.ReadSingle("Stationary Orientation", index);
            }

            if (hasAttackingTarget)
            {
                packet.ParseBitStream(attackingTargetGuid, 3, 6, 4, 1, 5, 7, 0, 2);
                packet.StoreBitstreamGuid("Attacking Target GUID", attackingTargetGuid, index);
            }

            if (transport)
                packet.ReadUInt32("Transport path timer", index);

            if (hasGameObjectRotation)
                packet.ReadPackedQuaternion("GameObject Rotation", index);

            if (hasVehicleData)
            {
                moveInfo.VehicleId = packet.ReadUInt32("Vehicle Id", index);
                packet.ReadSingle("Vehicle Orientation", index);
            }

            if (hasAnimKits)
            {
                if (hasAnimKit2)
                    packet.ReadUInt16("Anim Kit 2", index);
                if (hasAnimKit3)
                    packet.ReadUInt16("Anim Kit 3", index);
                if (hasAnimKit1)
                    packet.ReadUInt16("Anim Kit 1", index);
            }

            if (bit284)
                packet.ReadUInt32();

            return moveInfo;
        }

        private static MovementInfo ReadMovementUpdateBlock434(ref Packet packet, Guid guid, int index)
        {
            var moveInfo = new MovementInfo();

            // bits
            /*var bit3 =*/ packet.ReadBit();
            /*var bit4 =*/ packet.ReadBit();
            var hasGameObjectRotation = packet.ReadBit("Has GameObject Rotation", index);
            var hasAnimKits = packet.ReadBit("Has AnimKits", index);
            var hasAttackingTarget = packet.ReadBit("Has Attacking Target", index);
            packet.ReadBit("Self", index);
            var hasVehicleData = packet.ReadBit("Has Vehicle Data", index);
            var living = packet.ReadBit("Living", index);
            var unkLoopCounter = packet.ReadBits(24);
            /*var bit1 =*/ packet.ReadBit();
            var hasGameObjectPosition = packet.ReadBit("Has GameObject Position", index);
            var hasStationaryPosition = packet.ReadBit("Has Stationary Position", index);
            var bit456 = packet.ReadBit();
            /*var bit2 =*/ packet.ReadBit();
            var transport = packet.ReadBit("Transport", index);
            var hasOrientation = false;
            var guid2 = new byte[8];
            var hasPitch = false;
            var hasFallData = false;
            var hasSplineElevation = false;
            var hasTransportData = false;
            var hasTimestamp = false;
            var transportGuid = new byte[8];
            var hasTransportTime2 = false;
            var hasTransportTime3 = false;
            var bit216 = false;
            var hasSplineStartTime = false;
            var splineCount = 0u;
            var splineType = SplineType.Stop;
            var facingTargetGuid = new byte[8];
            var hasSplineVerticalAcceleration = false;
            var hasFallDirection = false;
            var goTransportGuid = new byte[8];
            var hasGOTransportTime2 = false;
            var hasGOTransportTime3 = false;
            var attackingTargetGuid = new byte[8];
            var hasAnimKit1 = false;
            var hasAnimKit2 = false;
            var hasAnimKit3 = false;

            if (living)
            {
                var hasMovementFlags = !packet.ReadBit();
                hasOrientation = !packet.ReadBit();
                guid2[7] = packet.ReadBit();
                guid2[3] = packet.ReadBit();
                guid2[2] = packet.ReadBit();
                if (hasMovementFlags)
                    moveInfo.Flags = packet.ReadEnum<MovementFlag>("Movement Flags", 30, index);

                packet.ReadBit();
                hasPitch = !packet.ReadBit();
                moveInfo.HasSplineData = packet.ReadBit("Has Spline Data", index);
                hasFallData = packet.ReadBit("Has Fall Data", index);
                hasSplineElevation = !packet.ReadBit();
                guid2[5] = packet.ReadBit();
                hasTransportData = packet.ReadBit("Has Transport Data", index);
                hasTimestamp = !packet.ReadBit();
                if (hasTransportData)
                {
                    transportGuid[1] = packet.ReadBit();
                    hasTransportTime2 = packet.ReadBit();
                    transportGuid[4] = packet.ReadBit();
                    transportGuid[0] = packet.ReadBit();
                    transportGuid[6] = packet.ReadBit();
                    hasTransportTime3 = packet.ReadBit();
                    transportGuid[7] = packet.ReadBit();
                    transportGuid[5] = packet.ReadBit();
                    transportGuid[3] = packet.ReadBit();
                    transportGuid[2] = packet.ReadBit();
                }

                guid2[4] = packet.ReadBit();
                if (moveInfo.HasSplineData)
                {
                    bit216 = packet.ReadBit("Has extended spline data", index);
                    if (bit216)
                    {
                        /*var splineMode =*/ packet.ReadEnum<SplineMode>("Spline Mode", 2, index);
                        hasSplineStartTime = packet.ReadBit();
                        splineCount = packet.ReadBits("Spline Waypoints Count", 22, index);
                        var bits57 = packet.ReadBits(2);
                        switch (bits57)
                        {
                            case 0:
                                splineType = SplineType.FacingAngle;
                                break;
                            case 1:
                                splineType = SplineType.FacingSpot;
                                break;
                            case 2:
                                splineType = SplineType.FacingTarget;
                                break;
                            case 3:
                                splineType = SplineType.Normal;
                                break;
                        }

                        if (splineType == SplineType.FacingTarget)
                            facingTargetGuid = packet.StartBitStream(4, 3, 7, 2, 6, 1, 0, 5);

                        hasSplineVerticalAcceleration = packet.ReadBit();
                        packet.Store("Spline type", splineType, index);
                        /*splineFlags =*/ packet.ReadEnum<SplineFlag434>("Spline flags", 25, index);
                    }
                }

                guid2[6] = packet.ReadBit();
                if (hasFallData)
                    hasFallDirection = packet.ReadBit("Has Fall Direction", index);

                guid2[0] = packet.ReadBit();
                guid2[1] = packet.ReadBit();
                packet.ReadBit();
                if (!packet.ReadBit())
                    moveInfo.FlagsExtra = packet.ReadEnum<MovementFlagExtra>("Extra Movement Flags", 12, index);
            }

            if (hasGameObjectPosition)
            {
                goTransportGuid[5] = packet.ReadBit();
                hasGOTransportTime3 = packet.ReadBit();
                goTransportGuid[0] = packet.ReadBit();
                goTransportGuid[3] = packet.ReadBit();
                goTransportGuid[6] = packet.ReadBit();
                goTransportGuid[1] = packet.ReadBit();
                goTransportGuid[4] = packet.ReadBit();
                goTransportGuid[2] = packet.ReadBit();
                hasGOTransportTime2 = packet.ReadBit();
                goTransportGuid[7] = packet.ReadBit();
            }

            if (hasAttackingTarget)
                attackingTargetGuid = packet.StartBitStream(2, 7, 0, 4, 5, 6, 1, 3);

            if (hasAnimKits)
            {
                hasAnimKit1 = !packet.ReadBit();
                hasAnimKit2 = !packet.ReadBit();
                hasAnimKit3 = !packet.ReadBit();
            }

            packet.ResetBitReader();

            // Reading data
            packet.StoreBeginList("Unk datas", index);
            for (var i = 0u; i < unkLoopCounter; ++i)
                packet.ReadUInt32("Unk UInt32", index, (int)i);
            packet.StoreEndList();

            if (living)
            {
                packet.ReadXORByte(guid2, 4);

                packet.ReadSingle("RunBack Speed", index);
                if (hasFallData)
                {
                    if (hasFallDirection)
                    {
                        packet.ReadSingle("Jump Cos", index);
                        packet.ReadSingle("Jump Velocity", index);
                        packet.ReadSingle("Jump Sin", index);
                    }

                    packet.ReadInt32("Time Fallen", index);
                    packet.ReadSingle("Fall Start Velocity", index);
                }

                packet.ReadSingle("SwimBack Speed", index);
                if (hasSplineElevation)
                    packet.ReadSingle("Spline Elevation", index);

                if (moveInfo.HasSplineData)
                {
                    if (bit216)
                    {
                        if (hasSplineVerticalAcceleration)
                            packet.ReadSingle("Spline Vertical Acceleration", index);
                        packet.ReadUInt32("Spline Time", index);
                        if (splineType == SplineType.FacingAngle)
                            packet.ReadSingle("Facing Angle", index);
                        else if (splineType == SplineType.FacingTarget)
                        {
                            packet.ParseBitStream(facingTargetGuid, 5, 3, 7, 1, 6, 4, 2, 0);
                            packet.StoreBitstreamGuid("Facing Target GUID", facingTargetGuid, index);
                        }

                        packet.StoreBeginList("Spline Waypoints", index);
                        for (var i = 0; i < splineCount; ++i)
                        {
                            var wp = new Vector3
                            {
                                Z = packet.ReadSingle(),
                                X = packet.ReadSingle(),
                                Y = packet.ReadSingle(),
                            };

                            packet.Store("Spline Waypoint", wp, index, i);
                        }
                        packet.StoreEndList();

                        if (splineType == SplineType.FacingSpot)
                        {
                            var point = new Vector3
                            {
                                X = packet.ReadSingle(),
                                Z = packet.ReadSingle(),
                                Y = packet.ReadSingle(),
                            };

                            packet.Store("Facing Spot", point, index);
                        }

                        packet.ReadSingle("Spline Duration Multiplier Next", index);
                        packet.ReadUInt32("Spline Full Time", index);
                        if (hasSplineStartTime)
                            packet.ReadUInt32("Spline Start time", index);

                        packet.ReadSingle("Spline Duration Multiplier", index);
                    }

                    var endPoint = new Vector3
                    {
                        Z = packet.ReadSingle(),
                        X = packet.ReadSingle(),
                        Y = packet.ReadSingle(),
                    };

                    packet.ReadUInt32("Spline Id", index);
                    packet.Store("Spline Endpoint", endPoint, index);
                }

                moveInfo.Position.Z = packet.ReadSingle();
                packet.ReadXORByte(guid2, 5);

                if (hasTransportData)
                {
                    packet.ReadXORByte(transportGuid, 5);
                    packet.ReadXORByte(transportGuid, 7);

                    packet.ReadUInt32("Transport Time", index);
                    moveInfo.TransportOffset.O = packet.ReadSingle();
                    if (hasTransportTime2)
                        packet.ReadUInt32("Transport Time 2", index);

                    moveInfo.TransportOffset.Y = packet.ReadSingle();
                    moveInfo.TransportOffset.X = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 3);

                    moveInfo.TransportOffset.Z = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 0);

                    if (hasTransportTime3)
                        packet.ReadUInt32("Transport Time 3", index);

                    packet.ReadSByte("Transport Seat", index);
                    packet.ReadXORByte(transportGuid, 1);
                    packet.ReadXORByte(transportGuid, 6);
                    packet.ReadXORByte(transportGuid, 2);
                    packet.ReadXORByte(transportGuid, 4);
                    moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(transportGuid, 0));
                    packet.Store("Transport GUID", moveInfo.TransportGuid, index);
                    packet.Store("Transport Position", moveInfo.TransportOffset, index);
                }

                moveInfo.Position.X = packet.ReadSingle();
                packet.ReadSingle("Pitch Speed", index);
                packet.ReadXORByte(guid2, 3);
                packet.ReadXORByte(guid2, 0);

                packet.ReadSingle("Swim Speed", index);
                moveInfo.Position.Y = packet.ReadSingle();
                packet.ReadXORByte(guid2, 7);
                packet.ReadXORByte(guid2, 1);
                packet.ReadXORByte(guid2, 2);

                moveInfo.WalkSpeed = packet.ReadSingle("Walk Speed", index) / 2.5f;
                if (hasTimestamp)
                    packet.ReadUInt32("Time", index);

                packet.ReadSingle("FlyBack Speed", index);
                packet.ReadXORByte(guid2, 6);

                packet.ReadSingle("Turn Speed", index);
                if (hasOrientation)
                    moveInfo.Orientation = packet.ReadSingle();

                moveInfo.RunSpeed = packet.ReadSingle("Run Speed", index) / 7.0f;
                if (hasPitch)
                    packet.ReadSingle("Pitch", index);

                packet.ReadSingle("Fly Speed", index);

                packet.StoreBitstreamGuid("GUID 2", guid2, index);
                packet.Store("Position", moveInfo.Position, index);
                packet.Store("Orientation", moveInfo.Orientation, index);
            }

            if (hasVehicleData)
            {
                packet.ReadSingle("Vehicle Orientation", index);
                moveInfo.VehicleId = packet.ReadUInt32("Vehicle Id", index);
            }

            if (hasGameObjectPosition)
            {
                packet.ReadXORByte(goTransportGuid, 0);
                packet.ReadXORByte(goTransportGuid, 5);
                if (hasGOTransportTime3)
                    packet.ReadUInt32("GO Transport Time 3", index);

                packet.ReadXORByte(goTransportGuid, 3);

                moveInfo.TransportOffset.X = packet.ReadSingle();
                packet.ReadXORByte(goTransportGuid, 4);
                packet.ReadXORByte(goTransportGuid, 6);
                packet.ReadXORByte(goTransportGuid, 1);

                packet.ReadSingle("GO Transport Time", index);
                moveInfo.TransportOffset.Y = packet.ReadSingle();
                packet.ReadXORByte(goTransportGuid, 2);
                packet.ReadXORByte(goTransportGuid, 7);

                moveInfo.TransportOffset.Z = packet.ReadSingle();
                packet.ReadByte("GO Transport Seat", index);
                moveInfo.TransportOffset.O = packet.ReadSingle();
                if (hasGOTransportTime2)
                    packet.ReadUInt32("GO Transport Time 2", index);

                moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(goTransportGuid, 0));
                packet.Store("GO Transport GUID", moveInfo.TransportGuid, index);
                packet.Store("GO Transport Position", moveInfo.TransportOffset, index);
            }

            if (hasGameObjectRotation)
                packet.ReadPackedQuaternion("GameObject Rotation", index);

            if (bit456)
            {
                // float[] arr = new float[16];
                // ordering: 13, 4, 7, 15, BYTE, 10, 11, 3, 5, 14, 6, 1, 8, 12, 0, 2, 9
                packet.ReadBytes(4 * 16 + 1);
            }

            if (hasStationaryPosition)
            {
                moveInfo.Orientation = packet.ReadSingle("Stationary Orientation", index);
                moveInfo.Position = packet.ReadVector3("Stationary Position", index);
            }

            if (hasAttackingTarget)
            {
                packet.ParseBitStream(attackingTargetGuid, 4, 0, 3, 5, 7, 6, 2, 1);
                packet.StoreBitstreamGuid("Attacking Target GUID", attackingTargetGuid, index);
            }

            if (hasAnimKits)
            {
                if (hasAnimKit1)
                    packet.ReadUInt16("Anim Kit 1", index);
                if (hasAnimKit2)
                    packet.ReadUInt16("Anim Kit 2", index);
                if (hasAnimKit3)
                    packet.ReadUInt16("Anim Kit 3", index);
            }

            if (transport)
                packet.ReadUInt32("Transport path timer", index);

            return moveInfo;
        }

        private static MovementInfo ReadMovementUpdateBlock433(ref Packet packet, Guid guid, int index)
        {
            var moveInfo = new MovementInfo();

            bool living = packet.ReadBit("Living", index);
            bool hasAttackingTarget = packet.ReadBit("Has Attacking Target", index);
            bool hasVehicleData = packet.ReadBit("Has Vehicle Data", index);
            uint unkLoopCounter = packet.ReadBits(24);
            bool hasStationaryPosition = packet.ReadBit("Has Stationary Position", index);
            /*bool bit1 =*/ packet.ReadBit();
            /*bool bit4 =*/ packet.ReadBit();
            bool unkInt = packet.ReadBit();
            bool unkFloats = packet.ReadBit();
            /*bool bit2 =*/ packet.ReadBit();
            /*bool bit0 =*/ packet.ReadBit();
            /*bool bit3 =*/ packet.ReadBit();
            bool hasGameObjectPosition = packet.ReadBit("Has GameObject Position", index);
            bool hasAnimKits = packet.ReadBit("Has AnimKits", index);
            bool hasGORotation = packet.ReadBit("Has GameObject Rotation", index);
            bool unkFloat1 = false;
            bool hasFallData = false;
            bool unkFloat2 = false;
            bool bit216 = false;
            bool bit256 = false;
            bool hasSplineDurationMult = false;
            SplineType splineType = SplineType.Normal;
            var facingTarget = new byte[8];
            uint splineCount = 0u;
            bool hasTransportData = false;
            var transportGuid = new byte[8];
            bool hasTransportTime2 = false;
            bool hasTransportTime3 = false;
            bool hasFallDirection = false;
            bool hasUnkUInt = false;
            bool hasOrientation = false;
            var attackingTarget = new byte[8];
            var goTransportGuid = new byte[8];
            bool hasGOTransportTime2 = false;
            bool hasGOTransportTime3 = false;
            bool hasAnimKit1 = false;
            bool hasAnimKit2 = false;
            bool hasAnimKit3 = false;
            var guid2 = new byte[8];

            // Reading bits
            if (living)
            {
                guid2[4] = packet.ReadBit();
                /*bool bit149 =*/ packet.ReadBit();
                guid2[5] = packet.ReadBit();
                unkFloat1 = !packet.ReadBit();
                hasFallData = packet.ReadBit("Has Fall Data", index);
                unkFloat2 = !packet.ReadBit();
                guid2[6] = packet.ReadBit();
                moveInfo.HasSplineData = packet.ReadBit("Has Spline Data", index);
                if (moveInfo.HasSplineData)
                {
                    bit216 = packet.ReadBit();
                    if (bit216)
                    {
                        bit256 = packet.ReadBit();
                        /*splineMode =*/ packet.ReadBits(2);
                        hasSplineDurationMult = packet.ReadBit();
                        uint bits57 = packet.ReadBits(2);
                        switch (bits57)
                        {
                            case 0:
                                splineType = SplineType.FacingSpot;
                                break;
                            case 1:
                                splineType = SplineType.Normal;
                                break;
                            case 2:
                                splineType = SplineType.FacingTarget;
                                break;
                            case 3:
                                splineType = SplineType.FacingAngle;
                                break;
                        }

                        if (splineType == SplineType.FacingTarget)
                            facingTarget = packet.StartBitStream(0, 2, 7, 1, 6, 3, 4, 5);

                        /*splineFlags =*/ packet.ReadEnum<SplineFlag422>("Spline Flags", 25, index);
                        splineCount = packet.ReadBits(22);
                    }
                }

                hasTransportData = packet.ReadBit("Has Transport Data", index);
                guid2[1] = packet.ReadBit();
                /*bit148 =*/ packet.ReadBit();
                if (hasTransportData)
                {
                    hasTransportTime2 = packet.ReadBit();
                    transportGuid = packet.StartBitStream(0, 7, 2, 6, 5, 4, 1, 3);
                    hasTransportTime3 = packet.ReadBit();
                }

                guid2[2] = packet.ReadBit();
                if (hasFallData)
                    hasFallDirection = packet.ReadBit("Has Fall Direction", index);

                bool hasMovementFlags = !packet.ReadBit();
                bool hasExtraMovementFlags = !packet.ReadBit();
                hasUnkUInt = !packet.ReadBit();
                guid2[7] = packet.ReadBit();
                if (hasExtraMovementFlags)
                    moveInfo.FlagsExtra = packet.ReadEnum<MovementFlagExtra>("Extra Movement Flags", 12, index);

                guid2[0] = packet.ReadBit();
                if (hasMovementFlags)
                    moveInfo.Flags = packet.ReadEnum<MovementFlag>("Movement Flags", 30, index);

                guid2[3] = packet.ReadBit();
                hasOrientation = !packet.ReadBit();
            }

            if (hasAttackingTarget)
                attackingTarget = packet.StartBitStream(2, 4, 0, 1, 3, 7, 5, 6);

            if (hasGameObjectPosition)
            {
                hasGOTransportTime2 = packet.ReadBit();
                goTransportGuid[1] = packet.ReadBit();
                goTransportGuid[4] = packet.ReadBit();
                goTransportGuid[5] = packet.ReadBit();
                goTransportGuid[0] = packet.ReadBit();
                goTransportGuid[6] = packet.ReadBit();
                goTransportGuid[7] = packet.ReadBit();
                goTransportGuid[3] = packet.ReadBit();
                hasGOTransportTime3 = packet.ReadBit();
                goTransportGuid[2] = packet.ReadBit();
            }

            if (hasAnimKits)
            {
                hasAnimKit3 = !packet.ReadBit();
                hasAnimKit1 = !packet.ReadBit();
                hasAnimKit2 = !packet.ReadBit();
            }

            // Reading data
            packet.StoreBeginList("UnkInts", index);
            for (var i = 0u; i < unkLoopCounter; ++i)
                packet.ReadUInt32("Unk UInt32", index, (int)i);
            packet.StoreEndList();

            if (living)
            {
                moveInfo.WalkSpeed = packet.ReadSingle("Walk Speed", index) / 2.5f;
                if (moveInfo.HasSplineData)
                {
                    if (bit216)
                    {
                        packet.StoreBeginList("Spline Waypoints", index);
                        for (var i = 0; i < splineCount; ++i)
                        {
                            var wp = new Vector3
                            {
                                X = packet.ReadSingle(),
                                Z = packet.ReadSingle(),
                                Y = packet.ReadSingle(),
                            };

                            packet.Store("Spline Waypoint", wp, index, i);
                        }
                        packet.StoreEndList();

                        if (splineType == SplineType.FacingTarget)
                        {
                            packet.ParseBitStream(facingTarget, 0, 6, 5, 4, 1, 3, 7, 2);
                            packet.StoreBitstreamGuid("Facing Target GUID", facingTarget, index);
                        }
                        else if (splineType == SplineType.FacingSpot)
                        {
                            var point = new Vector3
                            {
                                Z = packet.ReadSingle(),
                                Y = packet.ReadSingle(),
                                X = packet.ReadSingle(),
                            };

                            packet.Store("Facing Spot", point, index);
                        }

                        packet.ReadUInt32("Unknown Spline Int32 2", index);
                        if (bit256)
                            packet.ReadUInt32("Unknown Spline Int32 3", index);

                        packet.ReadSingle("Unknown Spline Float 2", index);
                        packet.ReadSingle("Unknown Spline Float 1", index);
                        packet.ReadUInt32("Unknown Spline Int32 1", index);
                        if (splineType == SplineType.FacingAngle)
                            packet.ReadSingle("Facing Angle", index);

                        if (hasSplineDurationMult)
                            packet.ReadSingle("Spline Duration Modifier", index);
                    }

                    var endPoint = new Vector3
                    {
                        Z = packet.ReadSingle(),
                        Y = packet.ReadSingle(),
                    };

                    packet.ReadUInt32("Spline Full Time", index);
                    endPoint.X = packet.ReadSingle();
                    packet.Store("Spline Endpoint", endPoint, index);
                }

                if (hasTransportData)
                {
                    if (hasTransportTime2)
                        packet.ReadInt32("Transport Time 2", index);

                    packet.ReadXORByte(transportGuid, 4);
                    packet.ReadXORByte(transportGuid, 6);
                    packet.ReadXORByte(transportGuid, 5);

                    if (hasTransportTime3)
                        packet.ReadInt32("Transport Time 3", index);

                    packet.ReadXORByte(transportGuid, 7);
                    packet.ReadXORByte(transportGuid, 3);

                    moveInfo.TransportOffset = new Vector4
                    {
                        X = packet.ReadSingle(),
                        Z = packet.ReadSingle(),
                        O = packet.ReadSingle(),
                    };

                    packet.ReadXORByte(transportGuid, 2);
                    packet.ReadXORByte(transportGuid, 1);
                    packet.ReadXORByte(transportGuid, 0);

                    moveInfo.TransportOffset.Y = packet.ReadSingle();
                    moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(transportGuid, 0));
                    packet.Store("Transport GUID", moveInfo.TransportGuid, index);
                    packet.Store("Transport Position", moveInfo.TransportOffset, index);
                    packet.ReadByte("Transport Seat", index);
                    packet.ReadInt32("Transport Time", index);
                }

                if (unkFloat1)
                    packet.ReadSingle("float +28", index);

                packet.ReadSingle("FlyBack Speed", index);
                packet.ReadSingle("Turn Speed", index);
                packet.ReadXORByte(guid2, 5);

                moveInfo.RunSpeed = packet.ReadSingle("Run Speed", index) / 7.0f;
                if (unkFloat2)
                    packet.ReadSingle("float +36", index);

                packet.ReadXORByte(guid2, 0);

                packet.ReadSingle("Pitch Speed", index);
                if (hasFallData)
                {
                    packet.ReadInt32("Time Fallen", index);
                    packet.ReadSingle("Fall Start Velocity", index);
                    if (hasFallDirection)
                    {
                        packet.ReadSingle("Jump Sin", index);
                        packet.ReadSingle("Jump Velocity", index);
                        packet.ReadSingle("Jump Cos", index);
                    }
                }

                packet.ReadSingle("RunBack Speed", index);
                moveInfo.Position = new Vector3();
                moveInfo.Position.X = packet.ReadSingle();
                packet.ReadSingle("SwimBack Speed", index);
                packet.ReadXORByte(guid2, 7);

                moveInfo.Position.Z = packet.ReadSingle();
                packet.ReadXORByte(guid2, 3);
                packet.ReadXORByte(guid2, 2);

                packet.ReadSingle("Fly Speed", index);
                packet.ReadSingle("Swim Speed", index);
                packet.ReadXORByte(guid2, 1);
                packet.ReadXORByte(guid2, 4);
                packet.ReadXORByte(guid2, 6);

                packet.StoreBitstreamGuid("GUID 2", guid2, index);
                moveInfo.Position.Y = packet.ReadSingle();
                if (hasUnkUInt)
                    packet.ReadUInt32();

                if (hasOrientation)
                    moveInfo.Orientation = packet.ReadSingle();

                packet.Store("Position", moveInfo.Position, index);
                packet.Store("Orientation", moveInfo.Orientation, index);
            }

            if (unkFloats)
            {
                int i;
                packet.StoreBeginList("UnkFloats1", index);
                for (i = 0; i < 13; ++i)
                    packet.ReadSingle("Unk float 456", index, i);
                packet.StoreEndList();

                packet.ReadByte("Unk byte 456", index);

                packet.StoreBeginList("UnkFloats2", index);
                for (; i < 16; ++i)
                    packet.ReadSingle("Unk float 456", index, i);
                packet.StoreEndList();
            }

            if (hasGameObjectPosition)
            {
                packet.ReadXORByte(goTransportGuid, 6);
                packet.ReadXORByte(goTransportGuid, 5);

                moveInfo.TransportOffset.Y = packet.ReadSingle();
                packet.ReadXORByte(goTransportGuid, 4);
                packet.ReadXORByte(goTransportGuid, 2);
                if (hasGOTransportTime3)
                    packet.ReadUInt32("GO Transport Time 3", index);

                moveInfo.TransportOffset.O = packet.ReadSingle();
                moveInfo.TransportOffset.Z = packet.ReadSingle();
                if (hasGOTransportTime2)
                    packet.ReadUInt32("GO Transport Time 2", index);

                packet.ReadByte("GO Transport Seat", index);
                packet.ReadXORByte(goTransportGuid, 7);
                packet.ReadXORByte(goTransportGuid, 1);
                packet.ReadXORByte(goTransportGuid, 0);
                packet.ReadXORByte(goTransportGuid, 3);

                moveInfo.TransportOffset.X = packet.ReadSingle();
                moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(goTransportGuid, 0));
                packet.ReadSingle("GO Transport Time", index);
                packet.Store("GO Transport Position", moveInfo.TransportOffset, index);
                packet.Store("GO Transport GUID", moveInfo.TransportGuid, index);
            }

            if (hasAttackingTarget)
            {
                packet.ParseBitStream(attackingTarget, 2, 4, 7, 3, 0, 1, 5, 6);
                packet.StoreBitstreamGuid("Attacking Target GUID", attackingTarget, index);
            }

            if (hasGORotation)
                moveInfo.Rotation = packet.ReadPackedQuaternion("GO Rotation", index);

            if (unkInt)
                packet.ReadUInt32("uint32 +412", index);

            if (hasAnimKits)
            {
                if (hasAnimKit3)
                    packet.ReadUInt16("Anim Kit 3", index);
                if (hasAnimKit1)
                    packet.ReadUInt16("Anim Kit 1", index);
                if (hasAnimKit2)
                    packet.ReadUInt16("Anim Kit 2", index);
            }

            if (hasStationaryPosition)
            {
                moveInfo.Position = new Vector3
                {
                    Z = packet.ReadSingle(),
                    X = packet.ReadSingle(),
                    Y = packet.ReadSingle(),
                };

                moveInfo.Orientation = packet.ReadSingle();
                packet.Store("Stationary Position", moveInfo.Position, index);
                packet.Store("Stationary Orientation", moveInfo.Orientation, index);
            }

            if (hasVehicleData)
            {
                packet.ReadSingle("Vehicle Orientation", index);
                moveInfo.VehicleId = packet.ReadUInt32("Vehicle Id", index);
            }

            packet.ResetBitReader();
            return moveInfo;
        }

        private static MovementInfo ReadMovementUpdateBlock432(ref Packet packet, Guid guid, int index)
        {
            var moveInfo = new MovementInfo();

            /*bool bit2 = */packet.ReadBit();
            /*bool bit3 = */packet.ReadBit();
            /*bool bit4 = */packet.ReadBit();
            var hasStationaryPosition = packet.ReadBit("Has Stationary Position", index);
            var hasAnimKits = packet.ReadBit("Has AnimKits", index);
            var unkLoopCounter = packet.ReadBits(24);
            /*bool bit1 = */packet.ReadBit();
            bool hasTransportExtra = packet.ReadBit("Has Transport Extra", index);
            bool hasGORotation = packet.ReadBit("Has GameObject Rotation", index);
            bool living = packet.ReadBit("Living", index);
            bool hasGameObjectPosition = packet.ReadBit("Has GameObject Position", index);
            bool hasVehicleData = packet.ReadBit("Has Vehicle Data", index);
            bool hasAttackingTarget = packet.ReadBit("Has Attacking Target", index);
            /*bool bit0 =*/packet.ReadBit();
            bool unkFloats = packet.ReadBit();

            bool unkFloat1 = false;
            bool hasFallData = false;
            bool unkFloat2 = false;
            bool bit216 = false;
            bool bit256 = false;
            bool hasSplineDurationMult = false;
            SplineType splineType = SplineType.Normal;
            var facingTarget = new byte[8];
            uint splineCount = 0u;
            bool hasTransportData = false;
            var transportGuid = new byte[8];
            bool hasTransportTime2 = false;
            bool hasTransportTime3 = false;
            bool hasFallDirection = false;
            bool hasUnkUInt = false;
            bool hasOrientation = false;
            var attackingTarget = new byte[8];
            var goTransportGuid = new byte[8];
            bool hasGOTransportTime2 = false;
            bool hasGOTransportTime3 = false;
            bool hasAnimKit1 = false;
            bool hasAnimKit2 = false;
            bool hasAnimKit3 = false;
            var guid2 = new byte[8];

            if (living)
            {
                unkFloat1 = !packet.ReadBit();
                hasOrientation = !packet.ReadBit();
                bool hasExtraMovementFlags = !packet.ReadBit();
                hasFallData = packet.ReadBit("Has Fall Data", index);
                guid2[0] = packet.ReadBit();
                guid2[5] = packet.ReadBit();
                guid2[4] = packet.ReadBit();
                bool hasMovementFlags = !packet.ReadBit();
                moveInfo.HasSplineData = packet.ReadBit("Has Spline Data", index);
                /*bool bit148 = */packet.ReadBit();

                if (hasExtraMovementFlags)
                    moveInfo.FlagsExtra = packet.ReadEnum<MovementFlagExtra>("Extra Movement Flags", 12, index);

                hasUnkUInt = !packet.ReadBit();
                guid2[3] = packet.ReadBit();
                /*bool bit149 = */packet.ReadBit();

                if (hasMovementFlags)
                    moveInfo.Flags = packet.ReadEnum<MovementFlag>("Movement Flags", 30, index);

                guid2[1] = packet.ReadBit();
                unkFloat2 = !packet.ReadBit();
                hasTransportData = packet.ReadBit("Has Transport Data", index);
                guid2[2] = packet.ReadBit();

                if (hasTransportData)
                {
                    transportGuid[3] = packet.ReadBit();
                    transportGuid[5] = packet.ReadBit();
                    transportGuid[1] = packet.ReadBit();
                    transportGuid[7] = packet.ReadBit();
                    hasTransportTime2 = packet.ReadBit();
                    transportGuid[4] = packet.ReadBit();
                    transportGuid[0] = packet.ReadBit();
                    transportGuid[2] = packet.ReadBit();
                    transportGuid[6] = packet.ReadBit();
                    hasTransportTime3 = packet.ReadBit();
                }

                if (moveInfo.HasSplineData)
                {
                    bit216 = packet.ReadBit();
                    if (bit216)
                    {
                        uint bits57 = packet.ReadBits(2);
                        splineCount = packet.ReadBits(22);
                        switch (bits57)
                        {
                            case 0:
                                splineType = SplineType.FacingTarget;
                                break;
                            case 1:
                                splineType = SplineType.FacingSpot;
                                break;
                            case 2:
                                splineType = SplineType.Normal;
                                break;
                            case 3:
                                splineType = SplineType.FacingAngle;
                                break;
                        }

                        if (splineType == SplineType.FacingTarget)
                            facingTarget = packet.StartBitStream(4, 3, 2, 5, 7, 1, 0, 6);

                        packet.ReadEnum<SplineFlag422>("Spline flags", 25, index);
                        /*splineMode =*/packet.ReadBits(2);
                        hasSplineDurationMult = packet.ReadBit("HasSplineDurationMult", index);
                        bit256 = packet.ReadBit();
                    }
                }

                if (hasFallData)
                    hasFallDirection = packet.ReadBit("Has Fall Direction", index);

                guid2[6] = packet.ReadBit();
                guid2[7] = packet.ReadBit();
            }

            if (hasGameObjectPosition)
            {
                goTransportGuid[5] = packet.ReadBit();
                goTransportGuid[4] = packet.ReadBit();
                hasGOTransportTime3 = packet.ReadBit();
                goTransportGuid[7] = packet.ReadBit();
                goTransportGuid[6] = packet.ReadBit();
                goTransportGuid[1] = packet.ReadBit();
                goTransportGuid[2] = packet.ReadBit();
                hasGOTransportTime2 = packet.ReadBit();
                goTransportGuid[0] = packet.ReadBit();
                goTransportGuid[3] = packet.ReadBit();
            }

            if (hasAnimKits)
            {
                hasAnimKit1 = !packet.ReadBit();
                hasAnimKit3 = !packet.ReadBit();
                hasAnimKit2 = !packet.ReadBit();
            }

            if (hasAttackingTarget)
                attackingTarget = packet.StartBitStream(4, 3, 2, 5, 0, 6, 1, 7);

            packet.StoreBeginList("UnkInts", index);
            for (var i = 0; i < unkLoopCounter; ++i)
            {
                packet.ReadInt32("UnkInt", index, (int)i);
            }
            packet.StoreEndList();

            if (hasGameObjectPosition)
            {
                if (hasGOTransportTime3)
                    packet.ReadInt32("GO Transport Time 3", index);

                packet.ReadXORByte(goTransportGuid, 7);

                moveInfo.TransportOffset.Y = packet.ReadSingle();
                packet.ReadByte("GO Transport Seat", index);
                moveInfo.TransportOffset.O = packet.ReadSingle();
                moveInfo.TransportOffset.Z = packet.ReadSingle();

                packet.ReadXORByte(goTransportGuid, 4);
                packet.ReadXORByte(goTransportGuid, 5);
                packet.ReadXORByte(goTransportGuid, 6);

                moveInfo.TransportOffset.X = packet.ReadSingle();
                packet.ReadInt32("GO Transport Time", index);

                packet.ReadXORByte(goTransportGuid, 1);

                if (hasGOTransportTime2)
                    packet.ReadInt32("GO Transport Time 2", index);

                packet.ReadXORByte(goTransportGuid, 0);
                packet.ReadXORByte(goTransportGuid, 2);
                packet.ReadXORByte(goTransportGuid, 3);

                moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(goTransportGuid, 0));
                packet.Store("GO Transport Position", moveInfo.TransportOffset, index);
                packet.Store("GO Transport GUID", moveInfo.TransportGuid, index);
            }

            if (living)
            {
                if (moveInfo.HasSplineData)
                {
                    if (bit216)
                    {
                        packet.ReadSingle("Unknown Spline Float 2", index);
                        packet.StoreBeginList("Spline Waypoints", index);
                        for (var i = 0; i < splineCount; ++i)
                        {
                            var wp = new Vector3
                            {
                                Y = packet.ReadSingle(),
                                Z = packet.ReadSingle(),
                                X = packet.ReadSingle(),
                            };

                            packet.Store("Spline Waypoint", wp, index, i);
                        }
                        packet.StoreEndList();

                        if (splineType == SplineType.FacingTarget)
                        {
                            packet.ParseBitStream(facingTarget, 2, 1, 3, 7, 0, 5, 4, 6);
                            packet.StoreBitstreamGuid("Facing Target GUID", facingTarget, index);
                        }
                        else if (splineType == SplineType.FacingSpot)
                        {
                            var point = new Vector3
                            {
                                Y = packet.ReadSingle(),
                                Z = packet.ReadSingle(),
                                X = packet.ReadSingle(),
                            };

                            packet.Store("Facing Spot", point, index);
                        }

                        if (hasSplineDurationMult)
                            packet.ReadSingle("Spline Duration Modifier", index);

                        if (bit256)
                            packet.ReadUInt32("Unknown Spline Int32 1", index);

                        packet.ReadUInt32("Unknown Spline Int32 2", index);
                        packet.ReadSingle("Unknown Spline Float 1", index);
                        if (splineType == SplineType.FacingAngle)
                            packet.ReadSingle("Facing Angle", index);

                        packet.ReadUInt32("Unknown Spline Int32 3", index);
                    }

                    packet.ReadUInt32("Spline Full Time", index);
                    var endPoint = new Vector3
                    {
                        Z = packet.ReadSingle(),
                        Y = packet.ReadSingle(),
                        X = packet.ReadSingle(),
                    };

                    packet.Store("Spline Endpoint", endPoint, index);
                }

                if (hasTransportData)
                {
                    packet.ReadXORByte(transportGuid, 6);
                    if (hasTransportTime2)
                        packet.ReadInt32("Transport Time 2", index);

                    packet.ReadByte("Transport Seat", index);
                    moveInfo.TransportOffset.O = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 7);
                    moveInfo.TransportOffset.Y = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 3);
                    if (hasTransportTime3)
                        packet.ReadInt32("Transport Time 3", index);

                    packet.ReadInt32("Transport Time", index);
                    packet.ReadXORByte(transportGuid, 0);
                    packet.ReadXORByte(transportGuid, 1);
                    moveInfo.TransportOffset.X = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 4);
                    moveInfo.TransportOffset.Z = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 5);
                    packet.ReadXORByte(transportGuid, 2);

                    moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(transportGuid, 0));
                    packet.Store("Transport GUID", moveInfo.TransportGuid, index);
                    packet.Store("Transport Position", moveInfo.TransportOffset, index);
                }

                moveInfo.Position = new Vector3();
                moveInfo.Position.Z = packet.ReadSingle();
                packet.ReadSingle("FlyBack Speed", index);
                moveInfo.Position.Y = packet.ReadSingle();
                packet.ReadXORByte(guid2, 4);
                packet.ReadXORByte(guid2, 0);
                moveInfo.Position.X = packet.ReadSingle();
                if (hasFallData)
                {
                    packet.ReadInt32("Time Fallen", index);
                    if (hasFallDirection)
                    {
                        packet.ReadSingle("Jump Sin", index);
                        packet.ReadSingle("Jump Velocity", index);
                        packet.ReadSingle("Jump Cos", index);
                    }
                    packet.ReadSingle("Fall Start Velocity", index);
                }

                if (hasOrientation)
                    moveInfo.Orientation = packet.ReadSingle();

                packet.Store("Position", moveInfo.Position, index);
                packet.Store("Orientation", moveInfo.Orientation, index);
                packet.ReadSingle("Swim Speed", index);
                moveInfo.RunSpeed = packet.ReadSingle("Run Speed", index) / 7.0f;
                packet.ReadSingle("Fly Speed", index);
                packet.ReadXORByte(guid2, 2);
                if (unkFloat2)
                    packet.ReadSingle("Unk float +36", index);

                if (unkFloat1)
                    packet.ReadSingle("Unk float +28", index);

                packet.ReadXORByte(guid2, 3);
                packet.ReadSingle("RunBack Speed", index);
                packet.ReadXORByte(guid2, 6);
                packet.ReadSingle("Pitch Speed", index);
                packet.ReadXORByte(guid2, 7);
                packet.ReadXORByte(guid2, 5);
                packet.ReadSingle("Turn Speed", index);
                packet.ReadSingle("SwimBack Speed", index);
                packet.ReadXORByte(guid2, 1);
                packet.StoreBitstreamGuid("GUID 2", guid2, index);
                if (hasUnkUInt)
                    packet.ReadInt32();

                moveInfo.WalkSpeed = packet.ReadSingle("Walk Speed", index) / 2.5f;
            }

            if (hasAttackingTarget)
            {
                packet.ParseBitStream(attackingTarget, 6, 5, 3, 2, 0, 1, 7, 4);
                packet.StoreBitstreamGuid("Attacking Target GUID", attackingTarget, index);
            }

            if (unkFloats)
            {
                int i;
                packet.StoreBeginList("UnkFloats1", index);
                for (i = 0; i < 13; ++i)
                    packet.ReadSingle("Unk float 456", index, i);
                packet.StoreEndList();

                packet.ReadByte("Unk byte 456", index);

                packet.StoreBeginList("UnkFloats2", index);
                for (; i < 16; ++i)
                    packet.ReadSingle("Unk float 456", index, i);
                packet.StoreEndList();
            }

            if (hasVehicleData)
            {
                packet.ReadSingle("Vehicle Orientation", index);
                moveInfo.VehicleId = packet.ReadUInt32("Vehicle Id", index);
            }

            if (hasGORotation)
                moveInfo.Rotation = packet.ReadPackedQuaternion("GO Rotation", index);

            if (hasStationaryPosition)
            {
                moveInfo.Position = new Vector3
                {
                    X = packet.ReadSingle(),
                    Z = packet.ReadSingle(),
                    Y = packet.ReadSingle(),
                };

                moveInfo.Orientation = packet.ReadSingle();
                packet.Store("Stationary Position", moveInfo.Position, index);
                packet.Store("Stationary Orientation", moveInfo.Orientation, index);
            }

            if (hasAnimKits)
            {
                if (hasAnimKit3)
                    packet.ReadUInt16("Anim Kit 3", index);
                if (hasAnimKit1)
                    packet.ReadUInt16("Anim Kit 1", index);
                if (hasAnimKit2)
                    packet.ReadUInt16("Anim Kit 2", index);
            }

            if (hasTransportExtra)
                packet.ReadInt32("Transport Time", index);

            packet.ResetBitReader();
            return moveInfo;
        }

        private static MovementInfo ReadMovementUpdateBlock430(ref Packet packet, Guid guid, int index)
        {
            var moveInfo = new MovementInfo();
            bool hasAttackingTarget = packet.ReadBit("Has Attacking Target", index);
            /*bool bit2 = */packet.ReadBit();
            bool hasVehicleData = packet.ReadBit("Has Vehicle Data", index);
            /*bool bit1 = */packet.ReadBit();
            /*bool bit4 = */packet.ReadBit();
            /*bool bit3 = */packet.ReadBit();
            bool hasTransportExtra = packet.ReadBit("Has Transport Extra", index);
            bool hasGameObjectPosition = packet.ReadBit("Has GameObject Position", index);
            bool unkFloats = packet.ReadBit();
            bool hasAnimKits = packet.ReadBit("Has AnimKits", index);
            bool hasGORotation = packet.ReadBit("Has GameObject Rotation", index);
            bool living = packet.ReadBit("Living", index);
            bool hasStationaryPosition = packet.ReadBit("Has Stationary Position", index);
            uint unkLoopCounter = packet.ReadBits(24);
            /*bool bit0 = */packet.ReadBit();

            bool unkFloat1 = false;
            bool hasFallData = false;
            bool unkFloat2 = false;
            bool bit216 = false;
            bool bit256 = false;
            bool hasSplineDurationMult = false;
            SplineType splineType = SplineType.Normal;
            var facingTarget = new byte[8];
            uint splineCount = 0u;
            bool hasTransportData = false;
            var transportGuid = new byte[8];
            bool hasTransportTime2 = false;
            bool hasTransportTime3 = false;
            bool hasFallDirection = false;
            bool hasUnkUInt = false;
            bool hasOrientation = false;
            var attackingTarget = new byte[8];
            var goTransportGuid = new byte[8];
            bool hasGOTransportTime2 = false;
            bool hasGOTransportTime3 = false;
            bool hasAnimKit1 = false;
            bool hasAnimKit2 = false;
            bool hasAnimKit3 = false;
            var guid2 = new byte[8];

            if (living)
            {
                hasTransportData = packet.ReadBit("Has Transport Data", index);
                if (hasTransportData)
                {
                    transportGuid[2] = packet.ReadBit();
                    transportGuid[7] = packet.ReadBit();
                    transportGuid[5] = packet.ReadBit();
                    hasTransportTime3 = packet.ReadBit();
                    transportGuid[3] = packet.ReadBit();
                    transportGuid[0] = packet.ReadBit();
                    transportGuid[4] = packet.ReadBit();
                    transportGuid[1] = packet.ReadBit();
                    hasTransportTime2 = packet.ReadBit();
                    transportGuid[6] = packet.ReadBit();
                }

                moveInfo.HasSplineData = packet.ReadBit("Has Spline Data", index);
                guid2[7] = packet.ReadBit();
                guid2[6] = packet.ReadBit();
                guid2[5] = packet.ReadBit();
                guid2[2] = packet.ReadBit();
                guid2[4] = packet.ReadBit();
                bool hasMovementFlags = !packet.ReadBit();
                guid2[1] = packet.ReadBit();
                /*bool bit148 = */packet.ReadBit();
                hasUnkUInt = !packet.ReadBit();
                bool hasExtraMovementFlags = !packet.ReadBit();
                if (moveInfo.HasSplineData)
                {
                    bit216 = packet.ReadBit();
                    if (bit216)
                    {
                        bit256 = packet.ReadBit();
                        /*splineFlags = */packet.ReadEnum<SplineFlag422>("Spline flags", 25, index);
                        /*splineMode = */packet.ReadBits(2);
                        hasSplineDurationMult = packet.ReadBit();
                        splineCount = packet.ReadBits(22);
                        uint bits57 = packet.ReadBits(2);
                        switch (bits57)
                        {
                            case 0:
                                splineType = SplineType.FacingSpot;
                                break;
                            case 1:
                                splineType = SplineType.Normal;
                                break;
                            case 2:
                                splineType = SplineType.FacingTarget;
                                break;
                            case 3:
                                splineType = SplineType.FacingAngle;
                                break;
                        }

                        if (splineType == SplineType.FacingTarget)
                            facingTarget = packet.StartBitStream(7, 3, 4, 2, 1, 6, 0, 5);
                    }
                }

                guid2[3] = packet.ReadBit();
                if (hasMovementFlags)
                    moveInfo.Flags = packet.ReadEnum<MovementFlag>("Movement Flags", 30, index);

                unkFloat1 = !packet.ReadBit();
                hasFallData = packet.ReadBit("Has Fall Data", index);
                if (hasExtraMovementFlags)
                    moveInfo.FlagsExtra = packet.ReadEnum<MovementFlagExtra>("Extra Movement Flags", 12, index);

                guid2[0] = packet.ReadBit();
                hasOrientation = !packet.ReadBit();
                if (hasFallData)
                    hasFallDirection = packet.ReadBit("Has Fall Direction", index);

                unkFloat2 = !packet.ReadBit();
            }

            if (hasGameObjectPosition)
            {
                goTransportGuid[1] = packet.ReadBit();
                hasGOTransportTime3 = packet.ReadBit();
                goTransportGuid[3] = packet.ReadBit();
                goTransportGuid[2] = packet.ReadBit();
                goTransportGuid[6] = packet.ReadBit();
                goTransportGuid[5] = packet.ReadBit();
                goTransportGuid[0] = packet.ReadBit();
                goTransportGuid[4] = packet.ReadBit();
                hasGOTransportTime2 = packet.ReadBit();
                goTransportGuid[7] = packet.ReadBit();
            }

            if (hasAnimKits)
            {
                hasAnimKit3 = !packet.ReadBit();
                hasAnimKit1 = !packet.ReadBit();
                hasAnimKit2 = !packet.ReadBit();
            }

            if (hasAttackingTarget)
                attackingTarget = packet.StartBitStream(3, 4, 6, 0, 1, 7, 5, 2);

            // Reading data
            packet.StoreBeginList("Unks", index);
            for (var i = 0u; i < unkLoopCounter; ++i)
                packet.ReadUInt32("Unk UInt32", index, (int)i);
            packet.StoreEndList();

            if (hasStationaryPosition)
            {
                moveInfo.Position = new Vector3();
                moveInfo.Position.Z = packet.ReadSingle();
                moveInfo.Orientation = packet.ReadSingle();
                moveInfo.Position.X = packet.ReadSingle();
                moveInfo.Position.Y = packet.ReadSingle();
                packet.Store("Stationary Position", moveInfo.Position, index);
                packet.Store("Stationary Orientation", moveInfo.Orientation, index);
            }

            if (hasVehicleData)
            {
                moveInfo.VehicleId = packet.ReadUInt32("Vehicle Id", index);
                packet.ReadSingle("Vehicle Orientation", index);
            }

            if (hasGameObjectPosition)
            {
                packet.ReadXORByte(goTransportGuid, 1);
                packet.ReadXORByte(goTransportGuid, 4);
                moveInfo.TransportOffset.Z = packet.ReadSingle();
                if (hasGOTransportTime3)
                    packet.ReadInt32("GO Transport Time 3", index);

                packet.ReadInt32("GO Transport Time", index);
                packet.ReadXORByte(goTransportGuid, 5);
                packet.ReadXORByte(goTransportGuid, 6);
                moveInfo.TransportOffset.X = packet.ReadSingle();
                packet.ReadXORByte(goTransportGuid, 2);
                if (hasGOTransportTime2)
                    packet.ReadInt32("GO Transport Time 2", index);

                packet.ReadByte("GO Transport Seat", index);
                packet.ReadXORByte(goTransportGuid, 3);
                moveInfo.TransportOffset.Y = packet.ReadSingle();
                moveInfo.TransportOffset.O = packet.ReadSingle();
                packet.ReadXORByte(goTransportGuid, 7);
                packet.ReadXORByte(goTransportGuid, 0);

                moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(goTransportGuid, 0));
                packet.Store("GO Transport Position", moveInfo.TransportOffset, index);
                packet.Store("GO Transport GUID", moveInfo.TransportGuid, index);
            }

            if (living)
            {
                if (moveInfo.HasSplineData)
                {
                    if (bit216)
                    {
                        packet.StoreBeginList("Spline Waypoints", index);
                        for (int i = 0; i < splineCount; ++i)
                        {
                            var wp = new Vector3
                            {
                                Y = packet.ReadSingle(),
                                X = packet.ReadSingle(),
                                Z = packet.ReadSingle(),
                            };

                            packet.Store("Spline Waypoint", wp, index, i);
                        }
                        packet.StoreEndList();

                        if (hasSplineDurationMult)
                            packet.ReadSingle("Spline Duration Modifier", index);

                        packet.ReadSingle("Unknown Spline Float 2", index);
                        if (splineType == SplineType.FacingTarget)
                        {
                            packet.ParseBitStream(facingTarget, 3, 4, 5, 7, 2, 0, 6, 1);
                            packet.StoreBitstreamGuid("Facing Target GUID", facingTarget, index);
                        }

                        if (bit256)
                            packet.ReadUInt32("Unknown Spline Int32 3", index);

                        packet.ReadSingle("Unknown Spline Float 1", index);
                        packet.ReadUInt32("Unknown Spline Int32 1", index);
                        if (splineType == SplineType.FacingSpot)
                        {
                            var point = new Vector3
                            {
                                Y = packet.ReadSingle(),
                                Z = packet.ReadSingle(),
                                X = packet.ReadSingle(),
                            };

                            packet.Store("Facing Spot", point, index);
                        }

                        packet.ReadUInt32("Unknown Spline Int32 2", index);
                        if (splineType == SplineType.FacingAngle)
                            packet.ReadSingle("Facing Angle", index);
                    }

                    var endPoint = new Vector3
                    {
                        Z = packet.ReadSingle(),
                        Y = packet.ReadSingle(),
                    };

                    packet.ReadUInt32("Spline Full Time", index);
                    endPoint.X = packet.ReadSingle();
                    packet.Store("Spline Endpoint", endPoint, index);
                }

                packet.ReadSingle("Pitch Speed", index);
                if (hasTransportData)
                {
                    packet.ReadXORByte(transportGuid, 4);
                    moveInfo.TransportOffset.Z = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 7);
                    packet.ReadXORByte(transportGuid, 5);
                    packet.ReadXORByte(transportGuid, 1);
                    moveInfo.TransportOffset.X = packet.ReadSingle();
                    packet.ReadXORByte(transportGuid, 3);
                    packet.ReadXORByte(transportGuid, 6);
                    if (hasTransportTime3)
                        packet.ReadInt32("Transport Time 3", index);

                    moveInfo.TransportOffset.Y = packet.ReadSingle();
                    packet.ReadByte("Transport Seat", index);
                    moveInfo.TransportOffset.O = packet.ReadSingle();
                    if (hasTransportTime2)
                        packet.ReadInt32("Transport Time 2", index);

                    packet.ReadXORByte(transportGuid, 2);
                    packet.ReadInt32("Transport Time", index);
                    packet.ReadXORByte(transportGuid, 0);

                    moveInfo.TransportGuid = new Guid(BitConverter.ToUInt64(transportGuid, 0));
                    packet.Store("Transport GUID", moveInfo.TransportGuid, index);
                    packet.Store("Transport Position", moveInfo.TransportOffset, index);
                }

                packet.ReadSingle("FlyBack Speed", index);
                moveInfo.Position = new Vector3();
                moveInfo.Position.X = packet.ReadSingle();
                if (unkFloat1)
                    packet.ReadSingle("Unk float +28", index);

                if (hasFallData)
                {
                    packet.ReadInt32("Time Fallen", index);
                    if (hasFallDirection)
                    {
                        packet.ReadSingle("Jump Sin", index);
                        packet.ReadSingle("Jump Velocity", index);
                        packet.ReadSingle("Jump Cos", index);
                    }
                    packet.ReadSingle("Fall Start Velocity", index);
                }

                packet.ReadXORByte(guid2, 7);
                packet.ReadSingle("SwimBack Speed", index);
                packet.ReadXORByte(guid2, 0);
                packet.ReadXORByte(guid2, 5);
                if (hasUnkUInt)
                    packet.ReadUInt32();

                moveInfo.Position.Z = packet.ReadSingle();
                packet.ReadSingle("Fly Speed", index);
                packet.ReadXORByte(guid2, 1);
                packet.ReadSingle("RunBack Speed", index);
                packet.ReadSingle("Turn Speed", index);
                packet.ReadSingle("Swim Speed", index);
                moveInfo.WalkSpeed = packet.ReadSingle("Walk Speed", index) / 2.5f;
                packet.ReadXORByte(guid2, 3);
                packet.ReadXORByte(guid2, 4);
                packet.ReadXORByte(guid2, 2);
                packet.ReadXORByte(guid2, 6);
                packet.StoreBitstreamGuid("GUID 2", guid2, index);
                if (unkFloat2)
                    packet.ReadSingle("Unk float +36", index);

                moveInfo.Position.Y = packet.ReadSingle();
                if (hasOrientation)
                    moveInfo.Orientation = packet.ReadSingle();

                moveInfo.RunSpeed = packet.ReadSingle("Run Speed", index) / 7.0f;
                packet.Store("Position", moveInfo.Position, index);
                packet.Store("Orientation", moveInfo.Orientation, index);
            }

            if (unkFloats)
            {
                packet.StoreBeginList("UnkFloats", index);
                for (int i = 0; i < 16; ++i)
                    packet.ReadSingle("Unk float 456", index, i);
                packet.StoreEndList();
                packet.ReadByte("Unk byte 456", index);
            }

            if (hasTransportExtra)
                packet.ReadInt32("Transport Time", index);

            if (hasAnimKits)
            {
                if (hasAnimKit2)
                    packet.ReadUInt16("Anim Kit 2", index);
                if (hasAnimKit3)
                    packet.ReadUInt16("Anim Kit 3", index);
                if (hasAnimKit1)
                    packet.ReadUInt16("Anim Kit 1", index);
            }

            if (hasGORotation)
                moveInfo.Rotation = packet.ReadPackedQuaternion("GO Rotation", index);

            if (hasAttackingTarget)
            {
                packet.ParseBitStream(attackingTarget, 3, 5, 0, 7, 2, 4, 6, 1);
                packet.StoreBitstreamGuid("Attacking Target GUID", attackingTarget, index);
            }

            packet.ResetBitReader();
            return moveInfo;
        }

        private static MovementInfo ReadMovementUpdateBlock(ref Packet packet, Guid guid, int index)
        {
            if (ClientVersion.Build == ClientVersionBuild.V5_1_0_16309 || ClientVersion.Build == ClientVersionBuild.V5_1_0a_16357)
                return ReadMovementUpdateBlock510(ref packet, guid, index);

            if (ClientVersion.Build == ClientVersionBuild.V5_0_4_16016 || ClientVersion.Build == ClientVersionBuild.V5_0_5_16048 || ClientVersion.Build == ClientVersionBuild.V5_0_5a_16057 || ClientVersion.Build == ClientVersionBuild.V5_0_5b_16135)
                return ReadMovementUpdateBlock504(ref packet, guid, index);

            if (ClientVersion.Build == ClientVersionBuild.V4_3_4_15595)
                return ReadMovementUpdateBlock434(ref packet, guid, index);

            if (ClientVersion.Build == ClientVersionBuild.V4_3_3_15354)
                return ReadMovementUpdateBlock433(ref packet, guid, index);

            if (ClientVersion.Build == ClientVersionBuild.V4_3_2_15211)
                return ReadMovementUpdateBlock432(ref packet, guid, index);

            if (ClientVersion.Build == ClientVersionBuild.V4_3_0_15005 || ClientVersion.Build == ClientVersionBuild.V4_3_0a_15050)
                return ReadMovementUpdateBlock430(ref packet, guid, index);

            var moveInfo = new MovementInfo();

            var flagsTypeCode = ClientVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767) ? TypeCode.UInt16 : TypeCode.Byte;
            var flags = packet.ReadEnum<UpdateFlag>("Update Flags", flagsTypeCode, index);

            if (flags.HasAnyFlag(UpdateFlag.Living))
            {
                moveInfo = MovementHandler.ReadMovementInfo(ref packet, guid, index);
                var moveFlags = moveInfo.Flags;

                packet.StoreBeginList("Speeds", index);
                for (var i = 0; i < 9; ++i)
                {
                    var speedType = (SpeedType)i;
                    var speed = packet.ReadSingle(speedType + " Speed", index, i);

                    switch (speedType)
                    {
                        case SpeedType.Walk:
                        {
                            moveInfo.WalkSpeed = speed / 2.5f;
                            break;
                        }
                        case SpeedType.Run:
                        {
                            moveInfo.RunSpeed = speed / 7.0f;
                            break;
                        }
                    }
                }
                packet.StoreEndList();

                // Movement flags seem incorrect for 4.2.2
                // guess in which version they stopped checking movement flag and used bits
                if ((ClientVersion.RemovedInVersion(ClientVersionBuild.V4_2_0_14333) && moveFlags.HasAnyFlag(MovementFlag.SplineEnabled)) || moveInfo.HasSplineData)
                {
                    // Temp solution
                    // TODO: Make Enums version friendly
                    if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_2_0_14333))
                    {
                        var splineFlags422 = packet.ReadEnum<SplineFlag422>("Spline Flags", TypeCode.Int32, index);
                        if (splineFlags422.HasAnyFlag(SplineFlag422.FinalOrientation))
                        {
                            packet.ReadSingle("Final Spline Orientation", index);
                        }
                        else
                        {
                            if (splineFlags422.HasAnyFlag(SplineFlag422.FinalTarget))
                                packet.ReadGuid("Final Spline Target GUID", index);
                            else if (splineFlags422.HasAnyFlag(SplineFlag422.FinalPoint))
                                packet.ReadVector3("Final Spline Coords", index);
                        }
                    }
                    else
                    {
                        var splineFlags = packet.ReadEnum<SplineFlag>("Spline Flags", TypeCode.Int32, index);
                        if (splineFlags.HasAnyFlag(SplineFlag.FinalTarget))
                            packet.ReadGuid("Final Spline Target GUID", index);
                        else if (splineFlags.HasAnyFlag(SplineFlag.FinalOrientation))
                            packet.ReadSingle("Final Spline Orientation", index);
                        else if (splineFlags.HasAnyFlag(SplineFlag.FinalPoint))
                            packet.ReadVector3("Final Spline Coords", index);
                    }

                    packet.ReadInt32("Spline Time", index);
                    packet.ReadInt32("Spline Full Time", index);
                    packet.ReadInt32("Spline ID", index);

                    if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                    {
                        packet.ReadSingle("Spline Duration Multiplier", index);
                        packet.ReadSingle("Spline Duration Multiplier Next", index);
                        packet.ReadSingle("Spline Vertical Acceleration", index);
                        packet.ReadInt32("Spline Start Time", index);
                    }

                    var splineCount = packet.ReadInt32();
                    packet.StoreBeginList("Spline waypoints", index);
                    for (var i = 0; i < splineCount; i++)
                        packet.ReadVector3("Spline Waypoint", index, i);
                    packet.StoreEndList();

                    if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
                        packet.ReadEnum<SplineMode>("Spline Mode", TypeCode.Byte, index);

                    packet.ReadVector3("Spline Endpoint", index);
                }
            }
            else // !UpdateFlag.Living
            {
                if (flags.HasAnyFlag(UpdateFlag.GOPosition))
                {
                    moveInfo.TransportGuid = packet.ReadPackedGuid("GO Position GUID", index);

                    moveInfo.Position = packet.ReadVector3("GO Position", index);
                    packet.ReadVector3("GO Transport Position", index);

                    moveInfo.Orientation = packet.ReadSingle("GO Orientation", index);
                    packet.ReadSingle("Corpse Orientation", index);
                }
                else if (flags.HasAnyFlag(UpdateFlag.StationaryObject))
                {
                    moveInfo.Position = packet.ReadVector3();
                    moveInfo.Orientation = packet.ReadSingle();
                    packet.Store("Stationary Position", moveInfo.Position, index);
                    packet.Store("Stationary Orientation", moveInfo.Orientation, index);
                }
            }

            if (ClientVersion.RemovedInVersion(ClientVersionBuild.V4_2_2_14545))
            {
                if (flags.HasAnyFlag(UpdateFlag.Unknown1))
                    packet.ReadUInt32("Unk Int32", index);

                if (flags.HasAnyFlag(UpdateFlag.LowGuid))
                    packet.ReadUInt32("Low GUID", index);
            }

            if (flags.HasAnyFlag(UpdateFlag.AttackingTarget))
                packet.ReadPackedGuid("Target GUID", index);

            if (flags.HasAnyFlag(UpdateFlag.Transport))
                packet.ReadUInt32("Transport unk timer", index);

            if (flags.HasAnyFlag(UpdateFlag.Vehicle))
            {
                moveInfo.VehicleId = packet.ReadUInt32("Vehicle ID", index);
                packet.ReadSingle("Vehicle Orientation", index);
            }

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_2_2_14545))
            {
                if (flags.HasAnyFlag(UpdateFlag.AnimKits))
                {
                    packet.ReadInt16("Unk Int16 1", index);
                    packet.ReadInt16("Unk Int16 2", index);
                    packet.ReadInt16("Unk Int16 3", index);
                }
            }

            if (flags.HasAnyFlag(UpdateFlag.GORotation))
                moveInfo.Rotation = packet.ReadPackedQuaternion("GO Rotation", index);

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_2_2_14545))
            {
                if (flags.HasAnyFlag(UpdateFlag.TransportUnkArray))
                {
                    var count = packet.ReadByte("Count", index);
                    packet.StoreBeginList("Transport unks", index);
                    for (var i = 0; i < count; i++)
                        packet.ReadInt32("Unk Int32", index, i);
                    packet.StoreEndList();
                }
            }

            return moveInfo;
        }

        [Parser(Opcode.SMSG_COMPRESSED_UPDATE_OBJECT)]
        public static void HandleCompressedUpdateObject(Packet packet)
        {
            packet.Inflate(packet.ReadInt32());
            HandleUpdateObject(packet);
        }

        [Parser(Opcode.SMSG_DESTROY_OBJECT)]
        public static void HandleDestroyObject(Packet packet)
        {
            packet.ReadGuid("GUID");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.ReadBoolean("Despawn Animation");
        }

        [Parser(Opcode.CMSG_OBJECT_UPDATE_FAILED, ClientVersionBuild.Zero, ClientVersionBuild.V5_1_0_16309)] // 4.3.4
        public static void HandleObjectUpdateFailed(Packet packet)
        {
            var guid = packet.StartBitStream(6, 7, 4, 0, 1, 5, 3, 2);
            packet.ParseBitStream(guid, 6, 7, 2, 3, 1, 4, 0, 5);
            packet.StoreBitstreamGuid("Guid", guid);
        }

        [Parser(Opcode.CMSG_OBJECT_UPDATE_FAILED, ClientVersionBuild.V5_1_0_16309)]
        public static void HandleObjectUpdateFailed510(Packet packet)
        {
            var guid = packet.StartBitStream(5, 3, 0, 6, 1, 4, 2, 7);
            packet.ParseBitStream(guid, 2, 3, 7, 4, 5, 1, 0, 6);
            packet.StoreBitstreamGuid("Guid", guid);
        }
    }
}
