using Atoll.Swell.Configuration;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Atoll.Swell.Markdown;

/// <summary>
/// Deserializes YAML aspect ratio strings (e.g., "16/9", "4:3") into <see cref="AspectRatio"/> values.
/// </summary>
internal sealed class AspectRatioYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(AspectRatio);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        return scalar.Value.Replace(':', '/') switch
        {
            "16/9" => AspectRatio.Ratio16x9,
            "4/3"  => AspectRatio.Ratio4x3,
            "3/2"  => AspectRatio.Ratio3x2,
            _      => AspectRatio.Ratio16x9,
        };
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        => throw new NotSupportedException();
}
