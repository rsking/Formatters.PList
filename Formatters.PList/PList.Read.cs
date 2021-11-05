// <copyright file="PList.Read.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList;

using System.Xml;

/// <content>
/// <see cref="PList"/> read methods.
/// </content>
public partial class PList
{
    /// <inheritdoc />
    public void ReadXml(XmlReader reader)
    {
        if (reader is null || !string.Equals(reader.Name, PListElementName, StringComparison.Ordinal) || reader.NodeType != XmlNodeType.Element)
        {
            return;
        }

        this.Version = Version.Parse(reader.GetAttribute("version") ?? throw new ArgumentException(Properties.Resources.FailedToReadVersion, nameof(reader)));

        // read through the reader
        if (!ReadWhileWhiteSpace(reader))
        {
            return;
        }

        // read through the dictionary
        if (!string.Equals(reader.Name, DictionaryElementName, StringComparison.Ordinal))
        {
            return;
        }

        this.DictionaryImplementation = ReadDictionary(reader);

        if (ReadWhileWhiteSpace(reader) && string.Equals(reader.Name, PListElementName, StringComparison.Ordinal) && reader.NodeType == XmlNodeType.EndElement)
        {
            return;
        }

        throw new ArgumentException(Properties.Resources.InvalidXml, nameof(reader));
    }

    private static IDictionary<string, object> ReadDictionary(XmlReader reader)
    {
        IDictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
        string? key = null;
        object? value = null;

        while (ReadWhileWhiteSpace(reader))
        {
            if (string.Equals(reader.Name, DictionaryElementName, StringComparison.Ordinal) && reader.NodeType == XmlNodeType.EndElement)
            {
                return dictionary;
            }

            if (string.Equals(reader.Name, KeyElementName, StringComparison.Ordinal))
            {
                _ = ReadWhileWhiteSpace(reader);
                if (key is not null)
                {
                    AddToDictionary();
                }

                key = reader.Value;
                _ = ReadWhileWhiteSpace(reader);
                continue;
            }

            value = ReadValue(reader);
            AddToDictionary();

            void AddToDictionary()
            {
                dictionary.Add(key!, value!);
                key = null;
                value = null;
            }
        }

        return dictionary;
    }

    private static object? ReadValue(XmlReader reader)
    {
        return reader.Name switch
        {
            TrueElementName => true,
            FalseElementName => false,
            IntegerElementName => ReadInteger(reader),
            RealElementName => ReadDouble(reader),
            StringElementName => ReadString(reader),
            DateElementName => ReadDate(reader),
            DictionaryElementName => ReadDictionary(reader),
            ArrayElementName => ReadArray(reader),
            DataElementName => ReadData(reader),
            _ => throw new ArgumentException(Properties.Resources.InvalidPListValueType, nameof(reader)),
        };

        static long ReadInteger(XmlReader reader)
        {
            _ = ReadWhileWhiteSpace(reader);
            var longValue = long.Parse(reader.Value, System.Globalization.CultureInfo.InvariantCulture);
            _ = ReadWhileWhiteSpace(reader);
            return longValue;
        }

        static double ReadDouble(XmlReader reader)
        {
            _ = ReadWhileWhiteSpace(reader);
            var doubleValue = double.Parse(reader.Value, System.Globalization.CultureInfo.InvariantCulture);
            _ = ReadWhileWhiteSpace(reader);
            return doubleValue;
        }

        static string? ReadString(XmlReader reader)
        {
            // read until the end
            var builder = new System.Text.StringBuilder();
            var first = true;
            while (ReadWhileWhiteSpace(reader))
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    return builder.ToString();
                }

                if (!first)
                {
                    _ = builder.AppendLine();
                }

                first = false;
                _ = builder.Append(reader.Value);
            }

            return first ? null : builder.ToString();
        }

        static DateTime ReadDate(XmlReader reader)
        {
            _ = ReadWhileWhiteSpace(reader);
            var dateValue = DateTime.Parse(reader.Value, System.Globalization.CultureInfo.InvariantCulture);
            _ = ReadWhileWhiteSpace(reader);
            return dateValue;
        }

        static object?[] ReadArray(XmlReader reader)
        {
            var list = new List<object?>();

            while (ReadWhileWhiteSpace(reader))
            {
                if (string.Equals(reader.Name, ArrayElementName, StringComparison.Ordinal) && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                list.Add(ReadValue(reader));
            }

            return list.ToArray();
        }

        static byte[] ReadData(XmlReader reader)
        {
            _ = ReadWhileWhiteSpace(reader);
            var dataValue = Convert.FromBase64String(reader.Value);
            _ = ReadWhileWhiteSpace(reader);
            return dataValue;
        }
    }

    private static bool ReadWhileWhiteSpace(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Whitespace)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
