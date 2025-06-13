using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using Demo.Fagsystem.Models.ViewModels;
using System.Text.Json;

namespace Demo.Fagsystem.Models.Utils;
public static class WebUtils
{
    public static void GetRequestParams(HttpRequest req, out Dictionary<string, string> queryParams)
    {
        queryParams = [];
        foreach (var kvp in req.Query)
        {
            queryParams[kvp.Key] = kvp!.Value.ToString();
        }
        if (req.HasFormContentType)
        {
            foreach (var kvp in req.Form)
            {
                queryParams[kvp.Key] = kvp!.Value.ToString();
            }
        }
    }

    public static string GetRequestValue(Dictionary<string, string> RequestParams, string param)
    {
        if (RequestParams.TryGetValue(param, out var value))
            return value;
        return "";
    }
    public static bool ExistsRequestValue(Dictionary<string, string> RequestParams, string param)
    {
        return RequestParams.TryGetValue(param, out var _);
    }

    public static string GetRequestValue(HttpRequest req, string param)
    {
        GetRequestParams(req, out Dictionary<string, string> RequestParams);
        if (RequestParams == null)
            return "";
        return GetRequestValue(RequestParams, param);
    }

    /// <summary>
    /// Gets the value of the first key ending with the parameter 'partialKey'
    /// </summary>
    public static string GetRequestValuePartialKey(Dictionary<string, string> RequestParams, string partialKey)
    {
        foreach (var kvp in RequestParams)
        {
            if (kvp.Key.EndsWith(partialKey))
                return kvp.Value;
        }
        return "";
    }

    public static string Reformat(string dateTime, string currentFormat, string newFormat)
    {
        if (DateTime.TryParseExact(dateTime, currentFormat, null, System.Globalization.DateTimeStyles.None, out DateTime result))
            return result.ToString(newFormat);
        return "";

    }

    public static string getMandatoryMarker(PropertyRendererModel model, bool disable = false, bool overrideModel = false)
    {
        bool returnMarker = overrideModel;
        if (!returnMarker)
        {
            if (XsdUtils.IsSimpleType(model.Prop) && model.Mandatory || XsdUtils.AllChoiceElementsSimpleAndMandatory(model.Prop))
                returnMarker = true;
            else if (XsdUtils.ChoiceElementMandatory(model.Prop))
                returnMarker = true;
        }
        string disableCssClass = disable ? "disabled" : "";
        return returnMarker ? $"<span class='obligatorisk_markor {disableCssClass}'>*</span>" : "";
    }

    private const int MIN_LENGDE_FOR_A_VISE_MIN_MARKOR = 10;

    public static string getMinLengthMarker(PropertyRendererModel model)
    {
        int minlength = XsdUtils.GetMinLength(model.Prop);
        return minlength > MIN_LENGDE_FOR_A_VISE_MIN_MARKOR ? $"<span class='minlength_markor'>minst {minlength} tegn</span>" : "";
    }

    /// <summary>
    /// Perform a deep copy of the object via serialization.
    /// </summary>
    /// <typeparam name="T">The type of object being copied.</typeparam>
    /// <param name="source">The object instance to copy.</param>
    /// <returns>A deep copy of the object.</returns>
    public static T? Clone<T>(this T source)
    {
        // Don't serialize a null object, simply return the default for that object
        if (ReferenceEquals(source, null)) return default;
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<T>(json);
    }

    internal static void ObjectToDictionary(object obj, ref Dictionary<string, string> values, string path = "")
    {
        var propInfos = obj.GetType().GetProperties();
        foreach (var propInfo in propInfos)
        {
            string propName = propInfo.Name;
            string propPath = $"{path}.{propName}".Trim('.');
            if (!propInfo.PropertyType.FullName!.StartsWith("System."))
            {
                var childObj = propInfo.GetValue(obj);
                if (childObj != null)
                {
                    ObjectToDictionary(childObj, ref values, propPath);
                }
            }
            else
            {
                var value = propInfo.GetValue(obj);
                if (value != null)
                {
                    if (propInfo.PropertyType == typeof(DateTime) || propInfo.PropertyType.GenericTypeArguments.Length == 1 && propInfo.PropertyType.GenericTypeArguments[0] == typeof(DateTime))
                    {
                        if (DateTime.TryParseExact(value.ToString(), XmlUtils.DateTimeFormatXML, null, System.Globalization.DateTimeStyles.None, out DateTime dtResult))
                            values[propPath] = dtResult.ToString();
                    }
                    else
                        values[propPath] = value.ToString() ?? "";
                }
            }

        }
    }
}

