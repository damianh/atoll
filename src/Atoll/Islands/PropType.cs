namespace Atoll.Islands;

/// <summary>
/// Defines the prop type tags used in the type-tagged serialization format
/// for island props. Each value is serialized as a <c>[type, value]</c> tuple
/// where the type tag indicates how to deserialize the value on the client.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>PROP_TYPE</c> constants in
/// <c>runtime/server/serialize.ts</c>. The integer values match Astro's
/// format exactly, enabling client-side JavaScript to deserialize props
/// using the same algorithm.
/// </para>
/// <para>
/// Format: Each prop value is encoded as <c>[PropType, serializedValue]</c>.
/// For <see cref="Value"/> with <c>null</c> or undefined, the tuple may be
/// just <c>[PropType]</c> (no second element).
/// </para>
/// </remarks>
public enum PropType
{
    /// <summary>
    /// A primitive value (string, number, boolean, null).
    /// Serialized as-is with no transformation.
    /// </summary>
    Value = 0,

    /// <summary>
    /// An array/JSON collection. Elements are recursively serialized
    /// as type-tagged tuples.
    /// </summary>
    Json = 1,

    /// <summary>
    /// A regular expression pattern. Serialized as the pattern source string.
    /// Maps to JavaScript <c>RegExp</c>.
    /// </summary>
    RegExp = 2,

    /// <summary>
    /// A date/time value. Serialized as an ISO 8601 string.
    /// Maps to <see cref="System.DateTime"/> and <see cref="System.DateTimeOffset"/>
    /// on the .NET side and <c>Date</c> on the JavaScript side.
    /// </summary>
    Date = 3,

    /// <summary>
    /// A key-value map. Serialized as an array of <c>[key, value]</c> pairs.
    /// Maps to <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>
    /// on the .NET side and <c>Map</c> on the JavaScript side.
    /// </summary>
    Map = 4,

    /// <summary>
    /// A set of unique values. Serialized as an array of values.
    /// Maps to <see cref="System.Collections.Generic.HashSet{T}"/> on the .NET side
    /// and <c>Set</c> on the JavaScript side.
    /// </summary>
    Set = 5,

    /// <summary>
    /// A big integer value. Serialized as a string representation.
    /// Maps to <see cref="System.Numerics.BigInteger"/> on the .NET side
    /// and <c>BigInt</c> on the JavaScript side.
    /// </summary>
    BigInt = 6,

    /// <summary>
    /// A URL value. Serialized as a string.
    /// Maps to <see cref="System.Uri"/> on the .NET side
    /// and <c>URL</c> on the JavaScript side.
    /// </summary>
    Url = 7,

    /// <summary>
    /// A byte array. Serialized as an array of byte values.
    /// Maps to <c>byte[]</c> on the .NET side and <c>Uint8Array</c> on the JavaScript side.
    /// </summary>
    Uint8Array = 8,

    /// <summary>
    /// A 16-bit unsigned integer array. Serialized as an array of values.
    /// Maps to <c>ushort[]</c> on the .NET side and <c>Uint16Array</c> on the JavaScript side.
    /// </summary>
    Uint16Array = 9,

    /// <summary>
    /// A 32-bit unsigned integer array. Serialized as an array of values.
    /// Maps to <c>uint[]</c> on the .NET side and <c>Uint32Array</c> on the JavaScript side.
    /// </summary>
    Uint32Array = 10,

    /// <summary>
    /// Positive or negative infinity. The value is <c>1</c> for positive infinity
    /// and <c>-1</c> for negative infinity.
    /// </summary>
    Infinity = 11,
}
