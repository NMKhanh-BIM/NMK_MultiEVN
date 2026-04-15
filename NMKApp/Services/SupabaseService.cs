using Supabase;
using NMKApp.Models;
using Supabase.Postgrest;
using Supabase.Postgrest.Responses;

namespace NMKApp.Services;

public class SupabaseService
{
    private Supabase.Client? _client;
    private readonly string _url;
    private readonly string _key;

    public SupabaseService(string url, string key)
    {
        _url = url;
        _key = key;
    }

    public async Task InitializeAsync()
    {
        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        };
        _client = new Supabase.Client(_url, _key, options);
        await _client.InitializeAsync();
    }

    private Supabase.Client Client => _client ?? throw new InvalidOperationException("Supabase not initialized. Call InitializeAsync first.");

    // ===== Projects =====
    public async Task<List<NMKProject>> GetProjectsAsync()
    {
        var response = await Client.From<NMKProject>()
            .Filter("is_deleted", Constants.Operator.Equals, "false")
            .Order("created_at", Constants.Ordering.Descending)
            .Get();
        return response.Models;
    }

    public async Task<NMKProject?> GetProjectByIdAsync(string id)
    {
        var response = await Client.From<NMKProject>()
            .Filter("id", Constants.Operator.Equals, id)
            .Single();
        return response;
    }

    // ===== Users =====
    public async Task<List<NMKUser>> GetUsersAsync()
    {
        var response = await Client.From<NMKUser>()
            .Filter("is_deleted", Constants.Operator.Equals, "false")
            .Order("name", Constants.Ordering.Ascending)
            .Get();
        return response.Models;
    }

    public async Task<NMKUser?> GetUserByEmailAsync(string email)
    {
        var response = await Client.From<NMKUser>()
            .Filter("email", Constants.Operator.Equals, email)
            .Single();
        return response;
    }

    public async Task<NMKUser?> GetUserByIdAsync(string id)
    {
        var response = await Client.From<NMKUser>()
            .Filter("id", Constants.Operator.Equals, id)
            .Single();
        return response;
    }

    // ===== Tasks =====
    public async Task<List<NMKTask>> GetTasksAsync(string? projectId = null)
    {
        if (!string.IsNullOrEmpty(projectId))
        {
            var response = await Client.From<NMKTask>()
                .Filter("project_id", Constants.Operator.Equals, projectId)
                .Order("date_start", Constants.Ordering.Descending)
                .Get();
            return response.Models;
        }
        else
        {
            var response = await Client.From<NMKTask>()
                .Order("date_start", Constants.Ordering.Descending)
                .Get();
            return response.Models;
        }
    }

    public async Task<List<NMKTask>> GetTasksByAssigneeAsync(string userId)
    {
        var response = await Client.From<NMKTask>()
            .Filter("assignee_to", Constants.Operator.Equals, userId)
            .Order("date_start", Constants.Ordering.Descending)
            .Get();
        return response.Models;
    }

    public async Task<List<NMKTask>> GetTasksByAssignerAsync(string userId)
    {
        var response = await Client.From<NMKTask>()
            .Filter("assignee_by", Constants.Operator.Equals, userId)
            .Order("date_start", Constants.Ordering.Descending)
            .Get();
        return response.Models;
    }

    public async Task<List<NMKTask>> GetTasksByDateRangeAsync(string userId, DateTimeOffset start, DateTimeOffset end, bool assignedByMe)
    {
        var filterField = assignedByMe ? "assignee_by" : "assignee_to";
        var response = await Client.From<NMKTask>()
            .Filter(filterField, Constants.Operator.Equals, userId)
            .Filter("date_start", Constants.Operator.LessThanOrEqual, end.ToString("o"))
            .Filter("date_end", Constants.Operator.GreaterThanOrEqual, start.ToString("o"))
            .Order("date_start", Constants.Ordering.Ascending)
            .Get();
        return response.Models;
    }

    public async Task<NMKTask> CreateTaskAsync(NMKTask task)
    {
        var response = await Client.From<NMKTask>().Insert(task);
        return response.Models.First();
    }

    public async Task<NMKTask> UpdateTaskAsync(NMKTask task)
    {
        var response = await Client.From<NMKTask>()
            .Filter("id", Constants.Operator.Equals, task.Id)
            .Update(task);
        return response.Models.First();
    }

    public async Task UpdateTaskStatusAsync(string taskId, int status)
    {
        var task = await Client.From<NMKTask>()
            .Filter("id", Constants.Operator.Equals, taskId)
            .Single();
        if (task != null)
        {
            task.Status = status;
            task.UpdatedAt = DateTimeOffset.Now;
            if (status == (int)Models.TaskStatus.Accepted)
                task.DateAccepted = DateTimeOffset.Now;
            else if (status == (int)Models.TaskStatus.Start)
                task.DateStarted = DateTimeOffset.Now;
            else if (status == (int)Models.TaskStatus.Complete)
                task.DateComplete = DateTimeOffset.Now;
            else if (status == (int)Models.TaskStatus.Checked)
                task.DateChecked = DateTimeOffset.Now;
            await Client.From<NMKTask>()
                .Filter("id", Constants.Operator.Equals, taskId)
                .Update(task);
        }
    }

    // ===== Leave =====
    public async Task<List<NMKLeave>> GetLeavesByRequesterAsync(string userId)
    {
        var response = await Client.From<NMKLeave>()
            .Filter("requester_id", Constants.Operator.Equals, userId)
            .Order("created_at", Constants.Ordering.Descending)
            .Get();
        return response.Models;
    }

    public async Task<List<NMKLeave>> GetLeavesByApproverAsync(string userId)
    {
        var response = await Client.From<NMKLeave>()
            .Filter("approver_id", Constants.Operator.Equals, userId)
            .Order("created_at", Constants.Ordering.Descending)
            .Get();
        return response.Models;
    }

    public async Task<NMKLeave> CreateLeaveAsync(NMKLeave leave)
    {
        var response = await Client.From<NMKLeave>().Insert(leave);
        return response.Models.First();
    }

    public async Task<NMKLeave> UpdateLeaveAsync(NMKLeave leave)
    {
        var response = await Client.From<NMKLeave>()
            .Filter("id", Constants.Operator.Equals, leave.Id)
            .Update(leave);
        return response.Models.First();
    }

    public async Task ApproveLeaveAsync(string leaveId)
    {
        var leave = await Client.From<NMKLeave>()
            .Filter("id", Constants.Operator.Equals, leaveId)
            .Single();
        if (leave != null)
        {
            leave.Status = (int)LeaveStatus.Approved;
            leave.ApprovedAt = DateTimeOffset.Now;
            leave.UpdatedAt = DateTimeOffset.Now;
            await Client.From<NMKLeave>()
                .Filter("id", Constants.Operator.Equals, leaveId)
                .Update(leave);
        }
    }

    public async Task RejectLeaveAsync(string leaveId, string? note = null)
    {
        var leave = await Client.From<NMKLeave>()
            .Filter("id", Constants.Operator.Equals, leaveId)
            .Single();
        if (leave != null)
        {
            leave.Status = (int)LeaveStatus.Rejected;
            leave.RejectedAt = DateTimeOffset.Now;
            leave.UpdatedAt = DateTimeOffset.Now;
            if (note != null) leave.Note = note;
            await Client.From<NMKLeave>()
                .Filter("id", Constants.Operator.Equals, leaveId)
                .Update(leave);
        }
    }

    // ===== Notifications =====
    public async Task<List<NMKNotify>> GetNotificationsAsync(string userId)
    {
        var response = await Client.From<NMKNotify>()
            .Filter("receiver_id", Constants.Operator.Equals, userId)
            .Order("created_at", Constants.Ordering.Descending)
            .Limit(50)
            .Get();
        return response.Models;
    }

    public async Task<int> GetUnreadNotificationCountAsync(string userId)
    {
        var response = await Client.From<NMKNotify>()
            .Filter("receiver_id", Constants.Operator.Equals, userId)
            .Filter("is_read", Constants.Operator.Equals, "false")
            .Get();
        return response.Models.Count;
    }

    public async Task<NMKNotify> CreateNotificationAsync(NMKNotify notify)
    {
        var response = await Client.From<NMKNotify>().Insert(notify);
        return response.Models.First();
    }

    public async Task MarkNotificationReadAsync(string notifyId)
    {
        var notify = await Client.From<NMKNotify>()
            .Filter("id", Constants.Operator.Equals, notifyId)
            .Single();
        if (notify != null)
        {
            notify.IsRead = true;
            notify.ReadAt = DateTimeOffset.Now;
            await Client.From<NMKNotify>()
                .Filter("id", Constants.Operator.Equals, notifyId)
                .Update(notify);
        }
    }

    public async Task MarkNotificationSeenAsync(string notifyId)
    {
        var notify = await Client.From<NMKNotify>()
            .Filter("id", Constants.Operator.Equals, notifyId)
            .Single();
        if (notify != null)
        {
            notify.SeenAt = DateTimeOffset.Now;
            await Client.From<NMKNotify>()
                .Filter("id", Constants.Operator.Equals, notifyId)
                .Update(notify);
        }
    }

    // ===== Task Messages =====
    public async Task<List<NMKTaskMessage>> GetTaskMessagesAsync(string taskId)
    {
        var response = await Client.From<NMKTaskMessage>()
            .Filter("task_id", Constants.Operator.Equals, taskId)
            .Filter("deleted_at", Constants.Operator.Is, "null")
            .Order("created_at", Constants.Ordering.Ascending)
            .Get();
        return response.Models;
    }

    public async Task<NMKTaskMessage> CreateTaskMessageAsync(NMKTaskMessage message)
    {
        var response = await Client.From<NMKTaskMessage>().Insert(message);
        return response.Models.First();
    }

    // ===== Attendance =====
    public async Task<List<NMKUserAttendance>> GetAttendanceAsync(string userId, DateTime from, DateTime to)
    {
        var response = await Client.From<NMKUserAttendance>()
            .Filter("user_id", Constants.Operator.Equals, userId)
            .Filter("work_date", Constants.Operator.GreaterThanOrEqual, from.ToString("yyyy-MM-dd"))
            .Filter("work_date", Constants.Operator.LessThanOrEqual, to.ToString("yyyy-MM-dd"))
            .Order("work_date", Constants.Ordering.Ascending)
            .Get();
        return response.Models;
    }

    public async Task<NMKUserAttendance> UpsertAttendanceAsync(NMKUserAttendance attendance)
    {
        var response = await Client.From<NMKUserAttendance>().Upsert(attendance);
        return response.Models.First();
    }

    // ===== Project Avatar =====
    public async Task<string?> UpdateProjectAvatarAsync(string projectId, string localFilePath)
    {
        string avatarUrl;
        try
        {
            // Try Supabase Storage upload
            var ext        = System.IO.Path.GetExtension(localFilePath);
            var remotePath = $"project_{projectId}{ext}";
            var bytes      = await System.IO.File.ReadAllBytesAsync(localFilePath);
            var contentType = ext.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"            => "image/png",
                ".gif"            => "image/gif",
                ".bmp"            => "image/bmp",
                _                 => "image/jpeg"
            };
            await Client.Storage.From("project-avatars")
                .Upload(bytes, remotePath,
                    new Supabase.Storage.FileOptions { ContentType = contentType, Upsert = true });
            avatarUrl = Client.Storage.From("project-avatars").GetPublicUrl(remotePath);
        }
        catch
        {
            // Fallback: store local file URI
            avatarUrl = new Uri(localFilePath).AbsoluteUri;
        }

        try
        {
            var project = await Client.From<NMKProject>()
                .Filter("id", Constants.Operator.Equals, projectId)
                .Single();
            if (project != null)
            {
                project.Avatar    = avatarUrl;
                project.UpdatedAt = DateTimeOffset.Now;
                await Client.From<NMKProject>()
                    .Filter("id", Constants.Operator.Equals, projectId)
                    .Update(project);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateProjectAvatarAsync DB error: {ex.Message}");
        }

        return avatarUrl;
    }
}
