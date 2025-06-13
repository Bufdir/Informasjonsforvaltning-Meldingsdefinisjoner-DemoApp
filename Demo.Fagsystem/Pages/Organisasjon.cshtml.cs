using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using Demo.Fagsystem.Models.Utils;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Schema;

namespace Demo.Fagsystem.Pages
{
    public class OrganisasjonModel : PageModel
    {
        public void OnGet()
        {
            Init();
        }

        public void OnPost()
        {
            Init();
        }

        public FagsystemBase? Instans { get; set; }

        public XmlSchemaAnnotated? organisasjonElement { get; set; }

        public Dictionary<string, string> QueryParams = [];
        public string? path { get; set; }
        public void Init()
        {
            WebUtils.GetRequestParams(Request, out QueryParams);
            string instansId = WebUtils.GetRequestValue(QueryParams, Konstanter.InstansId);

            Instans = FagsystemAccessor.GetFagsystemInstans(instansId);
            /*path = "Avsender";
            var schema = DataFactory.XsdSchemasWithRootElement.First()
                ?? throw new Exception($"Finner ikke skjema som inneholder '{path}'");
            var element = schema.Items.OfType<XmlSchemaElement>().First();
            barnevernElement = DataFactory.FindByXPath(element, "Avsender").First();*/
        }
    }
}
