// -----------------------------------------------------------------------
// <copyright file="DictionaryExtensionsTests.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Formatters.PList.Tests;

/// <summary>
/// Tests for <see cref="DictionaryExtensions"/>.
/// </summary>
public class DictionaryExtensionsTests
{
    [Fact]
    internal void TestGetNullableInt32WithValidData() => TestGetWithValidData(DictionaryExtensions.GetNullableInt32, 1234L);

    [Fact]
    internal void TestGetInt32WithValidData() => TestGetWithValidData(DictionaryExtensions.GetInt32, 1234L);

    [Fact]
    internal void TestGetNullableInt32WithNullData() => TestGetWithNullData(DictionaryExtensions.GetNullableInt32, default(int?));

    [Fact]
    internal void TestGetNullableInt32WithNoKey() => TestGetWithNoKey(DictionaryExtensions.GetNullableInt32);

    [Fact]
    internal void TestGetNullableInt32WithInvalidData() => TestGetWithInvalidData(DictionaryExtensions.GetNullableInt32);

    [Fact]
    internal void TestGetNullableInt64WithValidData() => TestGetWithValidData(DictionaryExtensions.GetNullableInt64, 1234L);

    [Fact]
    internal void TestGetInt64WithValidData() => TestGetWithValidData(DictionaryExtensions.GetInt64, 1234L);

    [Fact]
    internal void TestGetNullableInt64WithNullData() => TestGetWithNullData(DictionaryExtensions.GetNullableInt64, default(long?));

    [Fact]
    internal void TestGetNullableInt64WithNoKey() => TestGetWithNoKey(DictionaryExtensions.GetNullableInt64);

    [Fact]
    internal void TestGetNullableInt64WithInvalidData() => TestGetWithInvalidData(DictionaryExtensions.GetNullableInt64);

    [Fact]
    internal void TestGetNullableBooleanWithValidData() => TestGetWithValidData(DictionaryExtensions.GetNullableBoolean, value: true);

    [Fact]
    internal void TestGetBooleanWithValidData() => TestGetWithValidData(DictionaryExtensions.GetBoolean, value: true);

    [Fact]
    internal void TestGetNullableBooleanWithNullData() => TestGetWithNullData(DictionaryExtensions.GetNullableBoolean, default(bool?));

    [Fact]
    internal void TestGetNullableBooleanWithNoKey() => TestGetWithNoKey(DictionaryExtensions.GetNullableBoolean);

    [Fact]
    internal void TestGetNullableBooleanWithInvalidData() => TestGetWithInvalidData(DictionaryExtensions.GetNullableBoolean);

    [Fact]
    internal void TestGetNullableDateTimeWithValidData() => TestGetWithValidData(DictionaryExtensions.GetNullableDateTime, DateTime.Now);

    [Fact]
    internal void TestGetDateTimeWithValidData() => TestGetWithValidData(DictionaryExtensions.GetDateTime, DateTime.Now);

    [Fact]
    internal void TestGetNullableDateTimeWithNullData() => TestGetWithNullData(DictionaryExtensions.GetNullableDateTime, default(DateTime?));

    [Fact]
    internal void TestGetNullableDateTimeWithNoKey() => TestGetWithNoKey(DictionaryExtensions.GetNullableDateTime);

    [Fact]
    internal void TestGetNullableDateTimeWithInvalidData() => TestGetWithInvalidData(DictionaryExtensions.GetNullableDateTime);

    [Fact]
    internal void TestGetNullableStringWithValidData() => DictionaryExtensions.GetNullableString(new Dictionary<string, object?>(StringComparer.Ordinal) { { "value", "value" } }, "value").Should().Be("value");

    [Fact]
    internal void TestGetStringWithValidData() => DictionaryExtensions.GetString(new Dictionary<string, object?>(StringComparer.Ordinal) { { "value", "value" } }, "value").Should().Be("value");

    [Fact]
    internal void TestGetNullableStringWithNoKey() => DictionaryExtensions.GetNullableString(new Dictionary<string, object?>(StringComparer.Ordinal) { { "value", "value" } }, "value_bad").Should().Be(default);

    [Fact]
    internal void TestGetNullableStringWithInvalidData() => new Dictionary<string, object?>(StringComparer.Ordinal) { { "value", 123456M } }.Invoking(values => values.GetNullableString("value")).Should().Throw<InvalidCastException>();

    private static void TestGetWithValidData<T1, T2>(Func<IDictionary<string, object?>, string, T1?> function, T2 value)
        where T1 : struct => function(new Dictionary<string, object?>(StringComparer.Ordinal) { { "value", value } }, "value").Should().Be(value);

    private static void TestGetWithValidData<T1, T2>(Func<IDictionary<string, object?>, string, T1> function, T2 value)
        where T1 : struct => function(new Dictionary<string, object?>(StringComparer.Ordinal) { { "value", value } }, "value").Should().Be(value);

    private static void TestGetWithNullData<T1, T2>(Func<IDictionary<string, object?>, string, T1?> function, T2 value)
        where T1 : struct => function(new Dictionary<string, object?>(StringComparer.Ordinal) { { "value", null } }, "value").Should().Be(value);

    private static void TestGetWithNoKey<T>(Func<IDictionary<string, object?>, string, T?> function)
        where T : struct => function(new Dictionary<string, object?>(StringComparer.Ordinal) { { "value", "value" } }, "value_bad").Should().Be(default(T?));

    private static void TestGetWithInvalidData<T>(Func<IDictionary<string, object?>, string, T?> function)
        where T : struct => new Dictionary<string, object?>(StringComparer.Ordinal) { { "value", 123456M } }.Invoking(_ => function(_, "value")).Should().Throw<InvalidCastException>();
}
