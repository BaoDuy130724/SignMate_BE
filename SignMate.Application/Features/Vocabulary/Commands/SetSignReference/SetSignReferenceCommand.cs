using MediatR;
using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Features.Vocabulary.Commands.SetSignReference;

/// <summary>
/// Lệnh gán trực tiếp dữ liệu keypoint mẫu cho một từ vựng — <c>POST /api/vocabulary/set-reference</c>.
/// Dùng khi đã có sẵn chuỗi keypoint (không qua luồng upload video + duyệt).
/// </summary>
public record SetSignReferenceCommand(SetReferenceRequest Request) : ICommand<Unit>;
