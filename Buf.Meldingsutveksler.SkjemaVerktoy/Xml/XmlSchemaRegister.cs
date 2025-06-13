using Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste;
using Buf.Meldingsutveksler.SkjemaVerktoy.FilSystem;
using Buf.Meldingsutveksler.SkjemaVerktoy.Tekster;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml.Models;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
public static class XmlSchemaRegister
{
    public const string FILENAME_REGISTER = "bufdir.xsd.oversikt.xml";
    public static XmlSchemaRecCollection Schemas { get; set; } = new();

    public static bool Load(IFileSystem fileSystem)
    {
        var serializer = new XmlSerializer(typeof(XmlSchemaRecCollection));
        if (!fileSystem.FileExists(FILENAME_REGISTER))
            return false;
        using var reader = fileSystem.GetStream(FILENAME_REGISTER);

        Schemas = serializer.Deserialize(reader) as XmlSchemaRecCollection
            ?? throw new Exception($"Kunne ikke lese skjemaregister ({FILENAME_REGISTER})");
        //ConvertToJson(fileSystem);
        return true;
    }

    public static void ConvertToJson(IFileSystem fileSystem)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var registerJSON = JsonSerializer.Serialize(Schemas, options);
        string filnavn = FILENAME_REGISTER.Replace(".xml", ".json");
        fileSystem.Save(filnavn, registerJSON, true);
    }

    public static void Init()
    {
        foreach (var item in Schemas.xsds)
        {
            if (item._kodelister != "")
                item.Kodelister = KodelisteUtils.kodelister.FirstOrDefault(k => k.Filnavn == $"{KodelisteUtils.KODELISTER_DIRECTORY}/{item._kodelister}")
                    ?? throw new Exception($"Kodelistefil '{item._kodelister}' ikke funnet");
            if (item._tekster != "")
            {
                string[] tekstFiler = item._tekster.Split(";");
                foreach (var fil in tekstFiler)
                {
                    var teksterObj = TeksterUtils.TekstFiler.FirstOrDefault(k => k.FileName == $"{TeksterUtils.TEKSTER_DIRECTORY}/{fil}")
                        ?? throw new Exception($"Tekstfil {fil} ikke funnet");
                    item.Tekster.Add(teksterObj);
                }
            }
        }
        foreach (var item in Schemas.xsds)
        {
            List<XmlSchema> stack = [];
            XsdUtils.CreateSchemaSet(item, ref stack);
        }
    }

    public static bool IsEmptyDefs()
    {
        return Schemas == null || XmlSchemaRegister.Schemas.xsds?.Count() == 0;
    }

    public static bool IsKnownSchema(string nmsp)
    {
        return Schemas.xsds.FirstOrDefault(s => s.nmsp == nmsp) == null;
    }

    private static bool WithinDates(DateTime? dateFrom, DateTime? dateTo)
    {
        if (dateFrom?.Date > DateTime.Now.Date == true)
            return false;
        if (dateTo?.Date < DateTime.Now.Date == true)
            return false;
        return true;
    }
    public static bool IsSchemaWithinValidPeriod(string nmsp)
    {
        var schema = Schemas.xsds.FirstOrDefault(s => s.nmsp == nmsp)
          ?? throw new Exception($"XmlSchema {nmsp} ikke funnet");
        return WithinDates(schema.gyldigFra, schema.gyldigTil);
    }
    public static bool IsSchemaWithRootElement(string nmsp)
    {
        return Schemas.xsds.FirstOrDefault(s => s.nmsp == nmsp)?.rotelement != "";
    }

    internal static XmlSchemaSet? GetSchemaSet(string? targetNamespace)
    {
        var schemaRec = Schemas.xsds.FirstOrDefault(s => s.nmsp == targetNamespace);
        if (schemaRec == null)
            return null;
        return schemaRec.SchemaSet;
    }
}
