using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Vocabulary;

namespace SignMate.Application.Features.Vocabulary.Commands.UploadReferenceVideo;

/// <summary>
/// Lệnh tải lên video mẫu cho một từ vựng và khởi tạo luồng xử lý ngầm tách keypoint —
/// <c>POST /api/vocabulary/{signId}/upload-reference</c>.
/// Controller chịu trách nhiệm bóc <c>IFormFile</c> thành stream để tầng Application không phụ thuộc
/// kiểu của ASP.NET (giữ ranh giới Clean Architecture).
/// </summary>
/// <param name="SignId">Id từ vựng nhận video mẫu.</param>
/// <param name="UploaderId">Id giáo viên tải lên (lấy từ JWT).</param>
/// <param name="Content">Luồng dữ liệu video.</param>
/// <param name="ContentType">Kiểu MIME của video (mặc định video/mp4 nếu trống).</param>
public record UploadReferenceVideoCommand(int SignId, int UploaderId, Stream Content, string? ContentType)
    : ICommand<UploadReferenceResultDto>;
