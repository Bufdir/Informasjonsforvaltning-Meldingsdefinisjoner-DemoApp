using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste.Models
{
    [Serializable]
    public class Kode
    {
        [XmlAttribute]
        public string verdi { get; set; } = "";

        [DefaultValue("")]
        [XmlAttribute]
        public string tekst { get; set; } = "";

        [XmlAttribute]
        public string? beskrivelse { get; set; }

        [XmlAttribute]
        public string? gyldigFra { get; set; }

        [XmlAttribute]
        public string? gyldigTil { get; set; }

        [XmlAttribute]
        public string? alternativVerdi { get; set; }

        [JsonPropertyName("variabel")]
        [XmlElement("variabel")]
        public KodeVariabel[] variabler { get; set; } = [];
    }
}
