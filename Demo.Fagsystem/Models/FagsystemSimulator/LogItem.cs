using Buf.Meldingsutveksler.Meldinger.Enums;
using System.Text.Json.Serialization;

namespace Demo.Fagsystem.Models.FagsystemSimulator;

public class LogItem
{
    [JsonInclude]
    public string Id { get; set; } = "";
    [JsonInclude]
    public DateTime DateTime { get; set; }
    [JsonInclude]
    public MeldingTransferState State { get; set; }
    [JsonInclude]
    public int Result { get; set; }
    [JsonInclude]
    public string Message { get; set; } = "";
}
