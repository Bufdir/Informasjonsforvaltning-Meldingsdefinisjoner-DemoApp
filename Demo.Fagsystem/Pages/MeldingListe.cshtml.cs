using Buf.Meldingsutveksler.Meldinger.Enums;
using Buf.Meldingsutveksler.Meldinger.Models;
using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using Demo.Fagsystem.Models.Utils;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Fagsystem.Pages
{
    public class MeldingListeModel() : PageModel
    {
        public void OnGet()
        {
            Init();
        }

        public void OnPost()
        {
            Init();
        }

        public Dictionary<string, string> QueryParams = [];

        public void Init()
        {
            WebUtils.GetRequestParams(Request, out QueryParams);
            FagsystemBase? instans = FagsystemAccessor.GetFagsystemInstans(QueryParams);
            var storage = instans?.MeldingStorage ??
                throw new Exception("Fagsystem har ikke definert MeldingStorage i config");
            /*            if (Utils.ExistsRequestValue(QueryParams, "process"))
                        {
                            broker.ProcessTasks();
                            NumPendingTasks = broker.GetNumPendingTasks();
                            if (NumPendingTasks > 0)
                                ErrorText = "Klarte ikke å prosessere meldinger";
                        }
                        else
                            NumPendingTasks = broker.GetNumPendingTasks();
            */
            string selectedSak = WebUtils.GetRequestValue(QueryParams, Konstanter.SelectedSak);
            string filterFra = $"{Konstanter.MeldingstypeAvsendersRef}={selectedSak}";
            string filterTil = $"{Konstanter.MeldingstypeMottakersRef}={selectedSak}";

            var meldingerArr = selectedSak != "" ? storage.List(MeldingDirection.undefined, selectedSak, false) : storage.List(MeldingDirection.undefined, false);

            Meldinger = [.. meldingerArr];
            Meldinger.Sort((container1, container2) => { return MeldingUtils.CompareMelding(container1, container2); });
        }


        public List<IMeldingRecord> Meldinger { get; set; } = [];

        //        public int NumPendingTasks { get; set; } = 0;

        public string ErrorText { get; private set; } = "";
    }
}
