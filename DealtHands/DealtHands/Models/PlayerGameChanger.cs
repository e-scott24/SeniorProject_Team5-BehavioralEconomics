namespace DealtHands.Models
{
    public class PlayerGameChanger
    {
        public int Id { get; set; }

        // Which player got this event?
        public int PlayerId { get; set; }
        public Player Player { get; set; }

        // Which game changer event was it?
        public int GameChangerEventId { get; set; }
        public GameChangerEvent GameChangerEvent { get; set; }

        // When did it happen?
        public DateTime OccurredAt { get; set; }

        // In which round?
        public int RoundNumber { get; set; }
    }
}