using System.Xml.Serialization;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste.Models;

[Serializable]
public class KodeVariabel
{
    [XmlAttribute]
    public string navn { get; set; } = "";
    [XmlAttribute]
    public string verdi { get; set; } = "";
}
