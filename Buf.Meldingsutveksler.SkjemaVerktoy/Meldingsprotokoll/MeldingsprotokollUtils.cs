using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll.Models;
using Buf.Meldingsutveksler.SkjemaVerktoy.Tekster;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using System.Xml;
using System.Xml.Schema;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll;
public static class MeldingsprotokollUtils
{
    public static List<AktorType> ArrayToAktorTyper(string[] aktorTyper)
    {
        List<AktorType> result = [];
        foreach (var aktorType in aktorTyper)
        {
            var aType = aktorType switch
            {
                "System" => AktorType.System,
                "1.Linje" => AktorType.Barnevern1Linje,
                "2.Linje" => AktorType.Barnevern2Linje,
                "Institusjon" => AktorType.BarnevernEksternInstitusjon,
                "*" => AktorType.All,
                _ => AktorType.undefined
            };
            result.Add(aType);
        }
        return result;
    }


    public static bool GetCanSend(XmlSchemaElement element, AktorType aktorType)
    {
        string meldingAktorType = XsdUtils.GetAppInfoValue(element, MeldingKonstanter.AktorAvsenderType, false, false);
        if (meldingAktorType == "*")
            return true;
        string[] aktorTyper = meldingAktorType.Split('|');
        List<AktorType> aktTyper = ArrayToAktorTyper(aktorTyper);
        return aktTyper.Contains(aktorType);
    }

    // aksjon bruker | som separator
    public static bool GetUsedInAksjon(XmlSchemaElement element, string aksjon)
    {
        var appInfoValue = XsdUtils.GetAppInfoValue(element, MeldingKonstanter.MeldingsForbindelse, false, true);
        if (string.IsNullOrEmpty(appInfoValue))
            return false;
        if (appInfoValue == "*")
            return true;
        return $"|{appInfoValue}|".Contains($"|{aksjon}|");
    }
    public static XmlSchemaElement? GetMeldingshode(IEnumerable<object> items, bool digDeeper = true)
    {
        foreach (var item in items)
        {
            if (item is XmlSchemaElement rootElement)
            {
                var hode = GetMeldingshode(rootElement, false);
                if (hode != null)
                    return rootElement;
                var children = XsdUtils.GetXsdChildren(rootElement);
                var element = children.FirstOrDefault(ch => XsdUtils.GetName(ch) == MeldingKonstanter.Meldingshode);
                if (element != null)
                {
                    return element as XmlSchemaElement;
                }
                else if (digDeeper) // ett nivå ned...
                {
                    foreach (var child in children)
                    {
                        if (child is XmlSchemaElement childElement && childElement.ElementSchemaType is XmlSchemaComplexType complexElement)
                        {
                            var childItems = XsdUtils.GetXsdChildren(complexElement);
                            var childHode = GetMeldingshode(childItems, false);
                            if (childHode != null)
                                return childHode;
                        }
                    }
                }
            }
        }
        return null;
    }

    public static Meldingshode? GetMeldingshode(XmlDocument? doc/*, bool digDeeper*/)
    {
        if (doc == null)
            throw new Exception($"XmlDocument == null i {nameof(GetMeldingshode)}");
        var rootElement = XmlUtils.GetRootElement(doc);
        var node = XmlUtils.GetNode(rootElement, MeldingKonstanter.Meldingshode);
        if (node is XmlElement element)
            return ElementToMeldingshode(element);
        return null;
    }

    public static XmlSchemaElement? GetMeldingshode(XmlSchemaElement element, bool digDeeper)
    {
        if (XsdUtils.GetName(element) == MeldingKonstanter.Meldingshode)
            return element;
        var childItems = XsdUtils.GetXsdChildren(element);
        return GetMeldingshode(childItems, digDeeper);
    }

    public static List<XmlSchemaElement> ElementsWithMeldingshode(XmlSchema schema)
    {
        List<XmlSchemaElement> result = [];
        foreach (var item in schema.Items)
        {
            if (item is XmlSchemaElement rootElement)
            {
                if (GetMeldingshode(rootElement, true) != null)
                    result.Add(rootElement);
            }
        }
        return result;
    }
    public static bool IsKlientMeldingType(XmlSchema schema)
    {
        var rootElement = XsdUtils.GetRootElement(schema);
        foreach (var child in XsdUtils.GetXsdChildElements(rootElement))
        {
            if (XsdUtils.GetName(child) == "Klient")
                return true;
        }
        return false;
    }
    public static Meldingshode ElementToMeldingshode(XmlElement element)
    {
        Meldingshode meldingshode = new();
        XmlUtils.ElementToObject(meldingshode, element);
        /*        XmlSerializer serializer = new XmlSerializer(typeof(Meldingshode));
                                                var hode = serializer.Deserialize(new XmlTextReader(element.OuterXml, XmlNodeType.Element, null));
                                                if (hode is Meldingshode meldingshode)*/
        return meldingshode;
    }

    public static Meldingshode GetReplyMeldingshode(XmlSchema schema, Meldingshode receivedMeldingshode, MeldingsForbindelseType forbindelse, FagsystemInfo fagsystem, /*KontaktInfo kontaktinfo, */string avsendersRef = "")
    {
        XmlSchemaElement rootElement = XsdUtils.GetRootElement(schema)
            ?? throw new Exception($"Ingen rotelement i schema '{schema.TargetNamespace}'");
        string caption = TeksterUtils.GetCaption(schema, rootElement, true);

        Meldingshode reply = new()
        {
            Id = Guid.NewGuid().ToString(),
            FagsystemAvsender = fagsystem,
            AvsendersRef = avsendersRef == "" ? receivedMeldingshode.AvsendersRef : avsendersRef,
            Meldingstype = caption,
            MeldingstypeNmsp = schema.TargetNamespace!,
            SendtTidspunkt = DateTime.Now.ToString(XmlUtils.DateTimeFormatXML),
            Avsender = receivedMeldingshode.Mottaker,
            Mottaker = receivedMeldingshode.Avsender,
            OppfolgingAvMelding = new()
            {
                MeldingId = receivedMeldingshode.Id,
                StartMeldingId = receivedMeldingshode.OppfolgingAvMelding != null ? receivedMeldingshode.OppfolgingAvMelding.StartMeldingId : receivedMeldingshode.Id,
                MeldingsForbindelse = forbindelse.ToString()
            }
        };
        return reply;
    }


}
