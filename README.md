# Grandma's Dream Numbers

An app that interprets dreams the way a traditional grandmother would: it
reads back the symbols in your dream and connects them to lucky numbers from
a private, curated cheat sheet. The AI identifies and explains symbols —
it never invents a number. Every number returned to a user always traces
back to a row in the database.

> These interpretations are based on traditional dream-number associations
> and are provided for entertainment and cultural purposes only. They do not
> predict lottery outcomes or guarantee winnings.

## Status

This repository is under active, phased development. See the phase log at
the bottom of this file for what's implemented so far.

## Architecture

**Backend** — ASP.NET Core Web API in Clean Architecture:

```text
backend/src/GrandmasDreamNumbers.Domain          entities, no dependencies
backend/src/GrandmasDreamNumbers.Application      use cases, validation, DTOs (depends on Domain)
backend/src/GrandmasDreamNumbers.Infrastructure   EF Core, Postgres, external services (depends on Application + Domain)
backend/src/GrandmasDreamNumbers.Api              controllers, DI composition root (depends on all of the above)
```

Business logic lives in Application-layer services, not in controllers.
The AI provider sits behind `IDreamInterpretationService` so the mock
implementation used in development can be swapped for a real provider
without touching the rest of the app. The AI is only ever allowed to
identify and explain symbols — matching against the cheat sheet and
selecting lucky numbers is deterministic, database-driven logic that the
AI has no path to influence.

**Mobile** — Flutter (Android first, structured for iOS), Material 3,
feature-based folder layout under `mobile/grandmas_dream_numbers/lib/`:

```text
lib/core/          theme, router, network client, secure storage — shared infrastructure
lib/features/      one folder per feature (home, dream submission, results, history, ...)
```

State management is Riverpod, networking is Dio, navigation is GoRouter,
and auth tokens are kept in `flutter_secure_storage` rather than
`SharedPreferences`.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) 10.0 or later
- [Flutter SDK](https://docs.flutter.dev/get-started/install) (stable channel)
- [PostgreSQL](https://www.postgresql.org/download/) 16+ (local instance or Docker)
- Android Studio / an Android emulator (or a physical device) for mobile development

## Local setup

### Backend

```bash
cd backend
dotnet restore
dotnet build
```

Set your local Postgres connection string and JWT signing key via user
secrets rather than editing `appsettings.json` directly:

```bash
cd src/GrandmasDreamNumbers.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=grandmas_dream_numbers;Username=postgres;Password=<your-local-password>"
dotnet user-secrets set "Jwt:SecretKey" "<a-long-random-string>"
```

`appsettings.json` ships with an obvious placeholder `Jwt:SecretKey` for
zero-friction local startup — it's fine for running on your own machine but
**must** be overridden (user secrets locally, an environment variable in any
deployed environment) before this ever runs anywhere real.

Apply EF Core migrations (creates the schema, including ASP.NET Identity's
tables):

```bash
dotnet ef database update --project src/GrandmasDreamNumbers.Infrastructure --startup-project src/GrandmasDreamNumbers.Api
```

Run the API:

```bash
dotnet run --project src/GrandmasDreamNumbers.Api
```

Swagger UI is available at `/swagger` in development. A `/health` endpoint
reports API and database status. On startup in the Development environment,
the app also seeds four sample dream symbols (Snake, River, Money, Baby) and
the `Admin`/`User` roles — **this is placeholder sample data**, not the real
cheat sheet; see `docs/cheat-sheet-import.md` (once written) for how to
replace it.

### Sample login/API flow

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Ada","email":"ada@example.com","password":"Sup3rSecret!"}'

curl -X POST http://localhost:5000/api/dreams/analyse \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <accessToken from the register/login response>" \
  -d '{"dreamText":"A snake chased me across a river"}'
```

### Replacing the mock AI with a real provider

`IDreamInterpretationService` (in `Application/Common/Interfaces`) is the
seam. The mock implementation lives at
`Infrastructure/Ai/MockDreamInterpretationService.cs`. To add a real
provider: implement the interface, then add a case for it in
`Infrastructure/DependencyInjection.cs`'s `AddAiProvider` method, selected via
the `Ai:Provider` configuration value (currently only `"Mock"` exists).

### Mobile

```bash
cd mobile/grandmas_dream_numbers
flutter pub get
flutter run
```

The API client currently points at `http://10.0.2.2:5000/api`, which is the
Android emulator's alias for the host machine's `localhost`. Update
`lib/core/network/api_client.dart` if you're running against a device or a
different backend URL.

The app requires the backend to be running and reachable at that URL —
registration, login, and dream analysis all go through real HTTP calls, not
mocks. `RECORD_AUDIO` and `INTERNET` permissions are declared in
`android/app/src/main/AndroidManifest.xml`.

## Running tests

```bash
# Backend
cd backend
dotnet test

# Mobile
cd mobile/grandmas_dream_numbers
flutter test
flutter analyze
```

## Security notes

- Never commit real secrets. Use `dotnet user-secrets` locally and
  environment variables in deployed environments.
- The full dream-symbol cheat sheet is never exposed through a public
  endpoint — only matched symbols and their numbers are returned for a
  specific submitted dream.

## Phase log

- **Phase 1** — repository scaffold, Clean Architecture solution, DI/health
  check wiring, Postgres configuration placeholder, Flutter app shell with
  a placeholder home screen and API client.
- **Phase 2** — domain entities, EF Core persistence + initial migration,
  ASP.NET Identity + JWT access/refresh tokens, auth endpoints
  (register/login/refresh/logout/profile), dev-only seed data.
- **Phase 3** — exact/alias symbol matcher, mock AI interpretation service,
  the full dream analysis pipeline (match → AI-assist → validate against DB
  → combinations → persist), dream history/search/admin endpoints, rate
  limiting on the analyse endpoints, secure headers, and the test suite
  covering matching, AI-symbol validation, user isolation, and combination
  generation.
- **Phase 4** — dream-history search endpoint, HTTPS/HSTS wiring outside
  Development, and all 14 Flutter screens wired to the real backend: auth
  flow (splash/onboarding/login/register) with a refresh-on-401 interceptor,
  voice-to-text recording via `speech_to_text`, the record → review →
  analysing → results flow, dream history with search and delete, dream
  details with re-analyse, profile/settings (light/dark/system theme) and
  the disclaimer/responsible-use screen.

Not yet built: a real AI provider (the mock is still in place, by design,
until one is wired in) and anything beyond MVP polish (skeleton loaders,
richer empty states, deeper accessibility passes).
