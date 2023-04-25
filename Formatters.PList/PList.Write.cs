// <copyright file="PList.Write.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList;

/// <content>
/// <see cref="PList"/> <see cref="System.Xml.Serialization.IXmlSerializable"/> write methods.
/// </content>
public partial class PList
{
    /// <inheritdoc />
    public void WriteXml(XmlWriter writer)
    {
        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.WriteAttributeString("version", "1.0");
        var indentLevel = writer.Settings?.Indent != false ? 0 : -1;
        WriteDictionary(writer, indentLevel, writer.Settings?.IndentChars ?? "\t", this);
    }

    private static void WriteDictionary(XmlWriter writer, int indentLevel, string indentChars, System.Collections.IDictionary dictionary)
    {
        var baseIndent = CreateIndent(indentLevel, indentChars);
        writer.WriteWhitespace(baseIndent);

        writer.WriteStartElement(DictionaryElementName);
        writer.WriteWhitespace(Environment.NewLine);

        var indent = CreateIndent(indentLevel + 1, indentChars);
        foreach (var key in dictionary.Keys)
        {
            if (dictionary[key] is object value)
            {
                writer.WriteWhitespace(indent);
                writer.WriteElementString(KeyElementName, key.ToString());
                WriteValue(writer, indentLevel + 1, indentChars, value);
                writer.WriteWhitespace(Environment.NewLine);
            }
        }

        writer.WriteWhitespace(baseIndent);
        writer.WriteEndElement();
    }

    private static void WriteValue(XmlWriter writer, int indentLevel, string indentChars, object value)
    {
        if (value is bool boolValue)
        {
            writer.WriteStartElement(boolValue ? TrueElementName : FalseElementName);
            writer.WriteEndElement();
        }
        else if (value is int intValue)
        {
            writer.WriteElementString(IntegerElementName, intValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        else if (value is long longValue)
        {
            writer.WriteElementString(IntegerElementName, longValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        else if (value is float floatValue)
        {
            writer.WriteElementString(RealElementName, floatValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        else if (value is double doubleValue)
        {
            writer.WriteElementString(RealElementName, doubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        else if (value is string stringValue)
        {
            writer.WriteElementString(StringElementName, stringValue);
        }
        else if (value is DateTime dateValue)
        {
            writer.WriteElementString(DateElementName, dateValue.ToUniversalTime().ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "Z");
        }
        else if (value is byte[] byteValue)
        {
            writer.WriteElementString(DataElementName, Convert.ToBase64String(byteValue));
        }
        else if (value is System.Collections.IDictionary dictionary)
        {
            // put the dictionary on a new line.
            writer.WriteWhitespace(Environment.NewLine);
            WriteDictionary(writer, indentLevel, indentChars, dictionary);
        }
        else if (value is System.Collections.IEnumerable enumerable)
        {
            writer.WriteStartElement(ArrayElementName);
            foreach (var item in enumerable)
            {
                WriteValue(writer, indentLevel, indentChars, item);
            }

            writer.WriteEndElement();
        }
    }

    private static string? CreateIndent(int level, string chars)
    {
        return level switch
        {
            < 0 => default,
            0 => string.Empty,
            1 => chars,
            int l => CreateIndentCore(l, chars),
        };

        static string? CreateIndentCore(int level, string chars)
        {
            var charArray = new char[level * chars.Length];

            for (var i = 0; i < level; i+= chars.Length)
            {
                for (var k = 0; k < chars.Length; k++)
                {
                    charArray[i + k] = chars[k];
                }
            }

            return new string(charArray);
        }
    }
}
