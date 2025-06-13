namespace Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll;

public static class MeldingKonstanter
{

    public static readonly string nmsp_ApplikasjonKvittering = "https://bufdir.no/applikasjonskvittering.v1.0.1";

    public static readonly string Meldingshode = "Meldingshode";
    public static readonly string MeldingsForbindelse = "MeldingsForbindelse";
    public static readonly string MeldingsForbindelseType = "MeldingsForbindelseType";
    public static readonly string MeldingsForbindelseType_Variabel_Retning = "retning";
    public static readonly string MeldingType = "MeldingType";
    public static readonly string MeldingsIdPartialPath = ".Meldingshode.Id";
    public static readonly string OrgnrPartialPath = ".Meldingshode.Avsender.Organisasjonsnummer";
    public static readonly string Orgnr = "Organisasjonsnummer";
    public static readonly string AktorAvsenderType = "AvsenderType";
    public static readonly string AktorMottakerType = "MottakerType";

    // i filinfo (lest Meldingshode via SAX-parser):
    public static readonly string MeldingPrefix = "mld:";
    public static readonly string MeldingstypeNmsp = "MeldingstypeNmsp";
    public static readonly string MeldingstypeNavn = "Meldingstype";
    public static readonly string MeldingstypeAvsendersRef = "AvsendersRef";
    public static readonly string MeldingstypeMottakersRef = "OppfolgingAvMelding.MottakersRef";
}
