// <copyright file="PListAsciiFormatter.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList;

/// <summary>
/// An ASCII formatter for PList values.
/// </summary>
public class PListAsciiFormatter : System.Runtime.Serialization.IFormatter
{
    private static System.Xml.Serialization.XmlSerializer? serializer;

    /// <inheritdoc />
    public System.Runtime.Serialization.SerializationBinder? Binder { get; set; }

    /// <inheritdoc />
    public System.Runtime.Serialization.StreamingContext Context { get; set; }

    /// <inheritdoc />
    public System.Runtime.Serialization.ISurrogateSelector? SurrogateSelector { get; set; }

    private static System.Xml.Serialization.XmlSerializer Serializer => serializer ??= new System.Xml.Serialization.XmlSerializer(typeof(PList));

    /// <inheritdoc />
    public object Deserialize(Stream serializationStream)
    {
        using var streamReader = new StreamReader(serializationStream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
        using var reader = XmlReader.Create(streamReader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreWhitespace = true });
        return Serializer.Deserialize(reader)!;
    }

    /// <inheritdoc />
    public void Serialize(Stream serializationStream, object graph)
    {
        using var writer = new StreamWriter(serializationStream, System.Text.Encoding.UTF8);
        using var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Encoding = writer.Encoding });
        xmlWriter.WriteDocType("plist", "-//Apple//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", subset: null);
        Serializer.Serialize(xmlWriter, graph);
    }
}
