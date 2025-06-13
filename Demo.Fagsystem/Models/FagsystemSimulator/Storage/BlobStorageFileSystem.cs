using Azure.Identity;
using Azure.Storage.Blobs;
using Buf.Meldingsutveksler.SkjemaVerktoy.FilSystem;
using Demo.Fagsystem.Models.FagsystemSimulator.Config;

namespace Demo.Fagsystem.Models.FagsystemSimulator.Storage;

public class BlobStorageFileSystem(BlobContainerConfig config) : IFileSystem
{
    public BlobContainerConfig Config => config;
    public BlobContainerClient GetBlobStorageClient()
    {
        BlobContainerClient blobClient;

        // Er det en connection string?
        if (!string.IsNullOrEmpty(Config.ConnectStr))
        {
            blobClient = new(Config.ConnectStr, Config.Container);
            return blobClient;
        }

        /*Bruker System assigned managed identity med tildelte rettigheter 'Storage Blob Data Contributor'*/
        //Hvis det er aktuelt å bruke 'User assigned managed identities' brukes ["StorageAccountClientId"]:
        string clientId = Config.StorageAccountClientId
            ?? throw new Exception($"Mangler config-verdi for StorageAccountClientId");
        DefaultAzureCredential credential;
        if (!string.IsNullOrEmpty(clientId))
        {
            credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId });
        }
        else
        {
            credential = new DefaultAzureCredential();
        }
        blobClient = new(new Uri(Config.Container ?? "?"), credential);

        return blobClient;
    }

    public string TestConnection()
    {
        try
        {
            var client = GetBlobStorageClient();
            return "OK"; //+ connectString;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    public void Save(string FileName, string Contents, bool doOverwrite)
    {
        var client = GetBlobStorageClient();
        var blobClient = client.GetBlobClient(FileName);
        MemoryStream stream = new() { Position = 0 };
        var writer = new StreamWriter(stream);
        writer.Write(Contents);
        writer.Flush();
        stream.Position = 0;
        var _ = blobClient.Upload(stream, overwrite: doOverwrite);
    }
    public void Save(string FileName, Stream ContentStream, bool doOverwrite)
    {
        var client = GetBlobStorageClient();
        var blobClient = client.GetBlobClient(FileName);
        ContentStream.Position = 0;
        var _ = blobClient.Upload(ContentStream, overwrite: doOverwrite);
    }

    public string[] ListFiles(string fileNameStart)
    {
        try
        {
            List<string> files = [];
            var client = GetBlobStorageClient();
            var listing = client.GetBlobs();
            foreach (var item in listing.Where(i => fileNameStart == "*" || i.Name.StartsWith(fileNameStart)))
            {
                files.Add(item.Name);
            }
            return [.. files];
        }
        catch (Exception e)
        {
            throw new Exception($"{e.Message}\n\n{e.StackTrace}");
        }
    }


    public async Task<MemoryStream> GetStreamFromBlob(string fileName)
    {
        var client = GetBlobStorageClient();
        var blobClient = client.GetBlobClient(fileName);
        MemoryStream stream = new();
        var downloadResponse = blobClient.DownloadTo(stream);
        if (downloadResponse.Status > 300)
            throw new Exception($"Feil i download av fil {fileName}: HTTP {downloadResponse}");
        stream.Position = 0;
        return await Task.FromResult(stream);
    }



    public async Task<string> GetFileContents(string fileName)
    {
        var client = GetBlobStorageClient();
        var blobClient = client.GetBlobClient(fileName);
        MemoryStream stream = new();
        var downloadResponse = blobClient.DownloadTo(stream);
        if (downloadResponse.Status > 300)
            throw new Exception($"Feil i download av fil {fileName}: HTTP {downloadResponse}");
        stream.Position = 0;
        var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        return content;
    }

    public void SaveFile(string fileName, string content, bool doOverwrite)
    {
        Save(fileName, content, doOverwrite);
    }

    public void DeleteFile(string fileName)
    {
        BlobContainerClient blobClient = GetBlobStorageClient();
        blobClient.DeleteBlob(fileName);
    }

    public bool FileExists(string fileName)
    {
        BlobContainerClient blobClient = GetBlobStorageClient();
        var client = blobClient.GetBlobClient(fileName);
        return client.Exists();
    }

    public Stream GetStream(string FileName)
    {
        var contentsTask = GetStreamFromBlob(FileName);
        return contentsTask.Result;
    }

    string IFileSystem.GetFileContents(string FileName)
    {
        var task = GetFileContents(FileName);
        return task.Result;
    }

}
