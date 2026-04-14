using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NMKApp.Models;

[Table("NMK_Leave")]
public class NMKLeave : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Column("project_id")]
    public string? ProjectId { get; set; }

    [Column("requester_id")]
    public string RequesterId { get; set; } = string.Empty;

    [Column("requester_email")]
    public string? RequesterEmail { get; set; }

    [Column("approver_id")]
    public string? ApproverId { get; set; }

    [Column("approver_email")]
    public string? ApproverEmail { get; set; }

    [Column("leave_type")]
    public int LeaveType { get; set; } = 1;

    [Column("status")]
    public int Status { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [Column("date_from")]
    public DateTimeOffset DateFrom { get; set; }

    [Column("date_to")]
    public DateTimeOffset DateTo { get; set; }

    [Column("total_days")]
    public decimal? TotalDays { get; set; }

    [Column("is_half_day")]
    public bool IsHalfDay { get; set; }

    [Column("half_day_slot")]
    public int? HalfDaySlot { get; set; }

    [Column("approved_at")]
    public DateTimeOffset? ApprovedAt { get; set; }

    [Column("rejected_at")]
    public DateTimeOffset? RejectedAt { get; set; }

    [Column("cancelled_at")]
    public DateTimeOffset? CancelledAt { get; set; }

    [Column("attachment_url")]
    public string? AttachmentUrl { get; set; }
}
