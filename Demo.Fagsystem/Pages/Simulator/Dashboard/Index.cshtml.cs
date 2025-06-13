using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using Demo.Fagsystem.Models.Utils;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Fagsystem.Pages.Simulator.Dashboard
{
    public class IndexModel : PageModel
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
            FagsystemBase? instans = FagsystemAccessor.GetFagsystemInstans(QueryParams);
        }

        public List<FagsystemBase> instanser = [];

        public Dictionary<string, string> QueryParams = [];
    }
}
