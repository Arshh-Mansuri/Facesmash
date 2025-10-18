namespace FacesmashAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Gender { get; set; }

        // Make PhotoUrl nullable
        public string? PhotoUrl { get; set; }
        // New bio field
        public string? Bio { get; set; }

        public int Rating { get; set; } = 0;

    }
}
