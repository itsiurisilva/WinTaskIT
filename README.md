# WinTaskIT

<p align="center">
  <a href="https://itsiurisilva.github.io/WinTaskIT/">
    <img src="assets/wintaskit.header.png" alt="WinTaskIT" width="640">
  </a>
</p>

<p align="center">
  <a href="https://github.com/itsiurisilva/WinTaskIT/releases/latest"><img alt="Release" src="https://img.shields.io/github/v/release/itsiurisilva/WinTaskIT?label=release&color=2f6fed"></a>
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white">
  <img alt="Windows 10/11" src="https://img.shields.io/badge/platform-Windows%2010%2F11-0078D6?logo=windows&logoColor=white">
  <a href="LICENSE"><img alt="License: MIT" src="https://img.shields.io/github/license/itsiurisilva/WinTaskIT?color=6f42c1"></a>
  <img alt="No admin rights required" src="https://img.shields.io/badge/admin%20rights-not%20required-success">
  <img alt="Lightweight, ~50MB idle" src="https://img.shields.io/badge/memory-~50MB_idle-2f6fed">
  <a href="https://github.com/itsiurisilva/WinTaskIT/pulls"><img alt="PRs Welcome" src="https://img.shields.io/badge/PRs-welcome-ff5a36"></a>
</p>

<p align="center">
  <strong>Keeps app windows like YouTube-as-an-app alive in the system tray instead of cluttering your taskbar, with Play/Pause/Next/Previous right from the tray icon.</strong>
</p>

<p align="center">
  <a href="#how-it-works">How it works</a> ·
  <a href="#settings">Settings</a> ·
  <a href="#screenshots">Screenshots</a> ·
  <a href="#install">Install</a> ·
  <a href="#uninstall">Uninstall</a> ·
  <a href="#build-from-source">Build from source</a> ·
  <a href="https://itsiurisilva.github.io/WinTaskIT/">Project page &amp; full guide</a>
</p>

A tiny Windows background utility that sends specific app windows to the
system tray instead of the taskbar when you minimize them either always
or only while they're actually playing audio. You pick, per app.

The motivating case: an installed web app window (e.g. YouTube as a Chrome
"app"). Minimizing it normally still leaves a taskbar button behind.
WinTaskIT hides it into a tray icon instead, so it keeps playing in the
background without cluttering your taskbar and gives you Play/Pause/Next/
Previous from the tray icon's right-click menu.

It's a little absurd that this needs to exist. Every native desktop media
player solved "play quietly in the background and stay out of my taskbar"
a couple of decades ago. Chrome, YouTube, Google the entire
installed-web-app ecosystem never got around to it. So here's a third-party
utility patching over a gap a trillion-dollar company could have closed
ages ago.

**Good combo:** pair this with [uBlock Origin Lite](https://github.com/uBlockOrigin/uBOL-home).
WinTaskIT keeps the tab alive quietly in the tray; uBlock Origin Lite keeps
whatever's running back there from burning CPU/bandwidth or throwing ads at
you while it's out of sight.

## How it works

- Runs invisibly in the background no persistent tray icon of its own.
- Watches every top-level window across the system for its Application User
  Model ID (AUMID) the same id Windows uses to identify installed web
  apps/PWAs starting to minimize.
- If that window's AUMID is in your configured list, it hides the window and
  shows a tray icon for it instead of letting it minimize normally either
  unconditionally ("Always send to tray"), or only while the window is
  actually producing sound right now ("Only while playing audio", checked
  via the same system that powers your volume flyout's "now playing"
  widget). Pick whichever fits each app: audio-gated makes sense for
  YouTube/Spotify-style tabs, "always" for anything you just want out of the
  taskbar unconditionally.
- Click the tray icon to restore the window. Right-click it for Play/Pause,
  Next, Previous, Close, or to remove it from the tracked list.
- Lightweight by design: idles around 50 MB of memory and 0% CPU in Task
  Manager, no background services, no telemetry.

## Settings

Launch `WinTaskIT.exe` a second time (or type `wintaskit` into Win+R) to open
Settings. From there you can add windows from what's currently open, enable
or disable individual entries, toggle "Run at Windows startup," and
right-click any entry to switch its tray behavior between "Always send to
tray" and "Only while playing audio."

## Screenshots

<p align="center">
  <img src="assets/Youtube.png" alt="Right-click menu on the WinTaskIT tray icon for a minimized YouTube tab: Restore, Previous, Pause, Next, Close, Remove from tray list" width="320">
</p>
<p align="center">
  <sub>YouTube playing quietly from the tray, full transport controls a right-click away. The feature nobody promised, delivered anyway.</sub>
</p>

<p align="center">
  <img src="assets/Taskmanager.png" alt="Task Manager showing the WinTaskIT process at 0% CPU and 51.8 MB of memory" width="480">
</p>
<p align="center">
  <sub>What "lightweight" actually looks like in Task Manager: 0% CPU, ~50 MB of memory.</sub>
</p>

## Install

Download the setup zip from Releases, unzip it, and run
`WinTaskIT-Setup.exe`. It's a normal Windows installer wizard still no
admin rights required, it installs just for your user account (to
`%LocalAppData%\Programs\WinTaskIT`), not system-wide.

## Uninstall

Uninstall it like any other app: Settings → Apps → Installed apps →
WinTaskIT → Uninstall. This also cleans up the startup registration, the
`wintaskit` Win+R shortcut, and all saved settings. Alternatively, relaunch
the exe to open Settings and click **Uninstall...**, which does the same
registry/settings cleanup without removing the installed program files.

## Project page

The full guide, FAQ, and download link live at
[itsiurisilva.github.io/WinTaskIT](https://itsiurisilva.github.io/WinTaskIT/).

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

MIT -> see [LICENSE](LICENSE).
