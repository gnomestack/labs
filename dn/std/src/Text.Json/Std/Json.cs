using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using GnomeStack.Text.Json;

namespace GnomeStack.Standard;

public static class Json
{
#pragma warning disable IL2026
    private static Lazy<IJsonSerializer> s_serializer = new(() => DefaultJsonSerializer.Instance);
#pragma warning restore IL2026

    public static IJsonSerializer JsonSerializerProvider
    {
        [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
        get => s_serializer.Value;
        set => s_serializer = new Lazy<IJsonSerializer>(() => value);
    }

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static string Stringify<[Dam(Dat.PublicProperties)] T>(T value)
        => JsonSerializerProvider.Serialize(value);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static string Stringify(object? value, [Dam(Dat.PublicProperties)] Type type)
        => JsonSerializerProvider.Serialize(value, type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static void Stringify<[Dam(Dat.PublicProperties)] T>(Stream stream, T value)
        => JsonSerializerProvider.Serialize(stream, value);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static void Stringify(Stream stream, object? value, [Dam(Dat.PublicProperties)] Type type)
        => JsonSerializerProvider.Serialize(stream, value, type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task StringifyAsync<[Dam(Dat.PublicProperties)] T>(Stream stream, T value, CancellationToken cancellationToken = default)
        => JsonSerializerProvider.SerializeAsync(stream, value, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task StringifyAsync(Stream stream, object? value, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default)
        => JsonSerializerProvider.SerializeAsync(stream, value, type, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<string> StringifyAsync<[Dam(Dat.PublicProperties)] T>(T value, CancellationToken cancellationToken = default)
        => JsonSerializerProvider.SerializeAsync(value, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<string> StringifyJsonAsync(object? value, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default)
        => JsonSerializerProvider.SerializeAsync(value, type, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static T Parse<[Dam(Dat.PublicProperties | Dat.PublicParameterlessConstructor)] T>(string json)
        => JsonSerializerProvider.Deserialize<T>(json);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static T Parse<[Dam(Dat.PublicProperties | Dat.PublicParameterlessConstructor)] T>(ReadOnlySpan<char> json)
        => JsonSerializerProvider.Deserialize<T>(json);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static object? Parse(ReadOnlySpan<char> json, [Dam(Dat.PublicProperties)] Type type)
        => JsonSerializerProvider.Deserialize(json, type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static T Parse<[Dam(Dat.PublicProperties)] T>(Stream stream)
        => JsonSerializerProvider.Deserialize<T>(stream);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static object? Parse(Stream stream, [Dam(Dat.PublicProperties)] Type type)
        => JsonSerializerProvider.Deserialize(stream, type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<T> ParseAsync<[Dam(Dat.PublicProperties)] T>(Stream stream, CancellationToken cancellationToken = default)
        => JsonSerializerProvider.DeserializeAsync<T>(stream, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<object?> ParseAsync(Stream stream, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default)
        => JsonSerializerProvider.DeserializeAsync(stream, type, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<T> ParseAsync<[Dam(Dat.PublicProperties)] T>(string json, CancellationToken cancellationToken = default)
        => JsonSerializerProvider.DeserializeAsync<T>(json, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<object?> ParseAsync(string json, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default)
        => JsonSerializerProvider.DeserializeAsync(json, type, cancellationToken);
}