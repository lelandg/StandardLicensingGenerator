@AGENTS.md

# CLAUDE.md — Claude Code specifics

The line above imports the canonical project rules (`AGENTS.md`). Everything
there applies. This file adds only Claude Code mechanisms and machine notes. (Copilot reads
this file too; nothing here is a rule other CLIs must avoid.)

## Mechanisms

- **Versioning:** the `land-pr` skill cuts the bump at merge time and the
  `version-manager` skill covers check, backfill and releases outside a PR.
  Both are wrappers over the scripts named in `AGENTS.md` ("Conventions"),
  which any CLI can run directly.

## Build notes (this machine)

- Leland builds in **JetBrains Rider on Windows** (bundled MSBuild:
  `D:\Program Files\JetBrains\Rider\tools\MSBuild\Current\Bin\MSBuild.exe`).
  Rider needs a real Windows .NET SDK installed for SDK resolution.
- WSL verification builds: user-local SDK at `~/.dotnet`, e.g.
  `DOTNET_ROOT=~/.dotnet ~/.dotnet/dotnet build -p:EnableWindowsTargeting=true`.
  Then `rm -rf obj bin` per the AGENTS.md rule — Linux restore artifacts break
  Rider (confirmed 2026-08-20: MSB4236 plus `/home/leland/.nuget` paths in
  `obj/`).
