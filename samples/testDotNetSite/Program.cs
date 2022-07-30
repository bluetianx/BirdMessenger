using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TestDotNetSite.Endpoints;
using tusdotnet;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(kestrel => { kestrel.Limits.MaxRequestBodySize = null; });

builder.Services.AddSingleton(CreateTusConfiguration);

var app = builder.Build();


app.UseDefaultFiles();
app.UseStaticFiles();

app.UseTus(httpContext => httpContext.RequestServices.GetRequiredService<DefaultTusConfiguration>());
//app.MapGet("/files/{fileId}", DownloadFileEndpoint.HandleRoute);

app.Run();


static DefaultTusConfiguration CreateTusConfiguration(IServiceProvider serviceProvider)
{
    var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
    string dirName = @"tusfiles";
    if (!Directory.Exists(dirName))
    {
        Directory.CreateDirectory(dirName);
    }

    return new DefaultTusConfiguration
    {
        UrlPath = "/files",
        Store = new TusDiskStore(dirName),
        MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues,
        UsePipelinesIfAvailable = true,

        // Set an expiration time where incomplete files can no longer be updated.
        // This value can either be absolute or sliding.
        // Absolute expiration will be saved per file on create
        // Sliding expiration will be saved per file on create and updated on each patch/update.
        Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(5))
    };
}