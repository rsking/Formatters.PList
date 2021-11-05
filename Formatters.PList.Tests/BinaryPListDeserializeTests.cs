// -----------------------------------------------------------------------
// <copyright file="BinaryPListDeserializeTests.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList.Tests;

/// <summary>
/// Tests for <see cref="PListBinaryFormatter"/> reading.
/// </summary>
public class BinaryPListDeserializeTests
{
    private readonly PList plist;

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryPListDeserializeTests"/> class.
    /// </summary>
    public BinaryPListDeserializeTests()
    {
        using var stream = Resources.TestBin;
        var formatter = new PListBinaryFormatter();
        var deserialized = formatter.Deserialize(stream);
        this.plist = (PList)deserialized!;
    }

    [Fact]
    internal void TestVersion() => this.plist.Version.Should().Be(new Version(1, 0));

    [Fact]
    internal void TestCount() => this.plist.Count.Should().Be(11);

    [Fact]
    internal void TestIsReadOnly() => this.plist.IsReadOnly.Should().BeFalse();
}
