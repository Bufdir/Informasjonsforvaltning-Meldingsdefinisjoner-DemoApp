using Buf.Meldingsutveksler.Meldinger.Enums;

namespace Buf.Meldingsutveksler.Meldinger.Models
{
    public class ReceiverValdationResult
    {
        public string OrgNr { get; set; } = "";
        public string Navn { get; set; } = "";
        public ReceiverValidationState Result { get; set; }
    }
}
