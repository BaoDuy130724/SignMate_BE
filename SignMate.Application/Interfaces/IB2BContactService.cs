using SignMate.Application.DTOs.B2BContact;

namespace SignMate.Application.Interfaces;

public interface IB2BContactService
{
    Task<B2BContactLeadDto> SubmitContactFormAsync(CreateB2BContactRequest request);
}
