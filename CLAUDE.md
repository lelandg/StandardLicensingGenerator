@AGENTS.md

# CLAUDE.md — Claude Code specifics

The line above imports the canonical project rules (`AGENTS.md`). Everything
there applies. This file adds only Claude Code mechanisms and machine notes.

## Mechanisms

- **Versioning:** use the `version-manager` skill
  (`python3 ~/.claude/skills/version-manager/version_tool.py --repo <abs-path> …`).
  It owns the csproj version fields and the changelog.
- **Code review:** use the `code-reviewer` agent for review requests.
- **Dates:** read the real date from the `<env>` block or `date '+%Y-%m-%d %H:%M'`.

## Build notes (this machine)

- Leland builds in **JetBrains Rider on Windows** (bundled MSBuild:
  `D:\Program Files\JetBrains\Rider\tools\MSBuild\Current\Bin\MSBuild.exe`).
  Rider needs a real Windows .NET SDK installed for SDK resolution.
- WSL verification builds: user-local SDK at `~/.dotnet`, e.g.
  `DOTNET_ROOT=~/.dotnet ~/.dotnet/dotnet build -p:EnableWindowsTargeting=true`.
  Then `rm -rf obj bin` per the AGENTS.md rule — Linux restore artifacts break
  Rider (confirmed 2026-08-20: MSB4236 plus `/home/leland/.nuget` paths in
  `obj/`).
- Planned (after PR #9 merges): retarget `net9.0-windows` → `net10.0-windows`
  on `main` with a version bump, since .NET 9 left support 2026-05-12.
