using System;
using PacketParser.Misc;
using System.Text;

namespace PacketParser.DataStructures
{
    public abstract class Shorts
    {
        public UInt16 b1;
        public UInt16 b2;
        public uint value;

        public abstract Type GetEnumType1();
        public abstract Type GetEnumType2();
    }
    public class Shorts<T1, T2> : Shorts
        where T1: struct,IConvertible 
        where T2: struct,IConvertible
    {
        public static Type t1 = typeof(T1);
        public static Type t2 = typeof(T2);

        public T1 v1;
        public T2 v2;

        public Shorts(uint d)
        {
            value = d;
            b2 = (UInt16)((d & 0xFFFF0000) >> 16);
            b1 = (UInt16)((d & 0x000FFFF) >> 0);

            if (t1.IsEnum)
                v1 = (T1)Enum.ToObject(t1, b1);
            else
                v1 = (T1)(Object)b1;
            if (t2.IsEnum)
                v2 = (T2)Enum.ToObject(t2, b2);
            else
                v2 = (T2)(Object)b2;
        }

        public override Type GetEnumType1()
        {
            return t1;
        }

        public override Type GetEnumType2()
        {
            return t2;
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder(40);
            if (t1.IsEnum)
            {
                b.Append(Enum<T1>.ToString(b1));
                b.Append("(");
                b.Append(b1);
                b.Append(")");
            }
            else
                b.Append(b1.ToString());
            b.Append(" ");
            if (t2.IsEnum)
            {
                b.Append(Enum<T2>.ToString(b2));
                b.Append("(");
                b.Append(b2);
                b.Append(")");
            }
            else
                b.Append(b2.ToString());
            return b.ToString();
        }
    }
}
