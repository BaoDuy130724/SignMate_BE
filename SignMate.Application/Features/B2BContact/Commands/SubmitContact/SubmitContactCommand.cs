using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.B2BContact;

namespace SignMate.Application.Features.B2BContact.Commands.SubmitContact;

/// <summary>
/// Lệnh ghi nhận một lead B2B từ biểu mẫu liên hệ doanh nghiệp/trường học trên app mobile
/// (màn hình Contact Form — <c>POST /api/b2b/contact</c>).
/// </summary>
/// <param name="Request">Thông tin trung tâm và người liên hệ do khách hàng tiềm năng cung cấp.</param>
public record SubmitContactCommand(CreateB2BContactRequest Request) : ICommand<B2BContactLeadDto>;
