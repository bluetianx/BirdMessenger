using System;
using System.Threading.Tasks;
using BirdMessenger.Infrastructure;

namespace BirdMessenger.Delegates
{
    public delegate void TusUploadDelegate(ITusClient source, ITusUploadContext tusUploadContext);
}
