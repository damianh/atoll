using System.Numerics;
using System.Text.Json;
using Atoll.Islands;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Islands;

public sealed class PropSerializerTests
{
    // ── Null value tests ──

    [Fact]
    public void ShouldSerializeNullValue()
    {
        var props = new Dictionary<string, object?> { ["name"] = null };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("name");
        arr[0].GetInt32().ShouldBe((int)PropType.Value);
        arr[1].ValueKind.ShouldBe(JsonValueKind.Null);
    }

    // ── String tests ──

    [Fact]
    public void ShouldSerializeStringValue()
    {
        var props = new Dictionary<string, object?> { ["title"] = "Hello World" };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("title");
        arr[0].GetInt32().ShouldBe((int)PropType.Value);
        arr[1].GetString().ShouldBe("Hello World");
    }

    [Fact]
    public void ShouldSerializeEmptyString()
    {
        var props = new Dictionary<string, object?> { ["text"] = "" };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("text");
        arr[0].GetInt32().ShouldBe((int)PropType.Value);
        arr[1].GetString().ShouldBe("");
    }

    // ── Boolean tests ──

    [Fact]
    public void ShouldSerializeBooleanTrue()
    {
        var props = new Dictionary<string, object?> { ["active"] = true };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("active");
        arr[0].GetInt32().ShouldBe((int)PropType.Value);
        arr[1].GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public void ShouldSerializeBooleanFalse()
    {
        var props = new Dictionary<string, object?> { ["active"] = false };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("active");
        arr[0].GetInt32().ShouldBe((int)PropType.Value);
        arr[1].GetBoolean().ShouldBeFalse();
    }

    // ── Integer tests ──

    [Fact]
    public void ShouldSerializeIntValue()
    {
        var props = new Dictionary<string, object?> { ["count"] = 42 };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("count");
        arr[0].GetInt32().ShouldBe((int)PropType.Value);
        arr[1].GetInt32().ShouldBe(42);
    }

    [Fact]
    public void ShouldSerializeLongValue()
    {
        var props = new Dictionary<string, object?> { ["big"] = 9999999999L };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("big");
        arr[0].GetInt32().ShouldBe((int)PropType.Value);
        arr[1].GetInt64().ShouldBe(9999999999L);
    }

    // ── Floating point tests ──

    [Fact]
    public void ShouldSerializeDoubleValue()
    {
        var props = new Dictionary<string, object?> { ["price"] = 19.99 };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("price");
        arr[0].GetInt32().ShouldBe((int)PropType.Value);
        arr[1].GetDouble().ShouldBe(19.99);
    }

    [Fact]
    public void ShouldSerializeFloatValue()
    {
        var props = new Dictionary<string, object?> { ["ratio"] = 0.5f };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("ratio");
        arr[0].GetInt32().ShouldBe((int)PropType.Value);
        arr[1].GetSingle().ShouldBe(0.5f);
    }

    [Fact]
    public void ShouldSerializeDecimalValue()
    {
        var props = new Dictionary<string, object?> { ["amount"] = 123.45m };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("amount");
        arr[0].GetInt32().ShouldBe((int)PropType.Value);
        arr[1].GetDecimal().ShouldBe(123.45m);
    }

    // ── Infinity tests ──

    [Fact]
    public void ShouldSerializePositiveInfinityDouble()
    {
        var props = new Dictionary<string, object?> { ["inf"] = double.PositiveInfinity };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("inf");
        arr[0].GetInt32().ShouldBe((int)PropType.Infinity);
        arr[1].GetInt32().ShouldBe(1);
    }

    [Fact]
    public void ShouldSerializeNegativeInfinityDouble()
    {
        var props = new Dictionary<string, object?> { ["inf"] = double.NegativeInfinity };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("inf");
        arr[0].GetInt32().ShouldBe((int)PropType.Infinity);
        arr[1].GetInt32().ShouldBe(-1);
    }

    [Fact]
    public void ShouldSerializePositiveInfinityFloat()
    {
        var props = new Dictionary<string, object?> { ["inf"] = float.PositiveInfinity };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("inf");
        arr[0].GetInt32().ShouldBe((int)PropType.Infinity);
        arr[1].GetInt32().ShouldBe(1);
    }

    [Fact]
    public void ShouldSerializeNegativeInfinityFloat()
    {
        var props = new Dictionary<string, object?> { ["inf"] = float.NegativeInfinity };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("inf");
        arr[0].GetInt32().ShouldBe((int)PropType.Infinity);
        arr[1].GetInt32().ShouldBe(-1);
    }

    // ── DateTime tests ──

    [Fact]
    public void ShouldSerializeDateTimeAsIso8601()
    {
        var dt = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        var props = new Dictionary<string, object?> { ["created"] = dt };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("created");
        arr[0].GetInt32().ShouldBe((int)PropType.Date);
        arr[1].GetString()!.ShouldContain("2024-06-15");
    }

    [Fact]
    public void ShouldSerializeDateTimeOffsetAsIso8601()
    {
        var dto = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero);
        var props = new Dictionary<string, object?> { ["created"] = dto };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("created");
        arr[0].GetInt32().ShouldBe((int)PropType.Date);
        arr[1].GetString()!.ShouldContain("2024-06-15");
    }

    // ── URI tests ──

    [Fact]
    public void ShouldSerializeAbsoluteUri()
    {
        var props = new Dictionary<string, object?> { ["link"] = new Uri("https://example.com/path") };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("link");
        arr[0].GetInt32().ShouldBe((int)PropType.Url);
        arr[1].GetString().ShouldBe("https://example.com/path");
    }

    // ── BigInteger tests ──

    [Fact]
    public void ShouldSerializeBigInteger()
    {
        var props = new Dictionary<string, object?>
        {
            ["huge"] = BigInteger.Parse("12345678901234567890")
        };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("huge");
        arr[0].GetInt32().ShouldBe((int)PropType.BigInt);
        arr[1].GetString().ShouldBe("12345678901234567890");
    }

    // ── Typed array tests ──

    [Fact]
    public void ShouldSerializeByteArray()
    {
        var props = new Dictionary<string, object?> { ["data"] = new byte[] { 1, 2, 255 } };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("data");
        arr[0].GetInt32().ShouldBe((int)PropType.Uint8Array);
        arr[1].GetArrayLength().ShouldBe(3);
        arr[1][0].GetInt32().ShouldBe(1);
        arr[1][1].GetInt32().ShouldBe(2);
        arr[1][2].GetInt32().ShouldBe(255);
    }

    [Fact]
    public void ShouldSerializeUshortArray()
    {
        var props = new Dictionary<string, object?> { ["data"] = new ushort[] { 100, 65535 } };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("data");
        arr[0].GetInt32().ShouldBe((int)PropType.Uint16Array);
        arr[1].GetArrayLength().ShouldBe(2);
        arr[1][0].GetInt32().ShouldBe(100);
        arr[1][1].GetInt32().ShouldBe(65535);
    }

    [Fact]
    public void ShouldSerializeUintArray()
    {
        var props = new Dictionary<string, object?> { ["data"] = new uint[] { 1000, 4294967295 } };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("data");
        arr[0].GetInt32().ShouldBe((int)PropType.Uint32Array);
        arr[1].GetArrayLength().ShouldBe(2);
        arr[1][0].GetUInt32().ShouldBe(1000u);
        arr[1][1].GetUInt32().ShouldBe(4294967295u);
    }

    // ── Array tests ──

    [Fact]
    public void ShouldSerializeArrayOfPrimitives()
    {
        var props = new Dictionary<string, object?>
        {
            ["tags"] = new object[] { "alpha", "beta", "gamma" }
        };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("tags");
        arr[0].GetInt32().ShouldBe((int)PropType.Json);
        arr[1].GetArrayLength().ShouldBe(3);

        // Each element is a [type, value] tuple
        arr[1][0][0].GetInt32().ShouldBe((int)PropType.Value);
        arr[1][0][1].GetString().ShouldBe("alpha");
    }

    [Fact]
    public void ShouldSerializeArrayOfMixedTypes()
    {
        var props = new Dictionary<string, object?>
        {
            ["mixed"] = new object?[] { "text", 42, true, null }
        };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("mixed");
        arr[0].GetInt32().ShouldBe((int)PropType.Json);
        arr[1].GetArrayLength().ShouldBe(4);
        arr[1][0][1].GetString().ShouldBe("text");
        arr[1][1][1].GetInt32().ShouldBe(42);
        arr[1][2][1].GetBoolean().ShouldBeTrue();
        arr[1][3][1].ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public void ShouldSerializeListAsArray()
    {
        var props = new Dictionary<string, object?>
        {
            ["items"] = new List<object> { "one", "two" }
        };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("items");
        arr[0].GetInt32().ShouldBe((int)PropType.Json);
        arr[1].GetArrayLength().ShouldBe(2);
    }

    [Fact]
    public void ShouldSerializeEmptyArray()
    {
        var props = new Dictionary<string, object?> { ["empty"] = Array.Empty<object>() };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("empty");
        arr[0].GetInt32().ShouldBe((int)PropType.Json);
        arr[1].GetArrayLength().ShouldBe(0);
    }

    // ── Set tests ──

    [Fact]
    public void ShouldSerializeHashSet()
    {
        var props = new Dictionary<string, object?>
        {
            ["uniqueTags"] = new HashSet<string> { "a", "b", "c" }
        };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("uniqueTags");
        arr[0].GetInt32().ShouldBe((int)PropType.Set);
        arr[1].GetArrayLength().ShouldBe(3);
    }

    // ── Map/Dictionary tests ──

    [Fact]
    public void ShouldSerializeDictionaryAsMap()
    {
        var dict = new Dictionary<string, object?> { ["x"] = 1, ["y"] = 2 };
        var props = new Dictionary<string, object?> { ["coords"] = dict };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("coords");
        arr[0].GetInt32().ShouldBe((int)PropType.Map);
        // Each entry is a [[type, key], [type, value]] pair
        arr[1].GetArrayLength().ShouldBe(2);
    }

    // ── Object tests ──

    [Fact]
    public void ShouldSerializeObjectWithPublicProperties()
    {
        var props = new Dictionary<string, object?>
        {
            ["user"] = new TestUser { Name = "Alice", Age = 30 }
        };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("user");
        arr[0].GetInt32().ShouldBe((int)PropType.Value);

        var obj = arr[1];
        obj.GetProperty("Name")[0].GetInt32().ShouldBe((int)PropType.Value);
        obj.GetProperty("Name")[1].GetString().ShouldBe("Alice");
        obj.GetProperty("Age")[0].GetInt32().ShouldBe((int)PropType.Value);
        obj.GetProperty("Age")[1].GetInt32().ShouldBe(30);
    }

    [Fact]
    public void ShouldSerializeNestedObjects()
    {
        var props = new Dictionary<string, object?>
        {
            ["data"] = new TestParent
            {
                Label = "root",
                Child = new TestChild { Value = 99 }
            }
        };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var dataArr = doc.RootElement.GetProperty("data");
        dataArr[0].GetInt32().ShouldBe((int)PropType.Value);

        var childArr = dataArr[1].GetProperty("Child");
        childArr[0].GetInt32().ShouldBe((int)PropType.Value);
        childArr[1].GetProperty("Value")[1].GetInt32().ShouldBe(99);
    }

    // ── Multiple props tests ──

    [Fact]
    public void ShouldSerializeMultipleProps()
    {
        var props = new Dictionary<string, object?>
        {
            ["title"] = "Hello",
            ["count"] = 5,
            ["active"] = true
        };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("title")[1].GetString().ShouldBe("Hello");
        doc.RootElement.GetProperty("count")[1].GetInt32().ShouldBe(5);
        doc.RootElement.GetProperty("active")[1].GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public void ShouldSerializeEmptyProps()
    {
        var props = new Dictionary<string, object?>();

        var json = PropSerializer.Serialize(props);

        json.ShouldBe("{}");
    }

    // ── Cycle detection tests ──

    [Fact]
    public void ShouldDetectDirectCyclicReference()
    {
        var cyclic = new TestCyclicNode { Name = "root" };
        cyclic.Next = cyclic; // Self-reference

        var props = new Dictionary<string, object?> { ["node"] = cyclic };

        var ex = Should.Throw<InvalidOperationException>(() =>
            PropSerializer.Serialize(props, "TestComponent"));

        ex.Message.ShouldContain("Cyclic reference detected");
        ex.Message.ShouldContain("TestComponent");
    }

    [Fact]
    public void ShouldDetectIndirectCyclicReference()
    {
        var nodeA = new TestCyclicNode { Name = "A" };
        var nodeB = new TestCyclicNode { Name = "B" };
        nodeA.Next = nodeB;
        nodeB.Next = nodeA; // Cycle: A → B → A

        var props = new Dictionary<string, object?> { ["node"] = nodeA };

        var ex = Should.Throw<InvalidOperationException>(() =>
            PropSerializer.Serialize(props, "MyIsland"));

        ex.Message.ShouldContain("Cyclic reference detected");
        ex.Message.ShouldContain("MyIsland");
    }

    [Fact]
    public void ShouldAllowSameObjectReferencedFromDifferentPaths()
    {
        // Shared reference (diamond) is NOT a cycle — each path is separate
        var shared = new TestChild { Value = 42 };
        var parent = new TestWithTwoChildren { Left = shared, Right = shared };

        var props = new Dictionary<string, object?> { ["parent"] = parent };

        // Should NOT throw — diamond references are fine (only cycles are disallowed)
        var json = PropSerializer.Serialize(props);

        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("parent")[1].GetProperty("Left")[1]
            .GetProperty("Value")[1].GetInt32().ShouldBe(42);
        doc.RootElement.GetProperty("parent")[1].GetProperty("Right")[1]
            .GetProperty("Value")[1].GetInt32().ShouldBe(42);
    }

    // ── Argument validation tests ──

    [Fact]
    public void ShouldThrowWhenPropsIsNull()
    {
        Should.Throw<ArgumentNullException>(() => PropSerializer.Serialize(null!));
    }

    [Fact]
    public void ShouldThrowWhenComponentDisplayNameIsNull()
    {
        var props = new Dictionary<string, object?>();

        Should.Throw<ArgumentNullException>(() => PropSerializer.Serialize(props, null!));
    }

    // ── Overload without display name ──

    [Fact]
    public void ShouldSerializeWithDefaultDisplayName()
    {
        var props = new Dictionary<string, object?> { ["x"] = 1 };

        // Should not throw and should produce valid JSON
        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("x")[1].GetInt32().ShouldBe(1);
    }

    // ── Complex nested scenario ──

    [Fact]
    public void ShouldSerializeComplexNestedStructure()
    {
        var props = new Dictionary<string, object?>
        {
            ["title"] = "Blog Post",
            ["tags"] = new object[] { "tech", "dotnet" },
            ["metadata"] = new TestUser { Name = "Author", Age = 25 },
            ["created"] = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ["url"] = new Uri("https://example.com/post/1"),
            ["active"] = true,
            ["score"] = 4.5
        };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("title")[1].GetString().ShouldBe("Blog Post");
        doc.RootElement.GetProperty("tags")[0].GetInt32().ShouldBe((int)PropType.Json);
        doc.RootElement.GetProperty("metadata")[0].GetInt32().ShouldBe((int)PropType.Value);
        doc.RootElement.GetProperty("created")[0].GetInt32().ShouldBe((int)PropType.Date);
        doc.RootElement.GetProperty("url")[0].GetInt32().ShouldBe((int)PropType.Url);
        doc.RootElement.GetProperty("active")[1].GetBoolean().ShouldBeTrue();
        doc.RootElement.GetProperty("score")[1].GetDouble().ShouldBe(4.5);
    }

    [Fact]
    public void ShouldProduceValidJson()
    {
        var props = new Dictionary<string, object?>
        {
            ["text"] = "with \"quotes\" and <html>",
            ["num"] = 42
        };

        var json = PropSerializer.Serialize(props);

        // Should be parseable
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("text")[1].GetString().ShouldBe("with \"quotes\" and <html>");
    }

    // ── Empty byte array test ──

    [Fact]
    public void ShouldSerializeEmptyByteArray()
    {
        var props = new Dictionary<string, object?> { ["data"] = Array.Empty<byte>() };

        var json = PropSerializer.Serialize(props);
        var doc = JsonDocument.Parse(json);

        var arr = doc.RootElement.GetProperty("data");
        arr[0].GetInt32().ShouldBe((int)PropType.Uint8Array);
        arr[1].GetArrayLength().ShouldBe(0);
    }

    // ── Test fixture types ──

    private sealed class TestUser
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private sealed class TestParent
    {
        public string Label { get; set; } = "";
        public TestChild? Child { get; set; }
    }

    private sealed class TestChild
    {
        public int Value { get; set; }
    }

    private sealed class TestCyclicNode
    {
        public string Name { get; set; } = "";
        public TestCyclicNode? Next { get; set; }
    }

    private sealed class TestWithTwoChildren
    {
        public TestChild? Left { get; set; }
        public TestChild? Right { get; set; }
    }
}
