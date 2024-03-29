﻿// -----------------------------------------------------------------------
// <copyright file="PListBinaryFormatter.Read.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList;

using System.Text;
using static System.Buffers.Binary.BinaryPrimitives;

/// <summary>
/// The read methods for <see cref="PListBinaryFormatter"/>.
/// </summary>
public partial class PListBinaryFormatter
{
    private const byte LeftBits = 0xF0;

    private static object? Read(Stream stream, IList<long> offsetTable, int index, int objectReferenceSize)
    {
        var header = stream.ReadByte(offsetTable[index]);
        return (header & LeftBits) switch
        {
            DataType.Boolean => (header == 0) ? null : header == 9, // boolean, If the byte is 0 return null, 9 return true, 8 return false
            DataType.Int64 => ReadInt64(stream, offsetTable[index]),
            DataType.Double => ReadDouble(stream, offsetTable[index]),
            DataType.DateTime => ReadDateTime(stream, offsetTable[index]),
            DataType.Bytes => ReadBytes(stream, offsetTable[index]),
            DataType.Ascii => ReadAsciiString(stream, offsetTable[index]),
            DataType.Unicode => ReadUnicodeString(stream, offsetTable[index]),
            DataType.Array => ReadArray(stream, offsetTable, index, objectReferenceSize),
            DataType.Dictionary => ReadDictionary(stream, offsetTable, index, objectReferenceSize),
            _ => throw new NotSupportedException(),
        };
    }

    private static Dictionary<string, object?> ReadDictionary(Stream stream, IList<long> offsetTable, int index, int referenceSize)
    {
        var buffer = new Dictionary<string, object?>(StringComparer.Ordinal);
        var referenceCount = GetCount(stream, offsetTable[index], out _);

        // Check if the following integer has a header aswell so we increase the referenceStartPosition by two to account for that.
        var referenceStartPosition = referenceCount >= 15
            ? offsetTable[index] + 2 + GetByteCount(referenceCount)
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

    private static List<object?> ReadArray(Stream stream, IList<long> offsetTable, int index, int referenceSize)
    {
        var buffer = new List<object?>();
        var referenceCount = GetCount(stream, offsetTable[index], out _);

        // Check if the following integer has a header aswell so we increase the referenceStartPosition by two to account for that.
        var referenceStartPosition = referenceCount >= 15
            ? offsetTable[index] + 2 + GetByteCount(referenceCount)
            : offsetTable[index] + 1;

        var references = new List<int>();
        for (var i = referenceStartPosition; i < referenceStartPosition + (referenceCount * referenceSize); i += referenceSize)
        {
            var referenceBuffer = stream.Read(i, referenceSize);
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
        var byteCount = (int)Math.Pow(2, header & RightBits);

        var buffer = stream.Read(headerPosition + 1, byteCount);

        // Add one to account for the header byte
        newHeaderPosition = headerPosition + byteCount + 1;
        return GetInt64(buffer.AsSpan());
    }

    private static DateTime ReadDateTime(Stream stream, long headerPosition)
    {
        var buffer = stream.Read(headerPosition + 1, sizeof(double)).AsSpan();
        return ConvertFromAppleTimeStamp(GetDouble(buffer));

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static DateTime ConvertFromAppleTimeStamp(double timestamp)
        {
            return Origin.AddSeconds(timestamp);
        }
    }

    private static double ReadDouble(Stream stream, long headerPosition)
    {
        var header = stream.ReadByte(headerPosition);
        var byteCount = (int)Math.Pow(2, header & RightBits);
        var buffer = stream.Read(headerPosition + 1, byteCount).AsSpan();
        return GetDouble(buffer);
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
        return buffer.Length > 0 ? Encoding.BigEndianUnicode.GetString(buffer) : string.Empty;
    }

    private static byte[] ReadBytes(Stream stream, long headerPosition)
    {
        var byteCount = GetCount(stream, headerPosition, out var byteStartPosition);
        return stream.Read(byteStartPosition, (int)byteCount);
    }

    private static long GetCount(Stream stream, long bytePosition, out long newBytePosition)
    {
        var headerByte = stream.ReadByte(bytePosition);
        var headerByteTrail = Convert.ToByte(headerByte & RightBits);
        if (headerByteTrail < 15)
        {
            newBytePosition = bytePosition + 1;
            return headerByteTrail;
        }

        return ReadInt64(stream, bytePosition + 1, out newBytePosition);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static int GetInt32(ReadOnlySpan<byte> span) => GetInt32(span, span.Length);

    private static int GetInt32(ReadOnlySpan<byte> span, int length) => length switch
    {
        sizeof(byte) => span[0],
        sizeof(short) => ReadInt16BigEndian(span),
        sizeof(int) => ReadInt32BigEndian(span),
        _ => throw new InvalidCastException(),
    };

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static long GetInt64(ReadOnlySpan<byte> span) => GetInt64(span, span.Length);

    private static long GetInt64(ReadOnlySpan<byte> span, int length) => length switch
    {
        sizeof(byte) => span[0],
        sizeof(short) => ReadInt16BigEndian(span),
        sizeof(int) => ReadInt32BigEndian(span),
        sizeof(long) => ReadInt64BigEndian(span),
        _ => throw new InvalidCastException(),
    };

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double GetDouble(ReadOnlySpan<byte> span) => GetDouble(span, span.Length);

    private static double GetDouble(ReadOnlySpan<byte> span, int length)
#if NET6_0_OR_GREATER
        => length switch
        {
            sizeof(byte) => span[0],
            sizeof(short) => (double)ReadHalfBigEndian(span),
            sizeof(float) => ReadSingleBigEndian(span),
            sizeof(double) => ReadDoubleBigEndian(span),
            _ => throw new InvalidCastException(),
        };
#elif NET5_0_OR_GREATER
    {
        return length switch
        {
            sizeof(byte) => span[0],
            sizeof(short) => (double)ReadHalfBigEndian(span),
            sizeof(float) => ReadSingleBigEndian(span),
            sizeof(double) => ReadDoubleBigEndian(span),
            _ => throw new InvalidCastException(),
        };

        static Half ReadHalfBigEndian(ReadOnlySpan<byte> span)
        {
            return BitConverter.IsLittleEndian
                ? Int16BitsToHalf(ReverseEndianness(System.Runtime.InteropServices.MemoryMarshal.Read<short>(span)))
                : System.Runtime.InteropServices.MemoryMarshal.Read<Half>(span);

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static unsafe Half Int16BitsToHalf(short value)
            {
                return *(Half*)&value;
            }
        }
    }
#else
    {
        return length switch
        {
            sizeof(byte) => span[0],
            sizeof(short) => ReadHalfBigEndian(span),
            sizeof(float) => ReadSingleBigEndian(span),
            sizeof(double) => ReadDoubleBigEndian(span),
            _ => throw new InvalidCastException(),
        };

        static float ReadHalfBigEndian(ReadOnlySpan<byte> span)
        {
            // get the int16
            var @short = BitConverter.IsLittleEndian
                ? ReverseEndianness(System.Runtime.InteropServices.MemoryMarshal.Read<short>(span))
                : System.Runtime.InteropServices.MemoryMarshal.Read<short>(span);

            return Int32BitsToSingle(@short);
        }

        static float ReadSingleBigEndian(ReadOnlySpan<byte> span)
        {
            return BitConverter.IsLittleEndian
                ? Int32BitsToSingle(ReverseEndianness(System.Runtime.InteropServices.MemoryMarshal.Read<int>(span)))
                : System.Runtime.InteropServices.MemoryMarshal.Read<float>(span);
        }

        static double ReadDoubleBigEndian(ReadOnlySpan<byte> span)
        {
            return BitConverter.IsLittleEndian
                ? BitConverter.Int64BitsToDouble(ReverseEndianness(System.Runtime.InteropServices.MemoryMarshal.Read<long>(span)))
                : System.Runtime.InteropServices.MemoryMarshal.Read<double>(span);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static unsafe float Int32BitsToSingle(int value)
        {
            return *(float*)&value;
        }
    }
#endif
}
