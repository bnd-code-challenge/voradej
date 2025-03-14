namespace BankAccountDAL
{
    public class LogEntity
    {
        public DateTime LogDate { get; set; }
        public string? AccountSource { get; set; }
        public string? AccountDestination { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LogMessage { get; set; }
    }
}
