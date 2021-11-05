// <copyright file="DictionaryExtensions.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList;

/// <summary>
/// Extensions for <see cref="IDictionary{TKey, TValue}"/> instances.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Gets the <see cref="Nullable{T}"/> <see cref="int"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="Nullable{T}"/> <see cref="int"/>.</returns>
    public static int? GetNullableInt32(this IDictionary<string, object?> dictionary, string key) => (int?)dictionary.GetNullableInt64(key);

    /// <summary>
    /// Gets the <see cref="int"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="int"/>.</returns>
    public static int GetInt32(this IDictionary<string, object?> dictionary, string key) => (int)dictionary.GetInt64(key);

    /// <summary>
    /// Gets the <see cref="Nullable{T}"/> <see cref="long"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="Nullable{T}"/> <see cref="long"/>.</returns>
    public static long? GetNullableInt64(this IDictionary<string, object?> dictionary, string key) =>
        dictionary.ContainsKey(key) ? (long?)dictionary[key] : default;

    /// <summary>
    /// Gets the <see cref="long"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="long"/>.</returns>
    public static long GetInt64(this IDictionary<string, object?> dictionary, string key) => (long)dictionary[key]!;

    /// <summary>
    /// Gets the <see cref="Nullable{T}"/> <see cref="bool"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="Nullable{T}"/> <see cref="bool"/>.</returns>
    public static bool? GetNullableBoolean(this IDictionary<string, object?> dictionary, string key) =>
        dictionary.ContainsKey(key) ? (bool?)dictionary[key] : default;

    /// <summary>
    /// Gets the <see cref="bool"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="bool"/>.</returns>
    public static bool GetBoolean(this IDictionary<string, object?> dictionary, string key) => (bool)dictionary[key]!;

    /// <summary>
    /// Gets the <see cref="Nullable{T}"/> <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="Nullable{T}"/> <see cref="DateTime"/>.</returns>
    public static DateTime? GetNullableDateTime(this IDictionary<string, object?> dictionary, string key) =>
        dictionary.ContainsKey(key) ? (DateTime?)dictionary[key] : default;

    /// <summary>
    /// Gets the <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="DateTime"/>.</returns>
    public static DateTime GetDateTime(this IDictionary<string, object?> dictionary, string key) => (DateTime)dictionary[key]!;

    /// <summary>
    /// Gets the <see cref="string"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="string"/> value.</returns>
    public static string? GetNullableString(this IDictionary<string, object?> dictionary, string key) =>
        dictionary.ContainsKey(key) ? (string?)dictionary[key] : default;

    /// <summary>
    /// Gets the <see cref="string"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>The <see cref="string"/> value.</returns>
    public static string GetString(this IDictionary<string, object?> dictionary, string key) => (string)dictionary[key]!;
}
