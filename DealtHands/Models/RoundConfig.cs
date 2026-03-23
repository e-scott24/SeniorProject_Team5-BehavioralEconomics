namespace DealtHands.Models
{
    public class RoundConfig
    {
        public int RoundNumber { get; set; }         // 1,2,3,4,5
        public string RoundName { get; set; }        // "Career", "Loans", etc.
        public string RoundType { get; set; }        // Could be same as name or "Career"
        public List<string> Choices { get; set; }    // The options for the player
        public bool RequiresAmount { get; set; }     // True if input field is needed
    }
}