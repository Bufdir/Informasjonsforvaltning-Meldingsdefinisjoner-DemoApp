namespace Buf.Meldingsutveksler.Meldinger.Models;

public abstract class MeldingError(int id, int parentId, int errorCode, string message = "")
{
    public int Id { get; set; } = id;
    public int ParentId { get; set; } = parentId;
    public int ErrorCode { get; set; } = errorCode;
    public string Message { get; set; } = message;
    public MeldingError[] SubErrors { get; set; } = [];
}
