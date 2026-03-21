namespace DealtHands.Models
{
    public class Educator
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }


        // For password resets
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpires { get; set; }

        // One educator creates many sessions
        public List<Session> Sessions { get; set; } = new List<Session>();
    }
}
