// -----------------------------------------------------------------------
// <copyright file="PListBinaryFormatter.Read.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList;

using System.Runtime.CompilerServices;
using System.Text;
using static System.Buffers.Binary.BinaryPrimitives;

/// <summary>
/// The read methods for <see cref="PListBinaryFormatter"/>.
/// </summary>
public partial class PListBinaryFormatter
{
    private static object? Read(Stream stream, IList<long> offsetTable, int index, int objectReferenceSize)
    {
        var header = stream.ReadByte(offsetTable[index]);
        return (header & 0xF0) switch
        {
            0 => (header == 0) ? null : header == 9, // boolean, If the byte is 0 return null, 9 return true, 8 return false
            0x10 => ReadInt64(stream, offsetTable[index]), // int64
            0x20 => ReadDouble(stream, offsetTable[index]), // double
            0x30 => ReadDateTime(stream, offsetTable[index]), // date time
            0x40 => ReadBytes(stream, offsetTable[index]), // bytes
            0x50 => ReadAsciiString(stream, offsetTable[index]), // ASCII
            0x60 => ReadUnicodeString(stream, offsetTable[index]), // Unicode
            0xa0 => ReadArray(stream, offsetTable, index, objectReferenceSize), // Array
            0xd0 => ReadDictionary(stream, offsetTable, index, objectReferenceSize), // Dictionary
            _ => throw new NotSupportedException(),
        };
    }

    private static object ReadDictionary(Stream stream, IList<long> offsetTable, int index, int referenceSize)
    {
        var buffer = new Dictionary<string, object?>(StringComparer.Ordinal);
        var referenceCount = GetCount(stream, offsetTable[index], out _);

        // Check if the following integer has a header aswell so we increase the referenceStartPosition by two to account for that.
        var referenceStartPosition = referenceCount >= 15
            ? offsetTable[index] + 2 + GetByteCount(BitConverter.GetBytes(referenceCount))
            : offsetTable[index] + 1;

        var references = new int[referenceCount * 2];
        var current = referenceStartPosition;
        for (var i = 0; i < references.Length; i++)
        {
            var referenceBuffer = stream.Read(current, referenceSize);
            references[i] = GetInt32(referenceBuffer.AsSpan());
            current += referenceSize;
        }

        for (var i = 0; i < referenceCount; i++)
        {
            buffer.Add(
                (string)Read(stream, offsetTable, references[i], referenceSize)!,
                Read(stream, offsetTable, references[i + referenceCount], referenceSize));
        }

        return buffer;
    }

    private static IList<object?> ReadArray(Stream stream, IList<long> offsetTable, int index, int referenceSize)
    {
        var buffer = new List<object?>();
        var referenceCount = GetCount(stream, offsetTable[index], out _);

        // Check if the following integer has a header aswell so we increase the referenceStartPosition by two to account for that.
        var referenceStartPosition = referenceCount >= 15
            ? offsetTable[index] + 2 + GetByteCount(BitConverter.GetBytes(referenceCount))
            : offsetTable[index] + 1;

        var references = new List<int>();
        for (var i = referenceStartPosition; i < referenceStartPosition + (referenceCount * referenceSize); i += referenceSize)
        {
            var referenceBuffer = stream.Read(i, referenceSize).Reverse();
            references.Add(GetInt32(referenceBuffer.AsSpan()));
        }

        for (var i = 0; i < referenceCount; i++)
        {
            buffer.Add(Read(stream, offsetTable, references[i], referenceSize));
        }

        return buffer;
    }

    private static long ReadInt64(Stream stream, long headerPosition) => ReadInt64(stream, headerPosition, out _);

    private static long ReadInt64(Stream stream, long headerPosition, out long newHeaderPosition)
    {
        var header = stream.ReadByte(headerPosition);
        var byteCount = (int)Math.Pow(2, header & 0xf);

        var buffer = stream.Read(headerPosition + 1, byteCount);

        // Add one to account for the header byte
        newHeaderPosition = headerPosition + byteCount + 1;
        return GetInt64(buffer.AsSpan());
    }

    private static DateTime ReadDateTime(Stream stream, long headerPosition)
    {
        var buffer = stream.Read(headerPosition + 1, 8);
        return ConvertFromAppleTimeStamp(GetDouble(buffer.AsSpan()));

        static DateTime ConvertFromAppleTimeStamp(double timestamp)
        {
            return Origin.AddSeconds(timestamp);
        }
    }

    private static double ReadDouble(Stream stream, long headerPosition)
    {
        var header = stream.ReadByte(headerPosition);
        var byteCount = (int)Math.Pow(2, header & 0xf);
        var buffer = stream.Read(headerPosition + 1, byteCount);
        return GetDouble(buffer.AsSpan());
    }

    private static string ReadAsciiString(Stream stream, long headerPosition)
    {
        var charCount = (int)GetCount(stream, headerPosition, out var charStartPosition);
        var buffer = stream.Read(charStartPosition, charCount);
        return buffer.Length > 0 ? Encoding.UTF8.GetString(buffer) : string.Empty;
    }

    private static string ReadUnicodeString(Stream stream, long headerPosition)
    {
        var charCount = (int)GetCount(stream, headerPosition, out var charStartPosition) * 2;
        var buffer = stream.Read(charStartPosition, charCount);
        return Encoding.BigEndianUnicode.GetString(buffer);
    }

    private static byte[] ReadBytes(Stream stream, long headerPosition)
    {
        var byteCount = GetCount(stream, headerPosition, out var byteStartPosition);
        return stream.Read(byteStartPosition, (int)byteCount);
    }

    private static long GetCount(Stream stream, long bytePosition, out long newBytePosition)
    {
        var headerByte = stream.ReadByte(bytePosition);
        var headerByteTrail = Convert.ToByte(headerByte & 0xf);
        long count;
        if (headerByteTrail < 15)
        {
            count = headerByteTrail;
            newBytePosition = bytePosition + 1;
        }
        else
        {
            count = ReadInt64(stream, bytePosition + 1, out newBytePosition);
        }

        return count;
    }

    private static int GetInt32(ReadOnlySpan<byte> span) => GetInt32(span, span.Length);

    private static int GetInt32(ReadOnlySpan<byte> span, int length) => length switch
    {
        1 => span[0],
        2 => ReadInt16BigEndian(span),
        4 => ReadInt32BigEndian(span),
    };

    private static long GetInt64(ReadOnlySpan<byte> span) => GetInt64(span, span.Length);

    private static long GetInt64(ReadOnlySpan<byte> span, int length) => length switch
    {
        1 => span[0],
        2 => ReadInt16BigEndian(span),
        4 => ReadInt32BigEndian(span),
        8 => ReadInt64BigEndian(span),
    };

    private static double GetDouble(ReadOnlySpan<byte> span) => GetDouble(span, span.Length);

    private static double GetDouble(ReadOnlySpan<byte> span, int length)
#if NET6_0_OR_GREATER
        => length switch
        {
            1 => span[0],
            2 => (double)ReadHalfBigEndian(span),
            4 => ReadSingleBigEndian(span),
            8 => ReadDoubleBigEndian(span),
        };
#elif NET5_0_OR_GREATER
    {
        return length switch
        {
            1 => span[0],
            4 => ReadSingleBigEndian(span),
            8 => ReadDoubleBigEndian(span),
        };
    }
#else
    {
        return length switch
        {
            1 => span[0],
            4 => ReadSingle(span),
            8 => ReadDouble(span),
        };

        static double ReadDouble(ReadOnlySpan<byte> span)
        {
            return BitConverter.IsLittleEndian
                ? BitConverter.Int64BitsToDouble(ReverseEndiannessInt64(System.Runtime.InteropServices.MemoryMarshal.Read<long>(span)))
                : System.Runtime.InteropServices.MemoryMarshal.Read<double>(span);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static long ReverseEndiannessInt64(long value)
            {
                return (long)ReverseEndiannessUInt64((ulong)value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static ulong ReverseEndiannessUInt64(ulong value)
            {
                return ((ulong)ReverseEndiannessUInt32((uint)value) << 32) + ReverseEndiannessUInt32((uint)(value >> 32));
            }
        }

        static float ReadSingle(ReadOnlySpan<byte> span)
        {
            return !BitConverter.IsLittleEndian
                ? System.Runtime.InteropServices.MemoryMarshal.Read<float>(span)
                : Int32BitsToSingle(ReverseEndiannessInt32(System.Runtime.InteropServices.MemoryMarshal.Read<int>(span)));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static unsafe float Int32BitsToSingle(int value)
            {
                return *(float*)&value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int ReverseEndiannessInt32(int value)
            {
                return (int)ReverseEndiannessUInt32((uint)value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint ReverseEndiannessUInt32(uint value)
        {
            return RotateRight(value & 0x00FF00FFu, 8) // xx zz
                + RotateLeft(value & 0xFF00FF00u, 8); // ww yy
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint RotateRight(uint value, int offset)
        {
            return (value >> offset) | (value << (32 - offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint RotateLeft(uint value, int offset)
        {
            return (value << offset) | (value >> (32 - offset));
        }
    }
#endif
}
