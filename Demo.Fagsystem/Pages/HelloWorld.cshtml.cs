using Demo.Fagsystem.Models.Utils;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Fagsystem.Pages
{
    public class HelloWorldModel : PageModel
    {
        public string message { get; set; } = "";
        public Dictionary<string, string> QueryParams = [];
        public void OnGet()
        {
            try
            {
                WebUtils.GetRequestParams(Request, out QueryParams);
                string OrgNr = WebUtils.GetRequestValue(QueryParams, Konstanter.Orgnr);
                //var broker = MeldingBrokerAccessor.GetBroker(OrgNr, config, env);
                //                Task result = broker.Store( BlobShareReaderWriter.SaveFileContentsToBlob(config, "test.file", "Hello, world!");
                //                await result;
                message = "OK";
            }
            catch (Exception e)
            {
                message = e.Message + "\n" + e.StackTrace;
            }
        }
    }
}
