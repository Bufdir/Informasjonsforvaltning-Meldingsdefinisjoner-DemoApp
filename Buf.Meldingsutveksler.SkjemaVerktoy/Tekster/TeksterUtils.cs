using Buf.Meldingsutveksler.SkjemaVerktoy.FilSystem;
using Buf.Meldingsutveksler.SkjemaVerktoy.Tekster.Models;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml.Models;
using System.Text.Json;
using System.Xml.Schema;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.Tekster;
public static class TeksterUtils
{
    public static readonly string TEKSTER_DIRECTORY = "tekster";
    public static readonly string OVERSETTELSER_DIRECTORY = "oversettelse";
    public static readonly string STANDARD_SPRAK = "nb-NO";
    public static List<TekstFil> TekstFiler { get; internal set; } = [];
    public static List<OversettelseFil> OversettelseFiler { get; internal set; } = [];

    public static void Init(IFileSystem fileSystem)
    {
        var files = fileSystem.ListFiles(TEKSTER_DIRECTORY + "/").Where(f => !f.Contains(".schema."));
        foreach (var file in files)
        {
            using var reader = fileSystem.GetStream(file);
            var tekster = JsonSerializer.Deserialize<TekstFil>(reader)
                ?? throw new Exception($"Kunne ikke laste tekster (filnavn: {file})");
            tekster.FileName = file;
            TekstFiler.Add(tekster);
        }

        var oversettelsefiles = fileSystem.ListFiles(OVERSETTELSER_DIRECTORY + "/").Where(f => !f.Contains(".schema."));
        foreach (var file in oversettelsefiles)
        {
            using var reader = fileSystem.GetStream(file);
            var oversettelser = JsonSerializer.Deserialize<OversettelseFil>(reader)
                ?? throw new Exception($"Kunne ikke laste oversettelser (filnavn: {file})");
            oversettelser.FileName = file;
            OversettelseFiler.Add(oversettelser);
        }
        InitSprak();

    }

    public static void InitSprak()
    {
        SetFallbackLanguage();
        foreach (var oversettelse in OversettelseFiler)
        {
            var tekstFil = TekstFiler.FirstOrDefault(f => f.FileName == TEKSTER_DIRECTORY + "/" + oversettelse.fil)
                ?? throw new Exception($"Kunne ikke finne fil {oversettelse.fil} som oversettelse {oversettelse.FileName} refererer til");
            foreach (var element in oversettelse.skjemaelement)
            {
                var tekstelement = tekstFil.skjema?.skjemaelement.FirstOrDefault(e => e.id == element.id)
                    ?? throw new Exception($"Kunne ikke finne element med id={element.id} i tekstfil {tekstFil.FileName}");
                AddOversettelse(tekstelement.ledetekst, oversettelse.sprak, element.ledetekst);
            }
        }
    }

    private static void SetFallbackLanguage()
    {
        foreach (var tekster in TekstFiler)
        {
            if (tekster.skjema == null)
                return;
            foreach (var skjemaElement in tekster.skjema.skjemaelement)
            {
                SetFallback(skjemaElement.ledetekst.FirstOrDefault());
                SetFallback(skjemaElement.veiledning.FirstOrDefault());
            }
            foreach (var kodelisteElement in tekster.skjema.kodelistetekster)
            {
                foreach (var kode in kodelisteElement.verdier)
                {
                    SetFallback(kode.tekst.FirstOrDefault());
                    SetFallback(kode.beskrivelse.FirstOrDefault());
                }
            }
        }
    }

    private static void SetFallback(SprakElement? sprakElement)
    {
        if (sprakElement != null)
            sprakElement.fallback = true;
    }

    private static void AddOversettelse(List<SprakElement> element, string sprak, string tekst)
    {
        element.Add(new()
        {
            sprak = sprak,
            tekst = tekst
        });
    }

    public static Skjemaelement? GetTekstElement(XmlSchemaRec registerRec, XmlSchemaAnnotated prop)
    {
        foreach (var tekster in registerRec.Tekster)
        {
            var element = tekster.skjema?.skjemaelement.Where(el => el.id == prop.Id).FirstOrDefault();
            if (element != null)
                return element;
        }
        return null;
    }

    public static Skjemaelement? GetTekstElement(XmlSchema schema, XmlSchemaAnnotated prop)
    {
        XmlSchemaRec registerRec = XmlSchemaRegister.Schemas.xsds.FirstOrDefault(r => r.nmsp == schema.TargetNamespace)
            ?? throw new Exception($"Finner ikke sXsd med TargetNamespace = {schema.TargetNamespace}");
        return GetTekstElement(registerRec, prop);
    }

    public static string GetCaption(XmlSchema? schema, XmlSchemaAnnotated? prop, bool fallbackToName, string sprak = "nb-NO")
    {
        if (prop == null) return
                "<Prop=null>";
        if (schema == null) return
                "<schema=null>";
        var element = GetTekstElement(schema, prop);
        if (element != null)
            return GetTekstForSprak(element.ledetekst, sprak);
        if (fallbackToName)
        {
            return XsdUtils.GetName(prop);
        }
        return "";
    }

    public static string GetDescription(XmlSchema? schema, XmlSchemaAnnotated prop, string sprak = "nb-NO")
    {
        if (schema == null)
            return "";
        var element = GetTekstElement(schema, prop);
        if (element?.veiledning != null)
        {
            return GetTekstForSprak(element.veiledning, sprak);
        }
        return "";
    }

    public static List<EnrichedElement> GetEnrichedChildren(XmlSchema? schema, XmlSchemaAnnotated prop)
    {
        XmlSchemaRec registerRec = XmlSchemaRegister.Schemas.xsds.FirstOrDefault(r => r.nmsp == schema?.TargetNamespace)
            ?? throw new Exception($"Finner ikke sXsd med TargetNamespace = {schema?.TargetNamespace ?? "schema == null"}");
        List<EnrichedElement> result = [];
        var elements = XsdUtils.GetXsdChildren(prop);
        for (int i = 0; i < elements.Count; i++)
        {
            var element = GetTekstElement(registerRec, elements[i]);
            EnrichedElement enriched = new(elements[i])
            {
                OrigSort = i,
                SortOrder = element?.sortering ?? 100
            };
            result.Add(enriched);
        }
        return [.. result.OrderBy(e => e.SortOrder).ThenBy(e => e.OrigSort)];
    }

    public static string GetTekstForSprak(IEnumerable<SprakElement>? elementArray, string SprakKode)
    {
        if (elementArray == null)
            return "";
        var element = elementArray.FirstOrDefault(e => e.sprak == SprakKode);
        if (element == null)
        {
            element = elementArray.FirstOrDefault(e => e.fallback == true);
            if (element == null)
                return "";
            return element.tekst;
        }
        return element.tekst;
    }


    public static string GetKodelisteBeskrivelseFromVeiledning(XmlSchemaRec registerRec, string kodeliste_id, string verdi, string sprak = "nb-NO")
    {
        //        var kodelisteTekster = GetKodelisteTekster
        foreach (var tekstFil in registerRec.Tekster)
        {
            var tekstelement = tekstFil.skjema?.kodelistetekster.Where(el => el.id == kodeliste_id).FirstOrDefault();
            if (tekstelement != null)
            {
                var kodelisteverdi = tekstelement.verdier.Where(kv => kv.verdi == verdi).FirstOrDefault();
                if (kodelisteverdi != null)
                    return GetTekstForSprak(kodelisteverdi.beskrivelse, sprak);
            }
        }
        return "";
    }

    public static List<string> GetAvailableSprak(XmlSchema? schema)
    {
        List<string> result = [];
        result.Add(STANDARD_SPRAK);
        var schemaRec = XmlSchemaRegister.Schemas.xsds.FirstOrDefault(xsd => xsd.nmsp == schema?.TargetNamespace);
        if (schemaRec != null)
        {
            foreach (var tekstfil in schemaRec.Tekster)
            {
                var oversettelser = OversettelseFiler.Where(o => TEKSTER_DIRECTORY + "/" + o.fil == tekstfil.FileName);
                foreach (var oversettelse in oversettelser)
                {
                    result.Add(oversettelse.sprak);
                }
            }
        }
        return result;
    }
}
