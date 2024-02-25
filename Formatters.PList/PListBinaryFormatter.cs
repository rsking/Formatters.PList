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
    private static readonly DateTime Origin = new(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc/>
    public System.Runtime.Serialization.SerializationBinder? Binder { get; set; }

    /// <inheritdoc/>
    public System.Runtime.Serialization.StreamingContext Context { get; set; }

    /// <inheritdoc/>
    public System.Runtime.Serialization.ISurrogateSelector? SurrogateSelector { get; set; }

    /// <inheritdoc/>
    public object Deserialize(Stream serializationStream)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(serializationStream);
#else
        if (serializationStream is null)
        {
            throw new ArgumentNullException(nameof(serializationStream));
        }
#endif

        // see if this is a PList
        var header = serializationStream.Read(8);

        if (ReadInt64BigEndian(header) != 7093288613272891440)
        {
            throw new ArgumentException(Properties.Resources.StreamDoesNotContainAPList, nameof(serializationStream));
        }

        // get the last 32 bytes
        var trailer = serializationStream.Read(-32, 32, SeekOrigin.End);

        // parse the trailer
        var offsetByteSize = trailer[6];
        var objectReferenceSize = trailer[7];
        var numberObjects = ReadInt64BigEndian(trailer.AsSpan(8, 8));
        _ = ReadInt64BigEndian(trailer.AsSpan(16, 8));
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
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(serializationStream);
#else
        if (serializationStream is null)
        {
            throw new ArgumentNullException(nameof(serializationStream));
        }
#endif

        var calculatedReferenceCount = CountReferences(graph);

        // calculate the reference size.
        var referenceSize = GetByteCount(calculatedReferenceCount);

        // write the header
        Write(serializationStream, "bplist00", head: false);

        var offsetTable = new List<long> { serializationStream.Position };
        Write(serializationStream, offsetTable, [null], referenceSize, graph);

        var offsetTableOffset = serializationStream.Length;
        var offsetByteSize = GetByteCount(offsetTable[^1]);

        for (var i = 0; i < offsetTable.Count; i++)
        {
            var offsetTableSpan = GetSpan(offsetTable[i], offsetByteSize);
            serializationStream.Write(offsetTableSpan);
        }

        var trailer = new byte[32];
        trailer[6] = Convert.ToByte(offsetByteSize);
        trailer[7] = Convert.ToByte(referenceSize);

        Span<byte> trailerSpan = trailer;
        trailerSpan = trailerSpan[8..];
        WriteInt64BigEndian(trailerSpan, offsetTable.Count);

        trailerSpan = trailerSpan[sizeof(long)..];
        WriteInt64BigEndian(trailerSpan, 0L);

        trailerSpan = trailerSpan[sizeof(long)..];
        WriteInt64BigEndian(trailerSpan, offsetTableOffset);

        serializationStream.Write(trailer, 0, trailer.Length);
    }

    private static int GetByteCount(int value)
    {
        Span<byte> span = stackalloc byte[sizeof(int)];
        WriteInt32BigEndian(span, value);
        return GetByteCount(span);
    }

    private static int GetByteCount(long value)
    {
        Span<byte> span = stackalloc byte[sizeof(long)];
        WriteInt64BigEndian(span, value);
        return GetByteCount(span);
    }

    private static int GetByteCount(ReadOnlySpan<byte> value)
    {
        const byte Null = 0;

        for (var i = value.Length - 1; i >= 0; i--)
        {
            if (value[i] == Null)
            {
                return value.Length - i - 1;
            }
        }

        return value.Length;
    }

    private static ReadOnlySpan<byte> GetSpan(int value, int minBytes = 1)
    {
        Span<byte> span = new byte[sizeof(int)];
        WriteInt32BigEndian(span, value);
        Regulate(ref span, minBytes);
        return span;
    }

    private static ReadOnlySpan<byte> GetSpan(long value, int minBytes = 1)
    {
        Span<byte> span = new byte[sizeof(long)];
        WriteInt64BigEndian(span, value);
        Regulate(ref span, minBytes);
        return span;
    }

    private static void Regulate(ref Span<byte> span, int minBytes = 1)
    {
        var byteCount = GetByteCount(span);
        span = span[^Math.Max(byteCount, minBytes)..];
    }

    private static class DataType
    {
        public const byte Boolean = 0x00;
        public const byte Int64 = 0x10;
        public const byte Double = 0x20;
        public const byte DateTime = 0x30;
        public const byte Bytes = 0x40;
        public const byte Ascii = 0x50;
        public const byte Unicode = 0x60;
        public const byte Array = 0xA0;
        public const byte Dictionary = 0xD0;
    }
}
