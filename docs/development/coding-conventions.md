\# SkillIssue.GG C# Coding Conventions



\## Purpose



This document defines the coding standards used by the SkillIssue.GG project.



The goal is to keep the codebase consistent, readable, maintainable, and easy for multiple developers to work on.



Formatting rules are enforced where practical through the repository-level `.editorconfig`.



\---



\## Naming



\### Classes



Use PascalCase.



```csharp

public class PlayerStatistics

{

}

```



\### Methods



Use PascalCase.



```csharp

public PlayerStatistics CalculateStatistics()

{

}

```



\### Properties



Use PascalCase.



```csharp

public string PlayerName { get; set; }

```



\### Private fields



Use `\_camelCase`.



```csharp

private readonly IPlayerRepository \_playerRepository;

```



\### Local variables



Use camelCase.



```csharp

var playerStatistics = CalculateStatistics();

```



\### Parameters



Use camelCase.



```csharp

public void ProcessPlayer(string playerId)

{

}

```



\### Interfaces



Interfaces use the `I` prefix.



```csharp

public interface IPlayerRepository

{

}

```



\### Async methods



Asynchronous methods use the `Async` suffix.



```csharp

public async Task<Player> GetPlayerAsync(string playerId)

{

}

```



\---



\## Namespaces



Use file-scoped namespaces.



```csharp

namespace SkillIssueGG.Domain.Entities;



public class Player

{

}

```



Namespaces should generally reflect the project and folder structure.



\---



\## Accessibility



Explicitly declare accessibility for members where appropriate.



```csharp

public class Player

{

&#x20;   public string Name { get; private set; } = string.Empty;



&#x20;   private void Validate()

&#x20;   {

&#x20;   }

}

```



\---



\## Nullable Reference Types



Nullable reference types are enabled.



Use non-nullable types when a value is guaranteed to exist.



```csharp

public string Name { get; private set; } = string.Empty;

```



Use nullable types when a value can legitimately be absent.



```csharp

public string? ProfileIconUrl { get; private set; }

```



Avoid suppressing nullable warnings unless there is a clear justification.



\---



\## `var`



Use `var` when the type is obvious from the expression.



```csharp

var player = new Player();

```



Use an explicit type when it improves readability.



```csharp

PlayerStatistics statistics = CalculateStatistics();

```



The goal is readable code rather than maximizing or minimizing the use of `var`.



\---



\## Async/Await



Use asynchronous APIs for I/O operations.



This includes:



\* Database access

\* HTTP requests

\* External game APIs

\* File I/O



Avoid blocking asynchronous operations with `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` in normal application code.



\---



\## Dependency Injection



Use ASP.NET Core dependency injection for application services and infrastructure dependencies.



Prefer constructor injection.



```csharp

public class PlayerService

{

&#x20;   private readonly IPlayerRepository \_playerRepository;



&#x20;   public PlayerService(IPlayerRepository playerRepository)

&#x20;   {

&#x20;       \_playerRepository = playerRepository;

&#x20;   }

}

```



Avoid manually constructing infrastructure dependencies inside application code.



\---



\## Controllers



MVC controllers should remain thin.



Controllers are responsible for:



\* Receiving HTTP requests

\* Validating basic request input

\* Calling application services

\* Selecting the appropriate response/view



Controllers should not contain:



\* Database queries

\* External API implementation

\* Statistical calculations

\* Complex business logic



\---



\## Business Logic



Business logic belongs in the appropriate application, domain, or analysis component.



For example:



```text

Statistics → GameAnalytics.Analysis

Game rules → GameAnalytics.Domain

Use-case orchestration → GameAnalytics.Application

Database/API implementation → GameAnalytics.Infrastructure

Presentation → GameAnalytics.Web

```



\---



\## Comments



Comments should explain why something is done rather than simply describing what the code does.



Avoid comments that merely repeat the code.



Prefer clear code over excessive comments.



\---



\## Error Handling



Errors should be handled at the appropriate application boundary.



External API failures, database failures, and invalid user input should not result in uncontrolled application crashes.



Exceptions should not be silently swallowed.



\---



\## Testing



New business logic should include appropriate automated tests.



Statistics calculations should be independently testable without requiring:



\* ASP.NET Core

\* PostgreSQL

\* External APIs



Tests should use descriptive names that explain the expected behavior.



Example:



```csharp

\[Fact]

public void CalculatesWinRateFromCompletedMatches()

{

}

```



\---



\## Project Architecture



SkillIssue.GG follows a modular monolith architecture.



```text

Web

&#x20;↓

Application

&#x20;↓

Domain



Infrastructure → Application / Domain



Analysis → Domain

```



The Domain project should remain independent from infrastructure and presentation concerns.



\---



\## General Principle



Prefer:



\* Simple code

\* Explicit intent

\* Small classes

\* Small methods

\* Dependency injection

\* Testable logic

\* Meaningful names



Avoid:



\* Premature abstraction

\* Over-engineering

\* Large controllers

\* Large service classes

\* Hidden dependencies

\* Duplicated business logic

\* Unnecessary comments



