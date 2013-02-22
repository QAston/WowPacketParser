using System;
using PacketParser.Misc;
using System.Text;

namespace PacketParser.DataStructures
{
    public abstract class Bytes
    {
        public byte b1;
        public byte b2;
        public byte b3;
        public byte b4;
        public uint value;

        public abstract Type GetEnumType1();
        public abstract Type GetEnumType2();
        public abstract Type GetEnumType3();
        public abstract Type GetEnumType4();
    }
    public class Bytes<T1, T2, T3, T4> : Bytes
        where T1: struct,IConvertible 
        where T2: struct,IConvertible
        where T3 : struct,IConvertible
        where T4 : struct,IConvertible
    {
        public static Type t1 = typeof(T1);
        public static Type t2 = typeof(T2);
        public static Type t3 = typeof(T3);
        public static Type t4 = typeof(T4);

        public T1 v1;
        public T2 v2;
        public T3 v3;
        public T4 v4;

        public Bytes(uint d)
        {
            value = d;
            b4 = (byte)((d & 0xFF000000) >> 24);
            b3 = (byte)((d & 0x00FF0000) >> 16);
            b2 = (byte)((d & 0x0000FF00) >> 8);
            b1 = (byte)((d & 0x000000FF) >> 0);

            if (t1.IsEnum)
                v1 = (T1)Enum.ToObject(t1, b1);
            else
                v1 = (T1)(Object)b1;
            if (t2.IsEnum)
                v2 = (T2)Enum.ToObject(t2, b2);
            else
                v2 = (T2)(Object)b2;
            if (t3.IsEnum)
                v3 = (T3)Enum.ToObject(t3, b3);
            else
                v3 = (T3)(Object)b3;
            if (t4.IsEnum)
                v4 = (T4)Enum.ToObject(t4, b4);
            else
                v4 = (T4)(Object)b4;
        }

        public override Type GetEnumType1()
        {
            return t1;
        }

        public override Type GetEnumType2()
        {
            return t2;
        }

        public override Type GetEnumType3()
        {
            return t3;
        }

        public override Type GetEnumType4()
        {
            return t4;
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
            b.Append(" ");
            if (t3.IsEnum)
            {
                b.Append(Enum<T3>.ToString(b3));
                b.Append("(");
                b.Append(b3);
                b.Append(")");
            }
            else
                b.Append(b3.ToString());
            b.Append(" ");
            if (t4.IsEnum)
            {
                b.Append(Enum<T4>.ToString(b4));
                b.Append("(");
                b.Append(b4);
                b.Append(")");
            }
            else
                b.Append(b4.ToString());
            return b.ToString();
        }
    }
}
