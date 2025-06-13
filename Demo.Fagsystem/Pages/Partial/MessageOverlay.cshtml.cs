namespace Demo.Fagsystem.Pages.Partial;

public class MessageOverlayModel
{
    public MessageOverlayModel(string caption, string message)
    {
        Caption = caption;
        Message = message;
    }

    public string Caption { get; set; }
    public string Message { get; set; }
}
