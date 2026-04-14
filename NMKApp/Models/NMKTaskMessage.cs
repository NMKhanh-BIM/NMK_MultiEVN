using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NMKApp.Models;

[Table("NMK_TaskMessage")]
public class NMKTaskMessage : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Column("task_id")]
    public string TaskId { get; set; } = string.Empty;

    [Column("project_id")]
    public string? ProjectId { get; set; }

    [Column("sender_id")]
    public string SenderId { get; set; } = string.Empty;

    [Column("sender_email")]
    public string? SenderEmail { get; set; }

    [Column("message")]
    public string? Message { get; set; }

    [Column("message_type")]
    public int MessageType { get; set; } = 1;

    [Column("file_attach")]
    public string? FileAttach { get; set; }

    [Column("file_name")]
    public string? FileName { get; set; }

    [Column("file_size")]
    public long? FileSize { get; set; }

    [Column("mime_type")]
    public string? MimeType { get; set; }

    [Column("parent_message_id")]
    public string? ParentMessageId { get; set; }

    [Column("is_edited")]
    public bool IsEdited { get; set; }

    [Column("edited_at")]
    public DateTimeOffset? EditedAt { get; set; }

    [Column("is_pinned")]
    public bool IsPinned { get; set; }

    [Column("is_system")]
    public bool IsSystem { get; set; }

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }
}
