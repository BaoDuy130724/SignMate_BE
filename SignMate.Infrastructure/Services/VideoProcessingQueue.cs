using System.Threading.Channels;
using SignMate.Application.Interfaces;

namespace SignMate.Infrastructure.Services;

public class VideoProcessingQueue : IVideoProcessingQueue
{
    private readonly Channel<int> _queue;

    public VideoProcessingQueue()
    {
        // Define capacity, drop rule etc if needed. 100 requests should be ample.
        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<int>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(int signReferenceRequestId)
    {
        await _queue.Writer.WriteAsync(signReferenceRequestId);
    }

    public async ValueTask<int> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}
