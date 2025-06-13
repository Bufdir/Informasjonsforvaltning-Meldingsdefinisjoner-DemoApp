namespace Demo.Fagsystem.Models.FagsystemSimulator.Config;

public class FagsystemConfigBase
{
    public string? Id { get; set; }
    public string? Aktiv { get; set; }
    public string? Standalone { get; set; }

    public BlobContainerConfig? Meldinger { get; set; }
    public BlobContainerConfig? Definisjoner { get; set; }

}
