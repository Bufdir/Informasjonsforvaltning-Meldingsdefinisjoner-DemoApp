namespace Buf.Meldingsutveksler.SkjemaVerktoy.FilSystem;
public interface IFileSystem
{
    string[] ListFiles(string Path);
    Stream GetStream(string FileName);
    string GetFileContents(string FileName);
    void Save(string FileName, string Contents, bool doOverwrite);
    void Save(string FileName, Stream ContentStream, bool doOverwrite);
    bool FileExists(string FileName);
}
