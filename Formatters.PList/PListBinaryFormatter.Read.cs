// -----------------------------------------------------------------------
// <copyright file="PListBinaryFormatter.Read.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList;

using System.Text;

/// <summary>
/// The read methods for <see cref="PListBinaryFormatter"/>.
/// </summary>
public partial class PListBinaryFormatter
{
    private static object? Read(Stream stream, IList<int> offsetTable, int index, int objectReferenceSize)
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

    private static object ReadDictionary(Stream stream, IList<int> offsetTable, int index, int referenceSize)
    {
        var buffer = new Dictionary<string, object?>(StringComparer.Ordinal);
        var referenceCount = GetCount(stream, offsetTable[index], out _);

        // Check if the following integer has a header aswell so we increase the referenceStartPosition by two to account for that.
        var referenceStartPosition = referenceCount >= 15
            ? offsetTable[index] + 2 + RegulateNullBytes(BitConverter.GetBytes(referenceCount), 1).Length
            : offsetTable[index] + 1;

        var references = new int[referenceCount * 2];
        var current = referenceStartPosition;
        for (var i = 0; i < references.Length; i++)
        {
            var referenceBuffer = stream.Read(current, referenceSize).Reverse();
            references[i] = BitConverter.ToInt32(RegulateNullBytes(referenceBuffer, 4), 0);
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

    private static IList<object?> ReadArray(Stream stream, IList<int> offsetTable, int index, int referenceSize)
    {
        var buffer = new List<object?>();
        var referenceCount = GetCount(stream, offsetTable[index], out _);

        // Check if the following integer has a header aswell so we increase the referenceStartPosition by two to account for that.
        var referenceStartPosition = referenceCount >= 15
            ? offsetTable[index] + 2 + RegulateNullBytes(BitConverter.GetBytes(referenceCount), 1).Length
            : offsetTable[index] + 1;

        var references = new List<int>();
        for (var i = referenceStartPosition; i < referenceStartPosition + (referenceCount * referenceSize); i += referenceSize)
        {
            var referenceBuffer = stream.Read(i, referenceSize).Reverse();
            references.Add(BitConverter.ToInt32(RegulateNullBytes(referenceBuffer, 4), 0));
        }

        for (var i = 0; i < referenceCount; i++)
        {
            buffer.Add(Read(stream, offsetTable, references[i], referenceSize));
        }

        return buffer;
    }

    private static long ReadInt64(Stream stream, int headerPosition) => ReadInt64(stream, headerPosition, out _);

    private static long ReadInt64(Stream stream, int headerPosition, out int newHeaderPosition)
    {
        var header = stream.ReadByte(headerPosition);
        var byteCount = (int)Math.Pow(2, header & 0xf);

        var buffer = stream.Read(headerPosition + 1, byteCount).Reverse();

        // Add one to account for the header byte
        newHeaderPosition = headerPosition + byteCount + 1;
        return BitConverter.ToInt64(RegulateNullBytes(buffer, sizeof(long)), 0);
    }

    private static DateTime ReadDateTime(Stream stream, int headerPosition)
    {
        var buffer = stream.Read(headerPosition + 1, 8).Reverse();
        var appleTime = BitConverter.ToDouble(buffer, 0);
        return ConvertFromAppleTimeStamp(appleTime);

        static DateTime ConvertFromAppleTimeStamp(double timestamp)
        {
            return Origin.AddSeconds(timestamp);
        }
    }

    private static double ReadDouble(Stream stream, int headerPosition)
    {
        var header = stream.ReadByte(headerPosition);
        var byteCount = (int)Math.Pow(2, header & 0xf);
        var buffer = stream.Read(headerPosition + 1, byteCount).Reverse();
        return BitConverter.ToDouble(RegulateNullBytes(buffer, 8), 0);
    }

    private static string ReadAsciiString(Stream stream, int headerPosition)
    {
        var charCount = GetCount(stream, headerPosition, out var charStartPosition);
        var buffer = stream.Read(charStartPosition, charCount);
        return buffer.Length > 0 ? Encoding.UTF8.GetString(buffer) : string.Empty;
    }

    private static string ReadUnicodeString(Stream stream, int headerPosition)
    {
        var charCount = GetCount(stream, headerPosition, out var charStartPosition) * 2;
        var buffer = stream.Read(charStartPosition, charCount);
        return Encoding.BigEndianUnicode.GetString(buffer);
    }

    private static byte[] ReadBytes(Stream stream, int headerPosition)
    {
        var byteCount = GetCount(stream, headerPosition, out var byteStartPosition);
        return stream.Read(byteStartPosition, byteCount);
    }

    private static int GetCount(Stream stream, int bytePosition, out int newBytePosition)
    {
        var headerByte = stream.ReadByte(bytePosition);
        var headerByteTrail = Convert.ToByte(headerByte & 0xf);
        int count;
        if (headerByteTrail < 15)
        {
            count = headerByteTrail;
            newBytePosition = bytePosition + 1;
        }
        else
        {
            count = (int)ReadInt64(stream, bytePosition + 1, out newBytePosition);
        }

        return count;
    }
}
