using System.ComponentModel.DataAnnotations;

namespace SignMate.Application.DTOs.Course;

public class SetReferenceRequest
{
    [Required]
    public Guid SignId { get; set; }

    [Required]
    public string ReferenceKeypointData { get; set; } = null!;
}
