// -----------------------------------------------------------------------
// <copyright file="BinaryPListDeserializeTests.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList.Tests;

/// <summary>
/// Tests for <see cref="PListBinaryFormatter"/> reading.
/// </summary>
public class BinaryPListDeserializeTests : PListDeserializeTests
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryPListDeserializeTests"/> class.
    /// </summary>
    public BinaryPListDeserializeTests()
        : base(GetPList())
    {
    }

    private static PList GetPList()
    {
        using var stream = Resources.TestBin;
        var formatter = new PListBinaryFormatter();
        var deserialized = formatter.Deserialize(stream);
        return (PList)deserialized!;
    }
}
