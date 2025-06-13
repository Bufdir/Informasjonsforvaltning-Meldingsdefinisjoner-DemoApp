namespace Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll.Models;

public class Meldingshode
{
    public string Id { get; set; } = "";
    public OppfolgingAvMelding? OppfolgingAvMelding { get; set; }
    public string MeldingstypeNmsp { get; set; } = "";
    public string Meldingstype { get; set; } = "";
    public FagsystemInfo FagsystemAvsender { get; set; } = new();
    public string SendtTidspunkt { get; set; } = "";
    public Organisasjon Avsender { get; set; } = new();
    public Organisasjon Mottaker { get; set; } = new();
    public string AvsendersRef { get; set; } = "";
    public KontaktInfo? KontaktInfoAvsender { get; set; }
    public string GetValue(string name)
    {
        var propInfo = GetType().GetProperty(name);
        if (propInfo != null)
        {
            return propInfo.GetValue(this)?.ToString() ?? "";
        }
        return "";
    }
}

public class OppfolgingAvMelding
{
    public string MeldingsForbindelse { get; set; } = "";
    public string StartMeldingId { get; set; } = "";
    public string MeldingId { get; set; } = "";
}

public class FagsystemInfo
{
    public string Leverandor { get; set; } = "";
    public string Navn { get; set; } = "";
    public string Versjon { get; set; } = "";
}


public class KontaktInfo
{
    public Kontaktperson? Kontaktperson { get; set; }
    public Kontaktperson? KontaktpersonLeder { get; set; }
}

public class Kontaktperson
{
    public string Navn { get; set; } = "";
    public string Telefon { get; set; } = "";
    public string epost { get; set; } = "";
}

public class Organisasjon
{
    public string Organisasjonsnummer { get; set; } = "";
    public string Navn { get; set; } = "";
    public string Kortnavn { get; set; } = "";
    public AktorType Aktortype { get; set; }
    public Kommuneinfo? Kommuneinfo { get; set; }
}

public class Kommuneinfo
{
    public string Kommunenummer { get; set; } = "";
    public string Kommunenavn { get; set; } = "";
    public Bydelsinfo? Bydelsinfo { get; set; }
}

public class Bydelsinfo
{
    public string Bydelsnummer { get; set; } = "";
    public string Bydelsnavn { get; set; } = "";
}



