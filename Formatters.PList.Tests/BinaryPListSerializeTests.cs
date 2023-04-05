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
    private static readonly PList PList = new()
    {
        { "testBoolTrue", true },
        { "testDouble", 1.34223 },
        { "testImage", Resources.ImageBytes },
        {
            "testDictLarge",
            new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "15", 15 },
                { "03", 3 },
                { "07", 7 },
                { "06", 6 },
                { "04", 4 },
                { "12", 12 },
                { "02", 2 },
                { "17", 17 },
                { "01", 1 },
                { "05", 5 },
                { "08", 8 },
                { "09", 9 },
                { "10", 10 },
                { "00", 0 },
                { "11", 11 },
                { "13", 13 },
                { "14", 14 },
                { "16", 16 },
            }
        },
        { "testDate", new DateTime(2011, 9, 25, 2, 31, 04, DateTimeKind.Utc) },
        { "testDict", new Dictionary<string, object>(StringComparer.Ordinal) { { "test string", "inner dict item" } } },
        { "testInt", -3455 },
        { "testBoolFalse", false },
        { "testArray", new List<object> { 34, "string item in array" } },
        { "testArrayLarge", Enumerable.Range(0, 18).Cast<object>().ToList() },
        { "testString", "hello there" },
    };

    private readonly byte[] bytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryPListSerializeTests"/> class.
    /// </summary>
    public BinaryPListSerializeTests()
    {
        var formatter = new PListBinaryFormatter();

        using var stream = new MemoryStream();
        formatter.Serialize(stream, PList);
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
        temp1.Should().BeEquivalentTo(PList);
        temp1.Should().BeEquivalentTo(testBin);
    }

    [Fact]
    internal void TestOutput()
    {
        using var stream = Resources.TestBin;
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        this.bytes.Should().BeEquivalentTo(memoryStream.ToArray());
    }
}
