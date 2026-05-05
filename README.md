# DealtHands — Behavioral Economics Classroom Game

CS 483 / CIS 477 Senior Project — Team 5, Spring 2026  
University of Southern Indiana

DealtHands is an educational web game that simulates financial decision-making across five life stages (Career, Student Loans, Transportation, Housing, and Family). Educators run sessions in the classroom; students join with a code and make decisions in real time.

---

## Documentation

| Audience | Guide |
|---|---|
| Educators running a classroom session | [Educator Guide](docs/user-guide/educator-guide.md) |
| Students playing the game | [Student Guide](docs/user-guide/student-guide.md) |
| Developers inheriting this codebase | [Developer Guide](docs/developer-guide.md) |

---

## Quick Start (Developers)

1. Install **Visual Studio 2022** with the ASP.NET and web development workload (includes .NET 9 SDK and SQL Server LocalDB).
2. Clone this repo and open `DealtHands.sln`.
3. Create `DealtHands/appsettings.Development.json` (see the Developer Guide — the file is git-ignored).
4. In Package Manager Console: `Update-Database`
5. Press **F5** to run. The app opens at `https://localhost:7220`.

See the full [Developer Guide](docs/developer-guide.md) for architecture details, service documentation, and known gaps.

---

## Important Note

Since the application uses LocalDB, it can only be tested locally on one machine at a time. Each user (educator, student 1, student 2, etc.) must use a **separate browser** — not separate tabs in the same browser — because session cookies are shared across tabs.
