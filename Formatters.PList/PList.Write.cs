// <copyright file="PList.Write.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList;

/// <content>
/// <see cref="PList"/> write methods.
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
        WriteDictionary(writer, 0, this);
    }

    private static void WriteDictionary(XmlWriter writer, int indentLevel, System.Collections.IDictionary dictionary)
    {
        writer.WriteWhitespace(new string('\t', indentLevel));
        writer.WriteStartElement(DictionaryElementName);
        writer.WriteWhitespace(Environment.NewLine);

        var indent = new string('\t', indentLevel + 1);
        foreach (var key in dictionary.Keys)
        {
            if (dictionary[key] is object value)
            {
                writer.WriteWhitespace(indent);
                writer.WriteElementString(KeyElementName, key.ToString());
                WriteValue(writer, indentLevel + 1, value);
                writer.WriteWhitespace(Environment.NewLine);
            }
        }

        writer.WriteWhitespace(new string('\t', indentLevel));
        writer.WriteEndElement();
    }

    private static void WriteValue(XmlWriter writer, int indentLevel, object value)
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
            WriteDictionary(writer, indentLevel, dictionary);
        }
        else if (value is System.Collections.IEnumerable enumerable)
        {
            writer.WriteStartElement(ArrayElementName);
            foreach (var item in enumerable)
            {
                WriteValue(writer, indentLevel, item);
            }

            writer.WriteEndElement();
        }
    }
}
