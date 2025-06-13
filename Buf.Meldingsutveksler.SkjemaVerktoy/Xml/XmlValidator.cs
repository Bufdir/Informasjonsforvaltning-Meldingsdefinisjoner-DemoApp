using Buf.Meldingsutveksler.SkjemaVerktoy.Xml.Models;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.Xml;

public static class XmlValidator
{
    public static bool ValidateXmlBuiltIn(XmlSchema schema, string xml, out string error)
    {
        error = "";
        var rootElement = XsdUtils.GetRootElement(schema);
        XmlSchemaSet schemaSet = XsdUtils.GetSchemaSet(schema, out _);
        schemaSet.Compile();

        XmlReaderSettings settings = new();
        settings.Schemas.Add(schemaSet);
        settings.Schemas.Compile();
        settings.ValidationType = ValidationType.Schema;
        var doc = XDocument.Parse(xml);
        XmlReader xReader = XmlReader.Create(doc.CreateReader(), settings);
        bool keepReading = true;
        while (keepReading)
        {
            try
            {
                keepReading = xReader.Read();
            }
            catch (XmlSchemaValidationException ex)
            {
                error = ex.Message;
                return false;
            }
        }
        return true;
    }

    public static bool ValidateXmlBuiltIn(XmlDocument doc, string schemaNmsp, out string error)
    {
        string valError = "";
        error = "";
        if (doc.Schemas.Count == 0)
        {
            var schema = XsdUtils.SchemaFromTargetNmsp(schemaNmsp)
                ?? throw new Exception($"Skjema {schemaNmsp} mangler rotelement");
            var shemaSet = XsdUtils.GetSchemaSet(schema, out string schemaLoc);
            doc.Schemas.Add(shemaSet);
        }
        doc.Validate(new((o, ve) =>
        {
            valError = ve.Message;
        }));
        error = valError;
        return valError == "";
    }
    public static bool ValidateXmlBuiltIn(XmlDocument doc, out string error)
    {
        string valError = "";
        error = "";
        doc.Validate(new((o, ve) =>
        {
            valError = ve.Message;
        }));
        error = valError;
        return valError == "";
    }

    private static bool IsCorrespondingNodeName(XmlNode? node, XmlSchemaAnnotated elementType, Dictionary<string, string> nmspAbbrevs, List<XmlSchemaObject>? choiceItems)
    {
        if (node == null)
            return false;
        if (choiceItems != null)
        {
            foreach (var choiceItem in choiceItems.Cast<XmlSchemaElement>())
            {
                if (IsCorrespondingNodeName(node, choiceItem, nmspAbbrevs, null))
                    return true;
            }
            return false;
        }
        string nmsp = XsdUtils.GetQualifiedNamespace(elementType);
        if (!nmspAbbrevs.TryGetValue(nmsp, out string? elementPrefix))
            elementPrefix = "";
        return node.Name == elementPrefix + ":" + XsdUtils.GetName(elementType);
    }

    public static bool ValidateXml(XmlDocument doc, XmlSchema schema, Dictionary<string, string> nmspAbbrevs, List<XmlValidationError> errors)
    {
        var rootElement = doc.ChildNodes[0]!.NextSibling;
        var rootXsdElement = XsdUtils.GetRootElement(schema);
        List<XmlValidationError> _errors = ValidateChildren(rootElement!, rootXsdElement!, nmspAbbrevs);
        errors.AddRange(_errors);
        return errors.Count == 0;
    }

    internal static XmlValidationError? TextLengthWithinBounds(string text, XmlSchemaAnnotated schemaType)
    {
        string elementName = XsdUtils.GetName(schemaType) ?? "<ukjent navn>";
        XmlSchemaSimpleType simpleType = XsdUtils.GetSimpleType(schemaType)
            ?? throw new Exception($"Element {elementName} isw not a simple type!");
        int len = string.IsNullOrEmpty(text) ? 0 : text.Length;
        int minLen = XsdUtils.GetMinLength(simpleType);
        int maxLen = XsdUtils.GetMaxLength(simpleType);
        if (len < minLen)
            return new XmlValidationError(schemaType, XmlValidationErrorType.TooShortValue, $"Lengde skulle vært minst {minLen}, er {len}");
        if (len > maxLen)
            return new XmlValidationError(schemaType, XmlValidationErrorType.TooLongValue, $"Lengde skulle vært maks. {maxLen}, er {len}");
        return null;
    }

    internal static bool GetPatternOK(string pattern, string value)
    {
        Regex regex = new(pattern);
        return regex.IsMatch(value);
    }


    private static List<XmlValidationError> ValidateSimpleElement(XmlNode parentNode, XmlNode node, XmlSchemaAnnotated schemaType)
    {
        XmlValidationError? error = null;
        var regex = XsdUtils.GetRegExRestriction(schemaType);
        if (regex != null)
        {
            if (!GetPatternOK(regex, node.InnerText))
                error = new(schemaType, XmlValidationErrorType.PatternMatchError, $"Verdi '{node.InnerText}' matcher ikke pattern '{regex}'");
        }
        else
            error = TextLengthWithinBounds(node.InnerText, schemaType);
        error ??= ValidateEnablerCondition(parentNode, node, schemaType);

        if (error != null)
            return [error];
        return [];
    }

    private static XmlValidationError? ValidateEnablerCondition(XmlNode parentNode, XmlNode? node, XmlSchemaAnnotated schemaType)
    {
        string aktiveringExpression = XsdUtils.GetAppInfoValue(schemaType, "enable", false, false);
        if (aktiveringExpression != "")
        {
            string[] parts = aktiveringExpression.Split("=");
            string aktiveringControlID = parts[0];
            bool isInverted = aktiveringControlID.EndsWith('!');
            aktiveringControlID = aktiveringControlID.TrimEnd('!');
            string aktiveringValue = parts.Length > 0 ? parts[1] : "";
            var aktiveringNode = XmlUtils.GetNodeChild(parentNode, aktiveringControlID);
            //                ?? throw new Exception($"Finner ikke node med navn '{aktiveringControlID}' i enable-uttrykk i element '{XsdUtils.GetName(schemaType)}'");
            string aktiveringNodeValue = aktiveringNode == null ? "" : aktiveringNode.InnerText == "false" ? "0" : aktiveringNode.InnerText == "true" ? "1" : aktiveringNode.InnerText;
            bool enableConditionMet = aktiveringNodeValue == "" || (aktiveringValue == "" ? aktiveringNodeValue != "" : aktiveringNodeValue == aktiveringValue);
            if (isInverted)
                enableConditionMet = !enableConditionMet;
            if (enableConditionMet && (node?.InnerText ?? "") == "")
                return new(schemaType, XmlValidationErrorType.MissingValue, $"Mangler obligatorisk betinget verdi i element (betingelse: {aktiveringExpression})");
        }
        return null;
    }

    /*    private static List<XmlValidationError> ValidateChoiceElement(XmlNode node, XmlSchemaAnnotated schemaType, Dictionary<string, string> nmspAbbrevs)
        {
            var errors = new List<XmlValidationError>();
            foreach (var schemaChild in XsdUtils.GetXsdChildren(schemaType))
            {
                if (IsCorrespondingNodeName(node, schemaChild, nmspAbbrevs))
                    return errors;
            }
            List<string> missingTypes = [];
            var elements = XsdUtils.GetChoiceElements(schemaType);
            foreach (var choiceElement in elements)
            {
                if (choiceElement is XmlSchemaAnnotated schEl)
                    missingTypes.Add(XsdUtils.GetName(schEl) ?? "???");
            }
            errors.Add(new(XsdUtils.GetName(schemaType), XmlValidationErrorType.MissingValue, $"Ett av følgende element: {string.Join(", ", missingTypes)}"));
            return errors;
        }*/

    public static List<XmlValidationError> ValidateChildren(XmlNode node, XmlSchemaAnnotated schemaType, Dictionary<string, string> nmspAbbrevs)
    {
        var currentNode = node.ChildNodes.Count > 0 ? node.ChildNodes[0] : null;
        List<XmlValidationError> result = [];
        var children = XsdUtils.GetXsdChildren(schemaType);
        foreach (var schemaChild in children)
        {
            List<XmlSchemaObject>? choiceItems = XsdUtils.GetChoiceElements(schemaChild);
            bool isChoiceElement = choiceItems != null;
            string elementName = XsdUtils.GetName(schemaChild);
            int num = 0;
            while (IsCorrespondingNodeName(currentNode, schemaChild, nmspAbbrevs, choiceItems))
            {
                num++;
                List<XmlValidationError> elementErrors = [];
                XmlSchemaAnnotated schemaAnnotated = schemaChild;
                if (isChoiceElement)
                {
                    foreach (var choiceItem in choiceItems!)
                    {
                        if (choiceItem is XmlSchemaAnnotated thisChoice)
                        {
                            if (XsdUtils.GetName(thisChoice) == currentNode!.LocalName)
                                schemaAnnotated = thisChoice;
                        }
                    }
                }
                if (XsdUtils.IsSimpleType(schemaAnnotated))
                {
                    elementErrors = ValidateSimpleElement(node, currentNode!, schemaAnnotated);
                }
                else
                {
                    // Er det et Choice-element?
                    var complexChild = XsdUtils.GetComplexType(schemaAnnotated)
                        ?? throw new Exception("Ikke SimpleType og ikke ComplexType????");
                    elementErrors = ValidateChildren(currentNode!, schemaAnnotated, nmspAbbrevs);
                }
                currentNode = currentNode!.NextSibling;
                result.AddRange(elementErrors);
            }
            // Feil ved Choice..... 
            // må sjekke om ett av <choice> fins....
            int minOccurs = XsdUtils.GetMinOccurs(schemaChild);
            int maxOccurs = XsdUtils.GetMaxOccurs(schemaChild);
            if (num > 1 && XsdUtils.HasUniqueConstraintChild(schemaChild))
            {
                var unique = XsdUtils.GetUniqueConstraintChild(schemaChild)
                    ?? throw new Exception($"Kunne ikke finne element {XsdUtils.GetName(schemaChild)} i unique constraint");
                var uniqueValues = GetUniqueValues(node, unique, out var totalCount);
                if (uniqueValues.Count < totalCount)
                {
                    result.Add(new XmlValidationError(schemaChild, XmlValidationErrorType.MissingValue, $"Duplikatverdier for element '{XsdUtils.GetName(unique.Parent as XmlSchemaElement)}' detektert"));
                }
            }
            if (num == 0 && minOccurs == 0 && XsdUtils.GetSimpleType(schemaChild) != null)
            {
                var error = ValidateEnablerCondition(node, null, schemaChild);
                if (error != null)
                    result.Add(error);
            }
            else if (num == 0 && minOccurs > 0)
            {
                string errorText = minOccurs > 1 ? $"{minOccurs} obligatoriske elementer mangler" : "Obligatorisk element mangler";
                result.Add(new XmlValidationError(schemaChild, XmlValidationErrorType.MissingValue, errorText));
            }
            else if (num < minOccurs || num > maxOccurs)
                result.Add(new XmlValidationError(schemaChild, XmlValidationErrorType.CountOutOfRange, $"Feil antall elementer ({num}) - min={minOccurs}, max={maxOccurs})"));
        }
        return result;
    }

    private static List<string> GetUniqueValues(XmlNode node, XmlSchemaUnique uniqueElement, out int totalCount)
    {
        totalCount = 0;
        List<string> result = [];
        string uniqueElementName = uniqueElement.Selector!.XPath!;
        foreach (XmlNode childNode in node.ChildNodes)
        {
            foreach (XmlNode grandchildNode in childNode.ChildNodes)
            {
                if (grandchildNode.LocalName == uniqueElementName)
                {
                    totalCount++;
                    if (!result.Contains(grandchildNode.InnerText))
                        result.Add(grandchildNode.InnerText);
                }
            }
        }
        return result;
    }
}
