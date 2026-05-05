# DealtHands — Developer Guide

> This guide is written for CS student groups inheriting this codebase in future semesters. It assumes you are a CS student comfortable with C# and web development basics, but new to this specific project.

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Development Environment Setup](#2-development-environment-setup)
3. [Architecture Overview](#3-architecture-overview)
4. [Database](#4-database)
5. [Key Services and Feature Implementation](#5-key-services-and-feature-implementation)
6. [Known Gaps, TODOs, and Warnings](#6-known-gaps-todos-and-warnings)
7. [Suggested Next Steps for Incoming Teams](#7-suggested-next-steps-for-incoming-teams)

---

## 1. Project Overview

**Where to start:** Before reading everything else, here is how to orient yourself by feature:

| "I want to understand..." | Start here |
|---|---|
| How a student joins a session | `Pages/JoinSession.cshtml.cs` → `SessionTracker` |
| How the educator advances rounds | `Pages/Lobby.cshtml.cs` → `GameSessionService.OpenRoundAsync` |
| How cards are assigned to students | `GameSessionService.AssignCardsToPlayersAsync` |
| How Game Changers fire | `GameSessionService.ShouldAssignGameChanger` + `AssignGameChangerAsync` |
| How the budget calculator works | `Services/FinancialCalculator.cs` + `Controllers/FinancialsController.cs` |
| How PDF reports are generated | `Reports/PlayerResultsDocument.cs` + `Reports/SessionReportDocument.cs` |
| How authentication works | `Services/AuthenticationService.cs` + `Services/UserService.cs` |

### What This Application Does

DealtHands is a classroom-facing web game for teaching behavioral economics. An educator creates a session with a join code; students join without accounts. The game runs 5 rounds (Career, Student Loans, Transportation, Housing, Family). Each round the educator opens it, students receive a card with a financial decision and monthly dollar impact, and the educator closes the round when ready. Random "Game Changer" events fire between assignments based on difficulty and player state flags. After 5 rounds, a leaderboard and downloadable PDF session report are generated.

### Tech Stack Quick Reference

| Layer | Technology |
|---|---|
| Language | C# / .NET 9 |
| Web framework | ASP.NET Core — Razor Pages + MVC Controllers |
| ORM | Entity Framework Core 9 (Code First, migrations) |
| Database | SQL Server LocalDB (development only) |
| Real-time updates | JavaScript polling (setTimeout loops on pages) |
| Auth | Session-based (HttpContext.Session), BCrypt password hashing |
| PDF generation | QuestPDF |
| Front-end | Razor Pages, vanilla JavaScript, Bootstrap CSS |

### Completion Status

**Implemented and working:**
- Educator registration, login, session creation
- Student join flow (no account required)
- Lobby with live player list (polling-based)
- All 5 rounds with card assignment and submission
- Game Changer system with weighted random selection and player state flags
- Budget calculator (FinancialCalculator service + API endpoint)
- Results/leaderboard page
- PDF session report generation
- Session history on educator dashboard

**Incomplete or not implemented:**
- Password reset — structure exists but reset links are shown on-screen, not emailed (see [Section 6](#6-known-gaps-todos-and-warnings))
- Remote/production deployment — LocalDB is development-only and not deployable for production
- Automated tests — none exist
- SignalR — the library is loaded on the client side, but no Hub is registered in Program.cs; all real-time updates use polling

---

## 2. Development Environment Setup

### Prerequisites

| Tool |
|---|
| Visual Studio 2022 |
| .NET 9.0 |
| SQL Server LocalDB |
| EF Core Tools Package(s) |

### Steps

**1. Open the solution**

Open `DealtHands.sln` in Visual Studio 2022. NuGet packages restore automatically on first build.

**2. Create `appsettings.Development.json`**

This file is git-ignored. You must create it manually at `DealtHands/appsettings.Development.json`. At minimum it only needs to exist — the connection string is already in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

If you need to override the connection string for any reason, add a `ConnectionStrings` section here.

**3. Verify the connection string**

Open `DealtHands/appsettings.json`. The active connection string is:
```
Server=(localdb)\mssqllocaldb;Database=DealtHandsDBV2;Trusted_Connection=True;
```
This works on any machine with SQL Server LocalDB installed. No additional configuration is needed.

**4. Apply database migrations**

Open the **Package Manager Console** in Visual Studio (`Tools → NuGet Package Manager → Package Manager Console`) and run:
```powershell
Update-Database
```

This creates the `DealtHandsDBV2` database and applies all migrations. If migrations are out of sync with a teammate's schema changes, see [Section 4](#4-database) for details.

**5. Run the application**

Press **F5** or click the Start button. The app launches at:
- `https://localhost:7220` (HTTPS)
- `http://localhost:5194` (HTTP)

The browser opens automatically. If you get a certificate warning, click "Advanced → Proceed" — this is expected for localhost development.

**6. Seed data (if needed)**

The game requires Cards and GameChangers to exist in the database. Check the `Migration.sql` file at the repo root — it contains `INSERT` statements for the full card and game changer datasets. If your database is empty after running migrations, execute the INSERT sections of `Migration.sql` directly against the `DealtHandsDBV2` database using SQL Server Object Explorer in Visual Studio.

---

## 3. Architecture Overview

### Razor Pages Pattern

The application uses **ASP.NET Core Razor Pages** for all user-facing pages. Each page is a `.cshtml` file paired with a `.cshtml.cs` code-behind file. Business logic should live in the service layer (`Services/`), not in page models.

Key principle: page models handle HTTP input/output; services handle all game state.

### Project Layout

```
DealtHands/
  Controllers/         — API controllers (currently only FinancialsController)
  Data/                — EF Core DbContext (DealtHandsDbv2Context.cs)
  ModelsV2/            — EF Core entity classes
  Pages/               — Razor Pages (.cshtml + .cshtml.cs pairs)
    Shared/            — Layout, calculator partial
  Reports/             — QuestPDF document classes
  Services/            — All business logic services
  wwwroot/             — Static assets (CSS, JS, lib/)
```

### Authentication

Authentication is **session-based**, not cookie-auth or JWT. There is no ASP.NET Core Identity in this project.

- `AuthenticationService` reads/writes values in `HttpContext.Session`
- Educators store their `UserId` and a role marker ("Educator") in the session
- Students store their `UserId` and a role marker ("Student") in the session
- The session cookie is named `.DealtHands.Session` and expires after **2 hours of inactivity**
- Passwords are hashed with **BCrypt** (BCrypt.Net-Next library) in `UserService`
- There is no role-based middleware — pages manually check `AuthenticationService` to gate access

### Real-Time Updates

There is **no SignalR Hub** wired up in this application despite the SignalR client library being loaded. All "real-time" updates use JavaScript `setTimeout` polling loops.

Pages that poll:
- `Lobby.cshtml` — students poll for game start; educator polls for player count
- `Round.cshtml` — students poll for round open/close
- `GameChanger.cshtml` — students poll for game changer assignment

Each poll hits a page handler (e.g., `OnGetStatusAsync`) that returns JSON. Polling intervals are typically 2–4 seconds.

**Implication for future teams:** If you add a SignalR Hub, you can replace these polling loops with push notifications.

### `SessionTracker` — Singleton Warning

`SessionTracker` is registered as a **singleton** in `Program.cs`:

```csharp
builder.Services.AddSingleton<SessionTracker>();
```

This means a single `SessionTracker` instance is shared across **all requests** for the lifetime of the application process. It holds an in-memory dictionary of `gameSessionId → Set<userId>` for tracking which players are currently connected.

**Critical cautions:**
1. All public methods use `lock (_lock)` — this is intentional and must be preserved if you add new methods.
2. If the application restarts (IIS recycle, process crash, redeploy), all in-memory player tracking is lost. Students would need to rejoin. The database is unaffected.
3. Do not inject `SessionTracker` into a background service or hosted service without understanding .NET DI lifetime rules — singleton-in-transient is fine; the reverse is not.
4. `SessionTracker` does not persist to the database. It is purely a runtime cache for "who is currently in this session."

---

## 4. Database

### EF Core Migrations

The project uses **Code First** migrations. The entity classes live in `ModelsV2/` and the DbContext is `Data/DealtHandsDbv2Context.cs`.

**Adding a migration** (after changing an entity class):
```powershell
Add-Migration MigrationName
Update-Database
```

**Reverting a migration:**
```powershell
Update-Database PreviousMigrationName
Remove-Migration
```

### `Migration.sql` — Team Sync Tool

`Migration.sql` at the repo root is a manually maintained SQL script used to synchronize schema changes across team members without running EF migrations. It is useful when:
- A teammate's database is in an unknown state
- You need to apply only data changes (card/game changer inserts), not schema changes
- You are troubleshooting a migration conflict

Execute relevant sections of it directly in SQL Server Object Explorer or SSMS against `DealtHandsDBV2`.

### Schema Overview

**Users** — Educator and student accounts
- `IsEducator` flag distinguishes educator accounts from student accounts
- Student accounts are created by `UserService` when a student joins a session (no registration)
- Player state flags (`HasStudentLoans`, `HasCar`, `HasCarLoan`, `OwnsHome`, `HasApartment`, `HasRoommate`, `IsMarried`, `HasChildren`, `HasJob`) are updated as the student submits cards and receives Game Changers

**GameSessions** — One row per session created by an educator
- `Status` values: `Waiting` → `InProgress` → `Completed` (also `Paused`)
- `CurrentRoundNumber` increments when a round is closed
- `JoinCode` is a 5-digit number (unique among active sessions)

**GameRounds** — One row per round per session
- `RoundType` values: `Career`, `StudentLoan`, `Transportation`, `Housing`, `Family`
- `Status` values: `Open` → `Closed`

**Cards** — The life-decision choices available to students
- `RoundType` determines which round the card belongs to
- `CardType = "RoundCard"` for all playable cards
- `DifficultyLevel` filters which cards appear at a given session difficulty
- `Requires*` and `Sets*` flags control eligibility and state mutations

**GameChangers** — Random life events
- `Weight` controls probability (higher weight = more likely to be selected)
- `DifficultyLevel` caps which changers appear at a given difficulty
- `Requires*` flags match against the student's current state flags
- `Sets*` flags mutate the student's state when a Game Changer fires
- Financial effects: `MonthlyAmount` (one-time hit), `IncomeEffect` (flat monthly change), `IncomeEffectPercent` (percentage modifier), `ExpenseEffect` (flat monthly expense change)

**UGC (User Game Choice)** — Audit trail; one row per card or Game Changer assignment per player per round
- If `CardId` is set, it's a round card submission
- If `GameChangerId` is set, it's a Game Changer assignment
- `SubmittedAmount` — the dollar amount applied
- `RunningTotal` — cumulative total at the time of this entry (used for leaderboard)

---

## 5. Key Services and Feature Implementation

### Session Creation Flow

1. Educator submits the Create Session form → `Pages/CreateSession.cshtml.cs OnPostAsync`
2. `GameSessionService.CreateSessionAsync(hostUserId, gameId, sessionName, difficulty)` is called
3. A unique 5-digit `JoinCode` is generated (`GenerateJoinCode()`, retried if collision)
4. A `GameSession` row is inserted with `Status = "Waiting"`
5. The educator is redirected to `/Lobby?sessionId=X`

### Student Join Flow

1. Student submits name and join code on `/JoinSession`
2. `GameSessionService.GetSessionByJoinCodeAsync(code)` finds the session
3. `UserService` creates (or reuses) a student `User` row
4. `SessionTracker.AddPlayer(gameSessionId, userId)` registers the player in memory
5. Student session state is set (role = Student, userId, gameSessionId)
6. Student is redirected to `/Lobby`

### Round Progression (Educator Advances Rounds)

1. Educator clicks "Open Round" → calls `GameSessionService.OpenRoundAsync(gameSessionId, connectedUserIds)`
2. `OpenRoundAsync` wraps everything in a DB transaction:
   - Creates a `GameRound` row with the correct `RoundType` (determined by `CurrentRoundNumber`)
   - Calls `AssignCardsToPlayersAsync` — picks a random `Card` from the pool for each player, inserts `UGC` rows
3. Student pages poll for an open round and display the card when one is found
4. Each student submits → `GameSessionService.SubmitPlayerChoiceAsync` updates the UGC row with `SubmittedAt`, `SubmittedAmount`, `RunningTotal`, and mutates player state flags from `Card.Sets*` fields
5. Educator clicks "Close Round" → `GameSessionService.CloseRoundAsync` sets round `Status = "Closed"` and increments `session.CurrentRoundNumber`

### Game Changer Triggering

1. When a round opens, the server calls `GameSessionService.ShouldAssignGameChanger(difficulty)` per player
   - Easy: 30% chance, Medium: 60%, Hard: 90%
2. If true, `AssignGameChangerAsync(userId, gameRoundId, gameSessionId, roundType)` runs:
   - Loads `GameChanger` candidates filtered by difficulty level and `Requires*` flags matching the player's current state
   - Selects one using weighted random (`PickWeightedGameChanger` — sums weights, rolls against total)
   - Inserts a UGC row with `GameChangerId` set (no CardId)
   - Updates the player's `User` state flags from `GameChanger.Sets*` fields
   - Calculates and stores one-time balance impact in `SubmittedAmount`
3. Student's page polls for a Game Changer and shows the full-screen overlay on detection

### Budget Calculator

1. Student's browser calls `GET /api/financials?sessionId=X` during a round
2. `Controllers/FinancialsController.cs` calls `GameSessionService.GetPlayerFinancialStateAsync(userId, sessionId)`
3. This method replays the player's UGC history:
   - Pass 1: builds base `monthlyIncome` (Career card amount) and `monthlyExpenses` (all other submitted round cards)
   - Pass 2: applies Game Changer modifiers (`IncomeEffectPercent`, `IncomeEffect`, `ExpenseEffect`, `MonthlyAmount`)
4. Returns a `PlayerFinancialState` DTO with income, expenses, available, and health rating
5. `Pages/Shared/_Calculator.cshtml` partial renders the sidebar using this data
6. `Services/FinancialCalculator.cs` provides standalone loan payment math and Dave Ramsey budget checks (used in the calculator display)

### PDF Report Generation

Two QuestPDF document classes in `Reports/`:
- `PlayerResultsDocument` — individual player card with all round choices and Game Changer events
- `SessionReportDocument` — full session summary with all players

The educator triggers download from the Results or Session Report page. QuestPDF is configured for Community License in `Program.cs`:
```csharp
QuestPDF.Settings.License = LicenseType.Community;
```

The Community License is free but requires attribution and has a revenue cap. Review the QuestPDF license terms if the project ever moves to commercial use.

### Key Services Reference

| Service | What It Does | Registered As | One Gotcha |
|---|---|---|---|
| `GameSessionService` | All game lifecycle logic — sessions, rounds, cards, Game Changers, financials, leaderboard | Scoped | Uses EF Core execution strategies for retry; never call `SaveChangesAsync` inside a transaction that the service is also managing |
| `UserService` | Educator registration, student creation, password hashing | Scoped | Creates a new `User` row for every student join — if a student joins twice they get two User rows; handle this if multi-join is a concern |
| `AuthenticationService` | Reads/writes role and userId in HttpContext.Session | Scoped | Session state is tied to the browser session cookie; it is not DB-persisted |
| `FinancialCalculator` | Loan payment math, budget percentage checks, Dave Ramsey guidelines | Scoped | Standalone math — does not touch the DB |
| `SessionTracker` | In-memory set of userId per gameSessionId | **Singleton** | State is lost on app restart; all public methods must use `lock (_lock)` |

---

## 6. Known Gaps, TODOs, and Warnings

### Password Reset — Incomplete

**Status:** The Forgot Password page (`Pages/ForgotPassword.cshtml`) generates a reset token and displays it directly on screen as a raw URL. It is not emailed.

The page shows: `Reset link: /ResetPassword?token=<token>` with a comment "(In production, this would be emailed)."

**What needs to be done:** Integrate an email delivery service (SMTP, SendGrid, etc.) to send the token to the user's registered email address. The token generation logic in `ForgotPassword.cshtml.cs` is already in place; only delivery is missing.

**Files:** `Pages/ForgotPassword.cshtml`, `Pages/ForgotPassword.cshtml.cs`, `Pages/ResetPassword.cshtml`, `Pages/ResetPassword.cshtml.cs`

### No Remote Deployment Configuration

The app is configured for LocalDB only. `appsettings.json` has a single connection string pointing to `(localdb)\mssqllocaldb`. There is no production deployment configuration.

**What needs to be done for production:** Replace the LocalDB connection string with a full SQL Server connection string, configure HTTPS certificates properly (`CookieSecurePolicy.None` in `Program.cs` is flagged for localhost only), and consider adding environment-specific `appsettings.Production.json`.

### No Automated Tests

The project has zero automated tests — no unit tests, no integration tests, no end-to-end tests. The risk areas where bugs are most likely to appear:

- `GameSessionService.OpenRoundAsync` — transaction-wrapped, concurrent-safe card assignment; easy to break with schema changes
- `AssignGameChangerAsync` — player state flag filtering logic; edge cases when no candidates match
- `CalculateRunningTotalAsync` — cumulative total calculation; critical for leaderboard accuracy
- `SessionTracker` — concurrency; adding methods without locks will cause race conditions

**Recommendation:** Add integration tests using `WebApplicationFactory<Program>` with an in-memory or test SQL Server database before making changes to `GameSessionService`.

### Polling Instead of SignalR Push

The `libman.json` loads the SignalR client library, but no `Hub` is registered in `Program.cs`. All real-time page updates use JavaScript `setTimeout` polling.

Current polling behavior:
- Lobby page polls every ~2 seconds to detect game start
- Round page polls to detect when a round opens or a Game Changer is assigned
- Multiple open browser tabs will each independently poll

**Impact:** Under light classroom load (20–30 students), polling is fine. At higher scale, or if you need sub-second responsiveness, implementing a proper SignalR Hub is the right fix.

### Student Duplicate User Records

`UserService` creates a new `User` row every time a student joins a session using the same name. There is no deduplication logic. If a student accidentally closes the browser and rejoins, they get a second User row with a different `UserId`. The prior UGC history attached to their original `UserId` will appear orphaned.

**Impact:** Minor in classroom use (sessions are short-lived), but worth fixing if sessions are longer or if students are expected to reconnect.

### Other TODOs in the Codebase

- `AssignCardsToPlayersAsync`: The `Structured` game mode branch is a placeholder that just picks `availableCards.First()` — the actual structured card selection logic is not implemented.
- `GetSessionByIdAsync` does not include navigation properties (Game, HostUser) — callers that need these must load them separately or use `GetSessionByJoinCodeAsync`.

---

## 7. Suggested Next Steps for Incoming Teams

### Recommended First Tasks (in priority order)

1. **Read the codebase with this guide open.** Follow the feature implementation traces in [Section 5](#5-key-services-and-feature-implementation) while reading the actual source files. Do this before changing anything.

2. **Run the app end-to-end.** Open two separate browser windows (or use private/incognito mode). Create a session in one, join as a student in the other, play through all 5 rounds, and download the session report. This gives you ground truth for what "working" looks like before you touch any code.

3. **Implement email for password reset.** This is the most complete partial feature — the hard part (token generation) is done. Integrating a free SMTP service or SendGrid is well-documented and self-contained.

4. **Add integration tests for `GameSessionService`.** This service is the core of the application. Tests here will catch regressions from future changes.

5. **Replace polling with SignalR.** Register a Hub in `Program.cs`, move the per-page status polling into hub group subscriptions, and push events from service methods. This is the largest architecture improvement and will require reading the SignalR documentation carefully.

### Extending the Round System

To add a new round type:
1. Add a new string to the `ROUND_TYPES` array in `GameSessionService.cs`
2. Insert `Card` rows in the database for the new `RoundType`
3. Insert `GameChanger` rows if you want changers specific to this round
4. Update the round count (currently hardcoded to 5 via the `ROUND_TYPES` array length)
5. Update the educator and student guides in this `/docs` folder

To add new player state flags:
1. Add the column to the `Users` table (EF migration)
2. Add the corresponding property to the `User` model in `ModelsV2/`
3. Add `Requires*` and `Sets*` columns to `Cards` and `GameChangers` tables
4. Update `AssignCardsToPlayersAsync`, `AssignGameChangerAsync`, and `SubmitPlayerChoiceAsync` to read/write the new flag
5. Update `GetPlayerFinancialStateAsync` if the flag affects financial calculations

### Areas Most Likely to Cause Confusion

- **Why students don't have accounts:** By design — reducing friction for classroom use. The tradeoff is no persistent student identity across sessions.
- **Why `SessionTracker` exists at all:** EF Core + session state alone can tell you a player is *registered* for a session, but not whether they are *currently connected*. `SessionTracker` bridges that gap at runtime.
- **Why there are two UGC row types:** A UGC row with `CardId` is a round card submission. A UGC row with `GameChangerId` is a Game Changer assignment. Both contribute to `RunningTotal`. The leaderboard query uses `RunningTotal` on the most recent UGC row per player.
- **Why `CurrentRoundNumber` starts at 1, not 0:** `ROUND_TYPES[roundNumber - 1]` is the index. The DB stores human-readable round numbers (1–5). Don't change this without updating both.
