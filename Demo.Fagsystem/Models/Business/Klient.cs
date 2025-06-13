using System.Text.Json.Serialization;

namespace Demo.Fagsystem.Models.Business
{

    public enum KlientType
    {
        BarnevernStandard,
        BarnevernEMA,
        BarnevernUfodt
    }

    public class Klient
    {
        public int Id { get; set; }
        public int _FREG_Person_Id { get; set; }
        [JsonIgnore]
        public FREG_Person? FREG_Person { get; set; }
        public KlientType Type { get; set; } = KlientType.BarnevernStandard;

    }

    public class Barn : Klient
    {
        public List<NettverkPerson>? Nettverk { get; set; } = [];

    }
}
