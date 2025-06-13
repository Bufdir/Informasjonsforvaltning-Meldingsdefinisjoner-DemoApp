using Buf.Meldingsutveksler.Meldinger.Enums;
using Buf.Meldingsutveksler.Meldinger.Models;

namespace Demo.Fagsystem.Pages.Partial
{
    /*  Fra MeldingsAksjon.cshtml:
        var aksjonEnum = XsdUtils.GetTypeDefinition(Model.SelectedProtocol, Konstanter.MeldingsForbindelseType) ??
            throw new Exception($"Finner ikke aksjonsverdier for type {Konstanter.MeldingsForbindelseType}");
        var kodeliste = XsdUtils.GetKodelisteVerdier(aksjonEnum);
    */
    public class MeldingMenyModel
    {
        public MeldingMenyModel(IMeldingRecord melding)
        {
            string id = melding.Meldingshode?.Id ??
                throw new Exception("Meldingshode.Id is NULL");
            if (melding.State == MeldingTransferState.draft)
            {
                Actions.Add(new("Redigér videre", $"menuActionMeldingListe(event, 'Melding')"));
                Actions.Add(new("Redigér som annen type", $"menuActionMeldingListe(event, 'Melding', 'actionParam=EndreType')"));
                Actions.Add(new("Send", $"menuActionMeldingListe(event, 'LagreMelding', 'actionParam=sendt')"));
                Actions.Add(new("Slett (lokalt)", $"menuActionMeldingListe(event, 'SlettMelding')"));
            }
            else if (melding.Direction == MeldingDirection.outbound)
            {
                Actions.Add(new("Oppdatér/Redigér", $"menuActionMeldingListe(event, 'Melding', 'actionParam=Oppdatering')"));
                Actions.Add(new("Oppdatér/Redigér som annen type", $"menuActionMeldingListe(event, 'Melding', 'actionParam=EndreType')"));
                Actions.Add(new("Trekk tilbake / slett", $"menuActionMeldingListe(event, 'TrekkMelding')"));
            }
            else if (melding.State >= MeldingTransferState.inbox)
            {
                Actions.Add(new("Svar", $"menuActionMeldingListe(event, 'MeldingXX')"));
            }
            Actions.Add(new("Info", $"menuActionMeldingListe(event, 'MeldingInfo');"));
        }

        public List<KeyValuePair<string, string>> Actions { get; set; } = [];

    }
}
