using System.ComponentModel;

namespace Buf.Meldingsutveksler.Meldinger.Enums
{
    public enum MeldingTransferState
    {
        undefined = 0,
        [Description("Lagret utkast")]
        draft = 1,
        // outgoing = 2..9
        [Description("Validering (utgående)")]
        validatingOutgoing = 2,
        [Description("I utboks")]
        outbox = 3,
        [Description("Sendt")]
        sent = 4,
        [Description("Mottatt appRec")]
        appRecReceived = 5,
        [Description("Mottatt kvittering")]
        userRecReceived = 6,
        [Description("Feil ved sending")]
        sendError = 7,
        [Description("Feilmelding fra mottaker")]
        receiveErrorReceived = 8,
        [Description("Valideringsfeil fra mottaker")]
        validatingErrorReceived = 9,
        // incoming = 20..25
        [Description("I innboks")]
        inbox = 20,
        [Description("Validering (innkommende)")]
        validatingIncoming = 21,
        [Description("Sendt appRec")]
        appRecSent = 22,
        [Description("Sendt kvittering")]
        userRecSent = 23,
        [Description("Feil ved mottak")]
        receiveError = 24,
        [Description("Valideringsfeil ved mottak")]
        validatingError = 25
    }

}
