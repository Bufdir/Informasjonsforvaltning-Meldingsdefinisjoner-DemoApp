using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using Demo.Fagsystem.Models.Utils;

namespace Demo.Fagsystem.Pages.Partial
{
    public class ScenarioSelectorModel
    {
        public ScenarioSelectorModel(HttpRequest request) : base()
        {
            Init(request);
        }

        public List<FagsystemBase>? OrganisasjonListe { get; private set; }

        public FagsystemBase? selectedInstans { get; set; }

        public void Init(HttpRequest request)
        {
            OrganisasjonListe = FagsystemAccessor.FagsystemInstanser;
            WebUtils.GetRequestParams(request, out Dictionary<string, string> queryParams);
            string instansId = WebUtils.GetRequestValue(queryParams, Konstanter.InstansId);
            if (!string.IsNullOrEmpty(instansId))
            {
                selectedInstans = OrganisasjonListe.FirstOrDefault(fsi => fsi.Id == instansId);
            }
            if (selectedInstans == null)
            {
                var activeFagsystem = OrganisasjonListe.Where(f => f.Config?.Aktiv == "1");
                if (activeFagsystem.Count() == 1)
                {
                    selectedInstans = activeFagsystem.First();
                }
            }
        }
    }
}
