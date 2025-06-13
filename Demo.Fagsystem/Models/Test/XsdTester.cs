using Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste;
using Buf.Meldingsutveksler.SkjemaVerktoy.Tekster;
using Buf.Meldingsutveksler.SkjemaVerktoy.Tekster.Models;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using System.Collections;
using System.Xml.Schema;

namespace Demo.Fagsystem.Models.Test;

public static class XsdTester
{
    public static Dictionary<string, ICollection> TestJsonTekster()
    {
        Dictionary<string, ICollection> result = [];
        List<Skjemaelement> ikkeBrukteSkjemaElement = [];
        foreach (var tekstfil in TeksterUtils.TekstFiler)
        {
            if (tekstfil.skjema != null)
            {
                foreach (var skjemaelement in tekstfil.skjema.skjemaelement)
                {
                    ikkeBrukteSkjemaElement.Add(skjemaelement);
                }
            }
        }
        List<XmlSchemaAnnotated> elementSomManglerSkjemaElement = [];
        foreach (var schemaRec in XmlSchemaRegister.Schemas.xsds)
        {
            var rootElement = XsdUtils.GetRootElement(schemaRec.Schema);
            if (rootElement != null && schemaRec.Schema != null)
            {
                if (XsdUtils.GetName(rootElement) == "Henvisning")
                {
                    string path = "";// XsdUtils.GetName(rootElement);
                    TestXsd(schemaRec.Schema, path, rootElement, ikkeBrukteSkjemaElement, elementSomManglerSkjemaElement);
                }
            }
        }
        result["ikkeBrukte"] = ikkeBrukteSkjemaElement;
        result["mangler"] = elementSomManglerSkjemaElement;
        return result;
    }

    private static void TestXsd(XmlSchema schema, string path, XmlSchemaAnnotated element, List<Skjemaelement> elementer, List<XmlSchemaAnnotated> manglerTekst)
    {
        bool isChoiceElement = XsdUtils.IsChoiceElement(element);
        var name = XsdUtils.GetName(element);
        var xPath = $"{path}/{XsdUtils.GetName(element)}".TrimEnd('/');
        XmlSchemaComplexType? complexType = XsdUtils.GetComplexType(element);
        XmlSchemaSimpleType? simpleType = XsdUtils.GetSimpleType(element);
        var skjemaEl = TeksterUtils.GetTekstElement(schema, element);
        if (skjemaEl != null)
        {
            elementer.Remove(skjemaEl);
        }
        else
        {
            if (!manglerTekst.Any(e => e.Id == element.Id))
                manglerTekst.Add(element);
        }
        if (isChoiceElement)
        {
            var choices = XsdUtils.GetChoiceElements(element)
                ?? throw new Exception($"Finner ikke choice-element til definisjon {XsdUtils.GetName(element)}");
            foreach (var choiceElement in choices.Cast<XmlSchemaAnnotated>())
            {
                TestXsd(schema, xPath, choiceElement, elementer, manglerTekst);
            }
        }
        var children = XsdUtils.GetXsdChildElements(element);
        foreach (var child in children)
        {
            TestXsd(schema, xPath, child, elementer, manglerTekst);
        }
        //SortLists();
    }

    public static List<string> TestKodelister()
    {
        List<string> result = [];
        foreach (var schema in XsdUtils.XsdSchemas)
        {
            var types = schema.SchemaTypes;
            foreach (var typ in types.Values)
            {
                if (typ is XmlSchemaAnnotated schType)
                {
                    var simpleType = XsdUtils.GetSimpleType(schType);
                    if (XsdUtils.GetIsEnumType(simpleType))
                    {
                        string name = XsdUtils.GetName(simpleType);
                        var enumValues = XsdUtils.GetEnumValues(schType);
                        var kodeliste = KodelisteUtils.GetKodeliste(schType);
                        string kodelisteId = schType.Id ?? $"<mangler, navn = {XsdUtils.GetName(schType)}>";
                        if (kodeliste == null)
                        {
                            result.Add($"Mangler kodeliste med id={kodelisteId}");
                        }
                        else
                        {
                            if (name != kodeliste.navn)
                            {
                                // kan være arvet type (restricted)
                                var baseType = XsdUtils.GetSimpleType(simpleType!.BaseXmlSchemaType);
                                if (baseType == null || baseType.Name != kodeliste.navn)
                                    result.Add($"kodeliste med id={kodelisteId} har feil navn");
                            }
                            var allEnumValues = enumValues.Select(ev => ev.Key).ToList();
                            var allKodelisteValues = kodeliste.koder!.Select(k => k.verdi).ToList();
                            foreach (var key in allEnumValues)
                            {
                                int index = allKodelisteValues.IndexOf(key);
                                if (index < 0)
                                    result.Add($"kodeliste med id={kodelisteId} mangler verdi '{key}'");
                                else
                                    allKodelisteValues.RemoveAt(index);
                                if (allKodelisteValues.Contains(key))
                                    result.Add($"kodeliste med id={kodelisteId} har duplikatverdi '{key}'");
                            }
                            foreach (var key in allKodelisteValues)
                            {
                                result.Add($"kodeliste med id={kodelisteId} inneholder ikke-eksisterende enumverdi='{key}'");
                            }
                        }
                    }
                }
            }
        }
        return result;
    }

    private static void testType(XmlSchema schema, XmlSchemaAnnotated schType, List<string> usedIds, List<string> duplicates)
    {
        if (!(XsdUtils.GetQualifiedNamespace(schType) == schema.TargetNamespace))
            return;
        var parent = XsdUtils.GetParent(schType);
        var parentName = parent != null ? XsdUtils.GetName(parent) : "";
        string value = schType.Id + " => " + parentName + "/" + XsdUtils.GetName(schType);
        if (usedIds.Contains(value))
            duplicates.Add(value);
        else
            usedIds.Add(value);
        if (schType is XmlSchemaComplexType complexType)
        {
            foreach (var child in XsdUtils.GetXsdChildren(complexType))
            {
                testType(schema, child, usedIds, duplicates);
            }
        }
    }

    public static List<string> TestIder()
    {
        List<string> result = [];
        List<string> usedIds = [];
        foreach (var schema in XsdUtils.XsdSchemas)
        {
            foreach (var objType in schema.SchemaTypes.Values)
            {
                if (objType is XmlSchemaAnnotated schType)
                    testType(schema, schType, usedIds, result);
            }
        }
        return result;
    }

}
