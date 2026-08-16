# API reference

Swagger (`/swagger` in Development) has the authoritative, always-current
shape of every request/response. This doc covers what Swagger doesn't:
auth requirements, rate limits, and the error response shapes.

## Auth

All endpoints except `POST /api/auth/register`, `POST /api/auth/login`, and
`POST /api/auth/refresh` require a `Authorization: Bearer <accessToken>`
header. Endpoints under `/api/admin/*` additionally require the caller to
have the `Admin` role — see `docs/cheat-sheet-import.md` for how to grant it
to a user, since there's no self-service "become admin" flow.

| Method | Path | Auth | Notes |
|---|---|---|---|
| POST | `/api/auth/register` | none | Returns an `AuthResponse` (access + refresh token + profile), same as login |
| POST | `/api/auth/login` | none | 401 with `{ errors: [...] }` on bad credentials |
| POST | `/api/auth/refresh` | none | Body: `{ refreshToken }`. Rotates the refresh token - the old one is revoked |
| POST | `/api/auth/logout` | Bearer | Body: `{ refreshToken }`. Revokes that refresh token server-side |
| GET | `/api/profile` | Bearer | Returns the caller's own profile |
| POST | `/api/dreams/analyse` | Bearer | Rate-limited (see below). Body: `{ dreamText }` |
| GET | `/api/dreams` | Bearer | Query param `query` (optional) searches original text + summary |
| GET | `/api/dreams/{id}` | Bearer | 403 if the dream belongs to another user, 404 if it doesn't exist or hasn't been analysed |
| DELETE | `/api/dreams/{id}` | Bearer | Same ownership rules as GET |
| POST | `/api/dreams/{id}/analyse-again` | Bearer | Rate-limited (see below) |
| GET | `/api/dream-symbols/search` | Bearer | Query param `query`. Returns only `{id, name, description}` - never numbers, max 20 results |
| GET | `/api/admin/dream-symbols` | Bearer + Admin | Returns full symbol records including all lucky numbers |
| POST | `/api/admin/dream-symbols` | Bearer + Admin | Create a symbol |
| PUT | `/api/admin/dream-symbols/{id}` | Bearer + Admin | Full update (name/description/meaning/isActive) |
| DELETE | `/api/admin/dream-symbols/{id}` | Bearer + Admin | |
| POST | `/api/admin/dream-symbols/{id}/aliases` | Bearer + Admin | Body: `{ alias }` |
| POST | `/api/admin/dream-symbols/{id}/numbers` | Bearer + Admin | Body: `{ number, notes, isPrimary }` |
| GET | `/health` | none | Checks Postgres connectivity too |

## Why there's no public "list all symbols" endpoint

Per the spec's rule 8 ("do not expose the full private cheat sheet through
public API endpoints"), there is deliberately no endpoint that returns every
`DreamSymbol` with its numbers to a non-admin caller. The closest thing,
`GET /api/dream-symbols/search`, requires authentication, is capped at 20
results, and strips out `LuckyNumbers` and `TraditionalMeaning` entirely -
it exists for something like an autocomplete UI, not for browsing the
cheat sheet.

## Rate limiting

`POST /api/dreams/analyse` and `POST /api/dreams/{id}/analyse-again` share a
fixed-window limiter (`Program.cs`): **10 requests per minute**, no queueing
(`QueueLimit = 0` - the 11th request in a window is rejected immediately
rather than waiting). Exceeding it returns `429` with:

```json
{ "title": "Too many requests", "detail": "Please slow down and try again shortly." }
```

## Error response shapes

Three different shapes exist today, all intentional:

**Validation failures** (`400`, from `ValidationFilter` running FluentValidation
before the action executes - a standard ASP.NET Core `ValidationProblemDetails`):

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "DreamText": ["Dream text can't be longer than 4000 characters."] }
}
```

**Identity failures** (`400`/`401` from `AuthController`, e.g. duplicate
email or bad credentials):

```json
{ "errors": ["Email is already registered."] }
```

**Everything else** (`404`/`403`/`500`, from `ExceptionHandlingMiddleware`
mapping `NotFoundException`/`ForbiddenAccessException`/unhandled exceptions):

```json
{ "title": "Not found", "status": 404, "detail": "Dream '...' was not found." }
```

`500` responses never include the actual exception message or stack trace
in the body - only `"An unexpected error occurred"` - the real detail goes
to the Serilog console log instead.

## Sample flow

See the root [README](../README.md#sample-loginapi-flow) for a runnable
`curl` sequence (register → analyse).
