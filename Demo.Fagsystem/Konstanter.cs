namespace Demo.Fagsystem;

public static class Konstanter
{

    public static readonly string nmsp_ApplikasjonKvittering = "https://bufdir.no/applikasjonskvittering.v1.0.1";

    public static readonly string SelectedAksjon = "selectedAksjon";
    public static readonly string SelectedSkjema = "selectedSkjema";
    public static readonly string SelectedSak = "sak_id";
    public static readonly string SelectedSprak = "sprak";
    public static readonly string Meldingshode = "Meldingshode";
    public static readonly string MeldingsForbindelse = "MeldingsForbindelse";
    public static readonly string MeldingsForbindelseType = "MeldingsForbindelseType";
    public static readonly string MeldingsForbindelseType_Variabel_Retning = "retning";
    public static readonly int DagerFortid = -10000;
    public static readonly int DagerFremtid = 10000;
    public static readonly string BufdirMeldingProtokollFilnavn = "bufdir.melding.v";
    public static readonly string MeldingType = "MeldingType";
    public static readonly string MeldingsIdPartialPath = ".Meldingshode.Id";
    public static readonly string OrgnrPartialPath = ".Meldingshode.Avsender.Organisasjonsnummer";
    public static readonly string Orgnr = "Organisasjonsnummer";
    public static readonly string Rolle = "Rolle"; // Saksbehandler 1.linje BV, Saksbehandler Bufetat inntak, Admin kommunalt fagsystem, Admin Bufetat
    public static readonly string InstansId = "InstansId"; // Random Id ved Create() av instans
    public static readonly string AktorAvsenderType = "AvsenderType";
    public static readonly string AktorMottakerType = "MottakerType";
    public static readonly string ActionParam = "actionParam";



    // Variabler i forms
    public static readonly string Handling = "Handling";
    public static readonly string XmlId = "MELDING_ID";
    public static readonly string SelectedFagsystem = "selectedFagsystem";

    // i filinfo (lest Meldingshode via SAX-parser):
    public static readonly string MeldingPrefix = "mld:";
    public static readonly string MeldingstypeNmsp = "MeldingstypeNmsp";
    public static readonly string MeldingstypeNavn = "Meldingstype";
    public static readonly string MeldingstypeAvsendersRef = "AvsendersRef";
    public static readonly string MeldingstypeMottakersRef = "OppfolgingAvMelding.MottakersRef";
}
