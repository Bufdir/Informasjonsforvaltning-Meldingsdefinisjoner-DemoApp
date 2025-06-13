namespace Buf.Meldingsutveksler.Meldinger.Models;

public class MeldingValidationError(int id, int parentId, int errorCode, string message, string path) : MeldingError(id, parentId, errorCode, message)
{
    public string Path { get; set; } = path;
}
