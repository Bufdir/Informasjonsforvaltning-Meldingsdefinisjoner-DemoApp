using System.Xml;

namespace Buf.Meldingsutveksler.Meldinger.Models;

public class MeldingRecordEvent(IMeldingRecord rec, XmlDocument? doc = null) : IMeldingRecordEvent
{
    public IMeldingRecord Rec { get; } = rec;

    public XmlDocument? Melding { get; } = doc;

}
