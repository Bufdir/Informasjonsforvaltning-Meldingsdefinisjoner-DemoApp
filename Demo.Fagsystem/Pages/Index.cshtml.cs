using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using Demo.Fagsystem.Models.Business;
using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using Demo.Fagsystem.Models.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Fagsystem.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel()
        {
        }

        public IActionResult OnGet()
        {
            if (XmlSchemaRegister.IsEmptyDefs())
                return Redirect("~/Admin");
            else
                Init();
            return Page();
        }
        public IActionResult OnPost()
        {
            if (XmlSchemaRegister.IsEmptyDefs())
                return Redirect("~/Admin");
            else
                Init();
            return Page();
        }


        public Klient? SelectedKlient { get; set; }

        public FagsystemBase? Instans { get; set; }
        public void Init()
        {
            WebUtils.GetRequestParams(Request, out Dictionary<string, string> queryParams);
            Instans = FagsystemAccessor.GetFagsystemInstans(queryParams);

            string klientId = WebUtils.GetRequestValue(queryParams, Konstanter.SelectedSak);
            if (!string.IsNullOrEmpty(klientId))
                SelectedKlient = Instans?.Klienter.FirstOrDefault(b => b.Id.ToString() == klientId);
        }

    }
}
