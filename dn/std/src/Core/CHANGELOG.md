# CHANGE LOG

## 0.1.6

- Lib GnomeStack.Core reverted Env back to older API.

## 0.1.5

- Move source to github.com/gnomestack/dotnet-std from github.com/gnomestack/dotnet
- Rework the Result and Option types to use classes rather than structs to help with
  inheritance and allowing for replace and take methods.
- May added ValueResult and ValueOption at a later date.
- The the none generic result is `Result<Nil, Error>`.
- The result with one generic type is `Result<T, Error>`.
- Make Error the default TError type and rework Error to track
  the underlying exception if one was used to create the Error.
- Add GnomeStack.Extras.Functional with ToOption and ToResult extension
  methods.
- Move to using PolyFill library rather than using internal polyfills
  which decrease code to support.

## 0.1.2

- Move Color and Ansi namespaces to Fmt.Ansi and Fmt.Color.
- Fix Rgb methods on Standard.Ansi to applying Ansi codes to text when Ansi
  Mode is none like the rest of the Ansi methods.
- Add Map and MapError to Result module.
- Changes Result.ThrowIfError to return result instead of unwrapping
  the value.
- Changed PsCommand to be more of a builder pattern or allows to
  the implementation of PsArgs and executable to be changed.
- Added PsCommand to various Ps methods to make it easier to pass in
  objects that represent cli commands to be executed.
- Added ResourcePath class to help with common use case of strings
  that use '/', ':', or '.' as separator to navigate hierarchical
  objects or data.  

## 0.1.1

- Add Symbol struct for js/ruby like symbols of interned strings.
- Add Fs.ChangeOwner function to (chown) change the owner of a file.
- Add Fs.ChangeMode function to (chmod) change the mode of a file.
- Add Diagnostics.PsPathRegistry to store executable paths and
  and fallback paths to search for executables for different platforms.
  - Useful for post installation of executables.
  - Useful for common install locations for user folders or system folders.
  - Useful for overriding the default location for an executable through
    environment variables or lookup paths.

## 0.1.0 initial creation
