/*  Name: Jason Black
    Date: 3/20/2026
    Last Update: 3/23/2026

    Game Changer card definitions and display logic.
    Handles card creation, shuffling, and mapping to display models.

    Card logic is in GameChangerService.cs.
    Card rendering is in _GameChangerCard.cshtml.
    Card overlay (CSS, HTML, JS sequencer) is in _GameChangerOverlay.cshtml.
    Financial effects are applied in GameEngine.cs via ApplyGameChanger().
    Overlay model is in GameChangerOverlayModel.cs.

    The correct file paths are the following:
    ...\DealtHands\DealtHands\Services\GameChangerService.cs
    ...\DealtHands\DealtHands\Pages\Shared\Cards\_GameChangerCard.cshtml
    ...\DealtHands\DealtHands\Pages\Shared\Cards\_GameChangerOverlay.cshtml
    ...\DealtHands\DealtHands\Pages\Shared\GameChangerOverlayModel.cs
    ...\DealtHands\DealtHands\Services\GameEngine.cs

    Card structure:
    SalaryCard    = permanent monthly income change (positive or negative)
    BalanceCard   = one-time lump sum credit or debit to balance
    JobLossCard   = zeros monthly income + adds severance to balance
    RecurringCard = permanent monthly balance change (recurring expense, not income)
    DebtCard      = directly adjusts TotalDebt without touching balance
    ComboCard     = applies both a salary change and a balance hit simultaneously

    To add a new card:
    1. Call the appropriate helper
    2. Give it the next available Id
    3. Set TriggersInRound to the correct round number (1-5)
    4. No other files need to change
*/

using DealtHands.Models;
using DealtHands.Models.Cards;

namespace DealtHands.Services
{
    public class GameChangerService
    {
        private readonly Random _random = new Random();

        // Card builder methods for different types of game changers. Each method encapsulates the logic for constructing a specific type of card.

        // Permanent monthly income change (positive or negative)
        private static GameChangerEvent SalaryCard(int id, int round, string category,
            bool positive, string title, string desc, decimal delta, string note,
            string secondaryLabel = null, string secondaryAmountDisplay = null) => new()
            {
                Id = id,
                TriggersInRound = round,
                Category = category,
                DifficultyLevel = "All",
                IsPositive = positive,
                CardType = positive ? CardType.Positive : CardType.Negative,
                Title = title,
                Description = desc,
                SalaryChange = delta,
                EffectLabel = positive ? "Monthly Salary Increase" : "Monthly Salary Reduction",
                EffectAmount = positive ? $"+ {delta:C0} / mo" : $"- {Math.Abs(delta):C0} / mo",
                BadgeText = positive ? "Permanent increase" : "Permanent reduction",
                EffectNote = note,
                CardKind = CardKind.Salary,
                SecondaryLabel = secondaryLabel,
                SecondaryAmountDisplay = secondaryAmountDisplay
            };

        // One-time balance credit or debit
        private static GameChangerEvent BalanceCard(int id, int round, string category,
            bool positive, string title, string desc, decimal amount, string label, string note,
            string secondaryLabel = null, string secondaryAmountDisplay = null) => new()
            {
                Id = id,
                TriggersInRound = round,
                Category = category,
                DifficultyLevel = "All",
                IsPositive = positive,
                CardType = positive ? CardType.Positive : CardType.Negative,
                Title = title,
                Description = desc,
                BalanceChange = positive ? Math.Abs(amount) : -Math.Abs(amount),
                EffectLabel = label,
                EffectAmount = positive ? $"+ {Math.Abs(amount):C0}" : $"- {Math.Abs(amount):C0}",
                BadgeText = positive ? "Added to balance now" : "One-time deduction",
                EffectNote = note,
                CardKind = CardKind.Balance,
                SecondaryLabel = secondaryLabel,
                SecondaryAmountDisplay = secondaryAmountDisplay
            };

        // Job loss, zeros income and pays severance to balance
        private static GameChangerEvent JobLossCard(int id, int round, string category,
            string title, string desc, decimal severance, string note,
            string secondaryLabel = null, string secondaryAmountDisplay = null) => new()
            {
                Id = id,
                TriggersInRound = round,
                Category = category,
                DifficultyLevel = "All",
                IsPositive = false,
                CardType = CardType.Negative,
                Title = title,
                Description = desc,
                IsJobLoss = true,
                BalanceChange = severance,
                EffectLabel = "Monthly Income Lost",
                EffectAmount = "$0 / mo",
                EffectNote = note,
                SecondaryLabel = secondaryLabel ?? "Severance Pay (One-Time)",
                SecondaryAmountDisplay = secondaryAmountDisplay ?? $"+ {severance:C0}",
                CardKind = CardKind.JobLoss
            };

        // Permanent monthly expense, like SalaryCard but semantically an expense,
        // not income (e.g. new insurance bill, subscription, recurring fee)
        private static GameChangerEvent RecurringCard(int id, int round, string category,
            bool positive, string title, string desc, decimal monthlyAmount, string label, string note,
            string secondaryLabel = null, string secondaryAmountDisplay = null) => new()
            {
                Id = id,
                TriggersInRound = round,
                Category = category,
                DifficultyLevel = "All",
                IsPositive = positive,
                CardType = positive ? CardType.Positive : CardType.Negative,
                Title = title,
                Description = desc,
                SalaryChange = positive ? Math.Abs(monthlyAmount) : -Math.Abs(monthlyAmount),
                EffectLabel = label,
                EffectAmount = positive ? $"+ {Math.Abs(monthlyAmount):C0} / mo" : $"- {Math.Abs(monthlyAmount):C0} / mo",
                BadgeText = positive ? "Recurring saving" : "Recurring expense",
                EffectNote = note,
                CardKind = CardKind.Recurring,
                SecondaryLabel = secondaryLabel,
                SecondaryAmountDisplay = secondaryAmountDisplay
            };

        // Directly adjusts TotalDebt without touching balance
        // Negative DebtChange = debt reduced (positive card)
        // Positive DebtChange = debt added  (negative card)
        private static GameChangerEvent DebtCard(int id, int round, string category,
            bool positive, string title, string desc, decimal amount, string label, string note,
            string secondaryLabel = null, string secondaryAmountDisplay = null) => new()
            {
                Id = id,
                TriggersInRound = round,
                Category = category,
                DifficultyLevel = "All",
                IsPositive = positive,
                CardType = positive ? CardType.Positive : CardType.Negative,
                Title = title,
                Description = desc,
                DebtChange = positive ? -Math.Abs(amount) : Math.Abs(amount),
                EffectLabel = label,
                EffectAmount = positive ? $"- {Math.Abs(amount):C0} debt" : $"+ {Math.Abs(amount):C0} debt",
                BadgeText = positive ? "Debt reduced" : "New debt",
                EffectNote = note,
                CardKind = CardKind.Debt,
                SecondaryLabel = secondaryLabel,
                SecondaryAmountDisplay = secondaryAmountDisplay
            };

        // Applies both a salary change and a balance hit simultaneously
        // e.g. promotion with signing bonus, or pay cut with penalty payment
        private static GameChangerEvent ComboCard(int id, int round, string category,
            bool positive, string title, string desc,
            decimal salaryChange, decimal balanceAmount, string label, string note,
            string secondaryLabel = null, string secondaryAmountDisplay = null) => new()
            {
                Id = id,
                TriggersInRound = round,
                Category = category,
                DifficultyLevel = "All",
                IsPositive = positive,
                CardType = positive ? CardType.Positive : CardType.Negative,
                Title = title,
                Description = desc,
                SalaryChange = salaryChange,
                BalanceChange = positive ? Math.Abs(balanceAmount) : -Math.Abs(balanceAmount),
                EffectLabel = label,
                EffectAmount = positive
                ? $"+ {Math.Abs(salaryChange):C0} / mo  +  {Math.Abs(balanceAmount):C0}"
                : $"- {Math.Abs(salaryChange):C0} / mo  -  {Math.Abs(balanceAmount):C0}",
                BadgeText = positive ? "Salary + balance" : "Salary + balance hit",
                EffectNote = note,
                CardKind = CardKind.Combo,
                SecondaryLabel = secondaryLabel,
                SecondaryAmountDisplay = secondaryAmountDisplay
            };

        // Card definitions
        // true = good/positive, false = bad/negative
        // Difficulty is NOT! implemented yet but the structure is there to filter cards by difficulty level when drawing for the round and will be added to the method signatures when that part is implemented. For now all cards are "All" difficulty and show up in every game.
        private static List<GameChangerEvent> _gameChangers = new()
        {
            // ROUND 1 CAREER
            SalaryCard(1, 1, "Career", true,
                "You Landed a Promotion",
                "After months of taking on extra projects, staying late, and really stepping up, your manager called you in for a serious conversation. They told you it's time for the next level with a new title, more responsibility, and a solid permanent raise. This moment makes all the grind feel worth it because your paycheck now gets a lasting upgrade every month. You feel proud and motivated to keep pushing forward in your career.",
                500, "Your monthly take-home pay increases permanently by $500 starting right now. Keep building that momentum!"),
            SalaryCard(2, 1, "Career", true,
                "Raise Unlocked No Extra Work",
                "Your last performance review came back glowing with praise from your boss. They said you've been consistently strong, reliable, and a real team player all year. Instead of adding a new title or duties, they rewarded you with a straight pay increase that hits every paycheck going forward. It feels good to be recognized without the pressure of a bigger role right away.",
                300, "Permanent $300/month boost to your income. You earned this one fair and square."),
            BalanceCard(3, 1, "Career", true,
                "Huge Year-End Bonus Surprise",
                "The whole company crushed their goals this year with sales up and projects delivered early. Leadership decided to share the success by giving everyone an unexpected performance bonus. The money landed in your account and you can use it for debt, savings, or just something fun to celebrate the win. It feels amazing to get rewarded for the hard work you put in.",
                3000, "One-Time Bonus Payout",
                "Instant $3,000 added straight to your balance. Enjoy it because you helped make the year great."),
            SalaryCard(4, 1, "Career", true,
                "Certification Pays Off Immediately",
                "You spent evenings and weekends studying hard to pass that industry certification exam. When you showed your boss the official certificate, they were genuinely impressed with your initiative. They gave you an immediate raise to recognize the new skills you brought to the team. Your paycheck reflects that smart investment forever and it motivates you to keep learning.",
                200, "Permanent $200/month salary increase added to every paycheck. Smart investment in yourself!"),
            ComboCard(5, 1, "Career", true,
                "Employee of the Month Cash and Raise",
                "Your name went up on the wall and everyone clapped at the team meeting. Then they handed you a plaque and a check for being employee of the month. The award also came with a small permanent pay bump because your work really stands out. You got both instant cash and ongoing income and it feels like stacking wins early in your career.",
                100, 500, "Raise + Cash Award",
                "Permanent $100/month raise plus $500 cash added to your balance right now. Keep shining!"),
            JobLossCard(6, 1, "Career",
                "Layoff Came Out of Nowhere",
                "The company announced restructuring due to budget cuts and economic uncertainty. Unfortunately your role was one of the ones eliminated and they walked you out the same day with a box and some paperwork. It's a gut punch especially early in your career but they gave you a severance check to help while you look for the next thing. You start updating your resume and reaching out to your network right away.",
                5000, "Monthly income drops to $0 until new employment. Use the $5,000 severance wisely during your job search."),
            SalaryCard(7, 1, "Career", false,
                "Company Wide Pay Cut to Avoid Layoffs",
                "Business has been slower than expected and leadership chose to spread the pain evenly instead of letting people go. Everyone's salary took a hit including yours and it's frustrating to see your paycheck shrink. You still have the job but budgets are going to need some serious adjusting for a while. You start looking for ways to cut costs and stay positive.",
                -400, "Permanent $400/month reduction to your income. Time to tighten things up for a while."),
            BalanceCard(8, 1, "Career", false,
                "Three Days of Unpaid Sick Time",
                "You came down with a really bad flu and had to miss three full days of work. Your job doesn't offer paid sick leave so those days came straight out of your paycheck. It's a tough early lesson in how quickly small health issues can hurt your finances when benefits are minimal. You realize you need to build a bigger emergency fund for situations like this.",
                900, "Unpaid Leave Deduction",
                "$900 taken from your balance (3 days times daily rate). Short on cash? Savings or debt covers it."),
            BalanceCard(9, 1, "Career", false,
                "Severe Illness Extended Unpaid Absence",
                "You got hit with a serious illness that knocked you out for almost three weeks. The doctor said no work and no in-person school during that time. Your part-time job or internship has zero paid sick leave and no short-term disability so income stopped completely. You burned through savings fast and health had to come first but wow does it hurt the wallet.",
                2800, "Extended Unpaid Sick Leave",
                "$2,800 lost wages deducted from balance over three weeks. Short? Savings or debt covers the gap."),
            SalaryCard(10, 1, "Career", false,
                "Demoted After Department Re-org",
                "A major reorganization shifted reporting lines and priorities across the department. Your role got redefined to a lower-level position with reduced pay and it's humiliating to take a step back. Colleagues know about it and the company says it's not personal but just business. Your income takes a permanent hit and you start thinking about how to climb back up.",
                -400, "Permanent $400/month reduction to your income. Time to start rebuilding."),
            // ROUND 2 LOANS
            DebtCard(11, 2, "Loans", true,
                "Student Loan Forgiveness Program Approved",
                "You applied months ago and almost forgot about the application entirely. Then the email arrived saying a government relief program approved forgiveness on a portion of your federal student loans. Five thousand dollars just disappeared from your balance and it's a huge relief. One less thing is weighing you down and you feel lighter already.",
                5000, "Debt Forgiven",
                "$5,000 removed from your total debt. Plan for possible taxes on the forgiven amount later."),
            BalanceCard(12, 2, "Loans", true,
                "Retroactive Scholarship Finally Processed",
                "You had applied for several scholarships during school and didn't hear much back at the time. One of them was awarded retroactively and the school just issued a refund for overpaid tuition and fees. The check landed in your account and it's completely unexpected free money. It feels like winning the lottery on a small scale and you smile big.",
                1500, "Refund to Balance",
                "$1,500 added directly to your balance right now. Best surprise of the month."),
            RecurringCard(13, 2, "Loans", true,
                "Employer Launched Loan Repayment Assistance",
                "Your company rolled out a new benefits perk that many employees had been asking for. They will contribute toward your student loan payments each month as long as you stay employed and it's pre-tax. This lowers your monthly loan bill significantly and makes it easier to breathe financially. You chip away at the principal faster and feel more in control.",
                150, "Monthly Payment Reduced",
                "Your student loan payment drops by $150/month going forward. Big quality-of-life improvement."),
            BalanceCard(14, 2, "Loans", true,
                "Refinanced at a Much Lower Rate",
                "You spent weeks comparing lenders and filling out applications to find a better deal. You finally got approved to refinance your private student loans at a much lower interest rate. The new lender sent you a check for the estimated interest savings over the life of the loan as a cash incentive. It feels good to know your proactive work paid off with real money.",
                2000, "Interest Savings",
                "$2,000 one-time cash added to your balance from refinancing incentives and projected savings."),
            BalanceCard(15, 2, "Loans", true,
                "Student Loan Interest Deduction Refund",
                "You filed your taxes and claimed the student loan interest deduction you qualified for last year. The IRS processed it and sent a nice refund check in the mail. It's not huge but every dollar helps especially when it comes from interest you already paid anyway. You feel rewarded for being responsible with your filing.",
                900, "Tax Refund",
                "$900 added directly to your balance. Free money from a deduction you earned."),
            SalaryCard(16, 2, "Loans", false,
                "Variable Rate Student Loan Adjustment",
                "Your loan has a variable interest rate that is tied to market conditions. With recent Fed rate hikes your rate jumped from 4.5 percent to 7.5 percent. The required monthly payment increased automatically and it's frustrating. You can't lock in the old rate anymore and the extra interest will add up over time.",
                -200, "Permanent $200/month increase in loan payment due to variable rate adjustment."),
            BalanceCard(17, 2, "Loans", false,
                "Meal Swipes Grocery Money Ran Out Mid-Month",
                "Your meal plan swipes or grocery budget for the month ran completely dry halfway through. Between late nights studying, group projects, and unexpected social plans you spent faster than you planned. You had to use credit or borrow from friends to eat until the next paycheck or aid disbursement. It's stressful when basic needs become an emergency.",
                600, "Food Budget Shortfall",
                "$600 extra spent on food this month. Short cash? Savings or debt covers it."),
            SalaryCard(18, 2, "Loans", false,
                "Student Loan Entered Default",
                "You fell too far behind on payments and the loan officially entered default status. Now a portion of your paycheck is being garnished every month until you rehabilitate the loan or settle it. It's stressful and embarrassing but it's fixable with a plan. Your take-home pay is reduced for now and you start making calls to fix it.",
                -250, "Monthly wage garnishment of $250 until default is resolved."),
            BalanceCard(19, 2, "Loans", false,
                "Old Debt Sent to Collections",
                "An old medical bill or credit card balance you forgot about got sold to a collection agency. The calls started coming in constantly and it was overwhelming to deal with. You negotiated a lump-sum settlement to make it go away and stop the damage to your credit. It cost you a chunk upfront but you feel relief once the calls stop.",
                1500, "Settlement Payment",
                "$1,500 deducted from your balance to settle the debt. Short? Savings or debt covers it."),
            BalanceCard(20, 2, "Loans", false,
                "Overdraft Fee Chain Reaction After a Tight Week",
                "You were short on cash after a tough week of unexpected expenses and swiped your debit card anyway. The transaction overdrew your account and triggered a $35 overdraft fee plus additional fees on every automatic payment that tried to hit. What started as a $20 shortfall snowballed into a much bigger hit to your checking balance. You learn the hard way to watch your account more closely.",
                500, "Overdraft Fee Chain",
                "$500 total overdraft fees and returned payment charges deducted from balance. Short? Savings or debt covers it."),
            // ROUND 3 TRANSPORTATION
            BalanceCard(21, 3, "Transportation", true,
                "Went Car-Free and Sold Your Vehicle",
                "You decided to try life without a car and sold it to a friend for a fair price. No more gas, insurance, maintenance, parking, or surprise repairs eating your budget every month. You will use public transit, biking, or rideshares instead and the sale proceeds gave your bank account a serious boost. It feels freeing to simplify your transportation and save money.",
                4000, "Vehicle Sale Proceeds",
                "$4,000 added to your balance. Plan for about $80/month transit costs going forward."),
            RecurringCard(22, 3, "Transportation", true,
                "Employer Started Carpool Incentive Program",
                "Your company launched a new perk that many employees had been asking for. If you carpool or vanpool to work they pay you a monthly stipend for sharing rides. You teamed up with coworkers and now get extra cash every month that helps offset gas or transit costs. It's a small but consistent win that makes commuting feel better.",
                100, "Monthly Stipend",
                "Recurring $100/month added to your effective income. Nice little side benefit."),
            BalanceCard(23, 3, "Transportation", true,
                "Gas Company Class-Action Rebate Check",
                "Remember that big lawsuit against the fuel company for price-fixing a few years back. You were in the class because you bought gas during that period and a settlement check just arrived in the mail. It's unexpected money that feels like found cash and every bit helps right now. You decide to put it toward something useful.",
                600, "Rebate Check",
                "$600 added straight to your balance. Sweet random win."),
            BalanceCard(24, 3, "Transportation", true,
                "Insurance Premium Dropped Significantly",
                "Between a clean driving record, completing a defensive driving course, and turning 25 your auto insurance company moved you to a lower-risk tier. They issued a refund for the overpaid premium from the last few months. It's nice to see good behavior rewarded with actual money back in your account. You feel smart for taking those safe driving steps.",
                800, "Insurance Savings",
                "$800 one-time refund added to your balance. Keep driving smart."),
            RecurringCard(25, 3, "Transportation", true,
                "Pre-Tax Transit Benefit Added",
                "Your employer rolled out a commuter benefits program that many students and young workers love. They will now provide up to $120 per month pre-tax for public transit passes, parking, or vanpool costs. Since it's pre-tax your actual savings are even higher than the face value. It's a huge help for anyone relying on buses, trains, or the subway.",
                120, "Monthly Transit Benefit",
                "Recurring $120/month boost to your effective income via pre-tax transit perk."),
            BalanceCard(26, 3, "Transportation", false,
                "Transmission Failure Right After Warranty",
                "Your car was running fine until two months after the powertrain warranty expired. Then the transmission completely gave out and the mechanic quoted a full replacement at a painful price. No coverage and no payment plan from the shop so you had to pay upfront. It's one of those adulting is expensive moments that hits hard.",
                4200, "Repair Cost",
                "$4,200 including parts and labor deducted from balance. Short? Savings or credit card at 18 percent interest."),
            BalanceCard(27, 3, "Transportation", false,
                "At-Fault Minor Accident",
                "You were distracted for a second and tapped another car in a parking lot. It was clearly your fault and insurance covered most of the damage but not everything. Your deductible plus the inevitable rate increase for the next few years is hitting you hard right now. You learn a tough lesson about staying focused while driving.",
                1400, "Deductible + Rate Increase",
                "$1,400 total hit to your balance including deductible and first-year premium hike."),
            BalanceCard(28, 3, "Transportation", false,
                "Speeding Ticket on the Highway",
                "You were running late and pushed 15 over in a 65 zone on the highway. The officer wasn't having it and wrote you a ticket plus court costs and points on your license. The fine was expensive and your insurance will probably go up next renewal. One bad decision created multiple costs that sting for a while.",
                600, "Traffic Fine",
                "$600 deducted from balance for fine, court costs, and fees. Short cash? Savings or debt."),
            RecurringCard(29, 3, "Transportation", false,
                "Parking Permit Campus Fee Increase",
                "Your university or apartment complex raised parking fees this year and it hit hard. What used to be $400 per semester is now $650 and you rely on driving to campus or work. There's really no avoiding it so another monthly bite comes out of your already tight budget. You start looking for cheaper parking options or alternatives.",
                250, "Monthly Parking Cost Increase",
                "Recurring +$250/month added to transportation expenses."),
            BalanceCard(30, 3, "Transportation", false,
                "Car Towed from No-Parking Zone",
                "You parked in what you thought was a legal spot for just 20 minutes to run an errand. You came back to an empty space because the car was towed away. Tow fee plus daily storage charges and the ticket added up way more than you expected. It's an annoying expensive lesson about checking signs carefully.",
                600, "Towing & Storage Fees",
                "$600 total deducted from balance. Short? Savings or debt covers it."),
            // ROUND 4 HOUSING
            BalanceCard(31, 4, "Housing", true,
                "Rent Frozen for Two Years",
                "Your city has a rent stabilization program and your building qualifies for it. When your lease came up for renewal the landlord couldn't raise the rent at all. You're locked in at the current rate for two full years and that stability lets you plan ahead. You put the would-be increase toward loans or savings instead.",
                1200, "Rent Freeze Savings",
                "Equivalent to one full month's rent saved added to your balance as a one-time benefit."),
            BalanceCard(32, 4, "Housing", true,
                "Utility Company Overcharge Refund",
                "A class-action audit found that your electric and gas provider had been overbilling thousands of customers for years. They issued refunds to everyone affected including you because of a billing software glitch. Your share just arrived and it's unexpected money that feels like justice served. You decide to celebrate the small victory.",
                1400, "Overpayment Refund",
                "$1,400 added directly to your balance. Nice to win one against big companies."),
            RecurringCard(33, 4, "Housing", true,
                "Found a Great Reliable Roommate",
                "After months of searching you found someone trustworthy who pays on time and cleans up after themselves. They moved in and now you split rent and utilities 50/50 every month. Your monthly housing cost dropped dramatically and it frees up cash for other goals. You feel relieved and excited about the extra breathing room.",
                500, "Monthly Rent Reduction",
                "Recurring $500/month savings from splitting costs. Huge budget relief."),
            BalanceCard(34, 4, "Housing", true,
                "Full Security Deposit Returned",
                "When you moved out of your last place you were nervous because some landlords invent reasons to keep deposits. Your old landlord did a walk-through, said everything looked great, and sent the entire deposit back within the legal timeframe. It's rare but it happens and it just gave your savings a nice boost. You feel grateful for the fair treatment.",
                1500, "Deposit Returned",
                "$1,500 added straight to your balance. Celebrate the fair landlords!"),
            BalanceCard(35, 4, "Housing", true,
                "Energy Efficient Upgrade Rebate",
                "You replaced old appliances with energy-efficient ones like the fridge and washer. Your utility company offers rebates for verified upgrades and you finally got the paperwork submitted. The rebate check arrived and it's nice to get rewarded for choices that also lower your future bills. You feel smart for making the switch.",
                700, "Rebate Check",
                "$700 added directly to your balance from utility efficiency program."),
            BalanceCard(36, 4, "Housing", false,
                "Water Heater Burst Emergency Repair",
                "Middle of the night you heard a loud bang and then water was everywhere in the apartment. The water heater failed catastrophically and the plumber had to come immediately because you can't go without hot water. Insurance covered some but the deductible and uncovered parts still cost you thousands. It's an adulting emergency that hits hard and fast.",
                2200, "Repair Cost",
                "$2,200 deducted from balance after insurance. Short? Savings or high-interest credit."),
            RecurringCard(37, 4, "Housing", false,
                "Rent Increase at Lease Renewal",
                "Your lease is up and the landlord is raising rent by $300 per month within local legal limits. You love the place and moving is expensive and stressful so you're probably going to sign the new lease. It hurts to see that money disappear from your budget every month. It's the reality of renting in many markets right now.",
                300, "Monthly Rent Increase",
                "New recurring $300/month added to housing costs going forward."),
            BalanceCard(38, 4, "Housing", false,
                "Leaky Roof Mold Remediation Bill",
                "Rain came through the ceiling during a big storm and water damaged part of your apartment. The landlord called it normal wear and refused to pay for repairs or temporary housing. You had to cover mold testing and remediation out of pocket so you could keep living there safely. It's expensive and stressful to handle on your own.",
                1400, "Repair and Remediation Cost",
                "$1,400 deducted from balance. Short? Savings or debt covers it."),
            BalanceCard(39, 4, "Housing", false,
                "Application Move-In Fee Spike",
                "You found a new apartment but the complex now charges $500 or more in non-refundable fees. Application, admin, and move-in fees went up from $150 last year and you need the place. You paid it but it's another painful reminder that getting into housing keeps getting more expensive. You feel the squeeze on your savings.",
                600, "Move-In and Application Fees",
                "$600 extra deducted from balance just to secure the apartment."),
            BalanceCard(40, 4, "Housing", false,
                "Had to Break Lease Early",
                "A job or school opportunity came up in another city and you had to move quickly. Your lease still had 8 months left and the break clause was brutal with two months rent penalty plus advertising costs. Moving is already expensive and this made the whole situation much worse financially. You take the hit and try to move forward.",
                2500, "Lease Penalty",
                "$2,500 total penalty deducted from balance. Short? Savings or debt covers it."),
            // ROUND 5 FAMILY
            BalanceCard(41, 5, "Family", true,
                "Modest Family Inheritance Received",
                "A distant relative passed away and surprised everyone by leaving small inheritances to younger family members. You received a check that feels meaningful even though it's not life-changing money. It gives you enough to make a real difference in paying down debt or starting an emergency fund. You feel grateful for the unexpected help.",
                8000, "Inheritance Received",
                "$8,000 one-time deposit added straight to your balance."),
            SalaryCard(42, 5, "Family", true,
                "Partner Income Jumped Significantly",
                "Your spouse or partner worked hard and took on extra responsibility at their job. They got a well-deserved promotion and a big raise that increased their paycheck noticeably. Your combined household income just got a permanent boost and it's exciting. You see your team effort paying off financially.",
                400, "Permanent $400/month increase to household income going forward."),
            BalanceCard(43, 5, "Family", true,
                "Child Tax Credit Refund Bigger Than Expected",
                "You filed your taxes and qualified for the full child tax credit or additional child tax credit. Because of your income level and dependents the refund was larger than you budgeted for this year. The extra money just hit your account and it's a nice boost for family expenses or savings. You feel happy about the surprise win.",
                2200, "Tax Credit Refund",
                "$2,200 added directly to your balance. Family win!"),
            BalanceCard(44, 5, "Family", true,
                "Childcare Subsidy Program Approved",
                "You applied for a state or federal childcare assistance program and got accepted after waiting. They're covering a large portion of your daycare or preschool costs for the year. The full annual benefit was issued as a lump sum reimbursement and it's huge relief for working parents. You feel less stressed about balancing work and family.",
                3600, "Annual Subsidy",
                "$3,600 full year's benefit added to your balance as a one-time payment."),
            BalanceCard(45, 5, "Family", true,
                "Generous Cash Gifts at Wedding",
                "Your wedding was beautiful and your family and friends went above and beyond with cash gifts. Envelopes from aunts, uncles, cousins, and close friends added up to a meaningful amount in total. It's not every day people hand you thousands to help you start your new chapter together. You feel overwhelmed with gratitude.",
                4000, "Wedding Gifts",
                "$4,000 in cash gifts added straight to your balance. Grateful beyond words."),
            BalanceCard(46, 5, "Family", false,
                "Family Member Medical Emergency",
                "A close family member had a sudden health crisis that required an ER visit, tests, and hospital stay. Even with good insurance the deductibles, copays, uncovered specialists, and travel costs added up fast. You helped cover a big portion because family steps up. It's emotionally and financially draining but you do what needs to be done.",
                4500, "Medical Bills",
                "$4,500 total out-of-pocket cost deducted from balance. Short? Savings or debt."),
            RecurringCard(47, 5, "Family", false,
                "Extended Parental Leave Partially Unpaid",
                "You took 12 weeks of parental leave after your baby arrived but your employer only pays for 6 weeks. The remaining 6 weeks are unpaid or you used up all your PTO to cover part of it. Your monthly take-home pay dropped significantly during that time. It's a reminder that unpaid leave hits hard even with some protections.",
                500, "Monthly Income Reduction",
                "Recurring $500/month income drop during unpaid leave period."),
            BalanceCard(48, 5, "Family", false,
                "Unexpected Family Wedding Costs",
                "Your sibling or cousin is getting married soon and you want to be there to celebrate. Flights, hotel, nice outfit, and a thoughtful gift added up way faster than you planned. Family events are important but they can quietly drain the account more than expected. You pay it because being there matters.",
                1800, "Family Event Travel and Gifts",
                "$1,800 total extra spent. Short? Savings or debt covers it."),
            BalanceCard(49, 5, "Family", false,
                "Helping Sibling With First-Year Tuition Gap",
                "A younger sibling came up short on first-semester college costs after financial aid. Family asked if you could help bridge the gap so they don't have to take high-interest private loans. You sent what you could and it hurts your own savings. You wanted to help them get started right and feel good about supporting them.",
                2400, "Family Education Support",
                "$2,400 sent to help cover tuition gap. Short? Savings or debt."),
            BalanceCard(50, 5, "Family", false,
                "Laptop Needed for School Urgent Replacement",
                "Your main laptop that you use for classes, assignments, Zoom, and job apps suddenly died. It happened right before midterms and you had no backup device ready. You had to buy a replacement quickly so you wouldn't fall behind. Even the budget model cost more than you wanted to spend and it hurts.",
                1200, "Emergency Device Replacement",
                "$1,200 deducted from balance for urgent replacement. Short? Savings or debt.")
        };

        // Draw cards for the round, filtering by round number and difficulty
        public List<GameChangerCard> GetCardsForRound(int roundNumber, string difficulty)
        {
            var applicable = _gameChangers
                .Where(gc => gc.TriggersInRound == roundNumber)
                .Where(gc => gc.DifficultyLevel == difficulty || gc.DifficultyLevel == "All")
                .OrderBy(_ => _random.Next()) // Cards are shuffled randomly each time
                .Take(8) // Edit this to however many cards you want to draw
                .ToList();

            return applicable.Select(MapToCard).ToList();
        }

        private static GameChangerCard BaseFields(GameChangerCard card, GameChangerEvent gc)
        {
            card.Id = gc.Id;
            card.Category = gc.Category;
            card.Title = gc.Title;
            card.Description = gc.Description;
            card.IsPositive = gc.IsPositive;
            card.EffectLabel = gc.EffectLabel;
            card.EffectAmount = gc.EffectAmount;
            card.EffectNote = gc.EffectNote;
            card.BadgeText = gc.BadgeText;
            card.SecondaryLabel = gc.SecondaryLabel;
            card.SecondaryAmount = gc.SecondaryAmountDisplay;
            return card;
        }

        private GameChangerCard MapToCard(GameChangerEvent gc) => gc.CardKind switch
        {
            CardKind.Salary => (GameChangerCard)BaseFields(new SalaryCard { SalaryChange = gc.SalaryChange }, gc),
            CardKind.Balance => (GameChangerCard)BaseFields(new BalanceCard { BalanceChange = gc.BalanceChange }, gc),
            CardKind.JobLoss => (GameChangerCard)BaseFields(new JobLossCard { Severance = gc.BalanceChange }, gc),
            CardKind.Debt => (GameChangerCard)BaseFields(new DebtCard { DebtChange = gc.DebtChange }, gc),
            CardKind.Recurring => (GameChangerCard)BaseFields(new RecurringCard { MonthlyChange = gc.SalaryChange }, gc),
            CardKind.Combo => (GameChangerCard)BaseFields(new ComboCard
            {
                SalaryChange = gc.SalaryChange,
                BalanceChange = gc.BalanceChange
            }, gc),
            _ => throw new ArgumentOutOfRangeException(nameof(gc.CardKind), gc.CardKind, null)
        };

    }
}
