namespace FacesmashAPI.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }       // FK -> User
        public int ReceiverId { get; set; }     // FK -> User
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
