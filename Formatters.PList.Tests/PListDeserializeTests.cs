// <copyright file="PListDeserializeTests.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList.Tests;

public abstract class PListDeserializeTests
{
    private readonly PList plist;

    protected PListDeserializeTests(PList plist) => this.plist = plist;

    [Fact]
    internal void TestVersion() => this.plist.Version.Should().Be(new Version(1, 0));

    [Fact]
    internal void TestCount() => this.plist.Count.Should().Be(11);

    [Fact]
    internal void TestIsReadOnly() => this.plist.IsReadOnly.Should().BeFalse();

    [Fact]
    internal void TestBooleanTrue() => this.plist["testBoolTrue"].Should().BeOfType<bool>().Which.Should().BeTrue();

    [Fact]
    internal void TestBooleanFalse() => this.plist["testBoolFalse"].Should().BeOfType<bool>().Which.Should().BeFalse();

    [Fact]
    internal void TestArray() => this.plist["testArray"].Should().BeAssignableTo<IList<object>>().Which.Should().BeEquivalentTo(new object[] { 34, "string item in array" });

    [Fact]
    internal void TestLargeArray() => this.plist["testArrayLarge"].Should().BeAssignableTo<IList<object>>().Which.Should().BeEquivalentTo(Enumerable.Range(0, 18));

    [Fact]
    internal void TestDate() => this.plist["testDate"].Should().BeOfType<DateTime>().Which.Should().Be(new DateTime(2011, 09, 25, 2, 31, 4, DateTimeKind.Utc));

    [Fact]
    internal void TestDictionary() => this.plist["testDict"].Should().BeAssignableTo<IDictionary<string, object>>().Which.Should().BeEquivalentTo(new Dictionary<string, object>(StringComparer.Ordinal) { { "test string", "inner dict item" } });

    [Fact]
    internal void TestLargeDictionary() => this.plist["testDictLarge"].Should().BeAssignableTo<IDictionary<string, object>>().Which.Should().BeEquivalentTo(Enumerable.Range(0, 18).ToDictionary(i => i.ToString("00", System.Globalization.CultureInfo.InvariantCulture), i => i, StringComparer.Ordinal));

    [Fact]
    internal void TestDouble() => this.plist["testDouble"].Should().BeOfType<double>().Which.Should().Be(1.34223);

    [Fact]
    internal void TestInt() => this.plist["testInt"].Should().BeOfType<long>().Which.Should().Be(-3455);

    [Fact]
    internal void TestString() => this.plist["testString"].Should().BeOfType<string>().Which.Should().Be("hello there");
}
