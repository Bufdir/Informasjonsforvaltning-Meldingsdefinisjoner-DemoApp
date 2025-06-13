using Demo.Fagsystem.Models.Business;
using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using Demo.Fagsystem.Models.Utils;

namespace Demo.Fagsystem.Pages.Partial
{
    public class SakSelectorModel
    {
        public SakSelectorModel(HttpRequest request) : base()
        {
            Init(request);
        }

        public List<Klient>? KlientListe { get; private set; } = [];

        public Klient? selectedKlient { get; set; }

        public void Init(HttpRequest request)
        {

            WebUtils.GetRequestParams(request, out Dictionary<string, string> queryParams);
            string instansId = WebUtils.GetRequestValue(queryParams, Konstanter.InstansId);
            OrgSelected = instansId != "";
            if (!OrgSelected)
            {
                var OrganisasjonListe = FagsystemAccessor.FagsystemInstanser;
                var activeFagsystem = OrganisasjonListe.Where(f => f.Config?.Aktiv == "1");
                if (activeFagsystem.Count() == 1)
                {
                    instansId = activeFagsystem.First().Id;
                    OrgSelected = true;
                }

            }
            if (OrgSelected)
            {
                var instans = FagsystemAccessor.GetFagsystemInstans(instansId);
                KlientListe = instans?.Klienter;
                string barnId = WebUtils.GetRequestValue(queryParams, Konstanter.SelectedSak);
                if (!string.IsNullOrEmpty(barnId))
                    selectedKlient = instans?.Klienter.FirstOrDefault(b => b.Id.ToString() == barnId);
            }
        }

        public bool OrgSelected { get; set; }
    }
}
