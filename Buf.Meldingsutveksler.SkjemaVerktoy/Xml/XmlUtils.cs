using System.Xml;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.Xml;

public static class XmlUtils
{
    public static readonly string XmlTrue = "true";

    public static readonly string DateTimeFormatXML = "yyyy-MM-ddTHH:mm:ssK";
    public static XmlElement? GetRootElement(XmlDocument xmlDoc)
    {
        return xmlDoc.DocumentElement;
    }

    public static string GetElementValue(XmlElement element, string name)
    {
        if (!element.HasChildNodes)
            return "";
        foreach (var child in element.ChildNodes.Cast<XmlNode>())
        {
            if (child.LocalName == name)
            {
                return child.InnerText;
            }
        }
        return "";
    }

    public static string GetNodeValue(XmlNode node, string name)
    {
        if (!node.HasChildNodes)
            return "";
        foreach (var child in node.ChildNodes.Cast<XmlNode>())
        {
            if (child.LocalName == name)
            {
                return child.InnerText;
            }
        }
        return "";
    }

    public static XmlNode? GetNode(XmlElement? element, string name)
    {
        if (element == null)
            return null;
        if (!element.HasChildNodes == true)
            return null;
        foreach (var child in element.ChildNodes.Cast<XmlNode>())
        {
            if (child.LocalName == name)
            {
                return child;
            }
        }
        return null;
    }

    public static XmlNode? GetNodeChild(XmlNode node, string nodeName)
    {
        if (node.ChildNodes.Count > 0)
        {
            foreach (var child in node.ChildNodes.Cast<XmlNode>())
            {
                if (child.LocalName == nodeName)
                    return child;
            }
        }
        return null;
    }

    public static void ElementToObject(object? obj, XmlElement element)
    {
        if (obj is null)
            return;
        var props = obj.GetType().GetProperties();
        foreach (var prop in props)
        {
            if (prop.PropertyType.FullName!.StartsWith("System."))
            {
                var value = GetElementValue(element, prop.Name);
                prop.SetValue(obj, value, null);
            }
            else
            {
                var Node = GetNode(element, prop.Name);
                if (Node != null)
                {
                    var subObj = Activator.CreateInstance(prop.PropertyType);
                    prop.SetValue(obj, subObj);
                    if (Node is XmlElement el)
                        ElementToObject(subObj, el);
                }
            }
        }
    }

}
