using System.Collections;
using System.Numerics;
using System.Text.Json;

namespace Atoll.Core.Islands;

/// <summary>
/// Serializes component props into the type-tagged JSON format used by
/// <c>&lt;atoll-island&gt;</c> elements for client-side hydration.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>serializeProps</c> function in
/// <c>runtime/server/serialize.ts</c>. Each prop value is encoded as a
/// <c>[PropType, serializedValue]</c> tuple, where the type tag tells the
/// client-side deserializer how to reconstruct the original value.
/// </para>
/// <para>
/// Supported types:
/// </para>
/// <list type="bullet">
/// <item>Primitives: <c>string</c>, <c>int</c>, <c>long</c>, <c>double</c>, <c>float</c>, <c>decimal</c>, <c>bool</c>, <c>null</c></item>
/// <item>Date/time: <see cref="DateTime"/>, <see cref="DateTimeOffset"/></item>
/// <item>URI: <see cref="Uri"/></item>
/// <item>Big integers: <see cref="BigInteger"/></item>
/// <item>Arrays: <c>T[]</c>, <see cref="IList"/>, <see cref="IEnumerable"/></item>
/// <item>Byte arrays: <c>byte[]</c>, <c>ushort[]</c>, <c>uint[]</c></item>
/// <item>Maps: <see cref="IDictionary"/></item>
/// <item>Sets: <see cref="ISet{T}"/> (via <see cref="HashSet{T}"/>)</item>
/// <item>Objects: POCO types (serialized by public properties)</item>
/// </list>
/// <para>
/// Cyclic references are detected and result in an <see cref="InvalidOperationException"/>.
/// </para>
/// </remarks>
public static class PropSerializer
{
    /// <summary>
    /// Serializes a props dictionary into the type-tagged JSON string format.
    /// </summary>
    /// <param name="props">The props dictionary to serialize.</param>
    /// <returns>A JSON string containing the type-tagged serialized props.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="props"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a cyclic reference is detected in the props object graph.
    /// </exception>
    public static string Serialize(IReadOnlyDictionary<string, object?> props)
    {
        return Serialize(props, "unknown");
    }

    /// <summary>
    /// Serializes a props dictionary into the type-tagged JSON string format.
    /// </summary>
    /// <param name="props">The props dictionary to serialize.</param>
    /// <param name="componentDisplayName">
    /// The display name of the component, used in error messages for cyclic reference detection.
    /// </param>
    /// <returns>A JSON string containing the type-tagged serialized props.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="props"/> or <paramref name="componentDisplayName"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a cyclic reference is detected in the props object graph.
    /// </exception>
    public static string Serialize(IReadOnlyDictionary<string, object?> props, string componentDisplayName)
    {
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(componentDisplayName);

        var detector = new CycleDetector();

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();

        foreach (var kvp in props)
        {
            writer.WritePropertyName(kvp.Key);
            WriteSerializedForm(writer, kvp.Value, componentDisplayName, detector);
        }

        writer.WriteEndObject();
        writer.Flush();

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Serializes a single value into its type-tagged form and writes it to
    /// the JSON writer as a <c>[type, value]</c> array.
    /// </summary>
    internal static void WriteSerializedForm(
        Utf8JsonWriter writer,
        object? value,
        string componentDisplayName,
        CycleDetector detector)
    {
        switch (value)
        {
            case null:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Value);
                writer.WriteNullValue();
                writer.WriteEndArray();
                return;

            case DateTime dt:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Date);
                writer.WriteStringValue(dt.ToUniversalTime().ToString("O"));
                writer.WriteEndArray();
                return;

            case DateTimeOffset dto:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Date);
                writer.WriteStringValue(dto.UtcDateTime.ToString("O"));
                writer.WriteEndArray();
                return;

            case Uri uri:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Url);
                writer.WriteStringValue(uri.ToString());
                writer.WriteEndArray();
                return;

            case BigInteger bigInt:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.BigInt);
                writer.WriteStringValue(bigInt.ToString());
                writer.WriteEndArray();
                return;

            case double d when double.IsPositiveInfinity(d):
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Infinity);
                writer.WriteNumberValue(1);
                writer.WriteEndArray();
                return;

            case double d when double.IsNegativeInfinity(d):
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Infinity);
                writer.WriteNumberValue(-1);
                writer.WriteEndArray();
                return;

            case float f when float.IsPositiveInfinity(f):
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Infinity);
                writer.WriteNumberValue(1);
                writer.WriteEndArray();
                return;

            case float f when float.IsNegativeInfinity(f):
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Infinity);
                writer.WriteNumberValue(-1);
                writer.WriteEndArray();
                return;

            case byte[] bytes:
                WriteTypedArray(writer, PropType.Uint8Array, bytes);
                return;

            case ushort[] ushorts:
                WriteTypedArray(writer, PropType.Uint16Array, ushorts);
                return;

            case uint[] uints:
                WriteTypedArray(writer, PropType.Uint32Array, uints);
                return;

            // Primitives
            case string s:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Value);
                writer.WriteStringValue(s);
                writer.WriteEndArray();
                return;

            case bool b:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Value);
                writer.WriteBooleanValue(b);
                writer.WriteEndArray();
                return;

            case int i:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Value);
                writer.WriteNumberValue(i);
                writer.WriteEndArray();
                return;

            case long l:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Value);
                writer.WriteNumberValue(l);
                writer.WriteEndArray();
                return;

            case double d:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Value);
                writer.WriteNumberValue(d);
                writer.WriteEndArray();
                return;

            case float f:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Value);
                writer.WriteNumberValue(f);
                writer.WriteEndArray();
                return;

            case decimal dec:
                writer.WriteStartArray();
                writer.WriteNumberValue((int)PropType.Value);
                writer.WriteNumberValue(dec);
                writer.WriteEndArray();
                return;

            default:
                // Check for sets (before IDictionary and IEnumerable since sets implement IEnumerable)
                if (IsGenericSet(value))
                {
                    WriteSet(writer, (IEnumerable)value, componentDisplayName, detector);
                    return;
                }

                // Check for dictionaries/maps
                if (value is IDictionary dict)
                {
                    WriteMap(writer, dict, componentDisplayName, detector);
                    return;
                }

                // Check for arrays/lists
                if (value is IEnumerable enumerable and not string)
                {
                    WriteArray(writer, enumerable, componentDisplayName, detector);
                    return;
                }

                // Object — serialize public properties
                WriteObject(writer, value, componentDisplayName, detector);
                return;
        }
    }

    private static bool IsGenericSet(object value)
    {
        var type = value.GetType();

        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ISet<>))
            {
                return true;
            }
        }

        return false;
    }

    private static void WriteTypedArray(Utf8JsonWriter writer, PropType propType, byte[] values)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue((int)propType);
        writer.WriteStartArray();
        foreach (var v in values)
        {
            writer.WriteNumberValue(v);
        }
        writer.WriteEndArray();
        writer.WriteEndArray();
    }

    private static void WriteTypedArray(Utf8JsonWriter writer, PropType propType, ushort[] values)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue((int)propType);
        writer.WriteStartArray();
        foreach (var v in values)
        {
            writer.WriteNumberValue(v);
        }
        writer.WriteEndArray();
        writer.WriteEndArray();
    }

    private static void WriteTypedArray(Utf8JsonWriter writer, PropType propType, uint[] values)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue((int)propType);
        writer.WriteStartArray();
        foreach (var v in values)
        {
            writer.WriteNumberValue(v);
        }
        writer.WriteEndArray();
        writer.WriteEndArray();
    }

    private static void WriteArray(
        Utf8JsonWriter writer,
        IEnumerable values,
        string componentDisplayName,
        CycleDetector detector)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue((int)PropType.Json);
        writer.WriteStartArray();
        foreach (var item in values)
        {
            WriteSerializedForm(writer, item, componentDisplayName, detector);
        }
        writer.WriteEndArray();
        writer.WriteEndArray();
    }

    private static void WriteSet(
        Utf8JsonWriter writer,
        IEnumerable values,
        string componentDisplayName,
        CycleDetector detector)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue((int)PropType.Set);
        writer.WriteStartArray();
        foreach (var item in values)
        {
            WriteSerializedForm(writer, item, componentDisplayName, detector);
        }
        writer.WriteEndArray();
        writer.WriteEndArray();
    }

    private static void WriteMap(
        Utf8JsonWriter writer,
        IDictionary dict,
        string componentDisplayName,
        CycleDetector detector)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue((int)PropType.Map);
        writer.WriteStartArray();
        foreach (DictionaryEntry entry in dict)
        {
            // Each entry is a [key, value] pair
            writer.WriteStartArray();
            WriteSerializedForm(writer, entry.Key, componentDisplayName, detector);
            WriteSerializedForm(writer, entry.Value, componentDisplayName, detector);
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
        writer.WriteEndArray();
    }

    private static void WriteObject(
        Utf8JsonWriter writer,
        object value,
        string componentDisplayName,
        CycleDetector detector)
    {
        using var guard = detector.Enter(value, componentDisplayName);

        writer.WriteStartArray();
        writer.WriteNumberValue((int)PropType.Value);
        writer.WriteStartObject();

        var properties = value.GetType().GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (!prop.CanRead)
            {
                continue;
            }

            var propValue = prop.GetValue(value);
            writer.WritePropertyName(prop.Name);
            WriteSerializedForm(writer, propValue, componentDisplayName, detector);
        }

        writer.WriteEndObject();
        writer.WriteEndArray();
    }
}
