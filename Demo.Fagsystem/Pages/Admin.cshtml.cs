using Buf.Meldingsutveksler.SkjemaVerktoy;
using Buf.Meldingsutveksler.SkjemaVerktoy.EnumKodeliste;
using Buf.Meldingsutveksler.SkjemaVerktoy.FilSystem;
using Buf.Meldingsutveksler.SkjemaVerktoy.Tekster;
using Buf.Meldingsutveksler.SkjemaVerktoy.Xml;
using Demo.Fagsystem.Models.FagsystemSimulator.Fagsystem;
using Demo.Fagsystem.Models.Utils;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO.Compression;

namespace Demo.Fagsystem.Pages
{
    public class AdminModel() : PageModel
    {

        public void OnGet()
        {
            Init();
        }

        public void OnPost()
        {
            Init();
        }

        private void Init()
        {
            WebUtils.GetRequestParams(Request, out Dictionary<string, string> QueryParams);
            FagsystemBase? instans = FagsystemAccessor.GetFagsystemInstans(QueryParams);
            if (WebUtils.ExistsRequestValue(QueryParams, "deleteAll"))
            {
                try
                {
                    instans?.MeldingStorage.DeleteAll();
                    allDeleted = true;
                    Result = "Alt er slettet";
                }
                catch (Exception ex)
                {
                    Result = "Feilmelding: " + ex.ToString();
                }
            }
            if (WebUtils.ExistsRequestValue(QueryParams, "renewDefs"))
            {
                SkjemaVedrktoyInitializer.Reload();
            }
            if (WebUtils.ExistsRequestValue(QueryParams, "definisjonfil"))
            {
            }
            Error = (XmlSchemaRegister.IsEmptyDefs()) ? "Mangler definisjonsfiler! Last opp her" : "";
        }

        private static MemoryStream FormFileToStream(IFormFile file)
        {
            var stream = new MemoryStream();
            file.CopyTo(stream);
            return stream;
        }

        public void /*async Task<IActionResult>*/ OnPostUploadFiles(List<IFormFile> postedFiles)
        {
            string[] allowedExtensions = [".json", ".xsd"];
            List<string> savedFiles = [];
            List<string> nonSavedFiles = [];
            IFileSystem? fileSystem = SkjemaVedrktoyInitializer.FileSystem
                ?? throw new Exception("Har ikke tilgjengelig filsystem i filopplasting");
            var files = postedFiles;

            var recFile = files.FirstOrDefault(f => f.FileName.Contains(XmlSchemaRegister.FILENAME_REGISTER));
            if (recFile != null)
            {
                var stream = FormFileToStream(recFile);
                fileSystem.Save(recFile.FileName, stream, true);
                savedFiles.Add(recFile.FileName);
            }
            foreach (var file in files)
            {
                if (file != recFile)
                {
                    if (Path.GetExtension(file.FileName).ToLower() == ".zip")
                    {
                        int count = 0;
                        savedFiles.Add("Zip-arkiv med innhold: " + file.FileName);
                        using var stream = FormFileToStream(file);
                        ZipArchive zip = new ZipArchive(stream);
                        foreach (var entry in zip.Entries)
                        {
                            string fileName = entry.ToString().ToLower();
                            if (allowedExtensions.Contains(Path.GetExtension(fileName).ToLower()))
                            {
                                var zipStream = entry.Open();
                                fileSystem.Save(fileName, zipStream, true);
                                savedFiles.Add(fileName);
                                count++;
                            }
                        }
                        savedFiles.Add($"Totalt: {count} filer");
                    }
                    else
                    {
                        if (GetLocation(file, out string directory))
                        {
                            using var stream = FormFileToStream(file);
                            string fileName = directory + "/" + file.FileName;
                            fileSystem.Save(fileName, stream, true);
                            savedFiles.Add(fileName);
                        }
                        else
                            nonSavedFiles.Add(file.FileName);
                    }
                }
            }
            Result = "<b>Lagrede filer:</b><br />" + string.Join("<br />", savedFiles) + "<hr />";
            if (nonSavedFiles.Count > 0)
                Result += "<b>Utelatte filer:</b><br />" + string.Join("<br />", nonSavedFiles) + "<br />";
        }

        private static bool GetLocation(IFormFile file, out string directory)
        {
            directory = "";
            if (Path.GetExtension(file.FileName) == ".xsd")
            {
                var schema = XmlSchemaRegister.Schemas.xsds.FirstOrDefault(x => x.XsdFilnavn == XsdUtils.XSD_DIRECTORY + "/" + file.FileName);
                if (schema != null)
                {
                    directory = XsdUtils.XSD_DIRECTORY;
                    return true;
                }
            }
            else if (file.FileName.Contains(".kodelister."))
            {
                directory = KodelisteUtils.KODELISTER_DIRECTORY;
                return true;
            }
            else if (Path.GetExtension(file.FileName) == ".json")
            {
                var schemaRec = XmlSchemaRegister.Schemas.xsds.FirstOrDefault(x => x.Tekster.Any(t => t.FileName == file.FileName));
                if (schemaRec != null)
                {
                    directory = TeksterUtils.TEKSTER_DIRECTORY;
                    return true;
                }
            }
            return false;
        }

        public bool allDeleted { get; set; } = false;

        public string Result { get; set; } = "";

        public string Error { get; set; } = "";
    }

}
