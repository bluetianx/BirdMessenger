using BirdMessenger.Infrastructure;

namespace BirdMessenger.Delegates
{
    public delegate int TusChunkUploadSizeDelegate(ITusClient source, ITusUploadContext tusUploadContext);
}
