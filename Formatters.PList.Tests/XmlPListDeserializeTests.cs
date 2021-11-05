// -----------------------------------------------------------------------
// <copyright file="XmlPListDeserializeTests.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList.Tests;

/// <summary>
/// Tests for <see cref="PList"/>.
/// </summary>
public class XmlPListDeserializeTests
{
    private readonly PList plist;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlPListDeserializeTests"/> class.
    /// </summary>
    public XmlPListDeserializeTests()
    {
        using var reader = System.Xml.XmlReader.Create(Resources.TestXml, new System.Xml.XmlReaderSettings
        {
            XmlResolver = default,
            DtdProcessing = System.Xml.DtdProcessing.Ignore,
        });
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(PList));
        var deserialized = serializer.Deserialize(reader);
        this.plist = (PList)deserialized!;
    }

    [Fact]
    internal void TestVersion() => this.plist.Version.Should().Be(new Version(1, 0));

    [Fact]
    internal void TestCount() => this.plist.Count.Should().Be(11);

    [Fact]
    internal void TestIsReadOnly() => this.plist.IsReadOnly.Should().BeFalse();
}
