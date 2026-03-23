namespace DealtHands.Services
{
    public class FinancialCalculator
    {
        /// <summary>
        /// Calculate monthly car payment
        /// </summary>
        public decimal CalculateCarPayment(decimal purchasePrice, int months = 60, decimal interestRate = 0.05m)
        {
            if (months == 0) return 0;

            // Monthly interest rate
            decimal monthlyRate = interestRate / 12;

            // Loan payment formula: P * [r(1+r)^n] / [(1+r)^n - 1]
            if (monthlyRate == 0) return purchasePrice / months;

            decimal payment = purchasePrice * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, months))
                              / ((decimal)Math.Pow(1 + (double)monthlyRate, months) - 1);

            return Math.Round(payment, 2);
        }

        /// <summary>
        /// Calculate student loan payment
        /// </summary>
        public decimal CalculateLoanPayment(decimal loanAmount, int months = 120, decimal interestRate = 0.045m)
        {
            return CalculateCarPayment(loanAmount, months, interestRate);
        }

        /// <summary>
        /// Calculate what percentage of income this expense represents
        /// </summary>
        public decimal CalculatePercentageOfIncome(decimal expense, decimal income)
        {
            if (income == 0) return 0;
            return Math.Round((expense / income) * 100, 2);
        }

        /// <summary>
        /// Check if expense is within Dave Ramsey recommended percentage
        /// </summary>
        public bool IsWithinRecommendedPercentage(decimal expense, decimal income, string category)
        {
            decimal percentage = CalculatePercentageOfIncome(expense, income);

            // Dave Ramsey recommended percentages
            return category switch
            {
                "Housing" => percentage <= 25,
                "Transportation" => percentage <= 10,
                "Food" => percentage <= 15,
                "Insurance" => percentage <= 10,
                "Savings" => percentage >= 10,
                _ => true
            };
        }
    }
}