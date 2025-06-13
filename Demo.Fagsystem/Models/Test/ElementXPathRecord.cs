namespace Demo.Fagsystem.Models.Test;

public class ElementXPathRecord(string xpath, int slotCount)
{
    public string Xpath { get; set; } = xpath;
    public XsdElementRec[] elementInstances { get; set; } = new XsdElementRec[slotCount];
}
