; Inno Setup script for WinTaskIT.
;
; Compile with: ISCC.exe /DMyAppVersion=1.2.3 installer\WinTaskIT.iss
; (MyAppVersion defaults to 0.0.0 for local ad-hoc compiles with no /D flag.)
;
; Installs per-user, no admin rights (PrivilegesRequired=lowest) -- consistent
; with WinTaskIT never needing elevation for its own HKCU-only OS integration.

#ifndef MyAppVersion
  #define MyAppVersion "0.0.0"
#endif

#define MyAppName "WinTaskIT"
#define MyAppPublisher "Iuri Silva"
#define MyAppURL "https://github.com/itsiurisilva/WinTaskIT"
#define MyAppExeName "WinTaskIT.exe"

[Setup]
; This GUID is permanent -- generated once, must never change across releases,
; or Windows will treat every future version as a brand-new app instead of an
; upgrade, orphaning old Add/Remove Programs entries.
AppId={{D7B70145-D033-4BE8-BA7E-77446AC96C7D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={localappdata}\Programs\WinTaskIT
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\dist
OutputBaseFilename=WinTaskIT-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\WinTaskIT\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\WinTaskIT.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{userprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Runs before Inno removes program files, so WinTaskIT.exe can still undo its
; own HKCU registrations (startup, App Paths) and delete its %AppData% config
; -- the same cleanup the in-app Settings "Uninstall..." button performs.
Filename: "{app}\{#MyAppExeName}"; Parameters: "--uninstall-cleanup"; Flags: runhidden waituntilterminated; RunOnceId: "UninstallCleanup"
