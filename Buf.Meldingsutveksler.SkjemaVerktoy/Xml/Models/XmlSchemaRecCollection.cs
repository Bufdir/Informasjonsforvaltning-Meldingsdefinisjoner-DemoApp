using System.Xml.Serialization;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.Xml.Models;
[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "xsd_oversikt", Namespace = "", IsNullable = false)]
public class XmlSchemaRecCollection
{
    [XmlAttribute]
    public string dato { get; set; } = "";

    [XmlElement(ElementName = "xsd")]
    public XmlSchemaRec[] xsds { get; set; } = [];
}
