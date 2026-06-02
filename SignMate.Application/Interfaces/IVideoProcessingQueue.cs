using System.Threading.Channels;

namespace SignMate.Application.Interfaces;

public interface IVideoProcessingQueue
{
    ValueTask QueueBackgroundWorkItemAsync(int signReferenceRequestId);
    ValueTask<int> DequeueAsync(CancellationToken cancellationToken);
}
