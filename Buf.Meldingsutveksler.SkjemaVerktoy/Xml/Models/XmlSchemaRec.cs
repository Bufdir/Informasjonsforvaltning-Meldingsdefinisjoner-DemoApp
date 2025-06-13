using Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste.Models;
using Buf.Meldingsutveksler.SkjemaVerktoy.Tekster.Models;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace Buf.Meldingsutveksler.SkjemaVerktoy.Xml.Models;

[Serializable]
[XmlType(AnonymousType = true)]
public class XmlSchemaRec
{
    [XmlAttribute("nmsp")]
    public string nmsp { get; set; } = "";

    [JsonIgnore]
    [XmlAttribute]
    public string navn { get; set; } = "";

    [XmlAttribute("beskrivelse")]
    public string beskrivelse { get; set; } = "";

    [XmlAttribute("rotelement")]
    public string rotelement { get; set; } = "";

    [XmlAttribute("status")]
    public string status { get; set; } = "";

    [XmlAttribute("gyldigFra")]
    public string _gyldigFra { get; set; } = "";

    [XmlAttribute("gyldigTil")]
    public string _gyldigTil { get; set; } = "";

    [XmlAttribute("kodelister")]
    public string _kodelister { get; set; } = "";

    [XmlAttribute("tekster")]
    public string _tekster { get; set; } = "";

    public DateTime? gyldigFra
    {
        get
        {
            if (_gyldigFra != "")
                return DateTime.ParseExact(_gyldigFra, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            return null;
        }
    }

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

    public string XsdFilnavn { get; set; } = "";

}
