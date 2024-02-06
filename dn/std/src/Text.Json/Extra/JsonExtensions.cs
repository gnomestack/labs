using System.Diagnostics.CodeAnalysis;

namespace GnomeStack.Extras.Json;

public static class JsonExtensions
{
    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static T? FromJson<[Dam(Dat.PublicProperties)] T>(this ReadOnlySpan<char> value)
        => Standard.Json.JsonSerializerProvider.Deserialize<T>(value);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static object? FromJson(this ReadOnlySpan<char> value, [Dam(Dat.PublicProperties)] Type type)
        => Standard.Json.JsonSerializerProvider.Deserialize(value, type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static T? FromJson<[Dam(Dat.PublicProperties)] T>(this Stream value)
        => Standard.Json.JsonSerializerProvider.Deserialize<T>(value);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static object? FromJson(this Stream value, [Dam(Dat.PublicProperties)] Type type)
        => Standard.Json.JsonSerializerProvider.Deserialize(value, type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<T> FromJsonAsync<[Dam(Dat.PublicProperties)] T>(this string value, CancellationToken cancellationToken = default)
        => Standard.Json.JsonSerializerProvider.DeserializeAsync<T>(value, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<object?> FromJsonAsync(this string value, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default)
        => Standard.Json.JsonSerializerProvider.DeserializeAsync(value, type, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static T? ReadJson<[Dam(Dat.PublicProperties)] T>(this Stream stream)
        => Standard.Json.JsonSerializerProvider.Deserialize<T>(stream);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static object? ReadJson(this Stream stream, [Dam(Dat.PublicProperties)] Type type)
        => Standard.Json.JsonSerializerProvider.Deserialize(stream, type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<T> ReadJsonAsync<T>(this Stream stream, CancellationToken cancellationToken = default)
        => Standard.Json.JsonSerializerProvider.DeserializeAsync<T>(stream, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<object?> ReadJsonAsync(this Stream stream, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default)
        => Standard.Json.JsonSerializerProvider.DeserializeAsync(stream, type, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static string ToJson<[Dam(Dat.PublicProperties)] T>(this T value)
        => Standard.Json.JsonSerializerProvider.Serialize(value);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static string ToJson(this object? value)
        => Standard.Json.JsonSerializerProvider.Serialize(value);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<string> ToJsonAsync<[Dam(Dat.PublicProperties)] T>(this T value, CancellationToken cancellationToken = default)
        => Standard.Json.JsonSerializerProvider.SerializeAsync<T>(value, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task<string> ToJsonAsync(this object value, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default)
        => Standard.Json.JsonSerializerProvider.SerializeAsync(value, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static void WriteJson<[Dam(Dat.PublicProperties)] T>(this Stream stream, T value)
        => Standard.Json.JsonSerializerProvider.Serialize(stream, value);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static void WriteJson(this Stream stream, object? value, [Dam(Dat.PublicProperties)] Type type)
        => Standard.Json.JsonSerializerProvider.Serialize(stream, value, type);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task WriteJsonAsync<[Dam(Dat.PublicProperties)] T>(this Stream stream, T value, CancellationToken cancellationToken = default)
        => Standard.Json.JsonSerializerProvider.SerializeAsync(stream, value, cancellationToken);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public static Task WriteJsonAsync(this Stream stream, object? value, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default)
        => Standard.Json.JsonSerializerProvider.SerializeAsync(stream, value, type, cancellationToken);
}