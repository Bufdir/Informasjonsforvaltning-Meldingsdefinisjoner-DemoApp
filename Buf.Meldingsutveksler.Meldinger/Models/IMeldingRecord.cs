using Buf.Meldingsutveksler.Meldinger.Enums;
using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll.Models;

namespace Buf.Meldingsutveksler.Meldinger.Models;

public interface IMeldingRecord
{
    public Meldingshode Meldingshode { get; }
    public MeldingDirection Direction { get; set; }
    public MeldingTransferState State { get; set; }
    public int Response { get; set; }
    public string ResponseText { get; set; }
    public string FileName { get; set; }
}
