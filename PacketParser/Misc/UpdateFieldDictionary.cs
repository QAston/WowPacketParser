using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PacketParser.DataStructures;
using Guid = PacketParser.DataStructures.Guid;

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
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return 2;
                default:
                    {
                        switch (Type.GetTypeCode(Nullable.GetUnderlyingType(type)))
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
            else if (type.IsEnum)
                return GetFieldsCount(Enum.GetUnderlyingType(type));
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
        public Object GetEnum(int fieldOffset, Type type)
        {
            // typeof (TK) is a nullable type (ObjectField?)
            // typeof (TK).GetGenericArguments()[0] is the non nullable equivalent (ObjectField)
            // we need to convert our int from UpdateFields to the enum type
            if (Nullable.GetUnderlyingType(type) != null)
                type = type.GetGenericArguments()[0];
            Object val = GetValue(fieldOffset, Enum.GetUnderlyingType(type));
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
        public TK GetEnum<TK>(int fieldOffset)
        {
            return (TK)GetEnum(fieldOffset, typeof(TK));
        }
        public Object GetValue(int fieldOffset, Type type)
        {
            if (type == typeof(Guid))
                return GetGuid(fieldOffset);
            else if (type.IsEnum)
                return GetEnum(fieldOffset, type);
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
                        switch (Type.GetTypeCode(Nullable.GetUnderlyingType(type)))
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
