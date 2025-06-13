using Buf.Meldingsutveksler.Meldinger.Enums;
using Buf.Meldingsutveksler.Meldinger.Models;
using System.Xml;

namespace Demo.Fagsystem.Models.FagsystemSimulator.Storage;

public interface IMeldingStorage
{
    public XmlDocument? Get(string meldingId);
    public IMeldingRecord? GetRecord(string meldingId);
    public IMeldingRecord[] List(MeldingDirection Direction, bool includeReceipts);
    public IMeldingRecord[] List(MeldingDirection Direction, string SakId, bool includeReceipts);
    public IMeldingRecord[] List(MeldingDirection Direction, MeldingTransferState State, bool includeReceipts);
    public void Store(XmlDocument melding);
    public bool TestConnection();
    public void DeleteAll();
}