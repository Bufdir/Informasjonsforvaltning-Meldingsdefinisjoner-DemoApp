using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste.Models
{
    [Serializable]
    [XmlRoot(ElementName = "kodelister")]//, Namespace = "kodelister.bufdir.no", DataType = "string", IsNullable = true)]
    public class Kodelister
    {
        /*        [XmlAttribute(AttributeName = "nmsp")]
                public string? nmsp { get; set; }*/

        [JsonIgnore]
        public string Filnavn { get; set; } = "";

        [XmlElement(ElementName = "kodeliste")]
        public Kodeliste[] kodelister { get; set; } = [];
    }
}
