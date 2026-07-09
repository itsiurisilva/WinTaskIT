# WinTaskIT

A tiny Windows background utility that sends specific app windows to the
system tray instead of the taskbar when you minimize them — either always,
or only while they're actually playing audio. You pick, per app.

The motivating case: an installed web app window (e.g. YouTube as a Chrome
"app"). Minimizing it normally still leaves a taskbar button behind.
WinTaskIT hides it into a tray icon instead, so it keeps playing in the
background without cluttering your taskbar — and gives you Play/Pause/Next/
Previous from the tray icon's right-click menu.

It's a little absurd that this needs to exist. Every native desktop media
player solved "play quietly in the background and stay out of my taskbar"
a couple of decades ago. Chrome, YouTube, Google — the entire
installed-web-app ecosystem — never got around to it. So here's a third-party
utility patching over a gap a trillion-dollar company could have closed
ages ago.

**Good combo:** pair this with [uBlock Origin Lite](https://github.com/uBlockOrigin/uBOL-home).
WinTaskIT keeps the tab alive quietly in the tray; uBlock Origin Lite keeps
whatever's running back there from burning CPU/bandwidth or throwing ads at
you while it's out of sight.

## How it works

- Runs invisibly in the background — no persistent tray icon of its own.
- Watches every top-level window across the system for its Application User
  Model ID (AUMID) — the same id Windows uses to identify installed web
  apps/PWAs — starting to minimize.
- If that window's AUMID is in your configured list, it hides the window and
  shows a tray icon for it instead of letting it minimize normally — either
  unconditionally ("Always send to tray"), or only while the window is
  actually producing sound right now ("Only while playing audio", checked
  via the same system that powers your volume flyout's "now playing"
  widget). Pick whichever fits each app: audio-gated makes sense for
  YouTube/Spotify-style tabs, "always" for anything you just want out of the
  taskbar unconditionally.
- Click the tray icon to restore the window. Right-click it for Play/Pause,
  Next, Previous, Close, or to remove it from the tracked list.

## Settings

Launch `WinTaskIT.exe` a second time (or type `wintaskit` into Win+R) to open
Settings. From there you can add windows from what's currently open, enable
or disable individual entries, toggle "Run at Windows startup," and
right-click any entry to switch its tray behavior between "Always send to
tray" and "Only while playing audio."

## Install

Download the setup zip from Releases, unzip it, and run
`WinTaskIT-Setup.exe`. It's a normal Windows installer wizard — still no
admin rights required, it installs just for your user account (to
`%LocalAppData%\Programs\WinTaskIT`), not system-wide.

## Uninstall

Uninstall it like any other app: Settings → Apps → Installed apps →
WinTaskIT → Uninstall. This also cleans up the startup registration, the
`wintaskit` Win+R shortcut, and all saved settings. Alternatively, relaunch
the exe to open Settings and click **Uninstall...**, which does the same
registry/settings cleanup without removing the installed program files.

## Project page

[itsiurisilva.github.io/WinTaskIT](https://itsiurisilva.github.io/WinTaskIT/)

## Build from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```sh
dotnet build                          # local debug build
dotnet publish -r win-x64 -c Release  # single-file, self-contained WinTaskIT.exe
```

The published exe lands in
`WinTaskIT/bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/`.

To build the installer, install [Inno Setup](https://jrsoftware.org/isinfo.php)
and compile `installer/WinTaskIT.iss` (`ISCC.exe /DMyAppVersion=X.Y.Z
installer\WinTaskIT.iss`). Pushing a `vX.Y.Z` tag also builds and publishes
this automatically via GitHub Actions.

## License

MIT — see [LICENSE](LICENSE).
