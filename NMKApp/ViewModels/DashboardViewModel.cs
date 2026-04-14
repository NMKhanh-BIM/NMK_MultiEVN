using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMKApp.Models;
using System.Collections.ObjectModel;

namespace NMKApp.ViewModels;

public partial class ProjectCardViewModel : ObservableObject
{
    [ObservableProperty] private string id = string.Empty;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string createdAt = string.Empty;
    [ObservableProperty] private string revitVersion = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string avatar = string.Empty;

    public ObservableCollection<TaskCardViewModel> Tasks { get; } = new();
}

public partial class TaskCardViewModel : ObservableObject
{
    [ObservableProperty] private string id = string.Empty;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private int status;
    [ObservableProperty] private string statusText = string.Empty;
    [ObservableProperty] private string projectId = string.Empty;
    [ObservableProperty] private string projectName = string.Empty;
    [ObservableProperty] private DateTimeOffset? dateStart;
    [ObservableProperty] private DateTimeOffset? dateEnd;
    [ObservableProperty] private string assigneeEmail = string.Empty;
    [ObservableProperty] private string assignerEmail = string.Empty;
    [ObservableProperty] private string statusColor = "#9E9E9E";

    public string StartText => DateStart?.LocalDateTime.ToString("dd/MM/yyyy HH:mm") ?? "";
    public string EndText => DateEnd?.LocalDateTime.ToString("dd/MM/yyyy HH:mm") ?? "";
}

public partial class DashboardViewModel : ObservableObject
{
    private readonly MainViewModel _mainVM;

    [ObservableProperty] private bool isAssignedByMe = true;
    [ObservableProperty] private bool isToday = true;
    [ObservableProperty] private int selectedMonth = DateTime.Now.Month;
    [ObservableProperty] private int selectedYear = DateTime.Now.Year;
    [ObservableProperty] private string? selectedProjectFilter;
    [ObservableProperty] private int? selectedStatusFilter;
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<ProjectCardViewModel> ProjectCards { get; } = new();
    public ObservableCollection<TaskCardViewModel> TaskCards { get; } = new();
    public ObservableCollection<NMKProject> Projects { get; } = new();
    public ObservableCollection<string> StatusFilters { get; } = new()
    {
        "All", "New", "Accepted", "Start", "Completed", "Checked"
    };

    public DashboardViewModel(MainViewModel mainVM)
    {
        _mainVM = mainVM;
    }

    public async Task LoadDataAsync()
    {
        if (_mainVM.CurrentUser == null) return;
        IsLoading = true;
        try
        {
            // Load projects
            var projects = await _mainVM.SupabaseService.GetProjectsAsync();
            Projects.Clear();
            foreach (var p in projects) Projects.Add(p);

            // Calculate date range
            DateTimeOffset start, end;
            if (IsToday)
            {
                start = new DateTimeOffset(DateTime.Today);
                end = start.AddDays(1).AddTicks(-1);
            }
            else
            {
                start = new DateTimeOffset(new DateTime(SelectedYear, SelectedMonth, 1));
                end = start.AddMonths(1).AddTicks(-1);
            }

            // Load tasks
            var tasks = await _mainVM.SupabaseService.GetTasksByDateRangeAsync(
                _mainVM.CurrentUser.Id, start, end, IsAssignedByMe);

            // Apply status filter
            if (SelectedStatusFilter.HasValue && SelectedStatusFilter.Value >= 0)
                tasks = tasks.Where(t => t.Status == SelectedStatusFilter.Value).ToList();

            // Apply project filter
            if (!string.IsNullOrEmpty(SelectedProjectFilter))
                tasks = tasks.Where(t => t.ProjectId == SelectedProjectFilter).ToList();

            // Build project cards with tasks
            ProjectCards.Clear();
            TaskCards.Clear();

            var projectIds = tasks.Select(t => t.ProjectId).Distinct().ToList();
            var relevantProjects = projects.Where(p => projectIds.Contains(p.Id)).ToList();

            foreach (var project in relevantProjects)
            {
                var card = new ProjectCardViewModel
                {
                    Id = project.Id,
                    Name = project.Name ?? "",
                    CreatedAt = project.CreatedAt?.LocalDateTime.ToString("dd/MM/yyyy") ?? "",
                    RevitVersion = project.RevitVersion ?? "",
                    Description = project.Description ?? "",
                    Avatar = project.Avatar ?? ""
                };

                var projectTasks = tasks.Where(t => t.ProjectId == project.Id);
                foreach (var task in projectTasks)
                {
                    var tcvm = CreateTaskCard(task, project.Name ?? "");
                    card.Tasks.Add(tcvm);
                    TaskCards.Add(tcvm);
                }
                ProjectCards.Add(card);
            }

            // Tasks without project
            var orphanTasks = tasks.Where(t => string.IsNullOrEmpty(t.ProjectId) || !projectIds.Contains(t.ProjectId));
            foreach (var task in orphanTasks)
            {
                TaskCards.Add(CreateTaskCard(task, "No Project"));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard load error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private TaskCardViewModel CreateTaskCard(NMKTask task, string projectName)
    {
        var statusInt = task.Status ?? 0;
        return new TaskCardViewModel
        {
            Id = task.Id,
            Name = task.Name ?? "",
            Status = statusInt,
            StatusText = statusInt switch
            {
                0 => "New",
                1 => "Accepted",
                2 => "Start",
                3 => "Completed",
                4 => "Checked",
                5 => "Rejected",
                _ => "Unknown"
            },
            StatusColor = statusInt switch
            {
                0 => "#4CAF50",
                1 => "#FF9800",
                2 => "#9C27B0",
                3 => "#2196F3",
                4 => "#607D8B",
                5 => "#F44336",
                _ => "#9E9E9E"
            },
            ProjectId = task.ProjectId ?? "",
            ProjectName = projectName,
            DateStart = task.DateStart,
            DateEnd = task.DateEnd,
            AssigneeEmail = task.AssigneeEmail ?? "",
            AssignerEmail = task.AssigneeByEmail ?? ""
        };
    }

    [RelayCommand]
    private async Task ToggleAssignedByMe()
    {
        IsAssignedByMe = true;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ToggleAssignedToMe()
    {
        IsAssignedByMe = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ToggleToday()
    {
        IsToday = IsToday;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task FilterChanged()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task AcceptTask(string taskId)
    {
        if (_mainVM.CurrentUser == null) return;
        try
        {
            await _mainVM.SupabaseService.UpdateTaskStatusAsync(taskId, (int)Models.TaskStatus.Accepted);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Accept task error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task StartTask(string taskId)
    {
        if (_mainVM.CurrentUser == null) return;
        try
        {
            await _mainVM.SupabaseService.UpdateTaskStatusAsync(taskId, (int)Models.TaskStatus.Started);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Start task error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CompleteTask(string taskId)
    {
        if (_mainVM.CurrentUser == null) return;
        try
        {
            await _mainVM.SupabaseService.UpdateTaskStatusAsync(taskId, (int)Models.TaskStatus.Completed);

            // Find task info for email
            var taskCard = TaskCards.FirstOrDefault(t => t.Id == taskId);
            if (taskCard != null && !string.IsNullOrEmpty(taskCard.AssignerEmail))
            {
                try
                {
                    _mainVM.OutlookService.SendTaskCompletedEmail(
                        taskCard.AssignerEmail,
                        taskCard.Name,
                        taskCard.ProjectName,
                        _mainVM.CurrentUserEmail);
                }
                catch (Exception emailEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Email error: {emailEx.Message}");
                }

                // Create notification
                await _mainVM.SupabaseService.CreateNotificationAsync(new NMKNotify
                {
                    TaskId = taskId,
                    ProjectId = taskCard.ProjectId,
                    ReceiverId = "", // would need assigner's user ID
                    ReceiverEmail = taskCard.AssignerEmail,
                    ActorId = _mainVM.CurrentUser.Id,
                    ActorEmail = _mainVM.CurrentUserEmail,
                    NotifyType = (int)Models.NotifyType.Task,
                    Title = $"Task Completed: {taskCard.Name}",
                    Content = $"{_mainVM.CurrentUserEmail} completed task '{taskCard.Name}'",
                    EntityType = (int)Models.EntityType.Task,
                    EventType = (int)Models.EventType.StatusChanged,
                    EntityId = taskId
                });
            }

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Complete task error: {ex.Message}");
        }
    }
}
