using Xunit;

namespace BirdMessenger.Test;

public class TusHeadersTest
{
    [Theory]
    [InlineData(null,false)]
    [InlineData("",false)]
    [InlineData(" ",false)]
    [InlineData("test",false)]
    [InlineData(TusHeaders.TusResumable,true)]
    [InlineData(TusHeaders.UploadLength,true)]
    [InlineData(TusHeaders.UploadOffset,true)]
    [InlineData(TusHeaders.UploadMetadata,true)]
    [InlineData(TusHeaders.Location,true)]
    [InlineData(TusHeaders.UploadDeferLength,true)]
    [InlineData(TusHeaders.ContentType,true)]
    [InlineData(TusHeaders.UploadChecksum,true)]
    [InlineData(TusHeaders.UploadConcat,true)]
    [InlineData(TusHeaders.UploadContentTypeValue,true)]
    [InlineData(TusHeaders.TusVersion,true)]
    [InlineData(TusHeaders.TusMaxSize,true)]
    [InlineData(TusHeaders.TusExtension,true)]
    public void TestReservedWordsTest(string header, bool isReserved)
    {
        bool isReservedTest = TusHeaders.IsReserved(header);
        
        Assert.Equal(isReserved,isReservedTest);
    }
}