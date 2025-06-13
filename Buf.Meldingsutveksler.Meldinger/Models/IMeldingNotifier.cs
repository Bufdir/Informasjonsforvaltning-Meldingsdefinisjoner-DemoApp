namespace Buf.Meldingsutveksler.Meldinger.Models
{
    public interface IMeldingNotifier
    {
        void Notify(IMeldingRecordEvent Event);
    }
}
