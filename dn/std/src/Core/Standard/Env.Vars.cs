using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

using GnomeStack.Extras.Strings;
using GnomeStack.Functional;

namespace GnomeStack.Standard;

/// <summary>
/// The <see cref="Env"/> class provides a set of static methods and properties that provide information about the
/// current environment and platform. The class is not designed for <c>using static</c> imports.
/// </summary>
public static partial class Env
{
    public static EnvVars Vars { get; } = new EnvVars();

    public static Option<string> OptionalVar(string name)
        => Option.From(Environment.GetEnvironmentVariable(name));

    public static string? Get(string name)
        => Environment.GetEnvironmentVariable(name);

    public static string? Get(string name, EnvironmentVariableTarget target)
        => Environment.GetEnvironmentVariable(name);

    public static Option<string> GetOptional(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return Option.From(value);
    }

    public static string GetRequired(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (value == null)
            throw new KeyNotFoundException($"Environment variable '{name}' not found.");

        return value;
    }

    public static bool Has(string name)
        => Environment.GetEnvironmentVariable(name) != null;

    public static void Set(string name, string value)
        => Environment.SetEnvironmentVariable(name, value);

    public static void Set(string name, string value, EnvironmentVariableTarget target)
        => Environment.SetEnvironmentVariable(name, value, target);

    public static void Remove(string name)
        => Environment.SetEnvironmentVariable(name, null);

    public static void Remove(string name, EnvironmentVariableTarget target)
        => Environment.SetEnvironmentVariable(name, null, target);

    public static bool TryGet(string name, [NotNullWhen(true)] out string? value)
    {
        value = Environment.GetEnvironmentVariable(name);
        return value != null;
    }

    public static IEnumerable<string> SplitPath(EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
    {
        var name = IsWindows ? "Path" : "PATH";
        var path = Get(name, target) ?? string.Empty;
        return path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
    }

    public static IEnumerable<string> SplitPath(string path)
        => path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

    public static string JoinPath(IEnumerable<string> paths)
    {
        var sb = new StringBuilder();
        foreach (var path in paths)
        {
            if (sb.Length > 0)
                sb.Append(Path.PathSeparator);

            sb.Append(path.ToArray());
        }

        return sb.ToString();
    }

    public sealed class EnvVars : IEnumerable<KeyValuePair<string, string>>
    {
        public KeyCollection Keys => new KeyCollection(System.Environment.GetEnvironmentVariables());

        public string? this[string name]
        {
            get => Environment.GetEnvironmentVariable(name);
            set => Environment.SetEnvironmentVariable(name, value);
        }

        public bool Contains(string name)
            => Environment.GetEnvironmentVariable(name) != null;

        public bool Contains(string name, EnvironmentVariableTarget target)
        {
            switch (target)
            {
                case EnvironmentVariableTarget.Process:
                    return Environment.GetEnvironmentVariable(name) != null;

                case EnvironmentVariableTarget.Machine:
                    {
                        if (Os.IsWindows())
                            return Environment.GetEnvironmentVariable(name, target) != null;

                        throw new PlatformNotSupportedException("Machine environment variables is only available on Windows.");
                    }

                case EnvironmentVariableTarget.User:
                    {
                        if (Os.IsWindows())
                            return Environment.GetEnvironmentVariable(name, target) != null;

                        throw new PlatformNotSupportedException("User environment variables is only available on Windows.");
                    }
            }

            return false;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (DictionaryEntry entry in System.Environment.GetEnvironmentVariables())
            {
                if (entry.Value is null)
                    continue;

                yield return new KeyValuePair<string, string>((string)entry.Key, (string)entry.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public class KeyCollection : ICollection<string>
        {
            private readonly IDictionary dictionary;

            public KeyCollection(IDictionary dictionary)
            {
                this.dictionary = dictionary;
            }

            public int Count => this.dictionary.Count;

            public bool IsReadOnly => true;

            public IEnumerator<string> GetEnumerator()
            {
                foreach (var key in this.dictionary.Keys)
                {
                    if (key is string name)
                        yield return name;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
                => this.GetEnumerator();

            public void Add(string item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(string item)
                => this.dictionary.Contains(item);

            public void CopyTo(string[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
#pragma warning disable S3218
            public bool Remove(string item)
            {
                throw new NotImplementedException();
            }
        }
    }
}