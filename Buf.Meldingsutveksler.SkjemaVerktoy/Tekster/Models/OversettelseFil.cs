namespace Buf.Meldingsutveksler.SkjemaVerktoy.Tekster.Models;

public class OversettelseFil
{
    public string sprak { get; set; } = "";
    public string fil { get; set; } = "";
    public string FileName { get; set; } = "";
    public ElementOversettelse[] skjemaelement { get; set; } = [];
    public KodelisteOversettelse[] kodelistetekster { get; set; } = [];
}

public class ElementOversettelse
{
    public string id { get; set; } = "";
    public string ledetekst { get; set; } = "";
    public string veiledning { get; set; } = "";
}

public class KodelisteOversettelse
{
    public string id { get; set; } = "";
    public KodelisteVerdiOversettelse[] verdier { get; set; } = [];
}

public class KodelisteVerdiOversettelse
{
    public string verdi { get; set; } = "";
    public string tekst { get; set; } = "";
    public string beskrivelse { get; set; } = "";
}
