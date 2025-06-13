using System.Xml.Schema;

namespace Demo.Fagsystem.Models.Test;

public class XsdElementRec(XmlSchemaAnnotated el, bool isChoiceElement)
{
    public bool IsChoiceElement => isChoiceElement;
    public XmlSchemaAnnotated element => el;
}
