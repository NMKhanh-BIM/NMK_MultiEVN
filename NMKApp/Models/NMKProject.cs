using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NMKApp.Models;

[Table("NMK_Project")]
public class NMKProject : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("key")]
    public string? Key { get; set; }

    [Column("color")]
    public string? Color { get; set; }

    [Column("create_by")]
    public string? CreateBy { get; set; }

    [Column("update_by")]
    public string? UpdateBy { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("avatar")]
    public string? Avatar { get; set; }

    [Column("revit_version")]
    public string? RevitVersion { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }
}
