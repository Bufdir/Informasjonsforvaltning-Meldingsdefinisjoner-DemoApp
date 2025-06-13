using Buf.Meldingsutveksler.Meldinger.Enums;
using Buf.Meldingsutveksler.Meldinger.Models;
using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll;
using Buf.Meldingsutveksler.SkjemaVerktoy.Meldingsprotokoll.Models;
using Demo.Fagsystem.Models.FagsystemSimulator.Config;
using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using Demo.Fagsystem.Models.Utils;
using System.Text.Json;
using System.Xml;

namespace Demo.Fagsystem.Models.FagsystemSimulator.Storage;

public class BlobMeldingStorage : IMeldingStorage
{
    public BlobMeldingStorage(FagsystemBase fagsystem, BlobContainerConfig? config)
    {
        Fagsystem = fagsystem;
        OrgNr = fagsystem.Organisasjon.Organisasjonsnummer;
        if (Brokers.ContainsKey(OrgNr))
            throw new Exception($"BlobStorageBroker for org. {OrgNr} finnes allerede");
        if (config == null)
            throw new Exception($"Config for BlobMeldingStorage for org. {OrgNr} finnes ikke");
        Brokers[OrgNr] = this;
        ReaderWriter = new(config);
    }

    public static BlobMeldingStorage GetStorage(FagsystemBase fagsystem, BlobContainerConfig config)
    {
        if (!Brokers.TryGetValue(fagsystem.Organisasjon.Organisasjonsnummer, out var broker))
        {
            broker = new BlobMeldingStorage(fagsystem, config);
        }
        return broker;
    }

    public BlobStorageFileSystem ReaderWriter { get; set; }

    public string Lock = "LOCK";

    FagsystemBase Fagsystem { get; }

    public static readonly Dictionary<string, BlobMeldingStorage> Brokers = [];

    private string OrgNr { get; }

    public string GetMeldingRecordFileName(MeldingDirection direction, string id)
    {
        string path = GetMeldingRecordFilePath(direction);
        return $"{path}/{id}.json";
    }

    public string GetMeldingRecordFileName(MeldingRecord record)
    {
        if (record.Meldingshode == null)
            throw new Exception("Meldingshode is null in " + nameof(GetMeldingRecordFileName));
        return GetMeldingRecordFileName(record.Direction, record.Meldingshode.Id);
    }
    public string GetLogItemFileName(string id, DateTime dateTime)
    {
        return $"{OrgNr}/log/{id}/{dateTime:yyyy-MM-dd_HH.mm.ss.fff}.json";
    }

    private string GetMeldingRecordFilePath(MeldingDirection direction)
    {
        return $"{OrgNr}/{direction}";
    }

    public string GetMeldingFileName(string id, string suffix = "")
    {
        return $"{OrgNr}/melding/{id}{suffix}.xml";
    }

    public MeldingRecord GetMeldingRecord(string fileName)
    {
        var contentsTask = ReaderWriter.GetFileContents(fileName);
        string contents = contentsTask.Result;
        var record = JsonSerializer.Deserialize<MeldingRecord>(contents)
            ?? throw new Exception("Deserialize av MeldingRecord returnerte <null>");
        record.FileName = fileName;
        return record!;
    }
    public MeldingRecord? GetRecord(string meldingId, MeldingDirection direction)
    {
        string path = GetMeldingRecordFileName(direction, meldingId);
        var files = ReaderWriter.ListFiles(path);
        if (files.Length > 0)
            return GetMeldingRecord(files[0]);
        return null;
    }

    public void StoreBlob(XmlDocument melding, MeldingTransferState state, bool deleteDuplicate = false)
    {
        var direction = TransferStateToDirection(state);
        var meldingshode = MeldingsprotokollUtils.GetMeldingshode(melding/*, true*/)
            ?? throw new Exception("Finner ikke meldingshode i melding");
        MeldingRecord rec = new(meldingshode, direction, state);
        if (rec.Meldingshode == null)
            throw new Exception("Meldingshode is NULL");
        string meldingFileName = GetMeldingFileName(rec.Meldingshode.Id);
        var duplicate = ReaderWriter.FileExists(meldingFileName);
        if (duplicate)
        {
            if (deleteDuplicate)
                ReaderWriter.DeleteFile(meldingFileName);
            else
                meldingFileName = GetMeldingFileName(rec.Meldingshode.Id, $"_duplicate_{DateTime.Now:yyyy-MM-dd_HH.mm.ss.fff)}");
        }
        ReaderWriter.SaveFile(meldingFileName, melding.OuterXml, false);
        rec.Response = Errorcodes.STORED;
        SaveMeldingRec(rec);
    }

    public void DeleteFile(string fileName)
    {
        ReaderWriter.DeleteFile(fileName);
    }

    public void SaveMeldingRec(MeldingRecord rec)
    {
        rec.FileName = GetMeldingRecordFileName(rec);
        var recContents = JsonSerializer.Serialize(rec);
        ReaderWriter.SaveFile(rec.FileName, recContents, true);
        Log(rec.Meldingshode!.Id, rec.State, rec.Response, rec.ResponseText);
    }

    public void Log(string id, MeldingTransferState state, int result, string message = "")
    {
        DateTime logTime = DateTime.Now;
        string fileName = GetLogItemFileName(id, logTime);
        LogItem logItem = new() { Id = id, State = state, DateTime = logTime, Result = result, Message = message };
        var contents = JsonSerializer.Serialize(logItem);
        ReaderWriter.SaveFile(fileName, contents, false);
    }

    internal void Receive(XmlDocument melding)
    {
        StoreBlob(melding, MeldingTransferState.inbox, true);
    }

    private static MeldingDirection TransferStateToDirection(MeldingTransferState state)
    {
        if (state == MeldingTransferState.undefined)
            return MeldingDirection.undefined;
        if (state < MeldingTransferState.inbox)
            return MeldingDirection.outbound;
        return MeldingDirection.inbound;
    }

    public XmlDocument Get(string meldingId)
    {
        string fileName = GetMeldingFileName(meldingId);
        var contentsTask = ReaderWriter.GetStreamFromBlob(fileName);
        var contentStream = contentsTask.Result;
        XmlDocument doc = new();
        doc.Load(contentStream);
        return doc;
    }


    public IMeldingRecord GetRecord(string meldingId)
    {
        var record = GetRecord(meldingId, MeldingDirection.inbound);
        if (record != null)
            return record;
        record = GetRecord(meldingId, MeldingDirection.outbound);
        if (record != null)
            return record;
        throw new Exception($"Fant ikke meldingsRecord for melding med id={meldingId}");
    }

    public Organisasjon GetOrganisasjon(string OrgNr)
    {
        throw new NotImplementedException();
    }

    public Organisasjon[] ListOrganisasjon(AktorType Rolle = AktorType.undefined)
    {
        throw new NotImplementedException();
    }

    public void Send(XmlDocument melding)
    {
        StoreBlob(melding, MeldingTransferState.outbox, true);
    }

    public void Store(XmlDocument melding)
    {
        StoreBlob(melding, MeldingTransferState.draft, true);
    }

    public bool TestConnection()
    {
        throw new NotImplementedException();
    }

    public void UpdateState(IMeldingRecord melding)
    {
        var file = GetMeldingRecordFileName((melding as MeldingRecord)!);
        var rec = GetMeldingRecord(file);
        throw new NotImplementedException(nameof(UpdateState));
    }

    public ReceiverValdationResult ValidateReceiver(string OrgNr, AktorType Rolle)
    {
        throw new NotImplementedException();
    }

    internal void SendAppRec(MeldingRecord meldingRec)
    {
        var appRec = XmlFactory.WriteAppRec(Konstanter.nmsp_ApplikasjonKvittering, meldingRec.Meldingshode);
        Send(appRec);
    }

    internal void SendErrorMessage(string errorText)
    {
        throw new NotImplementedException(nameof(SendErrorMessage));
    }

    public void DeleteAll()
    {
        var blobClient = ReaderWriter.GetBlobStorageClient();
        var files = ReaderWriter.ListFiles("*");
        foreach (var file in files)
        {
            blobClient.DeleteBlob(file);
        }
    }

    // IMeldingStorage members
    public IMeldingRecord[] List(MeldingDirection Direction, bool includeAppRecs)
    {
        if (Direction == MeldingDirection.undefined)
        {
            var outbound = List(MeldingDirection.outbound, includeAppRecs);
            var inbound = List(MeldingDirection.inbound, includeAppRecs);
            return [.. outbound.Concat(inbound)];
        }
        string path = GetMeldingRecordFilePath(Direction);
        var files = ReaderWriter.ListFiles(path);
        List<IMeldingRecord> result = [];
        foreach (var file in files)
        {
            var meldingRecord = GetMeldingRecord(file);
            if (includeAppRecs || meldingRecord.Meldingshode.OppfolgingAvMelding?.MeldingsForbindelse != MeldingsForbindelseType.ApplikasjonsKvittering.ToString())
                result.Add(meldingRecord);
        }
        return [.. result];
    }

    public IMeldingRecord[] List(MeldingDirection Direction, string SakId, bool includeAppRecs)
    {
        var allRecords = List(Direction, includeAppRecs);
        var result = allRecords.Where(rec => (rec.Meldingshode?.AvsendersRef ?? "?") == SakId);
        return [.. result];
    }

    public IMeldingRecord[] List(MeldingDirection Direction, MeldingTransferState State, bool includeAppRecs)
    {
        var allRecords = List(Direction, includeAppRecs);
        var result = allRecords.Where(rec => rec.State == State);
        return [.. result];
    }

}
