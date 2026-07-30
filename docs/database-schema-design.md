# Phase 0 — Database Schema Design
**Project:** Task Management Tool (10Pearls Shine — .NET Fullstack Track)
**Phase:** 0 — Solution Structure + Schema Design (pre-code)
**Branch:** feature/project-setup

---

## 1. Overview

This document captures the SQL schema design for the Task Management Tool, along with the key business-rule decisions made before any EF Core code was written. Six tables: `Roles`, `Users`, `TaskStatuses`, `TaskPriorities`, `Categories`, `Tasks`.

---

## 2. Tables

### Roles
| Column | Type | Notes |
|---|---|---|
| Id | INT, PK, Identity | |
| Name | NVARCHAR(50), NOT NULL, UNIQUE | "Admin", "User" |

### Users
| Column | Type | Notes |
|---|---|---|
| Id | INT, PK, Identity | |
| FullName | NVARCHAR(100), NOT NULL | |
| Email | NVARCHAR(255), NOT NULL, UNIQUE | used for login |
| PasswordHash | NVARCHAR(MAX), NOT NULL | never store plain text |
| RoleId | INT, FK → Roles.Id, NOT NULL | |
| CreatedAt | DATETIME2, NOT NULL, default GETUTCDATE() | audit field |
| UpdatedAt | DATETIME2, NULL | audit field |

### TaskStatuses
| Column | Type | Notes |
|---|---|---|
| Id | INT, PK, Identity | |
| Name | NVARCHAR(50), NOT NULL, UNIQUE | "To Do", "In Progress", "Completed" — powers Dashboard task counts by status |

### TaskPriorities
| Column | Type | Notes |
|---|---|---|
| Id | INT, PK, Identity | |
| Name | NVARCHAR(50), NOT NULL, UNIQUE | "Low", "Medium", "High" |

### Categories
| Column | Type | Notes |
|---|---|---|
| Id | INT, PK, Identity | |
| Name | NVARCHAR(100), NOT NULL, UNIQUE | Global, admin-managed (not per-user) |

### Tasks
| Column | Type | Notes |
|---|---|---|
| Id | INT, PK, Identity | |
| Title | NVARCHAR(200), NOT NULL | |
| Description | NVARCHAR(MAX), NULL | |
| DueDate | DATETIME2, NULL | |
| StatusId | INT, FK → TaskStatuses.Id, NOT NULL | |
| PriorityId | INT, FK → TaskPriorities.Id, NOT NULL | |
| CategoryId | INT, FK → Categories.Id, NULL | nullable — task may be uncategorized |
| CreatedByUserId | INT, FK → Users.Id, NOT NULL | who originally created the task |
| AssignedToUserId | INT, FK → Users.Id, NOT NULL | who the task is assigned to |
| IsDeleted | BIT, NOT NULL, default 0 | soft delete flag |
| CreatedAt | DATETIME2, NOT NULL, default GETUTCDATE() | audit field |
| UpdatedAt | DATETIME2, NULL | audit field |

---

## 3. Relationships

| Relationship | Type |
|---|---|
| Roles → Users | One-to-many |
| Users → Tasks (as creator) | One-to-many via `CreatedByUserId` |
| Users → Tasks (as assignee) | One-to-many via `AssignedToUserId` |
| TaskStatuses → Tasks | One-to-many |
| TaskPriorities → Tasks | One-to-many |
| Categories → Tasks | One-to-many (nullable) |

---

## 4. ER Diagram (plain text)

```
┌──────────────┐
│    Roles      │
│  PK Id        │
│     Name      │
└──────┬────────┘
       │ 1
       │ *
┌──────▼──────────────┐
│        Users          │
│  PK Id                │
│     FullName           │
│     Email                │
│     PasswordHash          │
│  FK RoleId ──► Roles.Id    │
│     CreatedAt                │
│     UpdatedAt                  │
└──────┬───────────┬────────────┘
       │ 1          │ 1
       │ (creator)   │ (assignee)
       │ *           │ *
┌──────▼───────────▼──────────┐
│              Tasks             │
│  PK Id                          │
│     Title                        │
│     Description                   │
│     DueDate                        │
│  FK StatusId    ──► TaskStatuses.Id
│  FK PriorityId  ──► TaskPriorities.Id
│  FK CategoryId  ──► Categories.Id (nullable)
│  FK CreatedByUserId  ──► Users.Id
│  FK AssignedToUserId ──► Users.Id
│     IsDeleted
│     CreatedAt
│     UpdatedAt
└─────────────────────────────────┘

┌─────────────────┐   ┌───────────────────┐   ┌──────────────┐
│  TaskStatuses     │   │  TaskPriorities     │   │  Categories    │
│  PK Id              │   │  PK Id                │   │  PK Id          │
│     Name              │   │     Name                │   │     Name          │
└─────────────────────┘   └───────────────────────┘   └──────────────────┘
```

---

## 5. Key Design Decisions

1. **Roles as a lookup table, not an enum/string on Users** — allows adding roles later without schema changes; keeps role names consistent (no typos like "admin" vs "Admin").

2. **Task creation ownership: both Users and Admin can create tasks.**
   - User creates their own task → `CreatedByUserId == AssignedToUserId`
   - Admin creates a task and assigns it to a user → `CreatedByUserId` (Admin) ≠ `AssignedToUserId` (target user)
   - "Was this assigned by admin?" is **derived**, not stored: `CreatedByUserId != AssignedToUserId`. Storing it as a separate boolean flag (e.g. `IsAssignedByAdmin`) was considered and rejected — it would be redundant, derivable data, and risks drifting out of sync with the source columns over time.

3. **One assignee per task** (not many-to-many) — deliberate simplicity choice since this is a solo project; no join table needed.

4. **Categories are global**, not per-user — admin-managed, keeps the initial build simpler. Per-user custom categories was considered and deferred.

5. **Soft delete via `IsDeleted` flag** — tasks are never hard-deleted from the database; preserves audit history and allows recovery. All queries in the Application layer will need to filter `WHERE IsDeleted = 0`.

6. **Password security** — `PasswordHash` column only; plain text passwords are never stored. Hashing will be implemented in the Application/Infrastructure layer in a later phase.

7. **Audit fields (`CreatedAt`, `UpdatedAt`)** — included on both `Users` and `Tasks` to support production-style traceability.

8. **Dual foreign keys to `Users` on `Tasks`** (`CreatedByUserId` and `AssignedToUserId`) — both point to the same table. This requires explicit EF Core Fluent API configuration in Phase 1 to avoid ambiguous relationship detection, using `.HasOne().WithMany().HasForeignKey()` for each, with `.OnDelete(DeleteBehavior.Restrict)` to prevent cascade-delete conflicts (deleting a User must not cascade-delete unrelated Tasks through two different paths).

---

## 6. Deferred to Phase 1 (EF Core)

- Fluent API configuration for the dual `Users` foreign keys on `Tasks`
- Migration generation and `DbContext` setup
- Seed data for `Roles`, `TaskStatuses`, `TaskPriorities` (fixed lookup values)
- Index planning (e.g., `Email` unique index, `AssignedToUserId` index for dashboard queries)
