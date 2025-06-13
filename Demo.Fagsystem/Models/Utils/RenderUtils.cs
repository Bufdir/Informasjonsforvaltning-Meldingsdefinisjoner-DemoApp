using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using System.Xml.Schema;

namespace Demo.Fagsystem.Models.Utils
{
    public static class RenderUtils
    {
        public static Dictionary<string, string> CustomRenderers { get; set; } = [];

        public static string GetControlNameForProperty(XmlSchemaAnnotated prop, bool isAChoiceOption = false)
        {
            if (prop.Id != null && CustomRenderers.TryGetValue(prop.Id, out string? customRenderer))
                return customRenderer;
            string controlName;
            XmlSchemaDatatype? datatype = null;
            var simpleType = XsdUtils.GetSimpleType(prop);
            if (XsdUtils.GetIsRepeating(prop))
            {
                if (XsdUtils.HasUniqueConstraint(prop))
                    return "Iterator";
                return "Duplicator";
            }
            else if (prop is XmlSchemaChoice)
            {
                return "Choice";
            }
            else if (prop is XmlSchemaElement element)
            {
                if (element.ElementSchemaType is XmlSchemaComplexType)
                {
                    if (XsdUtils.IsChoiceElement(element))
                        return "Choice";
                    if (XsdUtils.IsOptionalBlock(element))
                        return "OptionalBlock";
                    return "Gruppe";
                }
                if (XsdUtils.GetIsEnumType(simpleType))
                {
                    return "SelectOne";
                }
                if (element?.ElementSchemaType?.Datatype == null)
                    throw new Exception("Datatype == null");
                datatype = element.ElementSchemaType.Datatype;
            }
            else if (prop is XmlSchemaAttribute)
            {
                datatype = simpleType?.Datatype ??
                    throw new Exception("simpleType?.Datatype == null");
            }

            if (isAChoiceOption && datatype?.TypeCode == XmlTypeCode.Boolean)
                return "FixedBoolean";

            controlName = datatype?.TypeCode switch
            {
                XmlTypeCode.Boolean => "Checkbox",
                XmlTypeCode.Date => "Dato",
                _ => "Tekst"
            };
            return controlName;
        }

        internal static bool AllElementsEqual(string[] array)
        {
            if (array.Length == 0)
                return true;
            for (int i = 1; i < array.Length; i++)
            {
                if (array[0] != array[i])
                    return false;
            }
            return true;
        }

        internal static void SetCustomRenderers()
        {
            CustomRenderers["BUF_CC9A7CDF-B998-415D-995A-6275CBCCA465"] = "ElementSpecific/BarnetsSituasjon";
        }

    }
}