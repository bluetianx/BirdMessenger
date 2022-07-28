namespace BirdMessenger.Delegates;

public abstract class UploadEvent
{
    public UploadEvent(TusRequestOptionBase tusRequestOption)
    {
        TusRequestOption = tusRequestOption;
    }
    public TusRequestOptionBase TusRequestOption { get; }
}