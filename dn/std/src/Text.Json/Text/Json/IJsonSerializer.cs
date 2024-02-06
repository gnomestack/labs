using System.Diagnostics.CodeAnalysis;

namespace GnomeStack.Text.Json;

public interface IJsonSerializer
{
    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    string Serialize<[Dam(Dat.PublicProperties)] T>(T value);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    string Serialize(object? value, [Dam(Dat.PublicProperties)] Type type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    void Serialize<[Dam(Dat.PublicProperties)] T>(Stream stream, T value);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    void Serialize(Stream stream, object? value, [Dam(Dat.PublicProperties)] Type type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    Task SerializeAsync<[Dam(Dat.PublicProperties)] T>(Stream stream, T value, CancellationToken cancellationToken = default);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    Task SerializeAsync(Stream stream, object? value, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    Task<string> SerializeAsync<[Dam(Dat.PublicProperties)] T>(T value, CancellationToken cancellationToken = default);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    Task<string> SerializeAsync(object? value, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    T Deserialize<[Dam(Dat.PublicProperties | Dat.PublicParameterlessConstructor)] T>(ReadOnlySpan<char> json);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    T Deserialize<[Dam(Dat.PublicProperties | Dat.PublicParameterlessConstructor)] T>(string json);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    object? Deserialize(ReadOnlySpan<char> json, [Dam(Dat.PublicProperties)] Type type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    object? Deserialize(string json, [Dam(Dat.PublicProperties)] Type type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    T Deserialize<[Dam(Dat.PublicProperties | Dat.PublicParameterlessConstructor)] T>(Stream stream);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    object? Deserialize(Stream stream, [Dam(Dat.PublicProperties)] Type type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    Task<T> DeserializeAsync<[Dam(Dat.PublicProperties | Dat.PublicParameterlessConstructor)] T>(Stream stream, CancellationToken cancellationToken = default);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    Task<object?> DeserializeAsync(Stream stream, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    Task<T> DeserializeAsync<[Dam(Dat.PublicProperties)] T>(string json, CancellationToken cancellationToken = default);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    Task<object?> DeserializeAsync(string json, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default);
}