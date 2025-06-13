using Demo.Fagsystem.Models.Test;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections;

namespace Demo.Fagsystem.Pages
{
    public class TestTeksterModel : PageModel
    {
        public void OnGet()
        {
            Feil = XsdTester.TestJsonTekster();
        }

        public Dictionary<string, ICollection> Feil { get; set; } = [];
    }
}
