using Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste.Models;
using Buf.Meldingsutveksler.SkjemaVerktoy.FilSystem;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Schema;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste;

public static class KodelisteUtils
{
    public static string KODELISTER_DIRECTORY { get; } = "kodelister";
    public static List<Kodelister> kodelister { get; set; } = [];
    public static void Clear()
    {
        kodelister.Clear();
    }

    /*public static void InitXml(IFileSystem fileSystem)
    {
        var serializer = new XmlSerializer(typeof(Kodelister));
        var fileNames = fileSystem.ListFiles(KODELISTER_DIRECTORY).Where(f => f.EndsWith(".xml"));
        foreach (var fileName in fileNames)
        {
            using var reader = fileSystem.GetStream(fileName);
            var innhold = serializer.Deserialize(reader) as Kodelister
                ?? throw new Exception($"Kunne ikke lese innhold i kodelistefil {fileName}");
            innhold.Filnavn = fileName;
            kodelister.Add(innhold);
        }
        //ConvertToJson(fileSystem);
        //InitKodelister();
    }
*/
    public static void Init(IFileSystem fileSystem)
    {
        var fileNames = fileSystem.ListFiles(KODELISTER_DIRECTORY).Where(f => f.EndsWith(".json"));
        foreach (var fileName in fileNames)
        {
            if (!fileName.Contains(".schema."))
            {
                using var reader = fileSystem.GetStream(fileName);
                var innhold = JsonSerializer.Deserialize<Kodelister>(reader)
                    ?? throw new Exception($"Kunne ikke lese innhold i kodelistefil {fileName}");
                innhold.Filnavn = fileName;
                kodelister.Add(innhold);
            }
        }
        //        ConvertToJson(fileSystem);
        //InitKodelister();
    }

    /*private static void InitKodelister()
    {
        if (kodelister.Count == 0)
            throw new Exception("Kodelister er ikke lastet");
        foreach (Kodelister lister in kodelister)
        {
            foreach (Kodeliste liste in lister.kodelister ?? [])
            {
                if (!string.IsNullOrEmpty(liste.utdragfra))
                {
                    var kildeListe = GetKildeKodeliste(lister.nmsp ?? "", liste.utdragfra);
                    if (liste.koder != null)
                    {
                        foreach (var kode in liste.koder)
                        {
                            var kildeKode = (kildeListe?.koder ?? []).FirstOrDefault(k => k.verdi == kode.verdi)
                                ?? throw new Exception($"Finner ikke kode {kode.verdi} i kildeliste {kildeListe.navn}");
                            kode.tekst = kildeKode.tekst;
                        }
                    }
                }
            }
        }
    }*/

    /*        public static Kodeliste? GetKildeKodeliste(string nmsp, string kilde, int level = 0)
            {
                if (level > 10)
                {
                    throw new Exception($"Søk etter kildeliste {kilde} går i loop");
                }
                Kodeliste? liste = null;
                foreach (Kodelister kodelisterXsd in kodelister)
                {
                    if (nmsp == kodelisterXsd.nmsp)
                    {
                        liste = (kodelisterXsd.kodelister ?? []).FirstOrDefault(k => k.navn == kilde);
                        if (!string.IsNullOrEmpty(liste?.utdragfra))
                            liste = GetKildeKodeliste(nmsp, liste.utdragfra, level + 1);
                        if (liste == null)
                            throw new Exception($"Finner ikke kildeliste {kilde}");
                    }
                }
                return liste;
            }
    */

    public static void ConvertToJson(IFileSystem fileSystem)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        foreach (var kodeliste in kodelister)
        {
            var kodelisteJSON = JsonSerializer.Serialize(kodeliste, options);
            string filnavn = kodeliste.Filnavn.Replace(".xml", ".json");
            fileSystem.Save(filnavn, kodelisteJSON, true);
        }
    }
    public static Kodeliste? GetKodeliste(XmlSchemaAnnotated prop)
    {
        var simpleType = XsdUtils.GetSimpleType(prop)
            ?? throw new Exception($"Finner ikke type {prop}");
        XmlSchema schema = XsdUtils.GetSchema(prop);
        Kodeliste? kodeliste = GetKodeliste(schema, simpleType.Id!);
        if (kodeliste == null) // En restricted utgave av enumeration
        {
            var ancestorSimpleType = XsdUtils.GetSimpleType(simpleType.BaseXmlSchemaType);
            if (XsdUtils.GetIsEnumType(ancestorSimpleType))
            {
                schema = XsdUtils.GetSchema(ancestorSimpleType!);
                kodeliste = GetKodeliste(schema, ancestorSimpleType!.Id!);
                if (kodeliste != null)
                {
                    var kodelisteCopyJSON = JsonSerializer.Serialize(kodeliste);
                    kodeliste = JsonSerializer.Deserialize<Kodeliste>(kodelisteCopyJSON)!;

                    List<Kode> filtered = [];
                    var values = XsdUtils.GetEnumValues(prop);

                    foreach (var kode in kodeliste.koder!)
                    {
                        if (values.Exists(kvp => kvp.Key == kode.verdi))
                        {
                            filtered.Add(kode);
                        }
                    }
                    kodeliste.koder = [.. filtered];
                }
            }
        }
        return kodeliste;
    }

    public static List<Kode> GetFilteredKodeliste(Kodeliste kodeliste, string variabel, string variabelVerdi)
    {
        List<Kode> result = [];
        if (kodeliste.variabler?.Length > 0 && kodeliste.variabler.Any(v => v.navn == variabel))
        {
            foreach (var kode in kodeliste.koder!)
            {
                if (kode.variabler.Any(v => v.navn == variabel && v.verdi == variabelVerdi))
                    result.Add(kode);
            }
            if (result.Count == 0)
                throw new Exception($"Tom kodefilterliste '{kodeliste.navn}' filtrert på {variabel}={variabelVerdi}");
        }
        else
            result = [.. kodeliste.koder!];
        return result;
    }

    public static List<KeyValuePair<string, string>> GetKodelisteVerdier(XmlSchemaAnnotated prop)
    {
        List<KeyValuePair<string, string>> result = [];
        var simpleType = XsdUtils.GetSimpleType(prop)
            ?? throw new Exception($"Finner ikke type {prop}");
        XmlSchema schema = XsdUtils.GetSchema(prop);
        var kodeliste = GetKodeliste(schema, simpleType.Id!);
        if (kodeliste?.koder != null)
        {
            foreach (var kode in kodeliste.koder)
            {
                if (!string.IsNullOrEmpty(kode.verdi) && !string.IsNullOrEmpty(kode.tekst))
                    result.Add(new(kode.verdi, kode.tekst));
            }
            return result;
        }
        else
            return XsdUtils.GetEnumValues(prop);
    }

    public static KodelisteRestricted? GetKodelisteRestricted(XmlSchemaAnnotated prop)
    {
        KodelisteRestricted? result = null;
        var simpleType = XsdUtils.GetSimpleType(prop)
            ?? throw new Exception($"Finner ikke type {prop}");
        XmlSchema schema = XsdUtils.GetSchema(prop);
        Kodeliste? kodeliste = GetKodeliste(schema, simpleType.Id!);
        var values = XsdUtils.GetEnumValues(prop);
        if (kodeliste == null) // En restricted utgave av enumeration
        {
            var ancestorSimpleType = XsdUtils.GetSimpleType(simpleType.BaseXmlSchemaType);
            if (XsdUtils.GetIsEnumType(ancestorSimpleType))
            {
                schema = XsdUtils.GetSchema(ancestorSimpleType!);
                kodeliste = GetKodeliste(schema, ancestorSimpleType!.Id!);
                if (kodeliste != null)
                {
                    result = CloneTo<Kodeliste, KodelisteRestricted>(kodeliste)
                        ?? throw new Exception("Kunne ikke filtrere kodeliste-utvalg i CloneTo<>()");

                }
            }
        }
        else
        {
            result = CloneTo<Kodeliste, KodelisteRestricted>(kodeliste);
        }
        if (result != null)
        {
            foreach (var kode in result.koder)
            {
                if (values.Exists(kvp => kvp.Key == kode.verdi))
                {
                    kode.Included = true;
                }
            }
        }
        return result;
    }

    public static Kodeliste? GetKodeliste(XmlSchema schema, string id)
    {
        var schemaRec = XmlSchemaRegister.Schemas.xsds.FirstOrDefault(s => s.nmsp == schema.TargetNamespace);
        var result = schemaRec?.Kodelister?.kodelister?.FirstOrDefault(k => k.id == id);
        return result;
    }

    public static U? CloneTo<T, U>(this T source)
    {
        // Don't serialize a null object, simply return the default for that object
        if (ReferenceEquals(source, null)) return default;
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<U>(json);
    }


}