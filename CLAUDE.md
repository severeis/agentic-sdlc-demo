# ClaimsAudit

Legacy ASP.NET MVC 5 app on .NET Framework 4.8. Compliance staff use it to review medical billing claims: each claim is checked against audit rules (impossible units, overbilling, unknown codes, future dates) and shown with its flags.

The app is in maintenance mode: it is kept healthy while a replacement is built. Fix what is asked, keep changes small, and do not modernise it.

## Build and test

```
pwsh -File scripts/test.ps1
```

Restores packages, builds `ClaimsAudit.sln` in Release with MSBuild and runs the NUnit tests. A change is done only when this passes.

`scripts/smoke.ps1` starts the site in IIS Express and checks the main pages. CI runs it; you do not need to.

## Production

The app runs in IIS 10 on a Windows server with the Russian system locale (ru-RU, ANSI code page 1251), .NET Framework 4.8, at http://213.155.29.61. CI and your build run on GitHub's Windows runners with an English locale (code page 1252), so anything that depends on the server's culture or default file encoding can pass in CI and still break in production.

## Layout

- `src/ClaimsAudit.Web/` - the web app (old-style csproj)
  - `Controllers/` - `HomeController` (dashboard), `ClaimsController` (list, filter, details, JSON search)
  - `Services/ClaimRepository.cs` - static in-memory claim store with seed data
  - `Services/ClaimRules.cs` - audit rules, fee schedule and max-units table
  - `Services/DashboardService.cs` - dashboard numbers
  - `Services/Clock.cs` - `Clock.Today`, the only seam for "today"
  - `Views/` - Razor views, `Scripts/claims.js` - jQuery search box
- `tests/ClaimsAudit.Tests/` - NUnit tests (SDK-style csproj, net48)

## Conventions

- C# 7.3. Views are compiled at runtime by the old Razor engine: keep view code simple (no string interpolation, no `?.`).
- The web project is an old-style csproj: a new `.cs` file must be added to `ClaimsAudit.Web.csproj` as `<Compile Include=... />`, a new view or script as `<Content Include=... />`. The test project picks up files automatically.
- No new NuGet packages.
- Money is `decimal`. Dates are compared as dates; claims have no time component.
- Keep the existing style of the file you touch, even where it is dated (static classes, `ViewBag`).

## Tests

- Tests are characterization tests: they describe what the code does today. If a fix changes behaviour that a test pins down, update that test and say why in the PR.
- Every bug fix comes with a test that fails without the fix.
- `ClaimRepository` is static: call `ClaimRepository.Reset()` in `[SetUp]`.
- Never depend on the real date: pass `today` explicitly or set `Clock.Today` and restore it in `[TearDown]`.

## Do not touch

`.github/`, `scripts/`, this file. Changes there are rejected by the pipeline's guardrails.
