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
    private static readonly double Log2 = Math.Log(2);

    private static int CountReferences(object value) => value switch
    {
        IDictionary<string, object> dict => dict.Values.Sum(CountReferences) + dict.Keys.Count + 1,
        IList<object> list => list.Sum(CountReferences) + 1,
        _ => 1,
    };

    private static void Write(Stream stream, IList<long> offsetTable, IList<object?> offsetValues, int objectReferenceSize, object value)
    {
        switch (value)
        {
            case IDictionary<string, object> dict:
                Write(stream, offsetTable, offsetValues, objectReferenceSize, dict);
                break;
            case IList<object> list:
                Write(stream, offsetTable, offsetValues, objectReferenceSize, list);
                break;
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
        }
    }

    private static void Write(Stream stream, IList<long> offsetTable, IList<object?> offsetValues, int referenceSize, IDictionary<string, object> dictionary)
    {
        if (dictionary.Count < 15)
        {
            stream.WriteByte(Convert.ToByte(0xD0 | dictionary.Count));
        }
        else
        {
            stream.WriteByte(0xD0 | 0x0F);
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

    private static void Write(Stream stream, IList<long> offsetTable, IList<object?> offsetValues, int referenceSize, IList<object> values)
    {
        // write the header
        if (values.Count < 15)
        {
            stream.WriteByte(Convert.ToByte(0xA0 | Convert.ToByte(values.Count)));
        }
        else
        {
            stream.WriteByte(0xA0 | 0x0F);
            Write(stream, values.Count);
        }

        // get the refs first
        var referencePosition = stream.Position;
        stream.Position += values.Count * referenceSize;

        var references = new List<int>();
        foreach (var value in values)
        {
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
        stream.WriteByte(Convert.ToByte(0x10 | (int)GetCountValue(count)));
        stream.Write(span);

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
        stream.WriteByte(Convert.ToByte(0x20 | (int)GetCountValue(count)));
        stream.Write(span);

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

        stream.WriteByte(0x33);
        stream.Write(span);

        static double ConvertToAppleTimeStamp(DateTime date)
        {
            var diff = date - Origin;
            return Math.Floor(diff.TotalSeconds);
        }
    }

    private static void Write(Stream stream, byte[] value)
    {
        if (value.Length < 15)
        {
            stream.WriteByte(Convert.ToByte(0x40 | Convert.ToByte(value.Length)));
        }
        else
        {
            stream.WriteByte(0x40 | 0xf);
            Write(stream, value.Length);
        }

        stream.Write(value);
    }

    private static void Write(Stream stream, string value, bool head)
    {
        const int MaxAnsiCode = 255;

        // see if this contains any unicode characters
        var id = value.Any(c => c > MaxAnsiCode)
            ? 0x60
            : 0x50;

        if (head)
        {
            if (value.Length < 15)
            {
                stream.WriteByte(Convert.ToByte(id | Convert.ToByte(value.Length)));
            }
            else
            {
                stream.WriteByte(Convert.ToByte(id | 0xf));
                Write(stream, value.Length);
            }
        }

        if (id == 0x60)
        {
            stream.Write(System.Text.Encoding.BigEndianUnicode.GetBytes(value));
        }
        else
        {
            stream.Write(System.Text.Encoding.UTF8.GetBytes(value));
        }
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
