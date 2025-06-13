using Buf.Meldingsutveksler.Meldinger.Enums;
using System.ComponentModel;


namespace Buf.Meldingsutveksler.Meldinger.Models;

public static class MeldingUtils
{
    public static string ConversationId(IMeldingRecord item)
    {
        return (item.Meldingshode?.OppfolgingAvMelding?.StartMeldingId ?? "") != "" ? item.Meldingshode!.OppfolgingAvMelding!.StartMeldingId : item.Meldingshode!.Id;
    }
    public static bool InSameConversation(IMeldingRecord Item1, IMeldingRecord Item2)
    {
        string item1StartingPoint = ConversationId(Item1);
        string item2StartingPoint = ConversationId(Item2);
        return item1StartingPoint == item2StartingPoint;
    }

    public static int CompareMelding(IMeldingRecord container1, IMeldingRecord container2)
    {
        if (!InSameConversation(container1, container2))
            return /*DateTime*/string.Compare(/*(DateTime)*/(container1.Meldingshode?.SendtTidspunkt ?? "?")!, /*(DateTime)*/(container2.Meldingshode?.SendtTidspunkt ?? "?")!);
        if ((container1.Meldingshode?.OppfolgingAvMelding?.MeldingId ?? "") == "")
            return -1;
        if ((container2.Meldingshode?.OppfolgingAvMelding?.MeldingId ?? "") == "")
            return 1;
        return /*DateTime*/string.Compare(/*(DateTime)*/(container1.Meldingshode?.SendtTidspunkt ?? "?")!, /*(DateTime)*/(container2.Meldingshode?.SendtTidspunkt ?? "?")!);
    }

    public static string GetEnumText(object value)
    {
        return (value.GetType().GetField(value.ToString()!)?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .SingleOrDefault() is not DescriptionAttribute attribute ? value.ToString() : attribute.Description) ?? "";
    }

    public static bool IsSent(MeldingTransferState state)
    {
        return state >= MeldingTransferState.sent && state < MeldingTransferState.inbox;
    }
}
