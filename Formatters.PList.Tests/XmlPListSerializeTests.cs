// <copyright file="XmlPListSerializeTests.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList.Tests;

public class XmlPListSerializeTests
{
    private static readonly System.Xml.Serialization.XmlSerializer Serializer = new(typeof(PList));

    private readonly PList plist = new()
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

    private readonly PList plistWithAmpersand = new()
    {
        { "stringAmpersand", "Test value & second value" },
    };

    [Fact]
    public void TestOutput()
    {
        var serialized = Serialize(this.plist);
        var resource = FromResource();

        serialized.Should().BeEquivalentTo(resource);

        static System.Xml.Linq.XDocument Serialize(PList value)
        {
            using var memoryStream = new MemoryStream();
            var formatter = new PListAsciiFormatter();
            formatter.Serialize(memoryStream, value);
            return Sanitize(System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()));
        }

        static System.Xml.Linq.XDocument FromResource()
        {
            using var stream = Resources.TestXml;
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return Sanitize(System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()));
        }

        static System.Xml.Linq.XDocument Sanitize(string input)
        {
            System.Xml.Linq.XDocument document;
            using (var inputStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(input)))
            {
                document = System.Xml.Linq.XDocument.Load(inputStream);
            }

            // sanitize any data fields
            foreach (var element in document.Descendants("data"))
            {
                element.Value = Convert.ToBase64String(Convert.FromBase64String(element.Value), Base64FormattingOptions.None);
            }

            return document;
        }
    }

    [Fact]
    public void NullXmlWriter()
    {
        var action = () => new PList().WriteXml(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StringWithAmpersand()
    {
        var xml = Serialize(this.plistWithAmpersand);
        xml.Should().NotContain(" & ");

        static string Serialize(PList plist)
        {
            using var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, plist);
            return System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}
