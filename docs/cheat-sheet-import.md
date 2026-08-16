# Replacing the sample cheat sheet with the real one

The four symbols seeded today (Snake, River, Money, Baby — see
`Infrastructure/Persistence/Seed/DevelopmentSeeder.cs`) are explicitly
**sample data for local development**, not the real grandmother's cheat
sheet. This doc covers what to do before any real deployment.

## The sample seeding already won't run in production

`DevelopmentSeeder.SeedAsync` is only ever called from `Program.cs` inside
the `app.Environment.IsDevelopment()` branch, and only inserts symbols if
the `DreamSymbols` table is completely empty. In any non-Development
environment it simply never runs — there's nothing to disable.

## Option A: import via the Admin API

The most straightforward path once the backend is deployed:

1. Grant your own account the `Admin` role (there's no self-service flow
   for this — see below).
2. For each real symbol, call:
   - `POST /api/admin/dream-symbols` with `{ name, description, traditionalMeaning }`
   - `POST /api/admin/dream-symbols/{id}/aliases` once per alternative wording
   - `POST /api/admin/dream-symbols/{id}/numbers` once per lucky number
     (`{ number, notes, isPrimary }`)

This is slow for a large cheat sheet but requires no direct database access
and goes through the same validation (`AddLuckyNumberRequestValidator`
currently restricts numbers to `1`–`99` — adjust that validator first if the
real cheat sheet uses a different range).

## Option B: bulk-load directly into Postgres

For a large cheat sheet, it's faster to insert directly against the
`DreamSymbols`, `DreamSymbolAliases`, and `LuckyNumbers` tables (see the
EF Core configurations in `Infrastructure/Persistence/Configurations/` for
exact column constraints — e.g. `DreamSymbol.Name` has a unique index).
A rough shape:

```sql
insert into "DreamSymbols" ("Id", "Name", "Description", "TraditionalMeaning", "IsActive", "CreatedAt", "UpdatedAt")
values (gen_random_uuid(), 'Fire', 'Flames or burning in the dream.', 'Passion, anger, or transformation.', true, now(), now());

-- then, using that symbol's generated Id:
insert into "DreamSymbolAliases" ("Id", "DreamSymbolId", "Alias") values (gen_random_uuid(), '<symbol-id>', 'flames');
insert into "LuckyNumbers" ("Id", "DreamSymbolId", "Number", "IsPrimary") values (gen_random_uuid(), '<symbol-id>', 23, true);
```

Whichever option you use, the existing four sample symbols should be
deactivated or deleted (`DELETE /api/admin/dream-symbols/{id}`, or set
`IsActive = false`) so they don't keep matching alongside the real data —
`DreamAnalysisService` only ever loads symbols where `IsActive = true`.

## Granting the Admin role

There's no registration-time or self-service way to become an Admin — by
design, since anyone hitting `/api/admin/*` can rewrite the entire cheat
sheet. Today the only path is directly against the Identity tables that
ASP.NET Core Identity created via the EF Core migration:

```sql
insert into "AspNetUserRoles" ("UserId", "RoleId")
select u."Id", r."Id"
from "AspNetUsers" u, "AspNetRoles" r
where u."Email" = 'you@example.com' and r."Name" = 'Admin';
```

(The `Admin` and `User` roles themselves are already seeded by
`DevelopmentSeeder` in Development. In a real deployment without dev
seeding, create the `Admin`/`User` `AspNetRoles` rows first.)

If this project grows past a handful of admins, replace this with a proper
admin-invitation flow rather than raw SQL — this is very much a
"get the MVP cheat sheet loaded" shortcut, not something to keep doing
indefinitely.
