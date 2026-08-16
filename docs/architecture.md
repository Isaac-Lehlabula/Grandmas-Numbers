# Architecture

## Backend: Clean Architecture layering

```text
GrandmasDreamNumbers.Domain          entities only, zero dependencies
GrandmasDreamNumbers.Application     use-case services, DTOs, validators, interfaces (depends on Domain)
GrandmasDreamNumbers.Infrastructure  EF Core, Identity, JWT, AI providers (depends on Application + Domain)
GrandmasDreamNumbers.Api             controllers, middleware, DI composition root (depends on all three)
```

The dependency rule is enforced by project references, not just convention:
`Domain` has no project references at all; `Application` references only
`Domain`; `Infrastructure` references `Application` and `Domain`; `Api`
references all three. Nothing in `Domain` or `Application` knows that
Postgres, EF Core, or ASP.NET Identity exist.

### Why `IApplicationDbContext` instead of injecting `DbContext` directly

`Application` services (`DreamAnalysisService`, `DreamHistoryService`, etc.)
depend on `IApplicationDbContext` — an interface declared in `Application`
exposing just the `DbSet<T>` properties they need plus `SaveChangesAsync`.
`Infrastructure`'s `ApplicationDbContext` implements it. This is what lets
those services live in `Application` (where the business rules belong)
without `Application` taking a dependency on EF Core's Npgsql provider or
Infrastructure itself.

### Identity lives in Infrastructure, not Domain

`ApplicationUser` extends `IdentityUser<Guid>`, a framework type from
`Microsoft.AspNetCore.Identity`. Rather than pull that package into `Domain`
to satisfy the spec's "Core entities" list literally, `ApplicationUser`
lives in `Infrastructure/Identity/`. Domain entities that need to reference
a user (`Dream.UserId`) just hold a `Guid` — no navigation property to
`ApplicationUser`, so `Domain` stays framework-agnostic.

### No MediatR / CQRS pipeline

Business logic lives in plain Application-layer service classes
(`DreamAnalysisService`, `DreamHistoryService`, `DreamSymbolSearchService`,
`AdminDreamSymbolService`) injected directly into controllers, rather than
behind a mediator/command-handler pipeline. This was a deliberate choice:
recent MediatR versions added commercial licensing terms for larger usage,
which isn't something to take on silently in someone else's project.
Controllers stay thin either way — they just call a service method instead
of `mediator.Send(new SomeCommand())`.

## Request flow: submitting a dream

```text
POST /api/dreams/analyse
  → DreamsController.Analyse (thin: pulls user id from the JWT, calls the service)
  → DreamAnalysisService.AnalyseAsync
      1. Create a Dream row (not yet saved)
      2. Load all IsActive DreamSymbol rows (with Aliases + LuckyNumbers)
      3. DreamSymbolMatcher.FindDirectMatches — pure, no I/O (see dream-matching.md)
      4. For symbols NOT matched directly, call IDreamInterpretationService
         (currently MockDreamInterpretationService) with only those symbols'
         id/name/description/aliases
      5. Validate every symbol ID the AI returned against the DB-loaded set;
         discard anything not in that set (defense in depth - the AI is only
         ever given valid IDs to begin with, but the result is checked again
         anyway)
      6. Collect LuckyNumbers from every matched symbol
      7. CombinationGenerator.Generate — pure, no I/O (see dream-matching.md)
      8. Persist Dream + DreamAnalysis + DreamSymbolMatch rows +
         SuggestedCombination rows in one SaveChangesAsync
      9. Return a DreamAnalysisResponse DTO (never the raw entities)
```

Steps 3 and 7 are deliberately pure functions with no database or network
access — that's what makes them unit-testable without EF Core or a mock AI
service (see `DreamSymbolMatcherTests` and `CombinationGeneratorTests`).

## The AI integration boundary

`IDreamInterpretationService` (declared in
`Application/Common/Interfaces/IDreamInterpretationService.cs`) is the only
seam between the app and any AI provider:

```csharp
public interface IDreamInterpretationService
{
    Task<DreamInterpretationResult> InterpretAsync(
        string dreamText,
        IReadOnlyCollection<AvailableDreamSymbol> availableSymbols,
        CancellationToken cancellationToken);
}
```

It can only return symbol IDs, matched text, a confidence score, and an
explanation — there's no field anywhere in that return type for a number.
Combined with the DB-validation step in `DreamAnalysisService` (step 5
above), this is what makes "the AI can identify symbols but never invent a
lucky number" true at the type level, not just by convention.

`MockDreamInterpretationService` (`Infrastructure/Ai/`) is the only
implementation today, selected via the `Ai:Provider` configuration value in
`Infrastructure/DependencyInjection.cs`. See `cheat-sheet-import.md`'s
sibling concern — replacing the mock — covered in the root README's
"Replacing the mock AI with a real provider" section.

## Mobile: feature-based structure

```text
lib/core/       theme, router, network client (with refresh-on-401), secure storage
lib/features/   one folder per feature area (auth, dreams, home, profile, settings, disclaimer)
                each feature: data/ (API + models) → application/ (Riverpod providers) → presentation/ (screens)
```

Routing/auth-gating: `GoRouter`'s `redirect` callback reads
`authControllerProvider`'s current `AsyncValue` on every navigation and
sends unauthenticated users to `/onboarding` and authenticated users away
from the auth routes to `/home`. A small `ChangeNotifier`
(`_AuthRefreshNotifier` in `core/router/app_router.dart`) bridges Riverpod's
`ref.listen` to GoRouter's `refreshListenable` so the redirect re-evaluates
on auth changes without recreating the router (which would otherwise drop
the navigation stack).
