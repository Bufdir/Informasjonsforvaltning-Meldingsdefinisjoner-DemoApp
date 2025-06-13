using Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste;
using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll;
using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll.Models;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using Demo.Fagsystem.Models.Demodata;
using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using System.Data;
using System.Xml;
using System.Xml.Schema;

namespace Demo.Fagsystem.Models.Utils
{
    public static class XmlFactory
    {

        public static XmlDocument WriteXmlReply(Meldingshode receivedMeldingshode, MeldingsForbindelseType forbindelse, string xmlNamespace, /*Dictionary<string, string> values, */ FagsystemBase fagsystem/*, KontaktInfo kontaktinfo*/)
        {
            var schema = XsdUtils.GetSchema(xmlNamespace);
            Meldingshode meldingshode = MeldingsprotokollUtils.GetReplyMeldingshode(schema, receivedMeldingshode, forbindelse, fagsystem.FagsystemInfo/*, kontaktinfo*/);
            var doc = new XmlDocument();
            throw new NotImplementedException(nameof(WriteXmlReply));
            //            return doc;
        }

        public static void PreWriteXml(XmlDocument doc, XmlSchema schema, out Dictionary<string, string> nmsps, out Dictionary<string, string> rootElementAttributes, out string schemaLoc)
        {
            var schemaElement = XsdUtils.GetRootElement(schema)
                ?? throw new Exception($"Ingen rotelement i {schema.TargetNamespace}");

            nmsps = [];
            XmlSchemaSet schemaSet = XsdUtils.GetSchemaSet(schema, out schemaLoc);

            doc.Schemas.Add(schemaSet);

            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            rootElementAttributes = [];

            foreach (XmlSchema sch in schemaSet.Schemas())
            {
                foreach (var nms in sch.Namespaces.ToArray().ToList())
                {
                    if (nms.Name != "" && nms.Namespace.StartsWith("https://bufdir.no") && !nmsps.ContainsKey(nms.Namespace))
                    {
                        nmsps.Add(nms.Namespace, nms.Name);
                        rootElementAttributes["xmlns:" + nms.Name] = nms.Namespace;
                    }
                }
            }
            nmsps.Add(schemaElement.QualifiedName.Namespace, "melding");
            rootElementAttributes["xmlns"] = schemaElement.QualifiedName.Namespace;

            rootElementAttributes["xmlns:xsi"] = "http://www.w3.org/2001/XMLSchema-instance";


        }

        public static void PostWriteXml(XmlDocument doc, XmlSchema schema, Dictionary<string, string> rootElementAttributes, string schemaLoc)
        {
            var schemaElement = XsdUtils.GetRootElement(schema)
                ?? throw new Exception($"Ingen rotelement i {schema.TargetNamespace}");
            var rootDocumentElement = doc.ChildNodes[1] as XmlElement
            ?? throw new Exception($"Rotelement '{schemaElement.Name}' ikke tilstede i dokument av typen '{schemaElement.QualifiedName.Namespace}'");


            foreach (var attr in rootElementAttributes)
            {
                rootDocumentElement.SetAttribute(attr.Key, attr.Value);
            }

            // schemalocation må legges til etter rotelement har xsi namespace for kunne få xsi prefix
            rootDocumentElement.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", schema.TargetNamespace + " " + schemaLoc);
        }

        public static XmlDocument WriteXml(Dictionary<string, string> values, out Dictionary<string, string> nmsps)
        {
            nmsps = [];

            XmlDocument doc = new();
            var schemaNmsp = WebUtils.GetRequestValue(values, Konstanter.SelectedSkjema);

            var schema = XsdUtils.SchemaFromTargetNmsp(schemaNmsp)
                ?? throw new Exception($"Finner ikke XSD med nmsp = '{schemaNmsp}'");

            PreWriteXml(doc, schema, out nmsps, out Dictionary<string, string> rootElementAttributes, out string schemaLoc);

            var schemaElement = XsdUtils.GetRootElement(schema)
                ?? throw new Exception($"Ingen rotelement i {schema.TargetNamespace}");


            WriteElement(true, doc, null, "", schemaElement, nmsps, schemaElement.QualifiedName.Namespace, values);

            PostWriteXml(doc, schema, rootElementAttributes, schemaLoc);
            return doc;

        }

        public static XmlDocument WriteAppRec(string schemaNmsp, Meldingshode meldingshode)
        {
            XmlDocument doc = new();
            var schema = XsdUtils.SchemaFromTargetNmsp(schemaNmsp)
                ?? throw new Exception($"Finner ikke XSD med nmsp = '{schemaNmsp}'");
            PreWriteXml(doc, schema, out Dictionary<string, string> nmsps, out Dictionary<string, string> rootAttr, out string schemaLoc);
            var values = MeldingshodeToDictionary(meldingshode);
            var schemaElement = XsdUtils.GetRootElement(schema)
                ?? throw new Exception($"Ingen rotelement i {schema.TargetNamespace}");
            WriteElement(true, doc, null, "", schemaElement, nmsps, schemaElement.QualifiedName.Namespace, values);

            PostWriteXml(doc, schema, rootAttr, schemaLoc);
            return doc;
        }

        public static Dictionary<string, string> MeldingshodeToDictionary(Meldingshode meldingshode)
        {
            Dictionary<string, string> values = [];
            WebUtils.ObjectToDictionary(meldingshode, ref values);
            return values;
        }

        public static bool WriteElement(bool useNmsp, XmlDocument xmlDoc, XmlElement? parent, string xmlPath, XmlSchemaAnnotated elementType,
                                        Dictionary<string, string> ns, string defaultNmsp, Dictionary<string, string> values, string index = "")
        {
            bool result = false;
            if (XsdUtils.GetIsRepeating(elementType) && index == "")
            {
                string[] indexValues = GetIndexValues(values, xmlPath.TrimStart('.'), elementType);
                foreach (var indexValue in indexValues)
                {
                    if (WriteElement(useNmsp, xmlDoc, parent, xmlPath, elementType, ns, defaultNmsp, values, indexValue))
                    {
                        result = true;
                    }
                }
            }
            else
            {
                string indexPart = index == "" ? "" : $":{index}";
                var newPath = $"{xmlPath.Trim('.')}.{XsdUtils.GetName(elementType).TrimStart('.')}{indexPart}";
                if (elementType is XmlSchemaChoice choiceElement)
                {
                    string elementName = $"{xmlPath.Trim('.')}.{XsdUtils.GetChoiceElementNames(choiceElement)}_CHOICE";
                    if (values.TryGetValue(elementName, out string? chosen))
                    {
                        foreach (var item in choiceElement.Items.Cast<XmlSchemaElement>())
                        {
                            if (item.Name == chosen)
                            {
                                if (WriteElement(useNmsp, xmlDoc, parent, newPath, item, ns, defaultNmsp, values))
                                {
                                    result = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    string nmsp = XsdUtils.GetQualifiedNamespace(elementType);
                    var elementPrefix = useNmsp ? ns[nmsp] : "";
                    var elementNmsp = useNmsp ? nmsp : defaultNmsp;
                    XmlElement element = xmlDoc.CreateElement(elementPrefix, XsdUtils.GetName(elementType), elementNmsp);

                    if (XsdUtils.GetComplexType(elementType) != null)
                    {
                        var complexType = XsdUtils.GetComplexType(elementType);
                        foreach (var child in XsdUtils.GetXsdChildren(complexType!))
                        {
                            if (WriteElement(useNmsp, xmlDoc, element, newPath, child, ns, defaultNmsp, values))
                            {
                                result = true;
                            }
                        }
                    }
                    else // is SimpleType
                    {
                        string input;

                        if (GetIsBoolProperty(elementType) && values.ContainsKey($"{newPath}_FALSE") && !values.ContainsKey(newPath))
                        {
                            element.InnerText = "false";
                            result = true;
                        }

                        else if (values.TryGetValue(newPath, out string? val))
                        {
                            input = val;
                            if (input != "")
                            {
                                element.InnerText = input == "on" ? "true" : input;
                                result = true;
                            }
                        }
                    }
                    if (result)
                    {
                        if (parent != null)
                            parent.AppendChild(element);
                        else
                            xmlDoc.AppendChild(element);
                    }
                }
            }
            return result;
        }

        // Bør sjekks, er ikke testet etter utledning fra Demo.Utils.GetControlNameFromProperty()
        public static bool GetIsBoolProperty(XmlSchemaAnnotated prop)
        {
            XmlSchemaDatatype? datatype = null;
            //            var simpleType = XsdUtils.GetSimpleType(prop);
            if (prop is XmlSchemaElement element)
            {
                datatype = element.ElementSchemaType?.Datatype;
            }

            if (datatype?.TypeCode == XmlTypeCode.Boolean)
                return true;
            return false;
        }

        private static string[] GetIndexValues(Dictionary<string, string> values, string xmlPath, XmlSchemaAnnotated elementType)
        {
            List<string> result = [];
            string elementPath = $"{xmlPath}.{XsdUtils.GetName(elementType)}:";
            var selection = values.Where(kvp => kvp.Key.StartsWith(elementPath));
            foreach (var kvp in selection)
            {
                string key = kvp.Key.Replace(elementPath, "");
                int endIndex = key.IndexOf('.');
                key = endIndex > 0 ? key[0..endIndex] : key;
                if (key != "" && !result.Contains(key))
                    result.Add(key);
            }
            return [.. result];
        }

        public static void ReadMeldingshodeElement(XmlReader reader, object current, string localName)
        {
            while (reader.Read())
            {
                if (reader.LocalName == localName) // elementets slutt-tag
                {
                    break;
                }

                var propInfo = current.GetType().GetProperty(reader.LocalName);
                if (propInfo != null)
                {
                    if (!propInfo.PropertyType.FullName!.StartsWith("System."))
                    {
                        var childObj = propInfo.GetValue(current);
                        if (childObj == null)
                        {
                            Type objPropType = propInfo.PropertyType.GenericTypeArguments.Length == 0 ? propInfo.PropertyType : propInfo.PropertyType.GenericTypeArguments[0];
                            childObj = Activator.CreateInstance(objPropType)!;
                            propInfo.SetValue(current, childObj);
                        }
                        ReadMeldingshodeElement(reader, childObj, reader.LocalName);
                    }
                    else
                    {
                        var text = reader.ReadString();
                        if (text != "")
                        {
                            if (propInfo.PropertyType == typeof(DateTime) || propInfo.PropertyType.GenericTypeArguments.Length == 1 && propInfo.PropertyType.GenericTypeArguments[0] == typeof(DateTime))
                            {
                                if (DateTime.TryParseExact(text.Trim(), XmlUtils.DateTimeFormatXML, null, System.Globalization.DateTimeStyles.None, out DateTime dtResult))
                                    propInfo.SetValue(current, dtResult);
                            }
                            else
                                propInfo.SetValue(current, text);
                        }
                    }
                }
            }

        }

        public static List<PrefilledValue> ReadXMLFile(XmlSchema schema, Stream stream)
        {
            var schemaElement = XsdUtils.GetRootElement(schema)
                ?? throw new Exception($"Schema med nmsp '{schema?.TargetNamespace ?? "<ukjent>"}' ikke funnet");

            List<PrefilledValue> result = [];
            var doc = new XmlDocument();
            doc.Load(stream);
            ReadElement(doc, null, "", schemaElement, result);
            return result;
        }

        public static List<PrefilledValue> ReadXMLFile(XmlDocument? doc, XmlSchema schema)
        {
            if (doc == null)
                throw new Exception($"XmlDocument == null i'{nameof(ReadXMLFile)}'");
            var schemaElement = XsdUtils.GetRootElement(schema)
                ?? throw new Exception($"Schema med nmsp '{schema?.TargetNamespace ?? "<ukjent>"}' ikke funnet");

            List<PrefilledValue> result = [];
            ReadElement(doc, null, "", schemaElement, result);
            return result;
        }

        public static void ReadXMLFile(XmlDocument doc, XmlSchema schema, ref Dictionary<string, string> queryParams)
        {
            var fileContents = ReadXMLFile(doc, schema);
            foreach (var rec in fileContents)
            {
                if (!queryParams.ContainsKey(rec.Xpath))
                    queryParams.Add(rec.Xpath, rec.Value);
            }
        }

        public static void ReadXMLFile(XmlSchema schema, Stream stream, ref Dictionary<string, string> queryParams)
        {
            var fileContents = ReadXMLFile(schema, stream);
            foreach (var rec in fileContents)
            {
                if (!queryParams.ContainsKey(rec.Xpath))
                    queryParams.Add(rec.Xpath, rec.Value);
            }
        }


        public static List<XmlElement> GetChildrenOfName(XmlElement? element, string name)
        {
            // 
            List<XmlElement> result = [];
            if (element != null)
            {
                foreach (XmlElement child in element.ChildNodes)
                {
                    if (child.LocalName == name)
                        result.Add(child);
                }
            }
            return result;
        }

        public static void ReadElement(XmlDocument xmlDoc, XmlElement? parent, string xmlPath, XmlSchemaAnnotated elementType, List<PrefilledValue> values, int index = -1)
        {

            if (XsdUtils.GetIsRepeating(elementType) && index < 0) // repeterende elementer
            {
                //                string nmsp = XsdUtils.GetQualifiedNamespace(elementType);
                string elementName = XsdUtils.GetName(elementType);
                var elements = GetChildrenOfName(parent, elementName);

                for (int i = 0; i < elements.Count; i++)
                {
                    ReadElement(xmlDoc, parent, xmlPath, elementType, values, i + 1);
                }
            }

            else
            {
                string indexPart = "";
                if (index > 0)
                {
                    if (XsdUtils.HasUniqueConstraint(elementType))
                    {
                        var enumType = XsdUtils.GetIterateTypeDefinition(elementType)
                            ?? throw new Exception($"Finner ikke enumType '{XsdUtils.GetName(elementType)}'");

                        var kodeliste = KodelisteUtils.GetKodeliste(enumType)!;
                        if (kodeliste?.koder?.Length < index)
                            throw new Exception($"Finner ikke kode i kodeliste for enumType '{enumType.Name}'");
                        indexPart = $":{kodeliste!.koder![index - 1].verdi!.Replace(".", "-")}";
                    }
                    else
                        indexPart = $":{index}";
                }
                var newPath = (xmlPath.TrimEnd('.') + "." + XsdUtils.GetName(elementType)).TrimStart('.') + indexPart;
                if (elementType is XmlSchemaChoice choiceElement)
                {
                    foreach (var item in choiceElement.Items.Cast<XmlSchemaElement>())
                    {
                        ReadElement(xmlDoc, parent, xmlPath, item, values);
                    }
                }

                else
                {
                    string nmsp = XsdUtils.GetQualifiedNamespace(elementType);
                    string elementname = XsdUtils.GetName(elementType);
                    XmlElement? element;

                    if (parent == null) // rotelement
                    {
                        element = XmlUtils.GetRootElement(xmlDoc);
                    }
                    else if (index > 0)
                    {
                        var elements = GetChildrenOfName(parent, elementname);
                        element = elements[index - 1];
                    }
                    else
                    {
                        element = parent[elementname, nmsp];

                    }

                    if (XsdUtils.GetComplexType(elementType) != null && element != null)
                    {
                        var complexType = XsdUtils.GetComplexType(elementType);
                        foreach (var child in XsdUtils.GetXsdChildren(complexType!))
                        {

                            ReadElement(xmlDoc, element, newPath, child, values);
                        }
                    }
                    else if (element != null) // simple types
                    {
                        var text = element.InnerText;
                        if (text != "" && text != "---" /*var brukt som null-verdi*/) // if element has value in xmldoc
                        {
                            values.Add(new(newPath, text, true, false));
                        }
                    }
                }
            }

        }

        internal static object CompareMeldingOrder(Dictionary<string, string> hode1, Dictionary<string, string> hode2)
        {
            var tid1 = hode1["mld:SendtTidspunkt"];
            var tid2 = hode2["mld:SendtTidspunkt"];
            return tid1.CompareTo(tid2);
        }
    }
}
