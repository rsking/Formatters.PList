// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList;

/// <summary>
/// Extension methods.
/// </summary>
internal static class ExtensionMethods
{
    /// <summary>
    /// Reads a byte from the stream at the specified offset and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
    /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
    /// <returns>The unsigned byte.</returns>
    public static byte ReadByte(this Stream stream, long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        stream.Seek(offset, origin);
        return (byte)stream.ReadByte();
    }

    /// <summary>
    /// Reads a sequence of bytes from the stream at the specified offset and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="length">A number of bytes to read.</param>
    /// <returns>The unsigned byte array.</returns>
    public static byte[] Read(this Stream stream, int length) => stream.Read(0L, length, SeekOrigin.Current);

    /// <summary>
    /// Reads a sequence of bytes from the stream at the specified offset and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
    /// <param name="length">A number of bytes to read.</param>
    /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
    /// <returns>The unsigned byte array.</returns>
    public static byte[] Read(this Stream stream, long offset, int length, SeekOrigin origin = SeekOrigin.Begin)
    {
        var bytes = new byte[length];
        stream.Seek(offset, origin);
        _ = stream.Read(bytes, 0, length);
        return bytes;
    }

    /// <summary>
    /// Writes the specfied bytes to the stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="buffer">The bytes to write.</param>
    public static void Write(this Stream stream, byte[] buffer) => stream.Write(buffer, 0, buffer.Length);

#if NETSTANDARD2_0
    /// <summary>
    /// Writes the specfied bytes to the stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="buffer">The bytes to write.</param>
    public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
    {
        var bytes = new byte[buffer.Length];
        buffer.CopyTo(bytes);
        stream.Write(bytes, 0, buffer.Length);
    }
#endif
}
