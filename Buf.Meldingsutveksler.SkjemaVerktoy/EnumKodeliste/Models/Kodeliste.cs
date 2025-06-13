using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste.Models
{
    [Serializable]
    public class Kodeliste
    {
        [XmlAttribute]
        public string id { get; set; } = "";
        [XmlAttribute]
        public string navn { get; set; } = "";

        [JsonPropertyName("variabel")]
        [XmlElement(ElementName = "variabel")]
        public KodelisteVariabel[] variabler { get; set; } = [];

        [JsonPropertyName("kode")]
        [XmlElement(ElementName = "kode")]
        public Kode[] koder { get; set; } = [];

    }
}
