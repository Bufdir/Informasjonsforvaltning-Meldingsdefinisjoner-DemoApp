namespace Buf.Meldingsutveksler.SkjemaVerktoy.Tekster.Models
{
    public class SprakElement
    {
        public string sprak { get; set; } = "";
        public string tekst { get; set; } = "";
        public bool fallback { get; set; } = false;
    }

    public class TekstFil
    {
        public List<string> missing = [];
        public List<string> used = [];
        public List<string> wrongName = [];

        public string[] sprak { get; set; } = [];
        public Skjema? skjema { get; set; }

        public string FileName { get; set; } = "";
    }

    public class Skjema
    {
        public string beskrivelse { get; set; } = "";
        public string versjon { get; set; } = "";
        public string versjonsInformasjon { get; set; } = "";
        public string gyldigFra { get; set; } = "";
        public string[] xsd { get; set; } = [];
        public Skjemaelement[] skjemaelement { get; set; } = [];
        public KodelisteTekster[] kodelistetekster { get; set; } = [];
    }

    public class Skjemaelement
    {
        public string navn { get; set; } = "";
        public string id { get; set; } = "";
        public List<SprakElement> ledetekst { get; set; } = [];
        public List<SprakElement> veiledning { get; set; } = [];
        public int sortering { get; set; }

        public override string ToString()
        {
            return $"{navn} / {ledetekst}";
        }
    }

    public class KodelisteTekster
    {
        public string id { get; set; } = "";
        public string navn { get; set; } = "";
        public KodelisteVerdi[] verdier { get; set; } = [];
    }

    public class KodelisteVerdi
    {
        public string verdi { get; set; } = "";
        public List<SprakElement> tekst { get; set; } = [];
        public List<SprakElement> beskrivelse { get; set; } = [];
    }
}
