namespace Demo.Fagsystem.Models.Business
{
    public class Sak
    {
        public int Id { get; set; }
        public int KlientId { get; set; }

        public Dictionary<string, string> Saksdata = [];

    }
}
