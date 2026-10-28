# Agentic SDLC demo: legacy ASP.NET maintenance on autopilot

A working pipeline that maintains a legacy ASP.NET MVC 5 / .NET Framework 4.8 app with AI agents. Someone files an issue, a coding agent opens a pull request, automated checks and an AI reviewer gate it, and a human only approves the merge. Deploy and release happen on their own.

The app, **ClaimsAudit**, is a small billing-compliance tool: it checks medical claims against audit rules and shows the flags. It is written the way real legacy code is (static classes, `ViewBag`, jQuery, an old-style csproj) and ships with a few real bugs, so the agents have something to do.

## The flow

```mermaid
flowchart LR
    A[Issue filed<br/>bug template] -->|maintainer adds<br/>label 'agent'| B[Coding agent<br/>Claude Code on Windows runner]
    B -->|test that reproduces the bug<br/>+ fix + green test run| G{Guardrails}
    G -->|pass| PR[Pull request]
    G -->|fail| H[needs-human]
    PR --> CI[CI: MSBuild, NUnit,<br/>IIS Express smoke test]
    CI -->|red| F[Agent fix attempt<br/>with the CI log]
    CI -->|green| R[AI reviewer<br/>second agent]
    R -->|changes requested| F
    F -->|max 2 attempts| CI
    F -->|still failing| H
    R -->|approve| HA[Human approval<br/>branch rule]
    HA -->|merge| D[Deploy: test, publish,<br/>smoke test, release]
    D --> I[Comment on the issue]
```

1. **Intake.** Anyone files a bug with the issue form (what happens, what should happen, steps, acceptance criteria). A collaborator adds the `agent` label. Labels from people without repo access are ignored, so strangers cannot spend the API budget.
2. **Coding agent** ([agent.yml](.github/workflows/agent.yml)). Claude Code runs headless on a Windows runner with MSBuild. It must first write an NUnit test that fails because of the bug, then fix the code and get the full build green. Its tools are limited to reading and editing files, running `scripts/test.ps1`, and `git diff`/`git status`. It has no GitHub token: the workflow commits, pushes and opens the PR. If the issue is unclear or needs a business decision, the agent stops and says so instead of guessing.
3. **Guardrails** ([guardrails.ps1](scripts/guardrails.ps1)), checked before anything is pushed: no edits to `.github/`, `scripts/` or `CLAUDE.md`, a diff under 400 lines, and a changed test for every fix.
4. **CI** ([ci.yml](.github/workflows/ci.yml)): restore, MSBuild, NUnit, then the site is started in IIS Express and smoke-tested. Result: status `ci/build-and-test`.
5. **AI review** ([ai-review.yml](.github/workflows/ai-review.yml)): a second agent reviews the diff for correctness, test quality and scope, posts the review on the PR and sets `ci/ai-review`. Its instructions are always read from `main`, so a PR cannot rewrite its own review rules.
6. **Retry loop.** A red CI run or a blocking review sends the PR back to the agent with the log or the review. After two failed attempts the PR is labelled `needs-human`.
7. **Human approval.** A branch rule on `main` requires both statuses and one approving review. The agent's PRs are authored by `github-actions[bot]`, so the maintainer can approve them.
8. **Deploy** ([deploy.yml](.github/workflows/deploy.yml)): after the merge, test again, publish, smoke-test the published build, create a GitHub release, and comment on the original issue. A failed deploy opens a `needs-human` issue.

## Why it is built this way

- **Prove the bug before fixing it.** A fix without a failing test is a guess. The most common failure of agent fixes is "the tests pass, but it is still broken in production", so the test has to reproduce the reported behaviour.
- **The agent never holds credentials.** All GitHub actions are deterministic workflow steps after the guardrails, so a confused or prompt-injected agent cannot push to `main`, change CI or leak secrets.
- **Bounded autonomy.** Turn limits, a retry cap and a diff limit keep cost and blast radius small. Anything unusual goes to a person with an explanation.
- **Right-sized.** The legacy app gets characterization tests and a smoke test, which is enough of a safety net to automate maintenance, not a rewrite.

One GitHub-specific detail: PRs opened with `GITHUB_TOKEN` do not trigger other workflows, so each stage starts the next one explicitly (`gh workflow run`) and reports a commit status that the branch rule requires.

## Setup

1. Add the repository secret `ANTHROPIC_API_KEY` (Settings → Secrets and variables → Actions).
2. Settings → Actions → General → Workflow permissions: **Read and write**, and tick **Allow GitHub Actions to create and approve pull requests**.
3. Add a ruleset for `main`: require a pull request with 1 approval, and require the status checks `ci/build-and-test` and `ci/ai-review`.
4. File a bug with the issue form and add the `agent` label.

Running locally (Windows, Visual Studio 2022+ with the ASP.NET workload):

```
pwsh -File scripts/test.ps1
pwsh -File scripts/smoke.ps1
```

## The same thing on Azure DevOps

| Here (GitHub) | Azure DevOps |
|---|---|
| Issue form + `agent` label | Work item template + tag, Service Hook on "work item updated" |
| `agent.yml` on a Windows runner | Pipeline with `resources: webhooks`, Windows agent pool |
| Commit status `ci/build-and-test` | Build validation branch policy |
| Commit status `ci/ai-review` | Status check policy fed by the PR Status API |
| Ruleset: 1 approval | Minimum number of reviewers policy |
| `gh workflow run` hand-offs | Pipeline completion triggers / REST queue calls |
| GitHub release + smoke test | Multi-stage YAML: deploy to an App Service staging slot, smoke test, swap |
| `ANTHROPIC_API_KEY` secret | Variable group linked to Key Vault |

The agent prompts, guardrails and test scripts carry over unchanged. Only the YAML and the triggers differ.

## Limits

- "Production" is a GitHub release plus a smoke test of the published build in IIS Express. A real setup would deploy to IIS or App Service and run the smoke test there before switching traffic.
- The AI reviewer is a gate, not a replacement for the human approval.
- The app keeps its data in memory; it exists to give the agents realistic legacy code to work on.
