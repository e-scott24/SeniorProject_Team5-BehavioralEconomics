# DealtHands — Educator Guide

## Table of Contents

1. [What Is DealtHands?](#1-what-is-dealthands)
2. [Registering an Account](#2-registering-an-account)
3. [Logging In](#3-logging-in)
4. [Creating a Game Session](#4-creating-a-game-session)
5. [Managing the Lobby](#5-managing-the-lobby)
6. [Running the Game](#6-running-the-game)
7. [End of Session](#7-end-of-session)
8. [The Five Rounds at a Glance](#8-the-five-rounds-at-a-glance)
9. [Difficulty Levels Explained](#9-difficulty-levels-explained)
10. [Troubleshooting](#10-troubleshooting)

---

## 1. What Is DealtHands?

DealtHands is a classroom simulation game built around behavioral economics. Students take on the role of a young adult making major life and financial decisions across five life stages. Each decision carries a real monthly cost or income change, and random life events ("Game Changers") can shake up a student's financial trajectory at any time.

As the educator, you create the session, set the difficulty, control when each round opens and closes, and receive a full report at the end. Your role is to facilitate discussion — the game gives you the data to do it.

---

## 2. Registering an Account

Only educators register for accounts. Students join sessions without an account.

1. Navigate to the application in a browser.
2. Click **Register** on the home page or navigation bar.
3. Fill in your **username**, **email address**, and a **password**.
   - Password must be at least 8 characters and include uppercase, lowercase, a number, and a special character.
4. Click **Register**.
5. You will be redirected to the Educator Dashboard upon success.

> **Note:** Each educator manages their own account and session history independently.

---

## 3. Logging In

1. Click **Login** on the navigation bar.
2. Enter your **username** and **password**.
3. Click **Login**.

Your session will remain active for **2 hours of inactivity** before automatically logging you out.

**Forgot your password?** Use the Forgot Password link on the login page. The application will display a reset link directly on screen — copy it and open it in the same browser. (Email delivery is not yet implemented; see the Developer Guide for details.)

---

## 4. Creating a Game Session

From the **Educator Dashboard**, click **Create Session**.

### Session Settings

| Setting | Description |
|---|---|
| **Session Name** | Optional friendly label for the session (e.g., "Period 3 – Finance Unit"). Shown in your session history. |
| **Game Mode** | *Random* picks each student's card for them. *Structured* allows the student to pick from a few different options. |
| **Difficulty** | Controls how often Game Changers fire. See [Section 9](#9-difficulty-levels-explained). |

Click **Create Session**. You will be taken to the **Lobby** with a **5-digit Join Code** displayed prominently.

> **Tip:** Project your screen or share the Join Code verbally. Students do not need an account — just the code.

---

## 5. Managing the Lobby

The Lobby is the waiting room before the game starts.

- The educator's view shows a live list of students who have joined. The list refreshes automatically every few seconds.
- Students see a waiting screen with their own name confirmed.
- There is **no minimum player count** required to start.

When your class is ready, click **Start Game**. All student screens will automatically advance to Round 1.

---

## 6. Running the Game

### Round Progression

DealtHands has **5 rounds**. You control when each round opens and when it closes.

**To open a round:**
Click **Open Round** on your educator view. Every student currently in the session is immediately assigned a card for that round. Their screens update automatically.

**To close a round:**
Click **Close Round**. This locks in the round results and advances the session to the next round. Students who have not yet submitted will have their card auto-submitted at the card's default amount.

> **Important:** Closing a round is permanent. You cannot reopen a round once it is closed.

### What Students Are Doing

During an open round, each student sees:
- Their assigned card (a life decision with a financial description and monthly dollar amount).
- A **Submit** button to confirm their choice.
- The **Budget Calculator** sidebar, which shows their running financial picture.

### Game Changers

Before or during a round, some students may be presented with a **Game Changer** — a random life event (positive or negative) that affects their finances. Game Changers appear as a full-screen overlay the student must dismiss before seeing their card.

Examples of Game Changers:
- *"You received a bonus at work"* — adds to income
- *"Car breakdown"* — one-time expense deducted from balance
- *"You were laid off"* — reduces monthly income by a percentage

You can see which students received Game Changers in the round results after closing the round.

### Monitoring Progress

Your educator view shows a submission count per round (e.g., "4 / 6 submitted"). When all students have submitted, the count is complete and you may close the round.

---

## 7. End of Session

After Round 5 is closed, click **End Session** to complete the game.

### Results and Leaderboard

The **Results** page shows:
- A ranked leaderboard of all students by their cumulative financial score (monthly net income after all rounds).
- Each student's final financial health status: **Healthy**, **Struggling**, or **Critical**.

Encourage classroom discussion around the leaderboard — why did some students end up in better financial shape than others? What decisions made the difference?

### Downloading the PDF Session Report

Click **Download Session Report** (PDF) on the Results page. The report includes:
- A summary table of every student's choices across all 5 rounds.
- Game Changer events each student received.
- Final scores and financial health ratings.

This PDF is suitable for printing or sharing with students as a post-game debrief resource.

### Session History

From the **Educator Dashboard**, your past sessions are listed under **Session History**. You can re-open session results at any time. Session records are stored permanently in the database.

---

## 8. The Five Rounds at a Glance

| Round | Topic | What Students Decide |
|---|---|---|
| 1 — Career | Income | Starting job / career path. Sets their monthly income for all subsequent rounds. |
| 2 — Student Loans | Debt | Whether they took student loans and the monthly payment amount. |
| 3 — Transportation | Expenses | Vehicle choice — no car, used car, car with a loan, etc. |
| 4 — Housing | Expenses | Living situation — renting alone, roommates, buying, staying home. |
| 5 — Family | Lifestyle | Marriage, children, and other family-related monthly costs. |

Each decision sets flags on the student's profile that affect which Game Changers can fire for the rest of the game. For example, a student who chose a car loan can later receive a "car breakdown" event; a student with no car cannot.

---

## 9. Difficulty Levels Explained

Difficulty controls **how often Game Changers fire**, not the dollar amounts on cards.

| Difficulty | Game Changer Frequency |
|---|---|
| Easy | 30% chance per round per player |
| Medium | 60% chance per round per player |
| Hard | 90% chance per round per player |

**Recommendation for first-time classes:** Start with **Easy** or **Medium**. Hard mode introduces significantly more chaos and is best for classes that are already familiar with the game mechanics.

---

## 10. Troubleshooting

**A student's screen is stuck / not updating.**
The application updates student screens every few seconds via automatic polling. Have the student refresh their browser tab.

**A student joined but isn't showing in the lobby.**
The lobby list refreshes automatically. Wait a few seconds. If they still don't appear, have the student refresh their browser and re-enter the join code.

**I accidentally closed a round too early.**
Rounds cannot be reopened. Students who hadn't submitted will have their card auto-submitted at the default amount. Continue to the next round and address it in the debrief discussion.

**The Join Code isn't working for a student.**
Confirm the student is typing exactly the 5-digit code shown on your screen (no spaces or extra characters). Also verify the session is still in **Waiting** or **InProgress** status — completed sessions cannot accept new joins.

**I can't log in.**
Try the **Forgot Password** link. Note that the reset link appears on-screen rather than being sent by email — copy the displayed link and open it in your browser.

**The PDF won't download.**
Ensure your browser is not blocking pop-ups or downloads from the application. Try a different browser if the issue persists.
