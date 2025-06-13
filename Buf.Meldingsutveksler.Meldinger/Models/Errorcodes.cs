namespace Buf.Meldingsutveksler.Meldinger.Models;

public static class Errorcodes
{
    public static int STORED = 1;
    // OK
    public static int OK = 200;
    public static int OK_DUPLICATE = 201;

    //Errors
    public static int ERR_APPLICATION_ERROR = 500;
    // Transportfeil
    public static int ERR_MESSAGER_NOT_AVAILABLE = 1050;
}
