using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml.Models;
using Demo.Fagsystem.Models.Test;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Schema;

namespace Demo.Fagsystem.Pages
{
    public class TestSchemaModel : PageModel
    {
        public TestSchemaModel()
        {
        }

        public void OnGet()
        {
            Init();
        }

        public void OnPost()
        {
            Init();
        }

        // SchemaRec sortert på rotelementnavn, xsder uten rotelement utelatt
        public Dictionary<string, List<XmlSchemaRec>> schemaRootElementTypes = [];
        public Dictionary<string, List<ElementXPathRecord>> elementTyper { get; } = [];
        public void Init()
        {

            foreach (var schemaRec in XmlSchemaRegister.Schemas.xsds)
            {
                var rootElement = XsdUtils.GetRootElement(schemaRec.Schema);
                if (rootElement?.Name != null)
                {
                    if (!schemaRootElementTypes.ContainsKey(rootElement.Name))
                    {
                        schemaRootElementTypes[rootElement.Name] = [];
                    }
                    schemaRootElementTypes[rootElement.Name].Add(schemaRec);
                }
            }

            foreach (var kvp in schemaRootElementTypes)
            {
                var rootElementName = kvp.Key;
                var schemaRecs = kvp.Value;
                elementTyper[rootElementName] = [];
                int schemaIndex = 0;
                foreach (var schemaRec in schemaRecs)
                {
                    var schema = schemaRec.Schema;
                    ElementToMatrix(schema, elementTyper[rootElementName], schemaRecs.Count, schemaIndex);
                    schemaIndex++;
                }
            }

            /*Kodelistefeil = XsdTester.TestKodelister();
            Tekstfeil = XsdTester.TestJsonTekster();
            Idfeil = XsdTester.TestIder();
            */
        }

        private static void ElementToMatrix(XmlSchema? schema, List<ElementXPathRecord> elementRecs, int schemaCount, int schemaIndex)
        {
            var rootElement = XsdUtils.GetRootElement(schema)!;
            UpdateMatrix("", rootElement, false, elementRecs, schemaCount, schemaIndex);
        }

        private static void UpdateMatrix(string path, XmlSchemaAnnotated element, bool elementIsChoice, List<ElementXPathRecord> elementRecs, int schemaCount, int schemaIndex)
        {
            bool isChoiceElement = XsdUtils.IsChoiceElement(element);
            var name = XsdUtils.GetName(element);
            var xPath = $"{path}/{XsdUtils.GetName(element)}".TrimEnd('/');
            if (!isChoiceElement)
            {
                var rec = elementRecs.FirstOrDefault(e => e.Xpath == xPath);
                if (rec == null)
                {
                    rec = new(xPath, schemaCount);
                    // sjekke om elementet burde insertes etter element med samme path eller legges på slutten
                    var insertAfterElement = elementRecs.LastOrDefault(er => er.Xpath.StartsWith(path));
                    if (insertAfterElement != null)
                    {
                        int index = elementRecs.IndexOf(insertAfterElement) + 1;
                        if (index < elementRecs.Count)
                            elementRecs.Insert(index, rec);
                        else
                            elementRecs.Add(rec);
                    }
                    else
                        elementRecs.Add(rec);
                }
                rec.elementInstances[schemaIndex] = new(element, elementIsChoice);
            }
            XmlSchemaComplexType? complexType = XsdUtils.GetComplexType(element);
            XmlSchemaSimpleType? simpleType = XsdUtils.GetSimpleType(element);
            if (isChoiceElement)
            {
                var choices = XsdUtils.GetChoiceElements(element)
                    ?? throw new Exception($"Finner ikke choice-element til definisjon {XsdUtils.GetName(element)}");
                foreach (var choiceElement in choices.Cast<XmlSchemaAnnotated>())
                {
                    UpdateMatrix(xPath, choiceElement, true, elementRecs, schemaCount, schemaIndex);
                }
            }
            var children = XsdUtils.GetXsdChildElements(element);
            foreach (var child in children)
            {
                UpdateMatrix(xPath, child, false, elementRecs, schemaCount, schemaIndex);
            }
            //SortLists();
        }

        private void SortLists()
        {
            foreach (var elTyp in elementTyper)
            {
                elTyp.Value.Sort((el1, el2) => el1.Xpath.CompareTo(el2.Xpath));
            }
        }

        public List<string> GetNonUsed()
        {
            List<string> result = [];
            /*            foreach (var item in TeksterUtils.TekstFiler.skjema?.skjemaelement ?? [])
                        {
                            string itemValue = $"{item.id} - {item.navn}";
                            if (TeksterUtils.TekstFiler.used.IndexOf(itemValue) < 0)
                                result.Add(itemValue);
                        }*/
            return result;
        }

        public List<string> Kodelistefeil { get; set; } = [];
        public List<string> Tekstfeil { get; set; } = [];
        public List<string> Idfeil { get; set; } = [];
    }
}
