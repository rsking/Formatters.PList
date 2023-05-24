// -----------------------------------------------------------------------
// <copyright file="XmlPListDeserializeTests.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList.Tests;

/// <summary>
/// Tests for <see cref="PList"/>.
/// </summary>
public class XmlPListDeserializeTests : PListDeserializeTests
{
    private const string Xml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
        <plist version="1.0">
        <dict>
        </dict>
        </plist>
        """;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlPListDeserializeTests"/> class.
    /// </summary>
    public XmlPListDeserializeTests()
        : base(GetPList())
    {
    }

    [Fact]
    public void CreateFromString() => PList.Create(Xml).Should().NotBeNull().And.HaveCount(0);

    [Fact]
    public void CreateFromStream()
    {
        PList.Create(GenerateStreamFromString(Xml)).Should().NotBeNull().And.HaveCount(0);

        static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }

    internal static PList GetPList()
    {
        using var reader = System.Xml.XmlReader.Create(Resources.TestXml, new System.Xml.XmlReaderSettings
        {
            XmlResolver = default,
            DtdProcessing = System.Xml.DtdProcessing.Ignore,
        });

        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(PList));
        return (PList)serializer.Deserialize(reader)!;
    }
}
