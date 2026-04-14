using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NMKApp.Models;

[Table("NMK_Task")]
public class NMKTask : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("project_id")]
    public string? ProjectId { get; set; }

    [Column("assignee_to")]
    public string? AssigneeTo { get; set; }

    [Column("assignee_email")]
    public string? AssigneeEmail { get; set; }

    [Column("date_start")]
    public DateTimeOffset? DateStart { get; set; }

    [Column("date_end")]
    public DateTimeOffset? DateEnd { get; set; }

    [Column("detail")]
    public string? Detail { get; set; }

    [Column("color")]
    public string? Color { get; set; }

    [Column("status")]
    public int? Status { get; set; }

    [Column("assignee_by")]
    public string? AssigneeBy { get; set; }

    [Column("assignee_by_email")]
    public string? AssigneeByEmail { get; set; }

    [Column("update_by")]
    public string? UpdateBy { get; set; }

    [Column("date_complete")]
    public DateTimeOffset? DateComplete { get; set; }

    [Column("parent_id")]
    public string? ParentId { get; set; }

    [Column("date_checked")]
    public DateTimeOffset? DateChecked { get; set; }

    [Column("area")]
    public double? Area { get; set; }

    [Column("is_interrupted")]
    public bool? IsInterrupted { get; set; }

    [Column("list_interrupted")]
    public string? ListInterrupted { get; set; }

    [Column("date_started")]
    public DateTimeOffset? DateStarted { get; set; }

    [Column("date_accepted")]
    public DateTimeOffset? DateAccepted { get; set; }

    [Column("is_onlychecked")]
    public bool? IsOnlyChecked { get; set; }

    [Column("folder")]
    public string? Folder { get; set; }

    [Column("file_attach")]
    public string? FileAttach { get; set; }
}
