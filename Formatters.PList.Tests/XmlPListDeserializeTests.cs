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
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlPListDeserializeTests"/> class.
    /// </summary>
    public XmlPListDeserializeTests()
        : base(GetPList())
    {
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
