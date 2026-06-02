using SignMate.Application.DTOs.Practice;

namespace SignMate.Application.Interfaces;

public interface IPracticeService
{
    Task<StartSessionResponse> StartSessionAsync(int userId, StartSessionRequest request);
    Task<AttemptResponse> SubmitAttemptAsync(int userId, int sessionId, Stream videoStream, string fileName);
    Task EndSessionAsync(int userId, int sessionId);
    Task<List<PracticeHistoryDto>> GetHistoryAsync(int userId, int signId);
    Task<AttemptResponse> ReportResultAsync(int userId, ReportResultRequest request);
}
