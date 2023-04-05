// <copyright file="PListAsciiFormatterTests.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList.Tests;

public class PListAsciiFormatterTests
{
    private readonly System.Runtime.Serialization.IFormatter formatter = new PListAsciiFormatter();

    [Fact]
    public void NoBinder() => this.formatter.Binder.Should().BeNull();

    [Fact]
    public void DefaultContext() => this.formatter.Context.Should().Be(default(System.Runtime.Serialization.StreamingContext));

    [Fact]
    public void NoSurrogateSelector() => this.formatter.SurrogateSelector.Should().BeNull();

#pragma warning disable SYSLIB0011 // Type or member is obsolete
    [Fact]
    public void Deserialise()
    {
        var fromString = XmlPListDeserializeTests.GetPList();
        var fromStream = this.formatter.Deserialize(Resources.TestXml);
        fromStream.Should().BeOfType<PList>().Which.Should().BeEquivalentTo(fromString);
    }
#pragma warning restore SYSLIB0011 // Type or member is obsolete
}
