using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BirdMessenger
{
    public interface ITusClient
    {
         Task<bool> Upload(Uri url,FileInfo uploadFileInfo,CancellationToken ct=default(CancellationToken))
    }
}