using System.Xml.Schema;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.Tekster.Models;

public class EnrichedElement(XmlSchemaAnnotated element)
{
    public XmlSchemaAnnotated Element => element;
    public int OrigSort { get; set; }
    public int SortOrder { get; set; }
}
