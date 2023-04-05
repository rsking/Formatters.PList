// -----------------------------------------------------------------------
// <copyright file="BinaryPListSerializeTests.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList.Tests;

/// <summary>
/// Tests for <see cref="PListBinaryFormatter"/> writing.
/// </summary>
public class BinaryPListSerializeTests
{
    private readonly byte[] bytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryPListSerializeTests"/> class.
    /// </summary>
    public BinaryPListSerializeTests()
    {
        var formatter = new PListBinaryFormatter();

        using var stream = new MemoryStream();
        formatter.Serialize(stream, Resources.PList);
        stream.Position = 0;
        this.bytes = stream.ToArray();
    }

    [Fact]
    internal void Serialize()
    {
        var formatter = new PListBinaryFormatter();

        PList? testBin;
        using (var stream = Resources.TestBin)
        {
            testBin = formatter.Deserialize(stream) as PList;
        }

        testBin.Should().NotBeNull();

        PList? temp1;
        using (var stream = new MemoryStream(this.bytes))
        {
            temp1 = formatter.Deserialize(stream) as PList;
        }

        temp1.Should().NotBeNull();
        temp1!["testImage"].As<byte[]>().Should().BeEquivalentTo(testBin!["testImage"].As<byte[]>());
        temp1["testDictLarge"].As<IDictionary<string, object>>().Should().BeEquivalentTo(testBin["testDictLarge"].As<IDictionary<string, object>>());
        temp1.Should().BeEquivalentTo(Resources.PList);
        temp1.Should().BeEquivalentTo(testBin);
    }

    [Fact]
    internal void TestOutput()
    {
        //File.WriteAllBytes(@"C:\Users\rsking\Source\Repos\github\personal\rsking\Formatters.PList\Formatters.PList.Tests\outputBin.plist", this.bytes);
        using var stream = Resources.TestBin;
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        this.bytes.Should().BeEquivalentTo(memoryStream.ToArray());
    }
}
