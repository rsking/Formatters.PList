// <copyright file="PList.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList;

using System.Collections;
using System.Xml.Serialization;

/// <summary>
/// Represents a PList.
/// </summary>
[XmlRoot(PListElementName)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This is the correct name")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0049:Type name should not match containing namespace", Justification = "This is by design")]
public partial class PList : IDictionary<string, object>, IDictionary, IXmlSerializable
{
    private const string ArrayElementName = "array";

    private const string PListElementName = "plist";

    private const string IntegerElementName = "integer";

    private const string RealElementName = "real";

    private const string StringElementName = "string";

    private const string DictionaryElementName = "dict";

    private const string DataElementName = "data";

    private const string DateElementName = "date";

    private const string KeyElementName = "key";

    private const string TrueElementName = "true";

    private const string FalseElementName = "false";

    private static XmlSerializer? serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PList"/> class.
    /// </summary>
    public PList()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PList"/> class.
    /// </summary>
    /// <param name="dictionary">The dictionary.</param>
    internal PList(IDictionary<string, object> dictionary)
    {
        this.Version = new Version(1, 0);
        this.DictionaryImplementation = dictionary;
    }

    /// <inheritdoc />
    public int Count => this.DictionaryImplementation.Count;

    /// <inheritdoc />
    public bool IsReadOnly => this.DictionaryImplementation.IsReadOnly;

    /// <summary>
    /// Gets the version.
    /// </summary>
    public Version? Version { get; private set; }

    /// <inheritdoc />
    public ICollection<string> Keys => this.DictionaryImplementation.Keys;

    /// <inheritdoc />
    public ICollection<object> Values => this.DictionaryImplementation.Values;

    /// <inheritdoc />
    bool IDictionary.IsFixedSize => false;

    /// <inheritdoc />
    bool IDictionary.IsReadOnly => this.IsReadOnly;

    /// <inheritdoc />
    ICollection IDictionary.Keys => (ICollection)this.Keys;

    /// <inheritdoc />
    ICollection IDictionary.Values => (ICollection)this.Values;

    /// <inheritdoc />
    int ICollection.Count => this.Count;

    /// <inheritdoc />
    bool ICollection.IsSynchronized => false;

    /// <inheritdoc />
    object ICollection.SyncRoot => this;

    /// <summary>
    /// Gets the implementation.
    /// </summary>
    protected IDictionary<string, object> DictionaryImplementation { get; private set; } = new Dictionary<string, object>(StringComparer.Ordinal);

    private static XmlSerializer Serializer => serializer ??= new XmlSerializer(typeof(PList));

    /// <inheritdoc />
    public object this[string key]
    {
        get => this.DictionaryImplementation[key];
        set => this.DictionaryImplementation[key] = value;
    }

    /// <inheritdoc />
    object? IDictionary.this[object key]
    {
        get => this[GetStringOrThrow(key)];
        set => this[GetStringOrThrow(key)] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Creates a new <see cref="PList"/> using the specified string value.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>The created <see cref="PList"/>.</returns>
    public static PList Create(string value)
    {
        using var stream = XmlReader.Create(new StringReader(value), new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreWhitespace = true });
        return (PList)Serializer.Deserialize(stream)!;
    }

    /// <summary>
    /// Creates a new <see cref="PList"/> using the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The created <see cref="PList"/>.</returns>
    public static PList Create(Stream stream)
    {
        using var streamReader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
        using var reader = XmlReader.Create(streamReader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreWhitespace = true });
        return (PList)Serializer.Deserialize(reader)!;
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => this.DictionaryImplementation.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => this.DictionaryImplementation.GetEnumerator();

    /// <inheritdoc />
    public void Add(KeyValuePair<string, object> item) => this.DictionaryImplementation.Add(item);

    /// <inheritdoc />
    public void Clear() => this.DictionaryImplementation.Clear();

    /// <inheritdoc />
    public bool Contains(KeyValuePair<string, object> item) => this.DictionaryImplementation.Contains(item);

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => this.DictionaryImplementation.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(KeyValuePair<string, object> item) => this.DictionaryImplementation.Remove(item);

    /// <inheritdoc />
    public void Add(string key, object value) => this.DictionaryImplementation.Add(key, value);

    /// <inheritdoc />
    public bool ContainsKey(string key) => this.DictionaryImplementation.ContainsKey(key);

    /// <inheritdoc />
    public bool Remove(string key) => this.DictionaryImplementation.Remove(key);

    /// <inheritdoc />
    public bool TryGetValue(
        string key,
#if NET5_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)]
#endif
        out object value)
    {
        if (this.DictionaryImplementation.TryGetValue(key, out var innerValue) && innerValue is not null)
        {
            value = innerValue;
            return true;
        }

        value =
#if NET5_0_OR_GREATER
            default;
#else
            new object();
#endif
        return false;
    }

    /// <inheritdoc/>
    void IDictionary.Add(object key, object? value) => this.Add(GetStringOrThrow(key), value ?? throw new ArgumentNullException(nameof(value)));

    /// <inheritdoc/>
    void IDictionary.Clear() => this.Clear();

    /// <inheritdoc/>
    bool IDictionary.Contains(object key) => this.ContainsKey(GetStringOrThrow(key));

    /// <inheritdoc/>
    IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)this.DictionaryImplementation).GetEnumerator();

    /// <inheritdoc/>
    void IDictionary.Remove(object key) => this.Remove(GetStringOrThrow(key));

    /// <inheritdoc/>
    void ICollection.CopyTo(Array array, int index) => ((IDictionary)this.DictionaryImplementation).CopyTo(array, index);

    /// <inheritdoc />
    public System.Xml.Schema.XmlSchema? GetSchema() => default;

    private static string GetStringOrThrow(object key) => key.ToString() ?? throw new ArgumentNullException(nameof(key));
}
