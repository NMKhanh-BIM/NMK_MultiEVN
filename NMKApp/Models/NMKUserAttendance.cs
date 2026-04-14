using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NMKApp.Models;

[Table("NMK_UserAttendance")]
public class NMKUserAttendance : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("user_email")]
    public string? UserEmail { get; set; }

    [Column("work_date")]
    public DateTime WorkDate { get; set; }

    [Column("attendance_status")]
    public int AttendanceStatus { get; set; }

    [Column("check_in_at")]
    public DateTimeOffset? CheckInAt { get; set; }

    [Column("check_out_at")]
    public DateTimeOffset? CheckOutAt { get; set; }

    [Column("work_minutes")]
    public int? WorkMinutes { get; set; }

    [Column("source_leave_id")]
    public string? SourceLeaveId { get; set; }

    [Column("note")]
    public string? Note { get; set; }
}
