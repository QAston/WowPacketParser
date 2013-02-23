using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PacketParser.DataStructures;
using Guid = PacketParser.DataStructures.Guid;
using System.Reflection;

namespace PacketParser.Misc
{
    public class UpdateFieldDictionary : Dictionary<int, UpdateField>
    {
        public UpdateFieldDictionary()
            : base()
        {
        }
        public UpdateFieldDictionary(UpdateFieldDictionary rhs)
            : base(rhs)
        {
        }
        public static int GetFieldsCount(Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return 2;
                default:
                    {
                        switch (Type.GetTypeCode(nullableType))
                        {
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                return 2;
                            default:
                                break;
                        }
                        break;
                    }
            }
            if (type == typeof(Guid))
                return 2;
            else if (type.IsEnum || nullableType != null && nullableType.IsEnum)
                return GetFieldsCount(Enum.GetUnderlyingType(type));
            else if (type.IsSubclassOf(typeof(StoreEnum)))
            {
                return GetFieldsCount(Enum.GetUnderlyingType((Type)(type.GetField("type", BindingFlags.Public | BindingFlags.Static).GetValue(null))));
            }
            return 1;
        }
        public uint? GetUInt(int fieldOffset)
        {
            UpdateField uf;
            if (TryGetValue(fieldOffset, out uf))
            {
                   return uf.UInt32Value;
            }
            return null;
        }
        public int? GetInt(int fieldOffset)
        {
            UpdateField uf;
            if (TryGetValue(fieldOffset, out uf))
            {
                return (int)uf.UInt32Value;
            }
            return null;
        }
        public float? GetFloat(int fieldOffset)
        {
            UpdateField uf;
            if (TryGetValue(fieldOffset, out uf))
            {
                return uf.SingleValue;
            }
            return null;
        }
        public Int64? GetInt64(int fieldOffset)
        {
            uint? lowg = GetUInt(fieldOffset);
            uint? highg = GetUInt(fieldOffset+1);
            if (lowg == null && highg == null)
                return null;
            if (lowg == null)
                lowg = 0;
            if (highg == null)
                highg = 0;
            Int64 num = (Int64)highg;
            num = (num << 32) | (Int64)lowg;
            return num;
        }
        public UInt64? GetUInt64(int fieldOffset)
        {
            uint? lowg = GetUInt(fieldOffset);
            uint? highg = GetUInt(fieldOffset + 1);
            if (lowg == null && highg == null)
                return null;
            if (lowg == null)
                lowg = 0;
            if (highg == null)
                highg = 0;
            UInt64 num = (UInt64)highg;
            num = (num << 32) | (UInt64)lowg;
            return num;
        }
        public Guid? GetGuid(int fieldOffset)
        {
            UInt64? num = GetUInt64(fieldOffset);
            if (num == null)
                return null;
            return new Guid((ulong)num);
        }
        public Object GetEnum(int fieldOffset, Type type, out Object val)
        {
            // typeof (TK) is a nullable type (ObjectField?)
            // typeof (TK).GetGenericArguments()[0] is the non nullable equivalent (ObjectField)
            // we need to convert our int from UpdateFields to the enum type
            if (Nullable.GetUnderlyingType(type) != null)
                type = Nullable.GetUnderlyingType(type);
            val = GetValue(fieldOffset, Enum.GetUnderlyingType(type));
            if (val == null)
                return null;
            try
            {
                object o1 = Enum.ToObject(type, val);
                return o1;
            }
            catch(Exception)
            {
            }
            return null;
        }
        public Object GetEnum(int fieldOffset, Type type)
        {
            Object val;
            return GetEnum(fieldOffset, type, out val);
        }

        public TK GetEnum<TK>(int fieldOffset)
        {
            return (TK)GetEnum(fieldOffset, typeof(TK));
        }
        public Object GetValue(int fieldOffset, Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (type == typeof(Guid) || nullableType == typeof(Guid))
                return GetGuid(fieldOffset);
            else if (type.IsEnum || (nullableType != null && nullableType.IsEnum))
                return GetEnum(fieldOffset, type);
            else if (type.IsSubclassOf(typeof(StoreEnum)))
            {
                var enumType = (Type)(type.GetField("type", BindingFlags.Public | BindingFlags.Static).GetValue(null));
                Object val;
                GetEnum(fieldOffset, enumType, out val);
                if (val == null)
                    return null;
                return Activator.CreateInstance(type, val);
            }
            else if (type.IsSubclassOf(typeof(Bytes)) || type.IsSubclassOf(typeof(Shorts)))
            {
                var val = GetUInt(fieldOffset);
                if (val == null)
                    return null;
                return Activator.CreateInstance(type, (uint)val);
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Char:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    return GetUInt(fieldOffset);
                case TypeCode.Int32:
                    return GetInt(fieldOffset);
                case TypeCode.Single:
                    return GetFloat(fieldOffset);
                case TypeCode.Int64:
                    return GetInt64(fieldOffset);
                case TypeCode.UInt64:
                    return GetUInt64(fieldOffset);
                default:
                    {
                        switch (Type.GetTypeCode(nullableType))
                        {
                            case TypeCode.UInt32:
                                return GetUInt(fieldOffset);
                            case TypeCode.Int32:
                                return GetInt(fieldOffset);
                            case TypeCode.Single:
                                return GetFloat(fieldOffset);
                            case TypeCode.Int64:
                                return GetInt64(fieldOffset);
                            case TypeCode.UInt64:
                                return GetUInt64(fieldOffset);
                            default:
                                break;
                        }
                        break;
                    }
            }
            return null;
        }
        public T GetValue<T>(int fieldOffset)
        {
            return (T)GetValue(fieldOffset, typeof(T));
        }

        public TK[] GetArray<TK>(int fieldOffset, int count)
        {
            int off = GetFieldsCount(typeof(TK));
            var result = new TK[count];
            for (var i = 0; i < count; i++)
                result[i] = GetValue<TK>(fieldOffset + i*off);

            return result;
        }
    }
}
