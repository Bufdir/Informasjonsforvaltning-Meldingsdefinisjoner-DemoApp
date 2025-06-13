using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste.Models
{
    [Serializable]
    public class KodelisteRestricted
    {
        [XmlAttribute]
        public string id { get; set; } = "";
        [XmlAttribute]
        public string navn { get; set; } = "";

        [JsonPropertyName("variabel")]
        public KodelisteVariabel[] variabler { get; set; } = [];

        [JsonPropertyName("kode")]
        public KodeRestricted[] koder { get; set; } = [];

    }
}
