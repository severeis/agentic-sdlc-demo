# Checks the agent's working-tree changes before anything is committed.
# Stages everything, then fails if the change breaks one of the rules below.
param(
    [string]$Base = "origin/main",
    [ValidateSet("implement", "fix")]
    [string]$Mode = "implement",
    [int]$MaxChangedLines = 400
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force agent-out | Out-Null

git add -A
$problems = @()

$newChanges = @(git diff --cached --name-only HEAD)
if ($newChanges.Count -eq 0) { $problems += "The agent made no changes" }

$files = @(git diff --cached --name-only $Base)

$protected = @($files | Where-Object { $_ -match '^(\.github/|scripts/|CLAUDE\.md$)' })
if ($protected.Count -gt 0) { $problems += "Protected files changed: $($protected -join ', ')" }

$lines = 0
git diff --cached --numstat $Base | ForEach-Object {
    $parts = $_ -split "`t"
    if ($parts[0] -ne '-') { $lines += [int]$parts[0] + [int]$parts[1] }
}
if ($lines -gt $MaxChangedLines) { $problems += "Diff is too large: $lines changed lines (limit $MaxChangedLines)" }

if ($Mode -eq "implement" -and -not ($files | Where-Object { $_ -like "tests/*" })) {
    $problems += "No test was added or changed, so the fix is not proven"
}

if ($problems.Count -gt 0) {
    $problems | ForEach-Object { Write-Host "GUARDRAIL: $_" }
    $problems -join "`n" | Set-Content agent-out/guardrails.txt
    git reset -q
    exit 1
}

Write-Host "Guardrails passed: $($files.Count) files, $lines changed lines vs $Base"
