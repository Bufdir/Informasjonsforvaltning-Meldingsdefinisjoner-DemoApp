using Buf.Meldingsutveksler.Meldinger.Models;
using Demo.Fagsystem.Models.Test;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Fagsystem.Pages.Simulator.TestProsessor
{
    public class TestProsessorModel : PageModel
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
            Tester.AddRange([
                new()
                {
                    Id = 1000,
                    Feilkode = Errorcodes.OK,
                    Tittel = "Forventer Kode 200 = OK",
                },
                new()
                {
                    Id = 1001,
                    Feilkode = Errorcodes.OK_DUPLICATE,
                    Tittel = "Forventer Kode 201 = OK (duplikat)"
                },
                new()
                {
                    Id = 1002,
                    Feilkode = Errorcodes.ERR_APPLICATION_ERROR,
                    Tittel = "Forventer Kode 500 = Application error"
                },

            ]);
        }

        public List<MeldingTest> Tester { get; set; } = [];
    }
}
