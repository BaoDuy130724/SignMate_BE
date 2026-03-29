using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Infrastructure.BackgroundJobs;

public class VideoProcessingBackgroundService : BackgroundService
{
    private readonly IVideoProcessingQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VideoProcessingBackgroundService> _logger;

    public VideoProcessingBackgroundService(
        IVideoProcessingQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<VideoProcessingBackgroundService> logger)
    {
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Video Processing Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItemId = await _taskQueue.DequeueAsync(stoppingToken);

                // Use a new scope since DbContext is Scoped
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ISignMateDbContext>();
                var aiClient = scope.ServiceProvider.GetRequiredService<IAIClientService>();

                // 1. Fetch Request
                var request = await dbContext.SignReferenceRequests.FirstOrDefaultAsync(x => x.Id == workItemId, stoppingToken);

                if (request == null || request.Status != ReferenceRequestStatus.Pending)
                    continue;

                _logger.LogInformation($"Processing SignReferenceRequest {workItemId} for SignId {request.SignId}");

                // 2. Extract Keypoints via AI Service
                var extractionResult = await aiClient.ExtractReferenceKeypointsAsync(request.VideoUrl);

                // 3. Update Request
                request.ExtractedKeypoints = extractionResult.ReferenceKeypoints;
                request.Status = ReferenceRequestStatus.ReadyForReview;

                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation($"Successfully processed video for Request {workItemId}. Ready for Admin Review.");
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was signaled
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Video Processing background queue task.");
            }
        }

        _logger.LogInformation("Video Processing Background Service is stopping.");
    }
}
