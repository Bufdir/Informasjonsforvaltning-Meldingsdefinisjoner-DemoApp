using Buf.Meldingsutveksler.Meldinger.Enums;

namespace Buf.Meldingsutveksler.Meldinger.Models;

public class MeldingTransferError(int id, int parentId, int errorCode, string message, MeldingTransferState stage) : MeldingError(id, parentId, errorCode, message)
{
    public MeldingTransferState Stage { get; set; } = stage;
}
