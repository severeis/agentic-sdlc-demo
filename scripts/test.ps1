# Builds the solution and runs the unit tests. Used by CI, by the agent and by humans.
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

$msbuild = (Get-Command msbuild.exe -ErrorAction SilentlyContinue).Source
if (-not $msbuild) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
}
if (-not $msbuild) { throw "MSBuild not found" }

& $msbuild (Join-Path $root "ClaimsAudit.sln") /restore /p:Configuration=$Configuration /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$tests = Join-Path $root "tests\ClaimsAudit.Tests\bin\$Configuration\net48\ClaimsAudit.Tests.dll"
dotnet test $tests --logger "trx;LogFileName=results.trx" --results-directory (Join-Path $root "TestResults")
exit $LASTEXITCODE
