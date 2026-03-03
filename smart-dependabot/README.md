# Smart Dependabot POC - .NET Focus

A smarter Dependabot that automatically detects .NET vulnerabilities and EOL risks, scores them by priority, and assigns high-risk issues to **GitHub Copilot Coding Agent** to create fix PRs.

## Scope (POC)

**Languages:** .NET only (Framework, Core, .NET 5+)
**Package Manager:** NuGet (packages.config, PackageReference)
**Target Repo:** ContosoUniversity-NoAuth-EFCore

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│  SMART DEPENDABOT POC (.NET)                                            │
│                                                                         │
│  ┌──────────┐    ┌──────────────┐    ┌────────────┐    ┌─────────────┐  │
│  │ Nightly  │───►│ Scanner      │───►│ Risk       │───►│ Copilot     │  │
│  │ Trigger  │    │ • Dependabot │    │ Scorer     │    │ Coding Agent│  │
│  │ (Cron)   │    │ • .NET EOL   │    │ (Filter)   │    │ (Fix + PR)  │  │
│  └──────────┘    └──────────────┘    └────────────┘    └─────────────┘  │
│                                             │                           │
│                                             ▼                           │
│                                    Only HIGH/CRITICAL                   │
│                                    issues get PRs                       │
└─────────────────────────────────────────────────────────────────────────┘
```

## What It Detects

### Dependabot Alerts
- NuGet package vulnerabilities (CVEs)
- Outdated packages with known security issues

### .NET EOL Detection
| Version | Status | Risk |
|---------|--------|------|
| .NET Framework 4.8 | Supported | MEDIUM (modernization) |
| .NET Core 3.1 | EOL Dec 2022 | CRITICAL |
| .NET 5 | EOL May 2022 | CRITICAL |
| .NET 6 | EOL Nov 2024 | CRITICAL |
| .NET 7 | EOL May 2024 | CRITICAL |
| .NET 8 | LTS until Nov 2026 | LOW |
| EF Core 3.1 | EOL Dec 2022 | CRITICAL |

## Current Repo Analysis

`ContosoUniversity-NoAuth-EFCore` has:
- **.NET Framework 4.8** → MEDIUM (modernization opportunity)
- **EF Core 3.1.32** → CRITICAL (EOL Dec 2022)
- **ASP.NET MVC 5.2.9** → MEDIUM (legacy)

## Quick Start

### 1. Enable Dependabot
```
Settings → Security → Enable Dependabot alerts
```

### 2. Run Scan Manually
```bash
gh workflow run smart-dependabot.yml
```

### 3. View Results
```bash
gh run list --workflow=smart-dependabot.yml
gh run view --log
```

## Files

```
smart-dependabot/
├── .github/workflows/
│   └── smart-dependabot.yml     # GitHub Action
├── src/
│   └── scan-and-assign.ps1      # All-in-one script
└── README.md
```

## How Copilot Assignment Works

1. Workflow creates GitHub Issue with fix instructions
2. Issue is assigned to `copilot-swe-agent` via API
3. Copilot Coding Agent analyzes and creates PR
4. **You review the PR** (no auto-merge)

## Risk Scoring

```
Score = (Severity × 0.4) + (Impact × 0.35) + (Exploitability × 0.25)

CRITICAL: ≥ 8.0 → Immediate PR
HIGH:     ≥ 6.0 → PR within 24h
MEDIUM:   ≥ 4.0 → Log only
LOW:      < 4.0 → Ignore
```
