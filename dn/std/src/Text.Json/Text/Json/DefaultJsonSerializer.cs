using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using GnomeStack.Functional;

namespace GnomeStack.Text.Json;

[SuppressMessage("AsyncUsage", "AsyncFixer01:Unnecessary async/await usage")]
public class DefaultJsonSerializer : IJsonSerializer
{
    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public DefaultJsonSerializer()
    {
        this.Options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(Modifiers.CustomAttributes);
        this.Options.TypeInfoResolver = resolver;
    }

    public DefaultJsonSerializer(DefaultJsonTypeInfoResolver resolver)
    {
        this.Options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = resolver,
        };
    }

    public DefaultJsonSerializer(JsonSerializerOptions options)
    {
        this.Options = options;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public static DefaultJsonSerializer Instance { get; } = new();

    public JsonSerializerOptions Options { get; set; }

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public DefaultJsonSerializer WithGnomeStackAttributes()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(Modifiers.CustomAttributes);
        this.Options.TypeInfoResolver = resolver;
        return this;
    }

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public string Serialize<[Dam(Dat.PublicProperties)] T>(T value)
        => JsonSerializer.Serialize(value, this.Options);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public string Serialize(object? value, [Dam(Dat.PublicProperties)] Type type)
        => JsonSerializer.Serialize(value, type, this.Options);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public void Serialize<[Dam(Dat.PublicProperties)] T>(Stream stream, T value)
        => JsonSerializer.Serialize(stream, value, this.Options);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public void Serialize(Stream stream, object? value, [Dam(Dat.PublicProperties)] Type type)
        => JsonSerializer.Serialize(stream, value, type, this.Options);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public async Task SerializeAsync(
        Stream stream,
        object? value,
        [Dam(Dat.PublicProperties)] Type type,
        CancellationToken cancellationToken = default)
    {
        await JsonSerializer.SerializeAsync(stream, value, type, this.Options, cancellationToken)
            .ConfigureAwait(false);
    }

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public async Task<string> SerializeAsync(
        object? value,
        [Dam(Dat.PublicProperties)] Type type,
        CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, value, type, this.Options, cancellationToken)
            .ConfigureAwait(false);

        stream.Position = 0;
        using var sr = new StreamReader(stream);
#if NET7_0_OR_GREATER
        return await sr.ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);
#else
        return await sr.ReadToEndAsync()
            .ConfigureAwait(false);
#endif
    }

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public async Task SerializeAsync<[Dam(Dat.PublicProperties)] T>(
        Stream stream,
        T value,
        CancellationToken cancellationToken = default)
    {
        await JsonSerializer.SerializeAsync(stream, value, this.Options, cancellationToken)
            .ConfigureAwait(false);
    }

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public async Task<string> SerializeAsync<[Dam(Dat.PublicProperties)] T>(
        T value,
        CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, value, this.Options, cancellationToken)
            .ConfigureAwait(false);

        stream.Position = 0;
        using var sr = new StreamReader(stream);
#if NET7_0_OR_GREATER
        return await sr.ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);
#else
        return await sr.ReadToEndAsync()
            .ConfigureAwait(false);
#endif
    }

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public T Deserialize<[Dam(Dat.PublicProperties | Dat.PublicParameterlessConstructor)] T>(string json)
        => JsonSerializer.Deserialize<T>(json, this.Options) ?? Activator.CreateInstance<T>();

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public T Deserialize<[Dam(Dat.PublicProperties | Dat.PublicParameterlessConstructor)] T>(ReadOnlySpan<char> json)
        => JsonSerializer.Deserialize<T>(json, this.Options) ?? Activator.CreateInstance<T>();

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public object? Deserialize(string json, [Dam(Dat.PublicProperties)] Type type)
        => JsonSerializer.Deserialize(json, type, this.Options);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public object? Deserialize(ReadOnlySpan<char> json, [Dam(Dat.PublicProperties)] Type type)
        => JsonSerializer.Deserialize(json, type, this.Options);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public T Deserialize<[Dam(Dat.PublicProperties | Dat.PublicParameterlessConstructor)] T>(Stream stream)
        => JsonSerializer.Deserialize<T>(stream, this.Options) ?? Activator.CreateInstance<T>();

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public object? Deserialize(Stream stream, [Dam(Dat.PublicProperties)] Type type)
        => JsonSerializer.Deserialize(stream, type, this.Options);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public async Task<T> DeserializeAsync<[Dam(Dat.PublicProperties | Dat.PublicParameterlessConstructor)] T>(Stream stream, CancellationToken cancellationToken = default)
    {
        var result = await JsonSerializer.DeserializeAsync<T>(stream, this.Options, cancellationToken)
            .ConfigureAwait(false);

        return result ?? Activator.CreateInstance<T>();
    }

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public async Task<object?> DeserializeAsync(Stream stream, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default)
        => await JsonSerializer.DeserializeAsync(stream, type, this.Options, cancellationToken)
            .ConfigureAwait(false);

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public async Task<T> DeserializeAsync<[Dam(Dat.PublicProperties)] T>(string json, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
#if NETLEGACY
        using var sw = new StreamWriter(ms);
#else
        await using var sw = new StreamWriter(ms);
#endif
        await sw.WriteAsync(json)
            .ConfigureAwait(false);
        ms.Position = 0;

        var result = await JsonSerializer.DeserializeAsync<T>(ms, this.Options, cancellationToken)
            .ConfigureAwait(false);

        return result ?? Activator.CreateInstance<T>();
    }

    [RequiresUnreferencedCode(Messages.JsonUnreferencedCode)]
    public async Task<object?> DeserializeAsync(string json, [Dam(Dat.PublicProperties)] Type type, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
#if NETLEGACY
        using var sw = new StreamWriter(ms);
#else
        await using var sw = new StreamWriter(ms);
#endif
        await sw.WriteAsync(json)
            .ConfigureAwait(false);
        ms.Position = 0;

        return await JsonSerializer.DeserializeAsync(ms, type, this.Options, cancellationToken)
            .ConfigureAwait(false);
    }
}