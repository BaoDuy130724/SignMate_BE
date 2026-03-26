using SignMate.Application.DTOs.Practice;

namespace SignMate.Application.Interfaces;

public interface IPracticeService
{
    Task<StartSessionResponse> StartSessionAsync(Guid userId, StartSessionRequest request);
    Task<AttemptResponse> SubmitAttemptAsync(Guid userId, Guid sessionId, Stream videoStream, string fileName);
    Task EndSessionAsync(Guid userId, Guid sessionId);
    Task<List<PracticeHistoryDto>> GetHistoryAsync(Guid userId, Guid signId);
    Task<AttemptResponse> ReportResultAsync(Guid userId, ReportResultRequest request);
}
