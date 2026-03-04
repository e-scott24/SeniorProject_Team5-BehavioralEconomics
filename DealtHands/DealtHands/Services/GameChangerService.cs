using DealtHands.Models;

namespace DealtHands.Services
{
    public class GameChangerService
    {
        private readonly Random _random = new Random();

        // Pre-defined game changers (later from database)
        private static List<GameChangerEvent> _gameChangers = new List<GameChangerEvent>
        {
            new GameChangerEvent
            {
                Id = 1,
                Title = "Car Transmission Failure",
                Description = "Your transmission failed 2 months after warranty expired. Need full replacement.",
                FinancialImpact = -3500,
                IsPositive = false,
                TriggersInRound = 3,
                DifficultyLevel = "All",
                Category = "Transportation"
            },
            new GameChangerEvent
            {
                Id = 2,
                Title = "Remote Work Approved",
                Description = "Your employer approved permanent work-from-home. Save on commute costs!",
                FinancialImpact = 300,
                IsPositive = true,
                TriggersInRound = 1,
                DifficultyLevel = "Easy",
                Category = "Career"
            }
            // Add more game changers...
        };

        /// <summary>
        /// Trigger a random game changer for a player
        /// </summary>
        public GameChangerEvent TriggerGameChanger(int playerId, int roundNumber, string difficulty)
        {
            // Get applicable game changers
            var applicable = _gameChangers
                .Where(gc => gc.TriggersInRound == roundNumber)
                .Where(gc => gc.DifficultyLevel == difficulty || gc.DifficultyLevel == "All")
                .ToList();

            if (!applicable.Any()) return null;

            // Pick random one
            var gameChanger = applicable[_random.Next(applicable.Count)];

            return gameChanger;
        }

        /// <summary>
        /// Should a game changer trigger this round? (based on difficulty)
        /// </summary>
        public bool ShouldTrigger(string difficulty, int roundNumber)
        {
            int chance = difficulty switch
            {
                "Easy" => 30,      // 30% chance
                "Medium" => 60,    // 60% chance
                "Hard" => 90,      // 90% chance
                _ => 50
            };

            return _random.Next(100) < chance;
        }
    }
}