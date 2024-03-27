namespace Whisper_Server.DbContexts
{
    public class Users
    {
        public int Id { get; set; }
        public string? login { get; set; }
        public string? password { get; set; }
        public string? phone { get; set; }
        public string? ip { get; set; }
        public byte[]? avatar { get; set; }
        public string? isOnline { get; set; }
        public virtual ICollection<Messages>? Messages { get; set; }
        public virtual ICollection<BlackList>? Block { get; set; }
    }
}
