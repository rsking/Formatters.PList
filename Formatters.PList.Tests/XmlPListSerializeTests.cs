// <copyright file="XmlPListSerializeTests.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList.Tests;

using System.Text;

public class XmlPListSerializeTests
{
    private readonly PList plist = new PList()
        {
            { "testArray", new object[] { 34, "string item in array" } },
            { "testArrayLarge", Enumerable.Range(0, 18).ToArray() },
            { "testBoolFalse", false },
            { "testBoolTrue", true },
            { "testDate", new DateTime(2011, 9, 25, 2, 31, 4, DateTimeKind.Utc) },
            { "testDict", new Dictionary<string, object>(StringComparer.Ordinal) { { "test string", "inner dict item" } } },
            { "testDictLarge", Enumerable.Range(0, 18).ToDictionary(i => i.ToString("00", System.Globalization.CultureInfo.InvariantCulture), i => i, StringComparer.Ordinal) },
            { "testDouble", 1.34223 },
            { "testImage", Resources.ImageBytes },
            { "testInt", -3455 },
            { "testString", "hello there" },
        };

    [Fact]
    internal void TestOutput()
    {
        var serialized = Serialize(this.plist);
        var resource = FromResource();

        serialized.Should().BeEquivalentTo(resource);

        static System.Xml.Linq.XDocument Serialize(PList value)
        {
            return Sanitize(value.ToString());
        }

        static System.Xml.Linq.XDocument FromResource()
        {
            using var stream = Resources.TestXml;
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return Sanitize(Encoding.UTF8.GetString(memoryStream.ToArray()));
        }

        static System.Xml.Linq.XDocument Sanitize(string input)
        {
            System.Xml.Linq.XDocument document;
            using (var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                document = System.Xml.Linq.XDocument.Load(inputStream);
            }

            // sanitize any data fields
            foreach (var element in document.Descendants("data"))
            {
                var text = Convert.ToBase64String(Convert.FromBase64String(element.Value), Base64FormattingOptions.None);
                element.Value = text;
            }

            return document;
        }
    }
}
