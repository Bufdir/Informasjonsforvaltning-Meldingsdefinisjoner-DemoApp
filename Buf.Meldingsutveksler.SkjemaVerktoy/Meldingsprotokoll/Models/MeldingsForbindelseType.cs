using System.ComponentModel;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll.Models;

public enum MeldingsForbindelseType
{
    [Description("[Ny] melding brukes ikke, melding er 'ny' når den ikke er relatert til tidligere melding")]
    Ny,
    //ref. til egen melding:
    [Description("[Duplikat] Samme melding sendt igjen (av tekniske årsaker)")]
    Duplikat,
    [Description("[Oppdatering] til tidligere melding")]
    Oppdatering,
    [Description("[Tillegg] til melding, f.eks. vedlegg")]
    Tillegg,
    [Description("[Trukket] melding (det melding gjaldt er ikke aktuelt mer)")]
    Trukket,
    [Description("[Slettet] melding (melding var sendt ved feiltakelse, skal slettes hos mottaker)")]
    Slettet,
    //ref. til annen parts melding:
    [Description("[ApplikasjonsKvittering] på mottatt melding - sendt av fagsystem")]
    ApplikasjonsKvittering,
    [Description("[SaksbehandlerKvittering] på mottatt melding - sendt av saksbehandler")]
    SaksbehandlerKvittering,
    [Description("[Svar] på mottatt melding")]
    Svar,
    [Description("[Feil] på mottatt melding")]
    Feil
}
