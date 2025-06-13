using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;

namespace Demo.Fagsystem.Pages.Simulator.Partial;

public class FagsystemToolbarModel(HttpRequest request, FagsystemBase instans)
{
    public HttpRequest Request => request;
    public FagsystemBase Instans => instans;
}
