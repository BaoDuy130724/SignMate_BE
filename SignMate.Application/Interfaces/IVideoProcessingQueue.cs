using System.Threading.Channels;

namespace SignMate.Application.Interfaces;

public interface IVideoProcessingQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Guid signReferenceRequestId);
    ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken);
}
