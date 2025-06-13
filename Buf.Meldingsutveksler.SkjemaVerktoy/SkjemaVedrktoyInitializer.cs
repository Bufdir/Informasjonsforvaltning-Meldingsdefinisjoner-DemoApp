using Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste;
using Buf.Meldingsutveksler.SkjemaVerktoy.FilSystem;
using Buf.Meldingsutveksler.SkjemaVerktoy.Tekster;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;

namespace Buf.Meldingsutveksler.SkjemaVerktoy;
public static class SkjemaVedrktoyInitializer
{
    public static IFileSystem? FileSystem { get; set; }
    public static void Init(IFileSystem? fileSystem)
    {
        FileSystem = fileSystem
            ?? throw new Exception("FileSystem == null i Init()");
        if (XmlSchemaRegister.Load(fileSystem))
        {
            XsdUtils.Init(fileSystem);
            KodelisteUtils.Init(fileSystem);
            TeksterUtils.Init(fileSystem);
            XmlSchemaRegister.Init();
        }
    }

    public static void ClearAll()
    {
        XmlSchemaRegister.Schemas = new();
        TeksterUtils.TekstFiler.Clear();
        KodelisteUtils.kodelister.Clear();
        XsdUtils.XsdSchemas.Clear();
    }

    public static void Reload()
    {
        ClearAll();
        Init(FileSystem);
    }
}
