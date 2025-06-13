using Buf.Meldingsutveksler.SkjemaVerktoy.FilSystem;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml.Models;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.Xml
{
    public static class XsdUtils
    {
        public static string XSD_DIRECTORY { get; } = "xsd";

        public static List<XmlSchema> XsdSchemas { get; set; } = [];
        public static void ClearXsds()
        {
            XsdSchemas.Clear();
        }

        public static void Init(IFileSystem fileSystem)
        {
            var files = fileSystem.ListFiles(XSD_DIRECTORY + "/");
            foreach (var file in files)
            {
                using var reader = fileSystem.GetStream(file);
                XmlSchema schema = XmlSchema.Read(reader, null)
                        ?? throw new Exception($"Kan ikke laste {file}");
                XsdSchemas.Add(schema);
                var rec = XmlSchemaRegister.Schemas.xsds.FirstOrDefault(sch => sch.nmsp == schema.TargetNamespace);
                if (rec != null)
                {
                    rec.Schema = schema;
                    rec.XsdFilnavn = file;
                }
            }
            XmlSchemaSet schemaSet = new();
            foreach (var schema in XsdSchemas)
            {
                schemaSet.Add(schema);
            }
            schemaSet.Compile();
        }

        private static bool ContainsTypeDefinition(XmlSchema schema, string typeName)
        {
            var type = GetTypeDefinition(schema, typeName);
            return type != null;
        }

        public static XmlSchemaAnnotated? GetTypeDefinition(XmlSchema schema, string typeName)
        {
            foreach (var item in schema.Items)
            {
                if (item is XmlSchemaAnnotated typedef)
                {
                    if (GetName(typedef) == typeName)
                        return typedef;
                }
            }
            return null;
        }

        public static string GetKodelisterNmsp(XmlSchema schema)
        {
            foreach (var nmsp in schema.Namespaces.ToArray())
            {
                if (nmsp.Name == "kodelister")
                    return nmsp.Namespace;
            }
            return "";
        }

        public static List<XmlSchemaAnnotated> GetXsdElements(XmlSchemaAnnotated prop, bool flat = false)
        {
            List<XmlSchemaAnnotated> list = [];
            if (prop != null)
                GetXsdElements(prop, flat, ref list);
            return list;
        }

        public static void CreateSchemaSet(XmlSchemaRec schemaRec, ref List<XmlSchema> stack)
        {
            XmlSchemaSet schemaSet = new();
            XmlSchema schema = schemaRec.Schema!;
            if (stack.Contains(schemaRec.Schema!))
                throw new Exception($"Sirkulære refereranser til skjema {schema.TargetNamespace}");
            stack.Add(schema);
            foreach (var nmsp in schema.Namespaces.ToArray())
            {
                if (!nmsp.Namespace.Contains("http://www.w3.org/"))
                {
                    var rec = XmlSchemaRegister.Schemas.xsds.FirstOrDefault(s => s.Schema?.TargetNamespace == nmsp.Namespace)
                        ?? throw new Exception($"Finner ikke skjema til referert namespace {nmsp.Namespace} i schema {schema.TargetNamespace}");
                    if (rec.SchemaSet == null && rec != schemaRec)
                    {
                        CreateSchemaSet(rec, ref stack);
                        // SchemaSet created and added to rec.SchemaSet!!!
                    }
                    if (rec.SchemaSet != null)
                    {
                        foreach (XmlSchema itemSchema in rec.SchemaSet!.Schemas())
                        {
                            schemaSet.Add(itemSchema);
                        }
                    }
                }
            }
            schemaSet.Add(schema);
            schemaSet.Compile();
            schemaRec.SchemaSet = schemaSet;
            stack.Remove(schema);
        }

        public static XmlSchema GetSchema(string targetNmsp)
        {
            var result = XsdSchemas.First(sch => sch.TargetNamespace == targetNmsp);
            return result;
        }

        public static void GetXsdElements(XmlSchemaAnnotated prop, bool flat, ref List<XmlSchemaAnnotated> list)
        {
            list.Add(prop);
            if (flat && prop is XmlSchemaElement element)
                if (element.ElementSchemaType is XmlSchemaComplexType complexElement && flat)
                {
                    if (complexElement.Particle is XmlSchemaSequence sequenceElement)
                    {
                        foreach (XmlSchemaElement child in sequenceElement.Items.Cast<XmlSchemaElement>())
                        {
                            GetXsdElements(child, false, ref list);
                        }
                    }
                }
        }

        public static XmlSchema GetSchema(XmlSchemaObject obj)
        {
            if (obj is XmlSchemaElement element && element.ElementSchemaType != null)
                return GetSchema(element.ElementSchemaType);
            if (obj.Parent is XmlSchema schema)
                return schema;
            if (obj.Parent != null)
                return GetSchema(obj.Parent);
            throw new Exception("Finner ikke Schema som obj er definert i");
        }

        public static List<XmlSchemaAnnotated> GetXsdChildren(XmlSchemaAnnotated prop)
        {
            List<XmlSchemaAnnotated> list = [];
            XmlSchemaComplexType? elementType = null;
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaComplexType complexElement)
                elementType = complexElement;
            if (elementType == null && prop is XmlSchemaComplexType compType)
                elementType = compType;
            if (elementType != null)
            {
                if (elementType.Particle is XmlSchemaSequence sequenceElement)
                {
                    foreach (var child in sequenceElement.Items.Cast<XmlSchemaAnnotated>())
                    {
                        list.Add(child);
                    }
                }
                else if (elementType.ContentTypeParticle is XmlSchemaSequence contentTypeSequence)
                {
                    foreach (var child in contentTypeSequence.Items.Cast<XmlSchemaAnnotated>())
                    {
                        list.Add(child);
                    }
                }

            }
            return list;
        }
        public static List<XmlSchemaAnnotated> GetXsdChildElements(XmlSchemaAnnotated? prop)
        {
            List<XmlSchemaAnnotated> list = [];
            if (prop == null)
                return list;
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaComplexType complexElement)
            {
                if (complexElement.Particle is XmlSchemaSequence sequenceElement)
                {
                    foreach (var child in sequenceElement.Items.Cast<XmlSchemaAnnotated>())
                    {
                        //TODO! Det er her et Choice - element dukker opp i lista sammen med alle XmlSchemaElement -typer!!!! må få med < choice >
                        if (child is XmlSchemaElement childElement)
                            list.Add(childElement);
                        else if (child is XmlSchemaChoice choice)
                            list.Add(choice);
                    }
                }
                else if (complexElement.ContentTypeParticle is XmlSchemaSequence contentTypeSequence)
                {
                    foreach (XmlSchemaAnnotated child in contentTypeSequence.Items.Cast<XmlSchemaAnnotated>())
                    {
                        if (child is XmlSchemaElement childElement)
                            list.Add(childElement);
                    }
                }

            }
            return list;
        }

        public static XmlSchemaSimpleType? GetIterateTypeDefinition(XmlSchemaAnnotated prop)
        {
            var constraint = GetUniqueConstraint(prop);
            if (constraint != null)
            {
                var iteratorProp = constraint.Selector!.XPath
                    ?? throw new Exception($"Ingen selector i unique constraint i element {GetName(prop)}");
                var enumProp = iteratorProp == "." ? prop : GetChildByPath(prop, iteratorProp);
                if (enumProp != null)
                    return GetSimpleType(enumProp);
            }
            return null;
        }


        public static XmlSchemaUnique? GetUniqueConstraint(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
            {
                foreach (var constraint in element.Constraints)
                {
                    if (constraint is XmlSchemaUnique result)
                        return result;
                }
            }
            return null;
        }

        public static XmlSchemaUnique? GetUniqueConstraintChild(XmlSchemaAnnotated prop)
        {
            var children = GetXsdChildren(prop);
            if (children != null)
            {
                foreach (var child in children)
                {
                    var uniqueConstraint = GetUniqueConstraint(child);
                    if (uniqueConstraint != null)
                        return uniqueConstraint;
                }
                if (prop is XmlSchemaElement element)
                {
                    foreach (var constraint in element.Constraints)
                    {
                        if (constraint is XmlSchemaUnique result)
                            return result;
                    }
                }
            }
            return null;
        }

        public static bool HasUniqueConstraint(XmlSchemaAnnotated prop)
        {
            var constraint = GetUniqueConstraint(prop);
            return constraint != null;
        }

        internal static bool HasUniqueConstraintChild(XmlSchemaAnnotated schemaChild)
        {
            var unique = GetUniqueConstraintChild(schemaChild);
            return unique != null;
        }

        public static int GetMaxOccurs(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
            {
                if (element.MaxOccurs > 1000)
                    return 1000;
                return Convert.ToInt32(element.MaxOccurs);
            }
            else if (prop is XmlSchemaChoice choice)
            {
                return Convert.ToInt32(choice.MaxOccurs);
            }
            return 1;
        }

        public static int GetMinOccurs(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
            {
                return Convert.ToInt32(element.MinOccurs);
            }
            else if (prop is XmlSchemaChoice choice)
            {
                return Convert.ToInt32(choice.MinOccurs);
            }
            return 0;
        }

        public static bool GetIsRepeating(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
            {
                return element.MaxOccurs > 1;
            }
            return false;
        }

        public static List<XmlSchemaObject>? GetChoiceElements(XmlSchemaAnnotated element)
        {
            if (element is XmlSchemaElement { ElementSchemaType: XmlSchemaComplexType compType })
            {
                if (compType.Particle is XmlSchemaChoice choice)
                {
                    return GetChoiceElements(choice);
                }
            }
            else if (element is XmlSchemaChoice choice)
                return GetChoiceElements(choice);
            return null;
        }
        public static List<XmlSchemaObject> GetChoiceElements(XmlSchemaChoice choiceElement)
        {
            List<XmlSchemaObject> result = [];

            foreach (var item in choiceElement.Items)
                result.Add(item);
            return result;
        }

        public static bool IsSimpleType(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaSimpleType)
                return true;
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaSimpleType)
                return true;
            return false;

        }

        public static bool IsChoiceElement(XmlSchemaAnnotated element)
        {
            if (element == null)
                return false;
            return element is XmlSchemaChoice;
        }

        public static bool IsChoiceElement(XmlSchemaElement? element)
        {
            if (element is null)
                return false;
            if (element.ElementSchemaType is XmlSchemaComplexType compType)
            {
                return IsChoiceElement(compType);
            }
            return false;
        }


        public static bool IsChoiceElement(XmlSchemaComplexType elementType)
        {
            if (elementType == null)
                return false;
            bool anyChoiceChildren = false;
            bool allChildrenChoice = true;
            if (elementType.Particle is XmlSchemaChoice choice)
                anyChoiceChildren = true;
            else
                allChildrenChoice = false;
            return anyChoiceChildren && allChildrenChoice;
        }

        public static bool IsOptionalBlock(XmlSchemaElement element)
        {
            return element.MinOccurs == 0 && element.MaxOccurs == 1 && AnyMandatoryElements(element);
        }

        private static bool AnyMandatoryElements(XmlSchemaElement element)
        {
            var children = GetXsdChildren(element);
            foreach (var child in children)
            {
                if (child is XmlSchemaElement childElement)
                {
                    if (childElement.MinOccurs > 0)
                        return true;
                }
            }
            return false;
        }

        public static string GetFixedValue(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
                return element.FixedValue ?? "";
            return "";
        }

        public static string GetDataTypeText(XmlSchemaAnnotated element, bool includeMultiplicity)
        {
            string dataType = "";
            var simpleType = XsdUtils.GetSimpleType(element);
            var complexType = XsdUtils.GetComplexType(element);
            var minOccurs = GetMinOccurs(element);
            string maxOccurs = GetMaxOccurs(element).ToString();
            if (maxOccurs == "1000")
                maxOccurs = "\u221E";
            string multiplicity = $" [{minOccurs}..{maxOccurs}]";
            if (GetIsEnumType(simpleType))
            {
                dataType = "enum";
            }
            else
            {
                if (simpleType != null)
                    dataType = simpleType.Datatype?.ValueType.Name ?? "(ukjent)";
            }
            if (includeMultiplicity)
                dataType += multiplicity;
            return dataType;
        }

        public static List<KeyValuePair<string, string>> GetEnumValues(XmlSchemaAnnotated prop)
        {
            var fixedValue = GetFixedValue(prop);
            if (fixedValue != "")
                return [new(fixedValue, fixedValue)];
            else
            {
                var simpleType = GetSimpleType(prop);
                if (simpleType != null)
                {
                    return GetEnumValues(simpleType);
                }
                throw new Exception($"{prop} is not an Enumeration type");
            }
        }

        public static List<KeyValuePair<string, string>> GetEnumValues(XmlSchemaSimpleType simpleType)
        {
            var result = new List<KeyValuePair<string, string>>();
            if (simpleType.Content is XmlSchemaSimpleTypeRestriction restriction)
            {
                foreach (XmlSchemaEnumerationFacet facet in restriction.Facets.OfType<XmlSchemaEnumerationFacet>())
                {
                    if (facet.Value != null)
                        result.Add(new(facet.Value, facet.Value));
                }
            }
            return result;
        }

        public static bool GetIsEnumType(XmlSchemaSimpleType? simpleType)
        {
            if (simpleType?.Content is XmlSchemaSimpleTypeRestriction restriction)
            {
                if (restriction.Facets.OfType<XmlSchemaEnumerationFacet>().Any())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool GetIsOptional(XmlSchemaAnnotated element)
        {
            if (element is XmlSchemaElement el)
                return el.MinOccurs == 0;
            return false;
        }

        public static XmlSchemaSimpleType? GetSimpleType(XmlSchemaAnnotated? prop)
        {
            if (prop is XmlSchemaSimpleType simpleType)
                return simpleType;
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaSimpleType elementSimpleType)
                return elementSimpleType;
            else if (prop is XmlSchemaAttribute attribute && attribute.AttributeSchemaType is XmlSchemaSimpleType attrSimpleType)
                return attrSimpleType;
            return null;
        }
        public static XmlSchemaComplexType? GetComplexType(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaComplexType complexType)
                return complexType;
            if (prop is XmlSchemaElement element && element.ElementSchemaType is XmlSchemaComplexType elementComplexType)
                return elementComplexType;
            return null;
        }

        public static int GetMinLength(XmlSchemaAnnotated prop)
        {
            var simpleType = GetSimpleType(prop);
            if (simpleType != null)
            {
                if (simpleType.Content is XmlSchemaSimpleTypeRestriction restriction)
                {
                    foreach (var facet in restriction.Facets.OfType<XmlSchemaMinLengthFacet>())
                    {
                        if (int.TryParse(facet.Value, out int len))
                            return len;
                    }
                }
            }
            return 0;
        }

        public static string GetRegExRestriction(XmlSchemaAnnotated prop)
        {
            var simpleType = GetSimpleType(prop);
            if (simpleType != null)
            {
                if (simpleType.Content is XmlSchemaSimpleTypeRestriction restriction)
                {
                    foreach (var facet in restriction.Facets.OfType<XmlSchemaPatternFacet>())
                    {
                        if (!string.IsNullOrEmpty(facet.Value))
                            return facet.Value;
                    }
                }
            }
            return "";
        }

        public static int GetMaxLength(XmlSchemaAnnotated prop)
        {
            var simpleType = GetSimpleType(prop);
            if (simpleType != null)
            {
                if (simpleType.Content is XmlSchemaSimpleTypeRestriction restriction)
                {
                    foreach (var facet in restriction.Facets.OfType<XmlSchemaMaxLengthFacet>())
                    {
                        if (int.TryParse(facet.Value, out int len))
                            return len;
                    }
                }
            }
            return 0;
        }
        internal static string GetMarkupText(XmlNode[] markup)
        {
            if (markup != null && markup.Length > 0)
            {
                StringBuilder sb = new();
                foreach (var node in markup)
                {
                    if (node != null)
                    {
                        sb.Append(node.Value?.Trim() ?? "");
                    }
                }
                return sb.ToString();
            }
            return "";
        }

        private static string GetAppInfoElement(XmlNode?[]? markup, string elementName, bool selectXml)
        {
            if (markup?.Length >= 1)
            {
                for (int i = 0; i < markup.Length; i++)
                {
                    if (markup[i]?.Name == elementName)
                    {
                        return selectXml ? markup[i]!.OuterXml : markup[i]!.InnerText.Trim();
                    }
                }
            }
            return "";
        }



        public static string GetAppInfoValue(XmlSchemaAnnotated? xsdType, string appInfoElement, bool selectXml, bool recurse)
        {
            XmlSchemaAnnotation? annotation = xsdType?.Annotation;
            // Hack for å kompensere at XmlSchemaChoice alltid har null i Annotation
            // Funker med dette "hacket":
            if (annotation == null && xsdType is XmlSchemaChoice)
            {
                int index = 0;
                for (int i = 0; i < (xsdType.Parent as XmlSchemaSequence)?.Items.Count; i++)
                {
                    var item = (xsdType.Parent as XmlSchemaSequence)?.Items[i];
                    if (item?.LineNumber == xsdType.LineNumber)
                    {
                        index = i; break;
                    }
                }
                if (index < 0)
                    throw new Exception("Finner ikke choice-element i parent.Items");
                annotation = ((xsdType.Parent as XmlSchemaSequence)?.Items[index] as XmlSchemaChoice)?.Annotation;
            }
            if (annotation != null)
            {
                foreach (var item in annotation.Items)
                {
                    if (item is XmlSchemaAppInfo appInfo)
                    {
                        string tmpValue = GetAppInfoElement(appInfo.Markup, appInfoElement, selectXml);
                        if (tmpValue != "")
                            return tmpValue;
                    }
                }
            }
            if (recurse)
            {
                if (xsdType is XmlSchemaElement element)
                {
                    return GetAppInfoValue(element.ElementSchemaType, appInfoElement, false, false);
                }
                else if (xsdType is XmlSchemaAttribute attribute)
                {
                    return GetAppInfoValue(attribute.AttributeSchemaType, appInfoElement, false, false);
                }
            }
            return "";
        }

        public static string GetName(XmlSchemaAnnotated? prop)
        {
            if (prop == null)
                return "";
            if (prop is XmlSchemaElement element)
                return element.Name ?? "";
            else if (prop is XmlSchemaUnique unique)
                return unique.Name ?? "";
            else if (prop is XmlSchemaAttribute attribute)
                return attribute.Name ?? "";
            else if (prop is XmlSchemaType type)
                return type.Name ?? "";
            return "";
        }

        public static List<XmlSchemaAnnotated> FindByXPath(XmlSchemaAnnotated startingPoint, string xpath)
        {
            List<XmlSchemaAnnotated> result = [];
            var children = GetXsdChildren(startingPoint);
            foreach (XmlSchemaAnnotated child in children)
            {
                if (string.Compare(GetName(child), xpath, true) == 0)
                    result.Add(child);
                var grandchildren = FindByXPath(child, xpath);
                if (grandchildren.Count > 0)
                    result.AddRange(grandchildren);
            }
            return result;
        }

        public static XmlSchemaAnnotated GetChildByPath(XmlSchemaAnnotated startingPoint, string xpath)
        {
            string[] pathArr = xpath.Split('/', StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries);
            var result = GetChildByPath(startingPoint, pathArr)
                ?? throw new Exception($"{GetName(startingPoint)}/{xpath} not found");
            return result;
        }
        internal static XmlSchemaAnnotated? GetChildByPath(XmlSchemaAnnotated startingPoint, string[] pathArr)
        {
            var child1stLevel = FindByXPath(startingPoint, pathArr[0]);
            if (child1stLevel.Count == 0)
                return null;
            if (pathArr.Length > 1) // neste nivå i path
                return GetChildByPath(child1stLevel[0], [.. pathArr.Skip(1)]);
            return child1stLevel[0];
        }

        public static bool PropIsMandatory(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement element)
                return element.MinOccurs > 0;
            return false;
        }



        public static bool Extends(XmlSchemaType elementType, string ancestorType)
        {
            bool extends = elementType?.BaseXmlSchemaType?.Name == ancestorType;
            if (!extends && elementType?.BaseXmlSchemaType != null)
                extends = Extends(elementType.BaseXmlSchemaType, ancestorType);
            return extends;
        }


        public static bool Extends(XmlSchemaElement element, string ancestorType)
        {
            if (element.ElementSchemaType == null)
                return false;
            return Extends(element.ElementSchemaType, ancestorType);
        }
        public static XmlSchemaElement? ElementsExtending(XmlSchema schema, string ancestorType)
        {
            var result = GetRootElement(schema);
            return result != null && Extends(result, ancestorType) ? result : null;
        }

        public static string GetQualifiedNamespace(XmlSchemaAnnotated elementType)
        {
            if (elementType is XmlSchemaElement schemaElement)
                return schemaElement.QualifiedName.Namespace;
            if (elementType is XmlSchemaSimpleType simpleType)
                return simpleType.QualifiedName.Namespace;
            if (elementType is XmlSchemaComplexType complexType)
                return complexType.QualifiedName.Namespace;
            return "";
        }

        public static bool ChoiceElementMandatory(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaChoice choiceElement)
            {
                return choiceElement.MinOccurs > 0;
            }
            return false;
        }

        public static bool AllChoiceElementsSimpleAndMandatory(XmlSchemaAnnotated prop)
        {
            if (prop is XmlSchemaElement schemaElement)
            {
                bool allChildrenMandatory = true;
                if (IsChoiceElement(schemaElement))
                {
                    var children = GetXsdChildren(schemaElement);
                    foreach (var child in children)
                    {
                        if (!PropIsMandatory(child))
                        {
                            allChildrenMandatory = false;
                            break;
                        }
                    }
                    return allChildrenMandatory;
                }
            }
            return false;
        }

        public static XmlSchema? SchemaFromTargetNmsp(string nmsp)
        {
            if (nmsp == null)
                return null;
            var schema = XsdSchemas.FirstOrDefault(sch => sch.TargetNamespace == nmsp);
            if (schema != null)
                return schema;
            return null;
        }

        public static List<XmlSchema> SchemaFromVersionNeutralTargetNmsp(string searchNmsp)
        {
            var versionPos = searchNmsp.LastIndexOf("_v");
            var regExStr = searchNmsp[0..versionPos] + "\\.v[0-9]*\\.[0-9]*\\.[0-9]*";
            var regEx = new Regex(regExStr);

            var result = new List<XmlSchema>();
            foreach (var schema in XsdSchemas.Where(sch => regEx.IsMatch(sch.TargetNamespace!)).OrderBy(sch => sch.TargetNamespace!))
            {
                result.Add(schema);
            }
            return result;
        }



        public static XmlSchemaElement? GetRootElement(XmlSchema? schema)
        {
            if (schema?.Elements.Count == 1)
            {
                // Litt odd, fant ingen annen måte å returnere første (eneste!) element....
                foreach (var element in schema.Elements)
                {
                    var entry = (DictionaryEntry)element;
                    return entry.Value as XmlSchemaElement;
                }
            }
            return null;
        }

        public static XmlSchema[] XsdSchemasWithNamedRootElement(string name)
        {
            List<XmlSchema> result = [];
            foreach (var schema in XsdSchemas.Where(sch => (GetRootElement(sch)?.Name ?? "-") == name))
            {
                result.Add(schema);
            }
            return [.. result];
        }

        public static string[] XsdSchemaNamesWithNamedRootElement(string name)
        {
            var schemas = XsdSchemasWithNamedRootElement(name);
            return [.. schemas.Select(sch => sch.TargetNamespace!)];
        }

        public static string GetChoiceElementNames(XmlSchemaChoice? element, string delimiter = "_")
        {
            if (element == null)
                return "";
            List<string> names = [];
            foreach (var item in element.Items)
            {
                names.Add(GetName(item as XmlSchemaAnnotated));
            }
            return string.Join(delimiter, names);
        }

        public static XmlSchemaAnnotated? GetParent(XmlSchemaAnnotated schType)
        {
            if (schType.Parent is XmlSchemaAnnotated ann && ann.Parent is XmlSchemaAnnotated annParent)
                return annParent;
            return null;
        }

        public static XmlSchemaSet? GetSchemaSet(XmlSchema schema, out string schemaLoc)
        {
            schemaLoc = schema.TargetNamespace;
            return XmlSchemaRegister.GetSchemaSet(schema.TargetNamespace);
        }
    }
}