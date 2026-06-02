using SignMate.Application.DTOs.B2BContact;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class B2BContactService : IB2BContactService
{
    private readonly IUnitOfWork _unitOfWork;

    public B2BContactService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<B2BContactLeadDto> SubmitContactFormAsync(CreateB2BContactRequest request)
    {
        var lead = new B2BContactLead
        {
            Id = 0, CenterName = request.CenterName,
            ContactPerson = request.ContactPerson, Phone = request.Phone, 
            Email = request.Email, NumberOfLearners = request.NumberOfLearners,
            Status = ContactStatus.New, CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<B2BContactLead>().AddAsync(lead);
        await _unitOfWork.SaveChangesAsync();

        return new B2BContactLeadDto
        {
            Id = lead.Id, CenterName = lead.CenterName, ContactPerson = lead.ContactPerson,
            Phone = lead.Phone, Email = lead.Email, NumberOfLearners = lead.NumberOfLearners,
            Status = lead.Status.ToString(), CreatedAt = lead.CreatedAt
        };
    }
}
