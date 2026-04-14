using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NMKApp.Models;

[Table("NMK_User")]
public class NMKUser : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("team")]
    public string? Team { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("create_by")]
    public string? CreateBy { get; set; }

    [Column("update_by")]
    public string? UpdateBy { get; set; }

    [Column("user_role")]
    public string? UserRole { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("avatar")]
    public string? Avatar { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    [Column("employment_status")]
    public int EmploymentStatus { get; set; } = 1;

    [Column("availability_status")]
    public int AvailabilityStatus { get; set; }

    [Column("availability_note")]
    public string? AvailabilityNote { get; set; }

    [Column("availability_until")]
    public DateTimeOffset? AvailabilityUntil { get; set; }

    [Column("last_seen_at")]
    public DateTimeOffset? LastSeenAt { get; set; }
}
