using Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste.Models;
using Buf.Meldingsutveksler.SkjemaVerktoy.Tekster.Models;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml.Schema;


namespace Buf.Meldingsutveksler.SkjemaVerktoy.Xml.Models;

[Serializable]
public class XmlSchemaRec
{
    public string nmsp { get; set; } = "";

    public string beskrivelse { get; set; } = "";

    public string rotelement { get; set; } = "";

    public string status { get; set; } = "";

    [JsonPropertyName("gyldigFra")]
    public string _gyldigFra { get; set; } = "";

    [JsonPropertyName("gyldigTil")]
    public string _gyldigTil { get; set; } = "";

    [JsonPropertyName("kodelister")]
    public string _kodelister { get; set; } = "";

    [JsonPropertyName("tekster")]
    public string _tekster { get; set; } = "";

    [JsonIgnore]
    public DateTime? gyldigFra
    {
        get
        {
            if (_gyldigFra != "")
                return DateTime.ParseExact(_gyldigFra, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            return null;
        }
    }

    [JsonIgnore]
    public DateTime? gyldigTil
    {
        get
        {
            if (_gyldigTil != "")
                return DateTime.ParseExact(_gyldigTil, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            return null;
        }
    }

    [JsonIgnore]
    public XmlSchema? Schema { get; set; }
    [JsonIgnore]
    public XmlSchemaSet? SchemaSet { get; set; }
    [JsonIgnore]
    public List<TekstFil> Tekster { get; set; } = [];
    [JsonIgnore]
    public Kodelister? Kodelister { get; set; }

    [JsonIgnore]
    public string XsdFilnavn { get; set; } = "";

}
