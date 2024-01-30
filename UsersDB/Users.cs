namespace UsersDB
{
    public class Users
    {
        public int Id { get; set; }
        public string? login { get; set; }
        public string? password { get; set; }
        public string? phone { get; set; }
        public string? ip { get; set; }
        public byte[]? avatar { get; set; }
        public bool isInContactList { get; set; }
    }
}
