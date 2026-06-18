using SignMate.Application.DTOs.Practice;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Practice.Common;

/// <summary>
/// Trim rubric đánh giá thật (giám khảo Gemini) theo gói cước trước khi trả về client — paywall
/// nằm ở server, app KHÔNG tự lọc:
///   • Free  : chỉ verdict + 1 câu summary (Rubric = null).
///   • Basic : verdict + summary + các tiêu chí CHƯA đạt (status != good).
///   • Pro/B2B: đầy đủ cả 4 tiêu chí + ghi chú.
/// </summary>
internal static class JudgeTiering
{
    private static readonly (string Key, string Label, Func<JudgeResult, JudgeCriterion> Pick)[] _criteria =
    {
        ("hand_shape", "Hình dạng tay", j => j.HandShape),
        ("location", "Vị trí", j => j.Location),
        ("movement", "Chuyển động", j => j.Movement),
        ("palm_orientation", "Hướng lòng bàn tay", j => j.PalmOrientation),
    };

    /// <summary>Rubric chi tiết đã trim theo gói. Trả null cho Free (chỉ dùng Verdict + Summary ở cấp trên).</summary>
    public static JudgeRubricDto? BuildTiered(JudgeResult judge, PlanType plan)
    {
        if (plan == PlanType.Free)
            return null;

        var items = _criteria.Select(c => (c.Key, c.Label, Crit: c.Pick(judge)));

        // Basic chỉ thấy tiêu chí cần cải thiện; Pro/B2B thấy tất cả.
        if (plan == PlanType.Basic)
            items = items.Where(x => !string.Equals(x.Crit.Status, "good", StringComparison.OrdinalIgnoreCase));

        return new JudgeRubricDto
        {
            Verdict = judge.Verdict,
            Confidence = judge.Confidence,
            Summary = judge.Summary,
            Criteria = items.Select(x => new JudgeCriterionDto
            {
                Key = x.Key,
                Label = x.Label,
                Status = x.Crit.Status,
                Note = x.Crit.Note,
            }).ToList(),
        };
    }
}
