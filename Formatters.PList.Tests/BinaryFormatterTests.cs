// <copyright file="BinaryFormatterTests.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList.Tests;

public class BinaryFormatterTests
{
    [Fact]
    internal void Serialize()
    {
        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        using var stream = new MemoryStream();

#pragma warning disable SYSLIB0011 // Type or member is obsolete
        formatter.Invoking(f => f.Serialize(stream, Resources.PList)).Should().NotThrow();

        stream.Position = 0;

        formatter.Invoking(f => f.Deserialize(stream))
            .Should().NotThrow()
            .Which.Should().BeOfType<PList>()
            .And.BeEquivalentTo(Resources.PList);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
    }
}
