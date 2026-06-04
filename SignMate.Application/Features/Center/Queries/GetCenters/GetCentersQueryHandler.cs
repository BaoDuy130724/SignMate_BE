using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Center;
using SignMate.Application.Interfaces;
using CenterEntity = SignMate.Domain.Entities.Center;

namespace SignMate.Application.Features.Center.Queries.GetCenters;

/// <summary>
/// Handler liệt kê toàn bộ trung tâm giáo dục liên kết trên hệ thống.
/// </summary>
public class GetCentersQueryHandler : IRequestHandler<GetCentersQuery, List<CenterDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCentersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<CenterDto>> Handle(GetCentersQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<CenterEntity>().Query()
            .AsNoTracking()
            .Select(c => new CenterDto
            {
                Id = c.Id,
                Name = c.Name,
                ContactPerson = c.ContactPerson,
                Phone = c.Phone,
                Email = c.Email,
                MaxSeats = c.MaxSeats,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
