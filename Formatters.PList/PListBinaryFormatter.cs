// -----------------------------------------------------------------------
// <copyright file="PListBinaryFormatter.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList;

using static System.Buffers.Binary.BinaryPrimitives;

/// <summary>
/// A binary formatter for PList values.
/// </summary>
public partial class PListBinaryFormatter : System.Runtime.Serialization.IFormatter
{
    private static readonly DateTime Origin = new(2001, 1, 1, 0, 0, 0, 0);

    /// <inheritdoc/>
    public System.Runtime.Serialization.SerializationBinder? Binder { get; set; }

    /// <inheritdoc/>
    public System.Runtime.Serialization.StreamingContext Context { get; set; }

    /// <inheritdoc/>
    public System.Runtime.Serialization.ISurrogateSelector? SurrogateSelector { get; set; }

    /// <inheritdoc/>
    public object Deserialize(Stream serializationStream)
    {
        if (serializationStream is null)
        {
            throw new ArgumentNullException(nameof(serializationStream));
        }

        // see if this is a PList
        var header = serializationStream.Read(8);

        if (ReadInt64BigEndian(header) != 7093288613272891440)
        {
            throw new ArgumentException(Properties.Resources.StreamDoesNotContainAPList, nameof(serializationStream));
        }

        // get the last 32 bytes
        var trailer = serializationStream.Read(-32, 32, SeekOrigin.End);

        // parse the trailer
        var offsetByteSize = (int)trailer[6];
        var objectReferenceSize = (int)trailer[7];
        var numberObjects = ReadInt64BigEndian(trailer.AsSpan(8, 8));
        var topObjectOffset = ReadInt64BigEndian(trailer.AsSpan(16, 8));
        var offsetTableStart = ReadInt64BigEndian(trailer.AsSpan(24, 8));

        var offsetTable = new long[numberObjects];
        _ = serializationStream.Seek(offsetTableStart, SeekOrigin.Begin);
        if (numberObjects > 0)
        {
            var totalBytes = numberObjects * offsetByteSize;
            var buffer = serializationStream.Read((int)totalBytes).AsSpan();
            for (var i = 0; i < numberObjects; i++)
            {
                offsetTable[i] = GetInt64(buffer, offsetByteSize);
                buffer = buffer[offsetByteSize..];
            }
        }

        return Read(serializationStream, offsetTable, 0, objectReferenceSize) switch
        {
            IDictionary<string, object> dictionary => new PList(dictionary),
            _ => throw new InvalidOperationException(),
        };
    }

    /// <inheritdoc/>
    public void Serialize(Stream serializationStream, object graph)
    {
        if (serializationStream is null)
        {
            throw new ArgumentNullException(nameof(serializationStream));
        }

        var calculatedReferenceCount = CountReferences(graph);

        // calculate the reference size.
        var referenceSize = RegulateNullBytes(BitConverter.GetBytes(calculatedReferenceCount)).Length;

        // write the header
        Write(serializationStream, "bplist00", head: false);

        var offsetTable = new List<int> { (int)serializationStream.Position };
        Write(serializationStream, offsetTable, new List<object?>() { null }, referenceSize, graph);

        var offsetTableOffset = serializationStream.Length;
        var offsetByteSize = RegulateNullBytes(BitConverter.GetBytes(GetLast(offsetTable))).Length;

        for (var i = 0; i < offsetTable.Count; i++)
        {
            serializationStream.Write(RegulateNullBytes(BitConverter.GetBytes(offsetTable[i]), offsetByteSize).Reverse());
        }

        serializationStream.Write(new byte[6]);
        serializationStream.WriteByte(Convert.ToByte(offsetByteSize));
        serializationStream.WriteByte(Convert.ToByte(referenceSize));

        serializationStream.Write(BitConverter.GetBytes((long)offsetTable.Count).Reverse());

        serializationStream.Write(BitConverter.GetBytes(0L));
        serializationStream.Write(BitConverter.GetBytes(offsetTableOffset).Reverse());

        static int GetLast(IList<int> offsetTable)
        {
            return offsetTable[^1];
        }
    }

    private static int GetByteCount(byte[] value)
    {
        if (BitConverter.IsLittleEndian)
        {
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == 0)
                {
                    return i;
                }
            }
        }
        else
        {
            for (var i = value.Length - 1; i >= 0; i--)
            {
                if (value[i] == 0)
                {
                    return i;
                }
            }
        }

        return value.Length;
    }

    private static byte[] RegulateNullBytes(byte[] value, int minBytes = 1)
    {
        Array.Reverse(value);
        var bytes = new List<byte>(value);
        for (var i = 0; i < bytes.Count; i++)
        {
            if (bytes[i] == 0 && bytes.Count > minBytes)
            {
                if (bytes.Remove(bytes[i]))
                {
                    i--;
                }
            }
            else
            {
                break;
            }
        }

        if (bytes.Count < minBytes)
        {
            var dist = minBytes - bytes.Count;
            for (var i = 0; i < dist; i++)
            {
                bytes.Insert(0, 0);
            }
        }

        value = bytes.ToArray();
        Array.Reverse(value);
        return value;
    }
}
