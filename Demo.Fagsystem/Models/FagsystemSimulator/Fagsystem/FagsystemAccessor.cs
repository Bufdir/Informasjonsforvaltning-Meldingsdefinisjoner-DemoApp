using Demo.Fagsystem.Models.Utils;

namespace Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;

public static class FagsystemAccessor
{
    public static List<FagsystemBase> FagsystemInstanser { get; } = [];
    public static FagsystemBase? GetFagsystemInstans(string id)
    {
        var result = FagsystemInstanser.FirstOrDefault(i => i.Id == id);
        return result;
    }
    public static FagsystemBase? GetFagsystemInstans(Dictionary<string, string> queryParams)
    {
        string instansId = WebUtils.GetRequestValue(queryParams, Konstanter.InstansId);
        var result = FagsystemInstanser.Where(i => i.Id == instansId);
        if (!result.Any())
            return null;
        if (result.Count() > 1)
            throw new Exception($"Finner flere fagsysteminstanser med id='{instansId}'");
        return result.First();
    }

}
