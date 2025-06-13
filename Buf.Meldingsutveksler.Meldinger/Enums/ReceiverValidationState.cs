namespace Buf.Meldingsutveksler.Meldinger.Enums
{
    public enum ReceiverValidationState
    {
        undefined = 0,
        ok = 1,
        notFound = 2,
        notApproved = 3,
        temporarilyUnavailable = 4,
        irrelevantReceiver = 5
    }
}
