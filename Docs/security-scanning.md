# Security scanning — repo-specific detail

Moved word for word from the "Scanning & review" section of `AGENTS.md`, which
keeps the summary. Read this before changing `.github/**` or triaging a
Dependabot or zizmor alert. Policy itself lives in the global rules
(`~/.config/agents/AGENTS.md` -> "Dependencies, advisories, and scanning";
full text `~/.claude/instructions/security-scanning.md`).

- CI runs `gitleaks`, `osv-scanner` and `zizmor` on every PR, every push to `main`, and
  weekly (`.github/workflows/security.yml`). osv-scanner reads both `.csproj`
  files directly; a committed `packages.lock.json` is NOT required for NuGet
  (verified locally with osv-scanner 2.7.0: 5 + 3 packages scanned, exit 0).
  The shared workflow's two npm jobs are present but skip with
  a notice, because there is no root `package-lock.json`.
- Dependabot: `nuget` plus `github-actions` (`.github/dependabot.yml`). Version
  updates run from the config alone. Security updates also need Dependabot
  alerts, a separate repo setting. Check it by HTTP STATUS, not by exit code. `gh api` exits 0 on success and 1
  on any failure; it never exits 404 or 403. `<owner>/<repo>` is also shell
  redirection, so the placeholder form cannot be pasted at all -- substitute
  the real slug and read the status line:
  `gh api -i -X GET repos/OWNER/REPO/vulnerability-alerts | head -1`
  204 = on, 403 = the token has no admin here (which is not "off"), 404 = off
  only when an admin token gets it: GitHub also answers 404 to a token without
  the permission, so confirm with an admin token first.
- SHA-pinned actions get **no Dependabot alerts**: GitHub generates them only
  for actions on semantic versions (docs: "About Dependabot alerts"). So no
  security-update PR can ever fire for a pinned action, an
  `applies-to: security-updates` group over github-actions is dead config, and
  the weekly version update is their only automatic remediation -- which is why
  the github-actions block carries no semver-major ignore. DETECTION comes
  from the `zizmor` job in `security.yml`: it maps each SHA pin to its
  release tag and fails on a known-vulnerable, impostor or unpinned action.
  osv-scanner's `github/actions` plugin only lists the pinned actions: with
  osv-scanner 2.7.0 it did not flag `tj-actions/changed-files`
  (GHSA-mrrh-fwg8-r2c3) at a vulnerable SHA or tag. zizmor looks up a SHA
  pin by one tag: for a root action (owner/repo@sha) the longest tag
  without a `/`, for a sub-action the longest tag under its path or its
  last directory (else the longest of all). The job fails unless that tag, and any tag tied
  with it, is a plain release version (v1, v1.2.3): an untagged commit,
  a non-version tag or a path-prefixed tag matches no advisory. Pin
  released versions.
- Alerts and automatic security **updates** are two switches, not one. Alerts
  only report; the half that opens PRs is `automated-security-fixes`. It is
  deliberately OFF: this repo took its first scan with a pre-existing advisory
  backlog, and enabling it would open that backlog as PRs at once.
