namespace Demo.Fagsystem.Models.Business;

public class Vedlegg
{
    public string Id { get; set; } = "";
    public string MeldingId { get; set; } = "";
    public string Filnavn { get; set; } = "";
    public string VedleggType { get; set; } = "";
    public string Beskrivelse { get; set; } = "";
    public string? Content { get; set; }
}
