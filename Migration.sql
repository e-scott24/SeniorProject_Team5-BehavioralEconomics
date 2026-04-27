-- ============================================================================
-- DealtHands: Migrate Teammate's Database to Current State
-- ============================================================================
-- Brings a teammate's older database snapshot up to the current production
-- state. Run this once on the teammate's database.
--
-- WHAT THIS DOES:
--   1. Adds the Weight column to GameChanger (older snapshots are missing it).
--   2. Wipes the existing GameChanger data and the UGC rows referencing it.
--   3. Repopulates GameChanger with the 60 v4 cards.
--   4. Adds CHECK/FK/index hardening (idempotent).
--   5. Verifies the result.
--
-- WHAT THIS DOES NOT DO:
--   - Does NOT drop the LifeSituation tables if they exist. They are left
--     untouched (empty and harmless).
--   - Does NOT modify the User table or its uniqueness constraints. The
--     teammate's filtered username uniqueness and PlayerCode column are
--     preserved if they exist.
--
-- SAFETY:
--   - Wrapped in a single transaction with TRY/CATCH.
--   - Failure at any step rolls back everything.
--   - Idempotent constraint additions: safe to re-run.
-- ============================================================================

SET XACT_ABORT ON;
SET NOCOUNT ON;

-- ====================================================================
-- STEP 1: Add Weight column to GameChanger if missing
-- ====================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.GameChanger') AND name = 'Weight'
)
BEGIN
    ALTER TABLE dbo.GameChanger ADD Weight INT NULL;
    PRINT 'Added Weight column to GameChanger.';
END
ELSE
    PRINT 'Weight column already exists on GameChanger.';
GO

-- ============================================================================
-- BEGIN STEP 2: v4 GameChanger replacement migration
-- (Wipes existing GameChanger data + UGC rows referencing it; inserts 60 v4 cards.)
-- ============================================================================
-- ============================================================================
-- DealtHands GameChanger v4 Migration
-- ============================================================================
-- Clears the existing GameChanger table and repopulates with 60 v4 cards.
-- Wrapped in a single transaction with verification at each phase.
-- TRY/CATCH ensures any failure rolls back instead of leaving the table empty.
--
-- WARNING: This script also deletes rows from dbo.UGC where GameChangerId
-- IS NOT NULL, because fk_ugc_gamechanger blocks the GameChanger DELETE
-- otherwise. UGC rows with GameChangerId = NULL are left alone.
--
-- ROUND-TYPE MAPPING (RoundType column, varchar):
--   Career         -> Round 1
--   StudentLoan    -> Round 2
--   Transportation -> Round 3
--   Housing        -> Round 4
--   Family         -> Round 5
--
-- COLUMN NAMES (verified against actual dbo.GameChanger schema):
--   Title (varchar), Description (varchar), RoundType (varchar),
--   DifficultyLevel (tinyint), Weight (int), IsActive (bit),
--   MonthlyAmount (decimal), IncomeEffect (decimal),
--   IncomeEffectPercent (decimal), ExpenseEffect (decimal),
--   RequiresJob, RequiresStudentLoans, RequiresCar, RequiresCarLoan,
--   RequiresApartment, RequiresOwnsHome, RequiresRoommate,
--   RequiresMarried, RequiresChildren  (all bit, nullable),
--   SetsJob, SetsStudentLoans, SetsCar, SetsCarLoan,
--   SetsApartment, SetsOwnsHome, SetsRoommate, SetsMarried, SetsChildren
--   (all bit, nullable)
-- ============================================================================

-- Optional: print the actual schema first to compare against the assumed columns
-- SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, ORDINAL_POSITION
-- FROM INFORMATION_SCHEMA.COLUMNS
-- WHERE TABLE_NAME = 'GameChanger'
-- ORDER BY ORDINAL_POSITION;

SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- ====================================================================
    -- PHASE 1: Clear referencing UGC rows (required by FK fk_ugc_gamechanger)
    -- ====================================================================
    DECLARE @UgcRefCount INT = (
        SELECT COUNT(*) FROM dbo.UGC WHERE GameChangerId IS NOT NULL
    );
    PRINT 'UGC rows referencing GameChanger before delete: ' + CAST(@UgcRefCount AS VARCHAR(10));

    DELETE FROM dbo.UGC WHERE GameChangerId IS NOT NULL;

    DECLARE @UgcRefAfter INT = (
        SELECT COUNT(*) FROM dbo.UGC WHERE GameChangerId IS NOT NULL
    );
    PRINT 'UGC rows referencing GameChanger after delete:  ' + CAST(@UgcRefAfter AS VARCHAR(10));

    IF @UgcRefAfter <> 0
        THROW 50000, 'UGC referencing rows were not fully cleared.', 1;

    -- ====================================================================
    -- PHASE 2: Show & clear existing GameChanger data
    -- ====================================================================
    DECLARE @BeforeCount INT = (SELECT COUNT(*) FROM dbo.GameChanger);
    PRINT 'GameChanger rows before delete: ' + CAST(@BeforeCount AS VARCHAR(10));

    DELETE FROM dbo.GameChanger;

    DECLARE @AfterDeleteCount INT = (SELECT COUNT(*) FROM dbo.GameChanger);
    PRINT 'GameChanger rows after delete:  ' + CAST(@AfterDeleteCount AS VARCHAR(10));

    IF @AfterDeleteCount <> 0
        THROW 50001, 'GameChanger table was not fully cleared by DELETE.', 1;

    -- Reset identity so the new cards start at GameChangerId = 1
    DBCC CHECKIDENT ('dbo.GameChanger', RESEED, 0);
    PRINT 'Identity reseeded.';

    -- ====================================================================
    -- PHASE 3: Insert v4 cards
    -- ====================================================================
    INSERT INTO dbo.GameChanger
        (Title, Description, RoundType, DifficultyLevel, Weight, IsActive,
         MonthlyAmount, IncomeEffect, IncomeEffectPercent, ExpenseEffect,
         RequiresJob, RequiresStudentLoans, RequiresCar, RequiresCarLoan,
         RequiresApartment, RequiresOwnsHome, RequiresRoommate,
         RequiresMarried, RequiresChildren,
         SetsJob, SetsStudentLoans, SetsCar, SetsCarLoan,
         SetsApartment, SetsOwnsHome, SetsRoommate, SetsMarried, SetsChildren)
    VALUES
  ('Promotion! +10% (of salary)', 'Your boss discreetly taps your shoulder and asks you to come to his office. For a moment your thoughts run wild, expecting the worst. To your pleasant surprise, you realize that you have been promoted!', 'Career', 1, 4, 1, NULL, NULL, 0.1, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Performance Bonus! +$650', 'Your effort around the company has not gone unnoticed. Your manager put in a good word for you and this reflected itself in your paycheck!', 'Career', 1, 6, 1, 650, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Overtime Pay! +$700', 'For two weeks you have been working overtime. It was draining and you''ve been coming home exhausted. Still, you did get something out of this.', 'Career', 1, 6, 1, 700, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Remote Work Approved! -$100 monthly', 'You were checking your emails and found out your request for remote work was approved. With less money spent on gas and commuting, you ended up saving a nice little amount. You definitely aren''t going to miss morning traffic. Nice!', 'Career', 1, 5, 1, NULL, NULL, NULL, -100, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Side Gig Pays off! +$400', 'You weren''t expecting much, but your side gig has actually paid off. It wasn''t much extra work, and you earned a nice amount of money from it. Good job!', 'Career', 1, 5, 1, 400, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Industry Boom! +5% (of salary)', 'It turns out the industry you''re in has been doing really well lately. Demand is up, work has been steady, and more opportunities have started opening up for you. With strong business, your paycheck ended up looking a little better this month. Awesome!', 'Career', 2, 3, 1, NULL, NULL, 0.05, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Pay Cut! -10% (of salary)', 'Things haven''t been going well for your company. A series of bad decisions by executives much higher up the corporate ladder has suddenly put your career in question. Ultimately, they decided on a pay cut for all employees. You can always try finding another job if you don''t like it...', 'Career', 2, 3, 1, NULL, NULL, -0.1, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Fired! (Remove career)', 'Your boss discreetly taps your shoulder and asks you to come to his office. For a moment, your thoughts run wild as you expect the worst. Soon, you realize your fears were justified: your position has been terminated, effective immediately! Better start applying!', 'Career', 3, 1, 1, -1500, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Sick Days! -15% (of salary, one-time)', 'You don''t know how you got it, but you woke up feeling absolutely miserable. After forcing yourself through a few miserable shifts, you finally gave in and had to stay home for several days. Unfortunately, missing work means missing pay, and your next paycheck reflects it.', 'Career', 1, 5, 1, -450, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Office Relocation! +$125 monthly', 'Your company recently announced that your office is being relocated across town. What used to be a simple drive to work has now turned into a longer commute with more gas, more traffic, and more frustration.', 'Career', 1, 4, 1, NULL, NULL, NULL, 125, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Demoted! -$400 monthly', 'After a rough quarter and some internal restructuring, management decided changes needed to be made. Unfortunately, your position was one of them. Your responsibilities have been reduced, your title doesn''t sound nearly as impressive anymore, and of course, your paycheck took a hit too.', 'Career', 2, 3, 1, NULL, -400, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Hours Reduced! -$300 monthly', 'Business has been slower than expected lately, and your employer has started cutting hours to save money. Sadly, your schedule was one of the first to be reduced. Fewer shifts means less money coming in, and suddenly your budget feels a whole lot tighter.', 'Career', 1, 4, 1, NULL, -300, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Federal Grant! +$1,500', 'After filling out what felt like endless paperwork, you finally receive some good news. Your federal aid application was approved, and a grant has been applied directly toward your education costs. Free money!', 'StudentLoan', 1, 4, 1, 1500, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Scholarship Approved! +$1,000', 'You honestly didn''t expect much when you submitted the application, but somehow it worked out. A scholarship has been approved, helping cover part of your tuition and reducing how much you need to borrow.', 'StudentLoan', 1, 5, 1, 1000, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Partial Loan Forgiveness! -$75 monthly', 'After review, part of your student loan balance has officially been forgiven. Watching that total number drop feels almost unreal. You still have plenty left to deal with, but at least now it feels a little less overwhelming.', 'StudentLoan', 2, 3, 1, NULL, NULL, NULL, -75, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Interest Freeze! -$40 monthly', 'For a limited time, interest on your student loans has been paused. Your balance finally stops growing for once, giving you a rare chance to actually make progress instead of just keeping up.', 'StudentLoan', 1, 5, 1, NULL, NULL, NULL, -40, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Forgot Application! -$500', 'With everything else going on in your life, one important deadline completely slipped past you. Unfortunately, that deadline was for your financial aid application. Missing it means losing access to money you were depending on, and now you''re left covering the difference yourself. Expensive mistake.', 'StudentLoan', 1, 5, 1, -500, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Missed Payment! -$50', 'Between work, bills, and trying to survive life in general, you completely forgot to make your student loan payment on time. A late fee gets added, and your account takes a small but frustrating hit. It may not seem huge now, but these things tend to snowball.', 'StudentLoan', 1, 7, 1, -50, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Interest Rate Spike! +$50 monthly', 'Without much warning, your student loan interest rate increases. Suddenly your monthly payments are higher, and even worse, the total amount you''ll pay over time keeps growing. Apparently the loans weren''t stressful enough already.', 'StudentLoan', 1, 5, 1, NULL, NULL, NULL, 50, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Default Notice! +$150 monthly', 'You have been falling behind on your student loan payments for a while now, hoping you could catch up before it became a real problem. Unfortunately, that time has passed. An official default notice arrives, and suddenly the situation feels a whole lot more serious.', 'StudentLoan', 3, 1, 1, NULL, NULL, NULL, 150, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Credit Score Drop! +$30 monthly', 'Late payments and missed deadlines have finally started catching up with you. Your credit score takes a hit, which means future loans, apartments, and even some jobs may become harder to get. This problem has a way of following you around.', 'StudentLoan', 1, 5, 1, NULL, NULL, NULL, 30, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Repayment Plan Denied! -$100', 'You applied for a repayment plan that would have made your monthly payments a little easier to manage. Unfortunately, your request was denied. That means you are stuck with your current payment amount whether you can comfortably afford it or not.', 'StudentLoan', 1, 5, 1, -100, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Collections Transfer! -$400', 'Since your student loans remained unpaid for too long, the account has officially been sent to collections. Extra fees start appearing, the phone calls begin, and the stress level rises immediately. Ignoring it is no longer an option.', 'StudentLoan', 2, 2, 1, -400, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Disqualified from Financial Aid! +$200 monthly', 'Due to previous financial aid issues, your eligibility for future assistance has been suspended. That means less help moving forward and much more coming directly out of your own pocket. School just became a lot more expensive.', 'StudentLoan', 2, 2, 1, NULL, NULL, NULL, 200, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Gas Prices Fall! -$50 monthly', 'For once, the numbers at the gas station actually went down instead of up. Filling your tank hurts a little less, and over the course of the month those smaller payments start adding up. It may not seem huge at first, but saving money every single week feels nice.', 'Transportation', 1, 6, 1, NULL, NULL, NULL, -50, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Vehicle Insurance Discount! -$25 monthly', 'After another year of avoiding accidents, tickets, and general disaster, your insurance company rewards you with a lower premium. It is not life changing money, but it is definitely better than another increase.', 'Transportation', 1, 5, 1, NULL, NULL, NULL, -25, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Toll Waiver! -$30 monthly', 'A temporary local program removes toll charges on your regular route to work. It may seem like a small thing, but not having to pay every single morning feels surprisingly nice. Sometimes the little things help the most.', 'Transportation', 1, 5, 1, NULL, NULL, NULL, -30, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Carpool Savings! -$60 monthly', 'You and a coworker finally decide to stop driving separately every single day. Sharing rides means less gas, less wear on your car, and a little more money staying in your account each month. It is a simple change, but a smart one.', 'Transportation', 1, 4, 1, NULL, NULL, NULL, -60, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Towed Car! -$300', 'You apparently overlooked a sign. By the time you came back to your car, you realized that it had been towed. After making phone calls, visiting the impound lot, and paying the fines, you eventually got your car back after a great deal of frustration.', 'Transportation', 1, 5, 1, -300, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Crash! -$1,500', 'You thought that your day couldn''t get any worse. That was until your car suddenly got into an accident. One second traffic was normal, and the next you were standing on the side of the road staring at the damage. Thankfully nobody was seriously hurt, but between repairs, insurance deductibles, towing costs, and all the frustrating phone calls, the whole situation became an expensive nightmare.', 'Transportation', 2, 2, 1, -1500, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Engine Failure! -$2,000', 'Out of nowhere your car started making a terrible grinding noise and then just died on the side of the road. The mechanic said the engine needed major work. The repair bill was painful and you were without a car for days while it got fixed.', 'Transportation', 2, 2, 1, -2000, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Gas Prices Soar! +$75 monthly', 'Recent political events overseas have taken a toll on your budget. Until now, it hadn''t affected you, but you finally felt it at the pump. Gas prices have increased, and it doesn''t seem like they''ll be going back down anytime soon.', 'Transportation', 1, 6, 1, NULL, NULL, NULL, 75, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('DUI Charge! -$2,500', 'Your friends invited you out to have a few drinks. No problem! However, one way or another, you decided to drive home afterwards. You didn''t think that you would get pulled over, until it actually happened. Now you''re stuck with court fees, legal costs, mandatory classes, and double the insurance rates.', 'Transportation', 3, 1, 1, -2500, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Insurance Premium Reassessment! +$40 monthly', 'This year, your insurance company looked over your policy again and decided that your rates needed to go up. It could have been because of where you lived, how old you were, or just because insurance companies like to make people suffer. In either case, your monthly premium is now much higher.', 'Transportation', 2, 3, 1, NULL, NULL, NULL, 40, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Speeding Ticket! -$200', 'You were driving down the highway before you noticed flashing red-and-blue lights in your mirror. The ticket ended up costing you a good chunk of money plus court costs, and now you''ve got points on your license that are going to make your insurance rates jump next renewal.', 'Transportation', 1, 6, 1, -200, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Flat Tire! -$150', 'You hit a pothole a little too hard and heard that familiar flapping sound. Pulling over, you found the tire completely flat. Yet another unexpected cost...', 'Transportation', 1, 7, 1, -150, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Rent Decrease! -$75 monthly', 'Your landlord sent a notice saying rent is actually being lowered this renewal. Maybe the market cooled down or they just want to keep you there, but your monthly payment just got a little smaller.', 'Housing', 1, 4, 1, NULL, NULL, NULL, -75, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Roommate Found! -$300 monthly', 'After putting up ads and talking to a few people, you finally found a decent roommate. They seem reliable, pay their share on time, and help split the bills. You even seem to genuinely bond with them. Having that extra person covering half the rent makes your budget feel a lot lighter.', 'Housing', 1, 4, 1, NULL, NULL, NULL, -300, NULL, NULL, NULL, NULL, 1, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL),
  ('Utility Refund! +$200', 'The utility company figured out they had overcharged you for the last few months. A refund check showed up in the mail, putting some unexpected money back into your account.', 'Housing', 1, 5, 1, 200, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Deposit Returned! +$800', 'You finally moved out of your old place and after the walkthrough, the landlord sent back your full security deposit with no deductions. Getting that money back right when you needed it certainly helped.', 'Housing', 1, 4, 1, 800, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Eviction! -$2,000', 'Things had been getting tighter for months, and eventually rent started falling behind. At first it was just one late payment, then another, and before long you were constantly trying to catch up while falling further behind. You kept hoping you could fix it before it became a real problem, but the notice on your door made it official. Now you are being forced to move, and finding a new place on short notice while already struggling financially is expensive, stressful, and frustrating.', 'Housing', 3, 1, 1, -2000, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 0, NULL, NULL),
  ('Lease Penalty! -$1,000', 'Apparently you violated one of the terms of your lease. You didn''t realize it at the time, and it honestly didn''t feel like a big deal when it happened, but your landlord saw it very differently. What started as a simple mistake ended up costing you more than expected, and now your already tight budget feels even more strained because of it.', 'Housing', 2, 2, 1, -1000, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Plumbing Leak! -$400', 'At first it started as something you barely noticed. Just a small, annoying drip you told yourself you would deal with later. By the time you finally addressed it, the problem had gotten worse and caused some water damage. What should have been a simple fix turned into repairs, cleanup, and an unexpected expense that hit your budget harder than expected.', 'Housing', 1, 5, 1, -400, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Nightmare Roommate! -$800', 'At first, having a roommate seemed like a great financial decision. Then reality set in. Late rent, constant arguments, broken things, loud nights, surprise guests, and enough stress to make your own home feel miserable. Eventually, they move out suddenly, leaving you stuck covering unpaid bills, damages, and the full rent yourself.', 'Housing', 2, 2, 1, -800, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL),
  ('Broken Appliance! -$500', 'One day, one of your essential appliances just stopped working. No warning, no gradual decline, just a sudden breakdown at the worst possible time. You didn''t expect it, and now replacing or repairing it wasn''t optional, and the cost ended up being higher than you would''ve liked.', 'Housing', 1, 5, 1, -500, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Ant Infestation! -$200', 'You started noticing a few ants around the kitchen, but it quickly became clear it wasn''t just a few. Before long, they were everywhere. You tried dealing with it yourself at first, but it only got worse. Getting rid of them meant cleaning, treatment, and replacing some food, all of which added up faster than expected.', 'Housing', 1, 5, 1, -200, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Mold Growth! -$1,200', 'A hidden moisture problem slowly turned into visible mold before you even realized what was happening. At first you thought it was nothing serious, but it kept spreading. By the time you caught it, it had spread enough to require professional cleaning and repairs, turning into an expensive and frustrating fix.', 'Housing', 2, 2, 1, -1200, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Late Fee! -$100', 'You missed the rent deadline by just a bit, thinking you had more time than you did. It completely slipped your mind with everything else going on. Unfortunately, your landlord didn''t. A late fee was added immediately, turning an already tight month into an even tighter one.', 'Housing', 1, 7, 1, -100, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Birthday Gift! +$300', 'Your beloved grandma gave you a check for your birthday, wishing you happiness and the best of luck with your financial struggles. How sweet!', 'Family', 1, 7, 1, 300, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Inheritance! +$2,000', 'A distant relative passes away and leaves you a portion of their inheritance. You didn''t know them well and only met a handful of times, most of which you barely remember, but the help is still appreciated.', 'Family', 2, 2, 1, 2000, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Spouse Got a Raise! +$400 monthly', 'You were having dinner when your spouse mentioned some good news. They got a raise! Great!', 'Family', 1, 4, 1, NULL, 400, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Help from Family! +$600', 'Your loving family decided to help you with your financial difficulties and took some of the burden off your shoulders. It''s definitely a welcome relief.', 'Family', 1, 5, 1, 600, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Phone Bill Covered! -$50 monthly', 'Your parent offers to step in and pay for your phone bill this month. It''s a small gesture, but it removes one of your recurring expenses and gives you a bit of breathing room when you need it most.', 'Family', 1, 5, 1, NULL, NULL, NULL, -50, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Got Money Back! +$200', 'A family member unexpectedly pays you back money they owed you from a while ago. You weren''t expecting it anymore, so the timing feels like a pleasant surprise that helps your budget a little.', 'Family', 1, 6, 1, 200, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Medical Emergency! -$1,500', 'A family member suddenly faces a medical emergency, and everything escalates quickly. Between hospital visits, treatments, and unexpected bills, the financial strain shows up almost immediately alongside the stress of the situation.', 'Family', 2, 2, 1, -1500, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Veterinarian Visit! -$300', 'Your fluffy pet starts acting strangely, so you take them to the vet expecting a routine checkup. Instead, it turns into tests, medication, and a much higher bill than you were prepared for.', 'Family', 1, 5, 1, -300, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Funeral Expenses! -$800', 'Losing a loved one brings emotional weight, but the financial side adds another layer of difficulty. Funeral arrangements, travel, and related costs quickly build up during an already overwhelming time.', 'Family', 1, 4, 1, -800, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Emergency Travel! -$500', 'You have to travel on short notice because of a family situation. With no time to plan ahead, transportation and lodging costs end up being significantly higher than usual, adding financial stress to an already urgent situation.', 'Family', 1, 5, 1, -500, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Bailed Out Relative! -$700', 'A family member ends up in financial trouble and needs help getting back on their feet. You step in to cover what they can''t. You don''t regret it, but it does end up placing yet another strain on your finances.', 'Family', 2, 3, 1, -700, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('Divorce! -$3,000', 'Sometimes things simply do not work out. What started as arguments and stress eventually turned into lawyers, paperwork, and the difficult decision to separate. Between legal fees, moving costs, dividing finances, and trying to figure out who gets what, divorce proved to be emotionally exhausting and financially brutal. Starting over is never cheap, and rebuilding your life takes far more than just time.', 'Family', 3, 1, 1, -3000, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL);

    -- ====================================================================
    -- PHASE 4: Verify insert
    -- ====================================================================
    DECLARE @AfterInsertCount INT = (SELECT COUNT(*) FROM dbo.GameChanger);
    PRINT 'Rows after insert:  ' + CAST(@AfterInsertCount AS VARCHAR(10));

    IF @AfterInsertCount <> 60
        THROW 50002, 'Expected 60 rows after INSERT.', 1;

    COMMIT TRANSACTION;
    PRINT 'Migration completed successfully. Run the verification queries below.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    PRINT '---- MIGRATION FAILED ----';
    PRINT 'Error: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO

-- ============================================================================
-- VERIFICATION QUERIES (run after the COMMIT above)
-- ============================================================================

-- 1) Total count (expected: 60)
SELECT 'Total cards' AS Check_Name, COUNT(*) AS Result FROM dbo.GameChanger;

-- 2) Cards per round-type (expected: 12 each, 5 rows)
SELECT RoundType, COUNT(*) AS CardCount
FROM dbo.GameChanger
GROUP BY RoundType
ORDER BY RoundType;

-- 3) Difficulty distribution per round-type (expected per round: 1=8, 2=3, 3=1)
SELECT RoundType, DifficultyLevel, COUNT(*) AS Cnt
FROM dbo.GameChanger
GROUP BY RoundType, DifficultyLevel
ORDER BY RoundType, DifficultyLevel;

-- 4) Catastrophic cards (expected exactly 5: Fired, Default Notice, DUI Charge, Eviction, Divorce)
SELECT Title, RoundType, Weight
FROM dbo.GameChanger
WHERE DifficultyLevel = 3
ORDER BY RoundType;

-- 5) Cards that change player state (expected exactly 5: Fired, Roommate Found, Eviction, Nightmare Roommate, Divorce)
SELECT Title, RoundType,
       SetsJob, SetsApartment, SetsRoommate, SetsMarried
FROM dbo.GameChanger
WHERE SetsJob IS NOT NULL
   OR SetsStudentLoans IS NOT NULL
   OR SetsCar IS NOT NULL
   OR SetsCarLoan IS NOT NULL
   OR SetsApartment IS NOT NULL
   OR SetsOwnsHome IS NOT NULL
   OR SetsRoommate IS NOT NULL
   OR SetsMarried IS NOT NULL
   OR SetsChildren IS NOT NULL
ORDER BY RoundType, Title;

-- 6) Effect column population (should add up to 60 across distinct cards;
--    a few cards have only Sets* and no $ effect, but every card has either
--    MonthlyAmount, IncomeEffect, IncomeEffectPercent, or ExpenseEffect)
SELECT
    SUM(CASE WHEN MonthlyAmount        IS NOT NULL THEN 1 ELSE 0 END) AS HasMonthlyAmount,
    SUM(CASE WHEN IncomeEffect         IS NOT NULL THEN 1 ELSE 0 END) AS HasIncomeEffect,
    SUM(CASE WHEN IncomeEffectPercent  IS NOT NULL THEN 1 ELSE 0 END) AS HasIncomeEffectPercent,
    SUM(CASE WHEN ExpenseEffect        IS NOT NULL THEN 1 ELSE 0 END) AS HasExpenseEffect
FROM dbo.GameChanger;


-- ============================================================================
-- STEP 3: Constraint hardening (idempotent)
-- ============================================================================
SET XACT_ABORT ON;
SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- 3.1 UGC xor invariant
    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'ck_ugc_card_xor_gamechanger')
    BEGIN
        DECLARE @bad_xor INT = (
            SELECT COUNT(*) FROM dbo.UGC
            WHERE (CardId IS NULL AND GameChangerId IS NULL)
               OR (CardId IS NOT NULL AND GameChangerId IS NOT NULL)
        );
        IF @bad_xor > 0
            THROW 60001, 'UGC has rows violating the card-or-gamechanger invariant.', 1;
        ALTER TABLE dbo.UGC
        ADD CONSTRAINT ck_ugc_card_xor_gamechanger CHECK (
            (CardId IS NOT NULL AND GameChangerId IS NULL)
            OR (CardId IS NULL AND GameChangerId IS NOT NULL)
        );
        PRINT 'Added ck_ugc_card_xor_gamechanger';
    END
    ELSE PRINT 'Skipped ck_ugc_card_xor_gamechanger (already exists)';

    -- 3.2 RoundType enums
    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'ck_card_roundtype')
    BEGIN
        ALTER TABLE dbo.Card ADD CONSTRAINT ck_card_roundtype
            CHECK (RoundType IN ('Career', 'StudentLoan', 'Transportation', 'Housing', 'Family'));
        PRINT 'Added ck_card_roundtype';
    END
    ELSE PRINT 'Skipped ck_card_roundtype (already exists)';

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'ck_gamechanger_roundtype')
    BEGIN
        ALTER TABLE dbo.GameChanger ADD CONSTRAINT ck_gamechanger_roundtype
            CHECK (RoundType IN ('Career', 'StudentLoan', 'Transportation', 'Housing', 'Family'));
        PRINT 'Added ck_gamechanger_roundtype';
    END
    ELSE PRINT 'Skipped ck_gamechanger_roundtype (already exists)';

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'ck_gameround_roundtype')
    BEGIN
        ALTER TABLE dbo.GameRound ADD CONSTRAINT ck_gameround_roundtype
            CHECK (RoundType IN ('Career', 'StudentLoan', 'Transportation', 'Housing', 'Family'));
        PRINT 'Added ck_gameround_roundtype';
    END
    ELSE PRINT 'Skipped ck_gameround_roundtype (already exists)';

    -- 3.3 Status enums
    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'ck_gamesession_status')
    BEGIN
        ALTER TABLE dbo.GameSession ADD CONSTRAINT ck_gamesession_status
            CHECK (Status IN ('Waiting', 'InProgress', 'Paused', 'Completed'));
        PRINT 'Added ck_gamesession_status';
    END
    ELSE PRINT 'Skipped ck_gamesession_status (already exists)';

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'ck_gameround_status')
    BEGIN
        ALTER TABLE dbo.GameRound ADD CONSTRAINT ck_gameround_status
            CHECK (Status IN ('NotStarted', 'Open', 'Closed'));
        PRINT 'Added ck_gameround_status';
    END
    ELSE PRINT 'Skipped ck_gameround_status (already exists)';

    -- 3.4 Difficulty enum
    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'ck_gamesession_difficulty')
    BEGIN
        ALTER TABLE dbo.GameSession ADD CONSTRAINT ck_gamesession_difficulty
            CHECK (Difficulty IS NULL OR Difficulty IN ('Easy', 'Medium', 'Hard'));
        PRINT 'Added ck_gamesession_difficulty';
    END
    ELSE PRINT 'Skipped ck_gamesession_difficulty (already exists)';

    -- 3.5 Numeric range checks
    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'ck_gameround_roundnumber_range')
    BEGIN
        ALTER TABLE dbo.GameRound ADD CONSTRAINT ck_gameround_roundnumber_range
            CHECK (RoundNumber BETWEEN 1 AND 5);
        PRINT 'Added ck_gameround_roundnumber_range';
    END
    ELSE PRINT 'Skipped ck_gameround_roundnumber_range (already exists)';

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'ck_gamechanger_difficulty_range')
    BEGIN
        ALTER TABLE dbo.GameChanger ADD CONSTRAINT ck_gamechanger_difficulty_range
            CHECK (DifficultyLevel BETWEEN 1 AND 3);
        PRINT 'Added ck_gamechanger_difficulty_range';
    END
    ELSE PRINT 'Skipped ck_gamechanger_difficulty_range (already exists)';

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'ck_card_difficulty_range')
    BEGIN
        ALTER TABLE dbo.Card ADD CONSTRAINT ck_card_difficulty_range
            CHECK (DifficultyLevel BETWEEN 0 AND 3);
        PRINT 'Added ck_card_difficulty_range';
    END
    ELSE PRINT 'Skipped ck_card_difficulty_range (already exists)';

    -- 3.6 Filtered unique index for game-changer UGC rows
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'uq_ugc_user_round_gamechanger' AND object_id = OBJECT_ID('dbo.UGC'))
    BEGIN
        CREATE UNIQUE INDEX uq_ugc_user_round_gamechanger
        ON dbo.UGC (UserId, GameRoundId, GameChangerId)
        WHERE GameChangerId IS NOT NULL;
        PRINT 'Added uq_ugc_user_round_gamechanger';
    END
    ELSE PRINT 'Skipped uq_ugc_user_round_gamechanger (already exists)';

    -- 3.7 Restore fk_ugc_session
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'fk_ugc_session')
    BEGIN
        DECLARE @orphans INT = (
            SELECT COUNT(*) FROM dbo.UGC u
            LEFT JOIN dbo.GameSession g ON u.GameSessionId = g.GameSessionId
            WHERE g.GameSessionId IS NULL
        );
        IF @orphans > 0
        BEGIN
            PRINT 'WARNING: UGC has ' + CAST(@orphans AS VARCHAR(20)) + ' orphan rows. fk_ugc_session NOT added.';
        END
        ELSE
        BEGIN
            ALTER TABLE dbo.UGC
            ADD CONSTRAINT fk_ugc_session FOREIGN KEY (GameSessionId)
                REFERENCES dbo.GameSession(GameSessionId);
            PRINT 'Added fk_ugc_session';
        END
    END
    ELSE PRINT 'Skipped fk_ugc_session (already exists)';

    COMMIT TRANSACTION;
    PRINT '';
    PRINT '====================================================================';
    PRINT 'Constraint hardening complete.';
    PRINT '====================================================================';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    PRINT '---- CONSTRAINT HARDENING FAILED ----';
    PRINT 'Error: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO

-- ============================================================================
-- STEP 4: Final verification
-- ============================================================================
PRINT '';
PRINT '====================================================================';
PRINT 'Final Verification';
PRINT '====================================================================';

-- Total cards (expect 60)
SELECT 'Total GameChangers' AS Check_Name, COUNT(*) AS Cnt,
       CASE WHEN COUNT(*) = 60 THEN 'PASS' ELSE 'FAIL' END AS Result
FROM dbo.GameChanger;

-- Per-round count (expect 12 each)
SELECT RoundType, COUNT(*) AS CardCount
FROM dbo.GameChanger GROUP BY RoundType ORDER BY RoundType;

-- DL distribution per round (expect 8/3/1)
SELECT RoundType, DifficultyLevel, COUNT(*) AS Cnt
FROM dbo.GameChanger GROUP BY RoundType, DifficultyLevel
ORDER BY RoundType, DifficultyLevel;

-- Catastrophic weight (expect max <= 2)
SELECT 'Catastrophic max weight' AS Check_Name, MAX(Weight) AS MaxW,
       CASE WHEN MAX(Weight) <= 2 THEN 'PASS' ELSE 'FAIL' END AS Result
FROM dbo.GameChanger WHERE DifficultyLevel = 3;

-- Constraint count on hardened tables (expect 11)
SELECT 'Hardening constraints' AS Check_Name, COUNT(*) AS Cnt
FROM sys.check_constraints
WHERE name IN (
    'ck_ugc_card_xor_gamechanger',
    'ck_card_roundtype', 'ck_gamechanger_roundtype', 'ck_gameround_roundtype',
    'ck_gamesession_status', 'ck_gameround_status', 'ck_gamesession_difficulty',
    'ck_gameround_roundnumber_range', 'ck_gamechanger_difficulty_range',
    'ck_card_difficulty_range'
);

-- fk_ugc_session present
SELECT 'fk_ugc_session present' AS Check_Name,
       CASE WHEN EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'fk_ugc_session')
            THEN 'YES' ELSE 'NO (check warning above)' END AS Result;

-- Filtered unique index present
SELECT 'uq_ugc_user_round_gamechanger present' AS Check_Name,
       CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'uq_ugc_user_round_gamechanger')
            THEN 'YES' ELSE 'NO' END AS Result;
