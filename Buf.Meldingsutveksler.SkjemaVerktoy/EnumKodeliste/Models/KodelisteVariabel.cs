using System.Xml.Serialization;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste.Models;

[Serializable]
public class KodelisteVariabel
{
    [XmlAttribute]
    public string id { get; set; } = "";
    [XmlAttribute]
    public string navn { get; set; } = "";
    [XmlAttribute]
    public string kodelisteId { get; set; } = "";
    [XmlAttribute]
    public string kodelisteNavn { get; set; } = "";

}
