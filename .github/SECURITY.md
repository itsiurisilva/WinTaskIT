# Security Policy

## Supported versions

Only the [latest release](https://github.com/itsiurisilva/WinTaskIT/releases/latest)
is supported. WinTaskIT doesn't have an auto-update mechanism yet, so please
make sure you're running the latest version before reporting a security
issue.

## Scope

WinTaskIT is a native Windows utility with no network access, no telemetry,
and no account system, it watches local window events and stores settings
in `%AppData%\WinTaskIT`. Relevant reports include things like the
installer, local privilege handling, or the app's handling of window/process
data. General bugs that aren't security-relevant should go through a normal
[issue](https://github.com/itsiurisilva/WinTaskIT/issues/new/choose) instead.

## Reporting a vulnerability

Please don't open a public issue for security vulnerabilities. Instead,
report it privately using one of these:

- GitHub's [private vulnerability reporting](https://github.com/itsiurisilva/WinTaskIT/security/advisories/new)
  (Security tab -> Report a vulnerability), or
- Email **iurisilvaparticular@gmail.com** with details and reproduction steps.

This is a solo-maintained open source project, so there's no guaranteed
response SLA, but reports are taken seriously and I'll do my best to
acknowledge them quickly and follow up with a fix or a timeline.
