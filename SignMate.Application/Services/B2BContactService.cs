using SignMate.Application.DTOs.B2BContact;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class B2BContactService : IB2BContactService
{
    private readonly ISignMateDbContext _db;

    public B2BContactService(ISignMateDbContext db) => _db = db;

    public async Task<B2BContactLeadDto> SubmitContactFormAsync(CreateB2BContactRequest request)
    {
        var lead = new B2BContactLead
        {
            Id = Guid.NewGuid(), CenterName = request.CenterName,
            ContactPerson = request.ContactPerson, Phone = request.Phone, 
            Email = request.Email, NumberOfLearners = request.NumberOfLearners,
            Status = ContactStatus.New, CreatedAt = DateTime.UtcNow
        };

        _db.B2BContactLeads.Add(lead);
        await _db.SaveChangesAsync();

        return new B2BContactLeadDto
        {
            Id = lead.Id, CenterName = lead.CenterName, ContactPerson = lead.ContactPerson,
            Phone = lead.Phone, Email = lead.Email, NumberOfLearners = lead.NumberOfLearners,
            Status = lead.Status.ToString(), CreatedAt = lead.CreatedAt
        };
    }
}
