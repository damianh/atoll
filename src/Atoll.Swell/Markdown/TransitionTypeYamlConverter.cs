using Atoll.Swell.Configuration;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Atoll.Swell.Markdown;

/// <summary>
/// Deserializes YAML transition strings (e.g., "fade", "slide-left") into <see cref="TransitionType"/> values.
/// </summary>
internal sealed class TransitionTypeYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(TransitionType);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        return scalar.Value.ToLowerInvariant() switch
        {
            "fade"        => TransitionType.Fade,
            "slide-left"  => TransitionType.SlideLeft,
            "slideleft"   => TransitionType.SlideLeft,
            "slide-right" => TransitionType.SlideRight,
            "slideright"  => TransitionType.SlideRight,
            "slide-up"    => TransitionType.SlideUp,
            "slideup"     => TransitionType.SlideUp,
            _             => TransitionType.None,
        };
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        => throw new NotSupportedException();
}
