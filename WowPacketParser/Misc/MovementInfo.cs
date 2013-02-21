using System;
using PacketParser.Enums;
using System.Reflection;

namespace PacketParser.DataStructures
{
    public sealed class MovementInfo
    {
        public MovementInfo()
        {
        }

        public MovementInfo(MovementInfo rhs)
        {
            if (rhs == null)
                return;
            // get all the fields in the class
            FieldInfo[] fields_of_class = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            // copy each value over to 'this'
            foreach (FieldInfo fi in fields_of_class)
            {
                fi.SetValue(this, fi.GetValue(rhs));
            }
        }
        public MovementFlag Flags;

        public MovementFlagExtra FlagsExtra;

        public bool HasSplineData;

        public Vector3 Position;

        public float Orientation;

        public Guid TransportGuid;

        public Vector4 TransportOffset;

        public Quaternion Rotation;

        public float WalkSpeed;

        public float RunSpeed;

        public UInt32 VehicleId; // Not exactly related to movement but it is read in ReadMovementUpdateBlock

        public bool HasWpsOrRandMov; // waypoints or random movement
    }
}
