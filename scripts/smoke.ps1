# Checks that the main pages respond.
# Usage: scripts\smoke.ps1 -SitePath publish      (starts the site in IIS Express)
#        scripts\smoke.ps1 -BaseUrl http://localhost (checks a site that is already running, e.g. in IIS)
param(
    [string]$SitePath = "src\ClaimsAudit.Web",
    [int]$Port = 8085,
    [string]$BaseUrl = ""
)

$ErrorActionPreference = "Stop"
$proc = $null

if ($BaseUrl) {
    $base = $BaseUrl.TrimEnd('/')
} else {
    $root = Split-Path -Parent $PSScriptRoot
    if (-not [System.IO.Path]::IsPathRooted($SitePath)) { $SitePath = Join-Path $root $SitePath }

    $iis = @("$env:ProgramFiles\IIS Express\iisexpress.exe", "${env:ProgramFiles(x86)}\IIS Express\iisexpress.exe") |
        Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $iis) { throw "IIS Express not found" }

    $proc = Start-Process -FilePath $iis -ArgumentList "/path:`"$SitePath`"", "/port:$Port", "/systray:false" -PassThru -WindowStyle Hidden
    $base = "http://localhost:$Port"
}

$checks = @(
    @{ Url = "/";                          Expect = "Audit dashboard" },
    @{ Url = "/Claims";                    Expect = "Mercy Family Clinic" },
    @{ Url = "/Claims/Details/1003";       Expect = "IMPOSSIBLE_UNITS" },
    @{ Url = "/Claims/Search?q=Riverbend"; Expect = "Riverbend Behavioral Health" }
)

$failed = 0
try {
    # first request compiles the views, give it time
    $deadline = (Get-Date).AddSeconds(90)
    while ($true) {
        try { Invoke-WebRequest "$base/" -UseBasicParsing -TimeoutSec 30 | Out-Null; break }
        catch { if ((Get-Date) -gt $deadline) { throw "Site did not start: $_" }; Start-Sleep -Seconds 2 }
    }

    foreach ($c in $checks) {
        try {
            $r = Invoke-WebRequest ($base + $c.Url) -UseBasicParsing -TimeoutSec 30
            if ($r.StatusCode -eq 200 -and $r.Content -like "*$($c.Expect)*") {
                Write-Host "OK   $($c.Url)"
            } else {
                Write-Host "FAIL $($c.Url): status $($r.StatusCode), '$($c.Expect)' not found"
                $failed++
            }
        } catch {
            Write-Host "FAIL $($c.Url): $_"
            $failed++
        }
    }
} finally {
    if ($proc) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue }
}

if ($failed -gt 0) { exit 1 }
Write-Host "Smoke test passed"
exit 0
