using Buf.Meldingsutveksler.Meldinger.Enums;
using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll.Models;
using System.Text.Json.Serialization;

namespace Buf.Meldingsutveksler.Meldinger.Models
{
    public class MeldingRecord(Meldingshode meldingshode, MeldingDirection direction, MeldingTransferState state) : IMeldingRecord
    {
        [JsonInclude]
        public Meldingshode Meldingshode { get; set; } = meldingshode;

        [JsonInclude]
        public MeldingDirection Direction { get; set; } = direction;
        [JsonInclude]
        public MeldingTransferState State { get; set; } = state;
        [JsonInclude]
        public int Response { get; set; } = -1;
        [JsonInclude]
        public string ResponseText { get; set; } = "";
        [JsonIgnore]
        public string FileName { get; set; } = "";
    }
}
