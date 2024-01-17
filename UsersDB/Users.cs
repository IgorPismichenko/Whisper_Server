namespace UsersDB
{
    public class Users
    {
        public int Id { get; set; }
        public string? login { get; set; }
        public string? password { get; set; }
        public string? phone { get; set; }
        public string? ip { get; set; }
        public virtual ICollection<Messages> Messages { get; set; } = new List<Messages>();
        public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
    }
}
