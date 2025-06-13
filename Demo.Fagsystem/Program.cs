using Buf.Meldingsutveksler.SkjemaVerktoy;
using Demo.Fagsystem.Models.Demodata;
using Demo.Fagsystem.Models.FagsystemSimulator.Config;
using Demo.Fagsystem.Models.FagsystemSimulator.Storage;
using Demo.Fagsystem.Models.Utils;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConnection:blobServiceUri"]!).WithName("StorageConnection");
    clientBuilder.AddQueueServiceClient(builder.Configuration["StorageConnection:queueServiceUri"]!).WithName("StorageConnection");
    clientBuilder.AddTableServiceClient(builder.Configuration["StorageConnection:tableServiceUri"]!).WithName("StorageConnection");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

BlobContainerConfig? defsConfig = app.Configuration.GetSection("definisjoner").Get<BlobContainerConfig>()
    ?? throw new Exception("seksjon 'definisjoner' mangler i config)");
var fileSystemDefs = new BlobStorageFileSystem(defsConfig);

SkjemaVedrktoyInitializer.Init(fileSystemDefs);

RenderUtils.SetCustomRenderers();
DemodataGenerator.InitFagsystemer(app.Configuration);


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
