using Buf.Meldingsutveksler.Meldinger.Models;
using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll;
using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll.Models;
using Buf.Meldingsutveksler.SkjemaVerktoy.Tekster;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using Demo.Fagsystem.Models.Business;
using Demo.Fagsystem.Models.Demodata;
using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using Demo.Fagsystem.Models.Utils;
using Demo.Fagsystem.Models.ViewModels;
using Demo.Fagsystem.Pages.Partial;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Schema;

namespace Demo.Fagsystem.Pages
{
    public class MeldingModel : PageModel
    {
        public void OnGet()
        {
            Init();
        }

        public void OnPost()
        {
            Init();
        }

        public void Init()
        {
            WebUtils.GetRequestParams(Request, out QueryParams);
            Fagsystem = FagsystemAccessor.GetFagsystemInstans(QueryParams)
               ?? throw new Exception("Finner ikke fagsystem");
            //            string OrgNr = Utils.GetRequestValue(QueryParams, Konstanter.Orgnr);
            var storage = Fagsystem.MeldingStorage;
            ExistingId = WebUtils.GetRequestValue(QueryParams, Konstanter.XmlId);
            SelectedSprak = WebUtils.GetRequestValue(QueryParams, Konstanter.SelectedSprak);
            if (SelectedSprak == "")
                SelectedSprak = TeksterUtils.STANDARD_SPRAK;
            SakSelectorModel model = new(Request);
            SelectedKlient = model.selectedKlient;
            if (ExistingId != "")
            {
                var record = storage.GetRecord(ExistingId)
                    ?? throw new Exception($"Fant ikke record med id = {ExistingId} i storage");
                var melding = storage.Get(ExistingId);
                bool isSent = MeldingUtils.IsSent(record.State);
                var existingTargetNmsp = record.Meldingshode?.MeldingstypeNmsp
                    ?? throw new Exception("Meldingshode.MeldingstypeNmsp is NULL");
                string SelectedNamespace = WebUtils.GetRequestValue(QueryParams, Konstanter.SelectedSkjema);
                bool changeMeldingstype = SelectedNamespace != "" && SelectedNamespace != existingTargetNmsp;
                if (changeMeldingstype) // Endre skjema på eksisterende fil!
                {
                    existingTargetNmsp = SelectedNamespace;
                }

                SelectedSchema = XsdUtils.SchemaFromTargetNmsp(existingTargetNmsp);
                if (SelectedSchema == null)
                {
                    List<XmlSchema> hits = XsdUtils.SchemaFromVersionNeutralTargetNmsp(existingTargetNmsp);
                    if (hits.Count > 0)
                    {
                        SelectedSchema = hits.Last();
                        Message = $"Schema endret fra '{existingTargetNmsp}' til '{SelectedSchema.TargetNamespace}'";
                    }
                }
                XmlSchemaElement rootElement = XsdUtils.GetRootElement(SelectedSchema)
                    ?? throw new Exception($"Schema {SelectedSchema!.TargetNamespace} mangler rotelement");
                if (changeMeldingstype)
                {
                    QueryParams[$"{rootElement.Name}.{Konstanter.Meldingshode}.{Konstanter.MeldingstypeNmsp}"] = SelectedNamespace;
                    SelectedSchema = XsdUtils.SchemaFromTargetNmsp(SelectedNamespace);
                    rootElement = XsdUtils.GetRootElement(SelectedSchema)
                        ?? throw new Exception($"Schema {SelectedSchema!.TargetNamespace} mangler rotelement");
                }
                //var selectedSchemaElement = XsdUtils.GetRootElement(SelectedSchema);

                PrefillValues = XmlFactory.ReadXMLFile(melding, SelectedSchema!);
                /*if (changeMeldingstype)
                {
                    string caption = TeksterUtils.GetCaption(SelectedSchema, rootElement, true);
                }*/
                SelectedAksjon = isSent ? "Oppdatering" : "Ny";
                SelectedAksjonDisabled = true;
                if (WebUtils.GetRequestValue(QueryParams, Konstanter.ActionParam) == "EndreType" || changeMeldingstype)
                {
                    AvailableSkjema = XsdUtils.XsdSchemaNamesWithNamedRootElement(rootElement.Name ?? "-");
                }
                else
                {
                    SelectedSkjemaDisabled = true;
                }
                if (isSent)
                {
                    DemodataGenerator.Update_MeldingsInfo(PrefillValues, SelectedAksjon, SelectedSchema!, MeldingsprotokollUtils.GetMeldingshode(melding/*, true*/)/*melding*/);
                }
                DemodataGenerator.UpdateEditableAndClosed(PrefillValues, rootElement.Name!);

            }
            else
            {
                SelectedAksjon = WebUtils.GetRequestValue(QueryParams, Konstanter.SelectedAksjon);
                if (SelectedAksjon == "")
                    SelectedAksjon = "Ny";
                SelectedSchema = XsdUtils.SchemaFromTargetNmsp(WebUtils.GetRequestValue(QueryParams, Konstanter.SelectedSkjema));
                //?? throw new Exception("Kan ikke finne XSD (skjema)");
                var rootElement = XsdUtils.GetRootElement(SelectedSchema);
                //var rootElementName = rootElement?.Name ?? "";
                //string? caption = TeksterUtils.GetCaption(SelectedSchema, rootElement, true) ?? "Melding";

                if (SelectedSchema != null)
                {
                    DemodataGenerator.GetAvsenderMottaker(Fagsystem, SelectedSchema, out Organisasjon? avsender, out Organisasjon? mottaker);
                    if (avsender == null)
                        throw new Exception("Avsenderorganisasjon ikke valgt");
                    if (mottaker == null)
                        throw new Exception("Mottakerorganisasjon ikke valgt");
                    if (rootElement != null && SelectedAksjon == "Ny")
                        PrefillValues = DemodataGenerator.GetPrefillValues(Request, null, Fagsystem.FagsystemInfo, avsender, mottaker, Fagsystem.Bruker.KontaktInfo);
                }
            }
            if (SelectedSchema != null)
            {
                AvailableSprak = TeksterUtils.GetAvailableSprak(SelectedSchema);
            }

        }

        public FagsystemBase? Fagsystem { get; set; }

        public string Message { get; set; } = "";
        public string ExistingId { get; set; } = "";

        public Dictionary<string, string> QueryParams = [];

        public List<PrefilledValue>? PrefillValues { get; set; } = [];
        public string MeldingId { get; set; } = "";

        public XmlSchema? SelectedSchema { get; set; } = null;

        public string SelectedSprak { get; set; } = "";
        public List<string> AvailableSprak { get; set; } = [];
        public bool SelectedSkjemaDisabled { get; set; } = false;

        public string SelectedAksjon { get; set; } = "";
        public bool SelectedAksjonDisabled { get; private set; } = false;

        public Klient? SelectedKlient { get; set; }
        public string[] AvailableSkjema { get; private set; } = [];
        public bool TestTekster { get; set; } = false;

        public List<ControlEnabler> controlEnablers { get; set; } = [];

        public PrefilledValue? GetPrefilledValueFor(string id)
        {
            var hit = PrefillValues?.FirstOrDefault(p => id.EndsWith(p.Xpath));
            return hit;
        }

        public List<List<PrefilledValue>> GetPrefilledValuesMultiple(string id)
        {
            List<List<PrefilledValue>> result = [];
            int itemNo = 1;
            while (true && itemNo < 100)
            {
                var items = PrefillValues?.Where(p => p.Xpath == $"{id}{itemNo}" || p.Xpath.StartsWith($"{id}{itemNo}.")).ToList();
                if (items?.Count > 0)
                    result.Add(items);
                else
                    break;
                itemNo++;
            }
            return result;
        }




        /*        public KodelisteFilter? kodelistefiltre { get; set; }

                public List<Kode> FilterKodeliste(Kodeliste liste, string filterliste_id = "", string[]? filterverdier = null)
                {
                    List<Kode> result = [];
                    if (filterliste_id != "" && filterverdier?.Length > 0)
                    {
                        result = XsdUtils.GetFilteredKodeliste(liste, filterliste_id, filterverdier);
                    }
                    else if (kodelistefiltre != null && kodelistefiltre.filter.Any(f => f.kodeliste_id == liste.id))
                    {
                        var filter = kodelistefiltre.filter.FirstOrDefault(f => f.kodeliste_id == liste.id);
                        result = XsdUtils.GetFilteredKodeliste(liste, filter.filterliste_id, filter.filterverdier);
                    }
                    else
                        result = liste.koder.ToList();
                    return result;

                }
        */
    }
}
