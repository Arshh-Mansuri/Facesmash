namespace FacesmashAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public int Rating { get; set; } = 1200;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
