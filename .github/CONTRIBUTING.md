# Contributing to WinTaskIT

Thanks for considering a contribution. WinTaskIT is a small, solo-maintained
project, so keeping things simple helps a lot.

## Reporting bugs

Open an [issue](https://github.com/itsiurisilva/WinTaskIT/issues/new/choose)
using the bug report template. Include your Windows version, the WinTaskIT
version, and steps to reproduce, that's usually enough to track it down.

## Suggesting features

Open an issue using the feature request template. Explain the problem
you're running into before jumping to the solution, it's easier to evaluate.

## Development setup

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
and Windows (WinTaskIT is a Windows-only WinForms app).

```sh
dotnet build                          # local debug build
dotnet publish -r win-x64 -c Release  # single-file, self-contained WinTaskIT.exe
```

The published exe lands in
`WinTaskIT/bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/`.

To build the installer, install [Inno Setup](https://jrsoftware.org/isinfo.php)
and compile `installer/WinTaskIT.iss` (`ISCC.exe /DMyAppVersion=X.Y.Z
installer\WinTaskIT.iss`).

## Pull requests

- Keep PRs focused: one fix or feature per PR, unrelated cleanup makes review harder.
- Match the existing code style in the file you're editing.
- Build and actually run the app on Windows before submitting, this is a
  tray/window-management app, behavior is hard to judge from a diff alone.
- Reference the related issue if there is one.
- Update the README or the [docs site](https://itsiurisilva.github.io/WinTaskIT/)
  if your change affects user-facing behavior.

## Code of conduct

Participation in this project is covered by the
[Code of Conduct](CODE_OF_CONDUCT.md).
