// -----------------------------------------------------------------------
// <copyright file="PListBinaryFormatter.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList;

using System.IO;
using System.Runtime.Serialization;

/// <summary>
/// A binary formatter for PList values.
/// </summary>
public partial class PListBinaryFormatter : IFormatter
{
    private static readonly DateTime Origin = new(2001, 1, 1, 0, 0, 0, 0);

    /// <inheritdoc/>
    public SerializationBinder? Binder { get; set; }

    /// <inheritdoc/>
    public StreamingContext Context { get; set; }

    /// <inheritdoc/>
    public ISurrogateSelector? SurrogateSelector { get; set; }

    /// <inheritdoc/>
    public object Deserialize(Stream serializationStream)
    {
        if (serializationStream is null)
        {
            throw new ArgumentNullException(nameof(serializationStream));
        }

        // see if this is a PList
        var header = serializationStream.Read(8);

        if (BitConverter.ToInt64(header, 0) != 3472403351741427810)
        {
            throw new ArgumentException(Properties.Resources.StreamDoesNotContainAPList, nameof(serializationStream));
        }

        // get the last 32 bytes
        var trailer = serializationStream.Read(-32, 32, SeekOrigin.End);

        // parse the trailer
        var offsetByteSize = BitConverter.ToInt32(RegulateNullBytes(trailer, 6, 1, 4), 0);
        var objectReferenceSize = BitConverter.ToInt32(RegulateNullBytes(trailer, 7, 1, 4), 0);
        var refCountBytes = trailer.GetRange(12, 4);
        Array.Reverse(refCountBytes);
        var refCount = BitConverter.ToInt32(refCountBytes, 0);
        var offsetTableOffsetBytes = trailer.GetRange(24, 8);
        Array.Reverse(offsetTableOffsetBytes);
        var offsetTableOffset = BitConverter.ToInt64(offsetTableOffsetBytes, 0);

        var offsetTable = new int[refCount];
        _ = serializationStream.Seek(offsetTableOffset, SeekOrigin.Begin);
        for (var i = 0; i < refCount; i++)
        {
            var buffer = serializationStream.Read(offsetByteSize);
            Array.Reverse(buffer);
            offsetTable[i] = BitConverter.ToInt32(RegulateNullBytes(buffer, 4), 0);
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
#if NETSTANDARD2_1_OR_GREATER
            return offsetTable[^1];
#else
            return offsetTable[offsetTable.Count - 1];
#endif
        }
}

    private static byte[] RegulateNullBytes(byte[] value, int start, int length, int minBytes = 1) => RegulateNullBytes(value.GetRange(start, length), minBytes);

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
