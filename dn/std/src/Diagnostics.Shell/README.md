# GnomeStack.Diagnostics.Shell

Provides a simple way to run shell scripts or shell files from C#.

The interpreters must be installed and exist on the path, otherwise
the code will throw an exception if the interpreter is not found.

The shell runner currently supports the following shells/interpreters:

- bash
- sh
- cmd
- powershell
- pwsh
- python
- ruby
- deno
- node
- dotnet-script
- fsi / fsharp

## Usage

```csharp
using GnomeStack.Standard;

// run scripts inline
Shell.Run("bash", "echo 'Hello World'");

// run script files
Shell.RunFile("bash", "/home/user/bin/my-script.sh");

// when the shell is not specified, the runner will try to interpret the file based on the shebang.
// if the shebang is not present, it will try to run it based on the file extension
// if there is a matching executor otherwise it will throw and exception.
// shebang = #!

// sample file:
// #!/usr/bin/env bash
// echo 'Hello from dynamically determined script!'
// sleep 1
Shell.RunFile("/home/user/bin/my-script.sh");

var output = await Shell.RunAsync("python", "print('Hello, World!')", new PsStartInfo().WithStdio(Stdio.Piped));
Console.WriteLine(string.Join(Environment.NewLine, output.StdError));
Console.WriteLine(string.Join(Environment.NewLine, output.StdOut));
```

MIT License
