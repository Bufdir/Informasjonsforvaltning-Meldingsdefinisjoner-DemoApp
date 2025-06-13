using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll.Models;
using Demo.Fagsystem.Models.Business;
using Demo.Fagsystem.Models.FagsystemSimulator.Config;
using Demo.Fagsystem.Models.FagsystemSimulator.Storage;

namespace Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;

public class FagsystemBase
{
    public FagsystemBase(FagsystemInfo systemInfo, Organisasjon organisasjon, FagsystemBruker bruker, FagsystemConfigBase? config)
    {
        Id = Guid.NewGuid().ToString();
        FagsystemInfo = systemInfo;
        Organisasjon = organisasjon;
        Bruker = bruker;
        if (config == null)
            throw new Exception($"Config for fagsystem {systemInfo.Navn} finnes ikke");
        Config = config;
        MeldingStorage = new BlobMeldingStorage(this, config.Meldinger);
    }
    public string Id { get; }
    public FagsystemInfo FagsystemInfo { get; set; }
    public Organisasjon Organisasjon { get; set; }
    public FagsystemBruker Bruker { get; set; }
    public List<Klient> Klienter { get; set; } = [];
    public string PageFolder { get; set; } = "";

    public FagsystemConfigBase Config { get; set; }

    public IMeldingStorage MeldingStorage { get; set; }

}
