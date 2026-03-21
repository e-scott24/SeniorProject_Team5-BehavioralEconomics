using DealtHands.Data;
using DealtHands.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace DealtHands.Services
{
    public class EducatorService
    {
        private readonly ApplicationDbContext _context;

        public EducatorService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Register a new educator
        /// </summary>
        public Educator Register(string email, string password, string name)
        {
            // Hash password (simple version - use ASP.NET Identity in production)
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var educator = new Educator
            {
                Email = email,
                PasswordHash = passwordHash,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Educators.Add(educator);
            _context.SaveChanges();

            return educator;
        }

        /// <summary>
        /// Login educator
        /// </summary>
        public Educator Login(string email, string password)
        {
            var educator = _context.Educators.FirstOrDefault(e => e.Email == email);

            if (educator == null) return null;

            // Verify password
            bool isValid = BCrypt.Net.BCrypt.Verify(password, educator.PasswordHash);

            return isValid ? educator : null;
        }

        /// <summary>
        /// Get educator by ID
        /// </summary>
        public Educator GetEducator(int educatorId)
        {
            return _context.Educators
                .Include(e => e.Sessions)
                .FirstOrDefault(e => e.Id == educatorId);
        }

        /// <summary>
        /// Get all sessions for an educator
        /// </summary>
        public List<Session> GetEducatorSessions(int educatorId)
        {
            return _context.Sessions
                .Where(s => s.EducatorId == educatorId)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Generate password reset token
        /// </summary>
        public string GeneratePasswordResetToken(string email)
        {
            var educator = _context.Educators.FirstOrDefault(e => e.Email == email);
            if (educator == null) return null;

            // Generate random token
            string token = Guid.NewGuid().ToString();

            educator.PasswordResetToken = token;
            educator.PasswordResetExpires = DateTime.UtcNow.AddHours(1); // 1 hour expiry
            _context.SaveChanges();

            return token;
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        public bool ResetPassword(string token, string newPassword)
        {
            var educator = _context.Educators.FirstOrDefault(e =>
                e.PasswordResetToken == token &&
                e.PasswordResetExpires > DateTime.UtcNow);

            if (educator == null) return false;

            educator.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            educator.PasswordResetToken = null;
            educator.PasswordResetExpires = null;
            _context.SaveChanges();

            return true;
        }
    }
}
