// -----------------------------------------------------------------------
// <copyright file="PListBinaryFormatter.Write.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList;

using static System.Buffers.Binary.BinaryPrimitives;

/// <summary>
/// The write methods for <see cref="PListBinaryFormatter"/>.
/// </summary>
public partial class PListBinaryFormatter
{
    private const byte RightBits = 0xF;

    private static readonly double Log2 = Math.Log(2);

    private static int CountReferences(object value)
    {
        return value switch
        {
            bool or short or int or long or float or double or string or byte[] or DateTime => 1,
            System.Collections.IDictionary dict => Sum(dict.Values) + dict.Keys.Count + 1,
            System.Collections.IList list => Sum(list) + 1,
            _ => throw new InvalidCastException(),
        };

        static int Sum(System.Collections.IEnumerable enumerable)
        {
            var total = 0;
            foreach (var item in enumerable)
            {
                total += CountReferences(item);
            }

            return total;
        }
    }

    private static void Write(Stream stream, IList<long> offsetTable, IList<object?> offsetValues, int objectReferenceSize, object value)
    {
        switch (value)
        {
            case byte[] bytes:
                Write(stream, bytes);
                break;
            case float floatValue:
                Write(stream, floatValue);
                break;
            case double doubleValue:
                Write(stream, doubleValue);
                break;
            case short shortValue:
                Write(stream, shortValue);
                break;
            case int intValue:
                Write(stream, intValue);
                break;
            case long longValue:
                Write(stream, longValue);
                break;
            case string stringValue:
                Write(stream, stringValue, head: true);
                break;
            case DateTime dateTime:
                Write(stream, dateTime);
                break;
            case bool boolValue:
                Write(stream, boolValue);
                break;
            case System.Collections.IDictionary dict:
                Write(stream, offsetTable, offsetValues, objectReferenceSize, dict);
                break;
            case System.Collections.IList list:
                Write(stream, offsetTable, offsetValues, objectReferenceSize, list);
                break;
        }
    }

    private static void Write(Stream stream, IList<long> offsetTable, IList<object?> offsetValues, int referenceSize, System.Collections.IDictionary dictionary)
    {
        if (dictionary.Count < 15)
        {
            stream.WriteByte(Convert.ToByte(DataType.Dictionary | dictionary.Count));
        }
        else
        {
            stream.WriteByte(DataType.Dictionary | RightBits);
            Write(stream, dictionary.Count);
        }

        // get the refs first
        var referencePosition = stream.Position;
        stream.Position += dictionary.Count * referenceSize * 2;

        var references = new List<int>();
        var keys = new string[dictionary.Count];
        dictionary.Keys.CopyTo(keys, 0);

        var values = new object[dictionary.Count];
        dictionary.Values.CopyTo(values, 0);

        for (var i = 0; i < dictionary.Count; i++)
        {
            references.Add(offsetTable.Count);
            offsetTable.Add((int)stream.Position);
            offsetValues.Add(item: null);
            Write(stream, offsetTable, offsetValues, referenceSize, keys[i]);
        }

        for (var i = 0; i < dictionary.Count; i++)
        {
            var value = values[i];
            var index = IndexOfPrimitive(offsetValues, value);
            if (index == -1)
            {
                references.Add(offsetTable.Count);
                offsetTable.Add((int)stream.Position);
                AddToOffsetValues(offsetValues, value);
                Write(stream, offsetTable, offsetValues, referenceSize, value);
            }
            else
            {
                references.Add(index);
            }
        }

        var endPosition = stream.Position;
        stream.Position = referencePosition;

        foreach (var reference in references)
        {
            stream.Write(GetSpan(reference, referenceSize));
        }

        stream.Position = endPosition;
    }

    private static void Write(Stream stream, IList<long> offsetTable, IList<object?> offsetValues, int referenceSize, System.Collections.IList values)
    {
        // write the header
        if (values.Count < 15)
        {
            stream.WriteByte(Convert.ToByte(DataType.Array | Convert.ToByte(values.Count)));
        }
        else
        {
            stream.WriteByte(DataType.Array | RightBits);
            Write(stream, values.Count);
        }

        // get the refs first
        var referencePosition = stream.Position;
        stream.Position += values.Count * referenceSize;

        var references = new List<int>();
        foreach (var value in values)
        {
            var index = IndexOfPrimitive(offsetValues, value);
            if (index is -1)
            {
                references.Add(offsetTable.Count);
                offsetTable.Add((int)stream.Position);
                AddToOffsetValues(offsetValues, value);
                Write(stream, offsetTable, offsetValues, referenceSize, value);
            }
            else
            {
                references.Add(index);
            }
        }

        var endPosition = stream.Position;
        stream.Position = referencePosition;

        foreach (var reference in references)
        {
            stream.Write(GetSpan(reference, referenceSize));
        }

        stream.Position = endPosition;
    }

    private static void Write(Stream stream, bool value) => stream.WriteByte(value ? (byte)0x09 : (byte)0x08);

    private static void Write(Stream stream, long value)
    {
        Span<byte> span = stackalloc byte[sizeof(long)];
        WriteInt64BigEndian(span, value);
        var count = Math.Max(GetByteCount(span), 1);
        while (count != Math.Pow(2, GetCountValue(count)))
        {
            count++;
        }

        span = span[^count..];
        stream.WriteByte(Convert.ToByte(DataType.Int64 | (int)GetCountValue(count)));
        stream.Write(span);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static double GetCountValue(int count)
        {
            return Math.Log(count) / Log2;
        }
    }

    private static void Write(Stream stream, double value)
    {
        Span<byte> span = stackalloc byte[sizeof(double)];
        WriteDoubleBigEndian(span, value);
        var count = Math.Max(GetByteCount(span), sizeof(float));
        while (count != Math.Pow(2, GetCountValue(count)))
        {
            count++;
        }

        span = span[^count..];
        stream.WriteByte(Convert.ToByte(DataType.Double | (int)GetCountValue(count)));
        stream.Write(span);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static double GetCountValue(int count)
        {
            return Math.Log(count) / Log2;
        }
    }

    private static void Write(Stream stream, DateTime value)
    {
        var appleTimeStamp = ConvertToAppleTimeStamp(value);
        Span<byte> span = stackalloc byte[sizeof(double)];
        WriteDoubleBigEndian(span, appleTimeStamp);
        Regulate(ref span, sizeof(double));

        stream.WriteByte(DataType.DateTime | 0x03);
        stream.Write(span);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static double ConvertToAppleTimeStamp(DateTime date)
        {
            return Math.Floor((date - Origin).TotalSeconds);
        }
    }

    private static void Write(Stream stream, byte[] value)
    {
        if (value.Length < 15)
        {
            stream.WriteByte(Convert.ToByte(DataType.Bytes | value.Length));
        }
        else
        {
            stream.WriteByte(DataType.Bytes | RightBits);
            Write(stream, value.Length);
        }

        stream.Write(value, 0, value.Length);
    }

    private static void Write(Stream stream, string value, bool head)
    {
        const int MaxAnsiCode = 255;

        // see if this contains any unicode characters
        var id = value.Any(c => c > MaxAnsiCode)
            ? DataType.Unicode
            : DataType.Ascii;

        if (head)
        {
            if (value.Length < 15)
            {
                stream.WriteByte(Convert.ToByte(id | value.Length));
            }
            else
            {
                stream.WriteByte(Convert.ToByte(id | RightBits));
                Write(stream, value.Length);
            }
        }

        var bytes = id switch
        {
            DataType.Unicode => System.Text.Encoding.BigEndianUnicode.GetBytes(value),
            _ => System.Text.Encoding.UTF8.GetBytes(value),
        };

        stream.Write(bytes, 0, bytes.Length);
    }

    private static void AddToOffsetValues(IList<object?> offsetValues, object value) => offsetValues.Add(value?.GetType().IsPrimitive != true ? null : value);

    private static int IndexOfPrimitive(IList<object?> offsetValues, object value) => value?.GetType().IsPrimitive != true ? -1 : offsetValues.IndexOf(value);

#if NETSTANDARD
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void WriteDoubleBigEndian(Span<byte> destination, double value)
    {
        if (BitConverter.IsLittleEndian)
        {
            var tmp = ReverseEndianness(BitConverter.DoubleToInt64Bits(value));
            System.Runtime.InteropServices.MemoryMarshal.Write(destination, ref tmp);
        }
        else
        {
            System.Runtime.InteropServices.MemoryMarshal.Write(destination, ref value);
        }
    }
#endif
}
