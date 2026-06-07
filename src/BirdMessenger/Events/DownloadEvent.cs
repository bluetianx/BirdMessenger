namespace BirdMessenger.Events;

public abstract class DownloadEvent
{
    public DownloadEvent(TusRequestOptionBase tusRequestOption)
    {
        TusRequestOption = tusRequestOption;
    }

    public TusRequestOptionBase TusRequestOption { get; }
}
