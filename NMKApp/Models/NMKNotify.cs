using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NMKApp.Models;

[Table("NMK_Notify")]
public class NMKNotify : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Column("project_id")]
    public string? ProjectId { get; set; }

    [Column("task_id")]
    public string? TaskId { get; set; }

    [Column("leave_id")]
    public string? LeaveId { get; set; }

    [Column("receiver_id")]
    public string ReceiverId { get; set; } = string.Empty;

    [Column("receiver_email")]
    public string? ReceiverEmail { get; set; }

    [Column("actor_id")]
    public string? ActorId { get; set; }

    [Column("actor_email")]
    public string? ActorEmail { get; set; }

    [Column("notify_type")]
    public int NotifyType { get; set; } = 1;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("content")]
    public string? Content { get; set; }

    [Column("action_url")]
    public string? ActionUrl { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; }

    [Column("read_at")]
    public DateTimeOffset? ReadAt { get; set; }

    [Column("priority")]
    public int Priority { get; set; }

    [Column("entity_type")]
    public int EntityType { get; set; }

    [Column("event_type")]
    public int EventType { get; set; }

    [Column("entity_id")]
    public string? EntityId { get; set; }

    [Column("is_deleted_event")]
    public bool IsDeletedEvent { get; set; }

    [Column("seen_at")]
    public DateTimeOffset? SeenAt { get; set; }
}
