# SkillIssue.GG Development Setup

## Prerequisites

The following software is required to develop SkillIssue.GG:

* Visual Studio 2026
* .NET 10 SDK
* Git
* A GitHub account with access to the SkillIssue.GG repository

Additional development dependencies will be documented as they are introduced.

## Clone the Repository

Clone the SkillIssue.GG repository from GitHub.

After cloning, switch to the development branch:

```bash
git checkout development
git pull
```

## Open the Project

Open:

```text
SkillIssue.GG.slnx
```

in Visual Studio 2026.

The solution contains the following projects:

```text
src/
├── SkillIssue.GG.Web
├── SkillIssue.GG.Application
├── SkillIssue.GG.Domain
├── SkillIssue.GG.Infrastructure
└── SkillIssue.GG.Analysis
```

## Build the Solution

From Visual Studio:

```text
Build → Build Solution
```

Or from the repository root:

```bash
dotnet build SkillIssue.GG.slnx
```

## Run the Application

Set `SkillIssue.GG.Web` as the startup project.

Run the application using Visual Studio:

```text
F5
```

The ASP.NET Core MVC application should open in a browser.

## Run Tests

Tests will be added as the project develops.

Once test projects exist, run:

```bash
dotnet test SkillIssue.GG.slnx
```

## Project Structure

### SkillIssue.GG.Web

The ASP.NET Core MVC presentation layer.

Responsible for:

* Controllers
* Views
* ViewModels
* HTTP request handling
* User interface concerns

The Web project should not contain core business logic.

### SkillIssue.GG.Application

Contains application-level use cases and orchestration.

Responsible for:

* Application services
* Use cases
* Interfaces required by application logic
* Coordination between domain and infrastructure

### SkillIssue.GG.Domain

Contains the core SkillIssue.GG domain model.

Responsible for:

* Entities
* Value objects
* Domain rules
* Domain abstractions

The Domain project should remain independent from infrastructure and presentation concerns.

### SkillIssue.GG.Infrastructure

Contains implementations for external systems.

Examples include:

* Database access
* External game APIs
* Persistence
* HTTP clients
* External service integrations

### SkillIssue.GG.Analysis

Contains game-analysis and statistical calculations.

Examples include:

* Win rate
* KDA
* CS/min
* Gold/min
* Champion statistics
* Match statistics

Analysis code should be independently testable.

## Architecture

The current architecture is a modular monolith:

```text
                    SkillIssue.GG.Web
                           │
                           ▼
                SkillIssue.GG.Application
                           │
                           ▼
                  SkillIssue.GG.Domain
                           ▲
                           │
             ┌─────────────┴─────────────┐
             │                           │
SkillIssue.GG.Infrastructure    SkillIssue.GG.Analysis
```

Dependencies should point toward the domain rather than the presentation layer.

## Development Workflow

Development is performed using feature branches.

Start from the latest development branch:

```bash
git checkout development
git pull
```

Create a feature branch:

```bash
git checkout -b feature/<feature-name>
```

Example:

```bash
git checkout -b feature/player-domain-model
```

Make changes, test them locally, and commit using the project's commit convention.

Push the branch:

```bash
git push -u origin feature/player-domain-model
```

Create a Pull Request targeting `development`.

## Continuous Integration

GitHub Actions automatically builds and tests the solution when:

* Code is pushed to `development`
* Code is pushed to `main`
* A Pull Request targets `development`
* A Pull Request targets `main`

A Pull Request should not be considered complete until CI passes.

## Coding Standards

Coding standards are defined in:

```text
docs/development/coding-conventions.md
```

The repository `.editorconfig` provides automated formatting and editor configuration.

## Configuration and Secrets

Secrets must never be committed to Git.

This includes:

* API keys
* API tokens
* Database passwords
* Connection strings containing credentials
* Authentication secrets

Local development configuration should use appropriate local configuration mechanisms.

Production secrets will be handled through the deployment environment rather than source control.

## Troubleshooting

### Solution does not build

Try:

```bash
dotnet restore SkillIssue.GG.slnx
dotnet build SkillIssue.GG.slnx
```

### Git branch is out of date

Run:

```bash
git checkout development
git pull
```

Then recreate or update your feature branch as appropriate.

### CI fails

Open the failed GitHub Actions workflow and inspect the failed step.

CI should be treated as a required verification step rather than something to bypass.
