namespace Cantina.Services
{
    public class ServicesOptions
    {
        public int ArchiveSavingInterval { get; set; } = 5;
        public int OnlineUsersCheckItnerval { get; set; } = 4;

        public ServicesOptions() { }
    }
}
