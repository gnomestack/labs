namespace GnomeStack;

internal static class Messages
{
    public const string JsonUnreferencedCode
        = "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.";
}