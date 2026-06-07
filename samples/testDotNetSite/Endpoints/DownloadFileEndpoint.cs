using System.Text;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace TestDotNetSite.Endpoints;

public static class DownloadFileEndpoint
{
    public static async Task HandleRoute(HttpContext context)
    {
        var config = context.RequestServices.GetRequiredService<DefaultTusConfiguration>();

        if (config.Store is not ITusReadableStore store)
        {
            return;
        }

        var fileId = (string?)context.Request.RouteValues["fileId"];
        var file = await store.GetFileAsync(fileId, context.RequestAborted);

        if (file == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync($"File with id {fileId} was not found.", context.RequestAborted);
            return;
        }

        var fileStream = await file.GetContentAsync(context.RequestAborted);
        var metadata = await file.GetMetadataAsync(context.RequestAborted);

        context.Response.ContentType = GetContentTypeOrDefault(metadata);

        if (metadata.TryGetValue("name", out var nameMeta))
        {
            context.Response.Headers.Add("Content-Disposition",
                new[] { $"attachment; filename=\"{nameMeta.GetString(Encoding.UTF8)}\"" });
        }

        var rangeHeader = context.Request.Headers.Range;
        if (!string.IsNullOrEmpty(rangeHeader) && fileStream.Length > 0)
        {
            var range = rangeHeader.ToString();
            if (range.StartsWith("bytes="))
            {
                var rangeSpec = range.Substring("bytes=".Length);
                var parts = rangeSpec.Split('-');
                if (long.TryParse(parts[0], out var from))
                {
                    var to = fileStream.Length - 1;
                    if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                    {
                        long.TryParse(parts[1], out to);
                    }

                    var length = to - from + 1;
                    context.Response.StatusCode = 206;
                    context.Response.Headers.Add("Content-Range", $"bytes {from}-{to}/{fileStream.Length}");
                    context.Response.ContentLength = length;

                    fileStream.Seek(from, SeekOrigin.Begin);
                    await fileStream.CopyToAsync(context.Response.Body, 81920, context.RequestAborted);
                    fileStream.Dispose();
                    return;
                }
            }
        }

        context.Response.ContentLength = fileStream.Length;
        using (fileStream)
        {
            await fileStream.CopyToAsync(context.Response.Body, 81920, context.RequestAborted);
        }
    }

    private static string GetContentTypeOrDefault(Dictionary<string, Metadata> metadata)
    {
        if (metadata.TryGetValue("contentType", out var contentType))
        {
            return contentType.GetString(Encoding.UTF8);
        }

        return "application/octet-stream";
    }
}