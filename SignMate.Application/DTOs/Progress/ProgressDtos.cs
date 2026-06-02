using System.ComponentModel.DataAnnotations;

namespace SignMate.Application.DTOs.Progress;

public class UpdateLessonProgressRequest
{
    [Required]
    public int LessonId { get; set; }

    [Required]
    public string Status { get; set; } = null!;

    public int WatchDurationSeconds { get; set; }
}

public class UpdateSignProgressRequest
{
    [Required]
    public int SignId { get; set; }

    public bool IsMastered { get; set; }
}
