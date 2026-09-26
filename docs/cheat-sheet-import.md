# Correcting or extending the cheat sheet

`DevelopmentSeeder.SeedAsync` (see
`Infrastructure/Persistence/Seed/DevelopmentSeeder.cs`) seeds the real
dream-number cheat sheet — numbers 1–52, transcribed from the traditional
dream book supplied by the project owner. This doc covers two things: how
that data reaches a database, and what to do when you need to add, correct,
or otherwise import entries beyond what's in `DevelopmentSeeder`.

## Seeding only ever runs once per database

`DevelopmentSeeder.SeedAsync` is only ever called from `Program.cs` inside
the `app.Environment.IsDevelopment()` branch, and only inserts symbols if
the `DreamSymbols` table is completely empty. In any non-Development
environment it never runs at all. This also means: if you already seeded a
database before `DevelopmentSeeder`'s data changed (e.g. it still has the
original 4-symbol placeholder set from early development), re-running the
app won't upgrade it — clear the `DreamSymbols` table (cascades to aliases
and lucky numbers) and restart, or use Option A/B below to add the missing
entries directly.

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

If you're correcting an entry rather than adding a new one, deactivate the
old row (`DELETE /api/admin/dream-symbols/{id}`, or set `IsActive = false`)
rather than leaving both in place — `DreamAnalysisService` only ever loads
symbols where `IsActive = true`, but a stale duplicate is still confusing
to anyone reading the cheat sheet later.

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
