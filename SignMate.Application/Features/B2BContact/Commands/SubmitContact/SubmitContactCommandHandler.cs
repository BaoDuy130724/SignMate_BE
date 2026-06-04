using MediatR;
using SignMate.Application.DTOs.B2BContact;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.B2BContact.Commands.SubmitContact;

/// <summary>
/// Handler cho <see cref="SubmitContactCommand"/>: tạo mới một bản ghi lead B2B ở trạng thái
/// <see cref="ContactStatus.New"/> và lưu xuống cơ sở dữ liệu thông qua Unit of Work.
/// </summary>
public class SubmitContactCommandHandler : IRequestHandler<SubmitContactCommand, B2BContactLeadDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitContactCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<B2BContactLeadDto> Handle(SubmitContactCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Khởi tạo lead mới — mọi lead vừa gửi đều bắt đầu ở trạng thái "New" để đội sales theo dõi.
        var lead = new B2BContactLead
        {
            CenterName = request.CenterName,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            NumberOfLearners = request.NumberOfLearners,
            Status = ContactStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<B2BContactLead>().AddAsync(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Trả về DTO đã có Id do DB sinh ra để client xác nhận lead được lưu thành công.
        return new B2BContactLeadDto
        {
            Id = lead.Id,
            CenterName = lead.CenterName,
            ContactPerson = lead.ContactPerson,
            Phone = lead.Phone,
            Email = lead.Email,
            NumberOfLearners = lead.NumberOfLearners,
            Status = lead.Status.ToString(),
            CreatedAt = lead.CreatedAt
        };
    }
}
