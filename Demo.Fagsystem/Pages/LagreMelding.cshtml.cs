using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml.Models;
using Demo.Fagsystem.Models.Demodata;
using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using Demo.Fagsystem.Models.Utils;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;

namespace Demo.Fagsystem.Pages
{
    public class LagreMeldingModel : PageModel
    {

        public void OnPost()
        {
            Init();
        }

        public void OnGet()
        {
            Init();
        }

        public void Init()
        {
            WebUtils.GetRequestParams(Request, out QueryParams);
            FagsystemBase? instans = FagsystemAccessor.GetFagsystemInstans(QueryParams);
            var storage = instans?.MeldingStorage
                ?? throw new Exception($"Fagsystem '{WebUtils.GetRequestValue(QueryParams, Konstanter.InstansId)}' har ikke gyldig konfigurasjon av meldingslager");
            string existingId = WebUtils.GetRequestValue(QueryParams, Konstanter.XmlId);
            bool meldingExists = existingId != "";
            XmlDocument? doc = null;
            string schemaNmsp;
            if (meldingExists)
            {
                doc = storage.Get(existingId);
                var record = storage.GetRecord(existingId);
                schemaNmsp = record?.Meldingshode?.MeldingstypeNmsp
                    ?? throw new Exception("Meldingshode.MeldingstypeNmsp is NULL");
            }
            else
                schemaNmsp = WebUtils.GetRequestValue(QueryParams, Konstanter.SelectedSkjema);
            var schema = XsdUtils.GetSchema(schemaNmsp);
            var rootElement = XsdUtils.GetRootElement(schema);
            if (QueryParams.ContainsKey(Konstanter.ActionParam))
            {
                Action = WebUtils.GetRequestValue(QueryParams, Konstanter.ActionParam);
            }
            if (!WebUtils.ExistsRequestValue(QueryParams, "Redigert"))
            {
                if (doc != null)
                    XmlFactory.ReadXMLFile(doc, schema, ref QueryParams);
                QueryParams[Konstanter.SelectedSkjema] = schemaNmsp;
            }
            /*var dateTime = */
            DemodataGenerator.Update_SendtTidspunkt(QueryParams, rootElement!.Name!);
            var document = XmlFactory.WriteXml(QueryParams, out Dictionary<string, string> nmsps);

            if (Action == "sendt")
            {
                bool validFormal = XmlValidator.ValidateXmlBuiltIn(document, out string error);
                bool validXtended = XmlValidator.ValidateXml(document, schema, nmsps, Errors);
                if (!(validFormal && validXtended))
                {
                    Action = "lagret";
                    ErrorText = error;
                }
            }
            if (Action == "sendt")
            {
                //broker.Send(document);
            }
            else if (Action == "lagret")
                storage.Store(document);
        }

        public string ErrorText { get; set; } = "";
        public List<XmlValidationError> Errors { get; set; } = [];

        public Dictionary<string, string> QueryParams = [];

        public string FileName { get; set; } = "";

        public string Action { get; set; } = "";

    }
}
