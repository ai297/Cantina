namespace Cantina.Models
{
    public class HashedPassword
    {
        public string Hash { get; set; }
        public string Salt { get; set; }
    }
}
