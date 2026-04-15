using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NMKApp.Models;
using System.Collections.ObjectModel;

namespace NMKApp.ViewModels;

// ─── Status Filter Item ────────────────────────────────────────────────────────
public partial class StatusFilterItem : ObservableObject
{
    [ObservableProperty] private bool isChecked = true;
    public string Label { get; set; } = string.Empty;
    public int? StatusValue { get; set; } // null = "All"
}

// ─── Project Card ViewModel ────────────────────────────────────────────────────
public partial class ProjectCardViewModel : ObservableObject
{
    [ObservableProperty] private string id = string.Empty;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string createdAt = string.Empty;
    [ObservableProperty] private string revitVersion = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string avatar = string.Empty;
    [ObservableProperty] private string color = "#1976D2";

    public ObservableCollection<TaskCardViewModel> Tasks { get; } = new();
}

// ─── Task Card ViewModel ───────────────────────────────────────────────────────
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
    public string EndText   => DateEnd?.LocalDateTime.ToString("dd/MM/yyyy HH:mm") ?? "";
}

// ─── Dashboard ViewModel ───────────────────────────────────────────────────────
public partial class DashboardViewModel : ObservableObject
{
    private readonly MainViewModel _mainVM;
    private bool _updatingAllFilter;
    private readonly Dictionary<string, TaskCardViewModel> _taskDict = new();

    private static readonly string[] ProjectColorPalette =
    [
        "#1976D2", "#7B1FA2", "#00897B", "#E64A19",
        "#388E3C", "#F57C00", "#0288D1", "#5D4037",
        "#455A64", "#C62828"
    ];

    [ObservableProperty] private bool isAssignedByMe = true;
    [ObservableProperty] private bool isToday = true;
    [ObservableProperty] private int selectedMonth = DateTime.Now.Month;
    [ObservableProperty] private int selectedYear = DateTime.Now.Year;
    [ObservableProperty] private string? selectedProjectFilter;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string statusFilterText = "Filter by status";

    public bool IsAdmin => _mainVM.CurrentUser?.UserRole is "Admin" or "AdminApp";

    public ObservableCollection<ProjectCardViewModel> ProjectCards { get; } = new();
    public ObservableCollection<NMKProject> Projects { get; } = new();
    public ObservableCollection<StatusFilterItem> StatusFilterItems { get; } = new();

    public DashboardViewModel(MainViewModel mainVM)
    {
        _mainVM = mainVM;
        InitStatusFilters();
    }

    private void InitStatusFilters()
    {
        var allItem = new StatusFilterItem { Label = "Task All", StatusValue = null, IsChecked = true };
        allItem.PropertyChanged += OnAllFilterChanged;
        StatusFilterItems.Add(allItem);

        StatusFilterItem[] items =
        [
            new() { Label = "Task Accepted",  StatusValue = 7 },
            new() { Label = "Task Start",     StatusValue = 6 },
            new() { Label = "Task New",       StatusValue = 3 },
            new() { Label = "Task Checked",   StatusValue = 4 },
            new() { Label = "Task ReChecked", StatusValue = 5 },
            new() { Label = "Task Complete",  StatusValue = 0 },
        ];

        foreach (var item in items)
        {
            item.PropertyChanged += OnStatusFilterChanged;
            StatusFilterItems.Add(item);
        }
        UpdateStatusFilterText();
    }

    private void OnAllFilterChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(StatusFilterItem.IsChecked) || _updatingAllFilter) return;
        _updatingAllFilter = true;
        var isAll = StatusFilterItems[0].IsChecked;
        foreach (var item in StatusFilterItems.Skip(1))
            item.IsChecked = isAll;
        _updatingAllFilter = false;
        UpdateStatusFilterText();
        _ = LoadDataAsync();
    }

    private void OnStatusFilterChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(StatusFilterItem.IsChecked) || _updatingAllFilter) return;
        _updatingAllFilter = true;
        StatusFilterItems[0].IsChecked = StatusFilterItems.Skip(1).All(i => i.IsChecked);
        _updatingAllFilter = false;
        UpdateStatusFilterText();
        _ = LoadDataAsync();
    }

    private void UpdateStatusFilterText()
    {
        if (StatusFilterItems[0].IsChecked)
        {
            StatusFilterText = "Task All";
            return;
        }
        var checkedItems = StatusFilterItems.Skip(1).Where(i => i.IsChecked).ToList();
        StatusFilterText = checkedItems.Count == 0 ? "No status"
            : checkedItems.Count == 1 ? checkedItems[0].Label
            : $"{checkedItems.Count} selected";
    }

    private HashSet<int> GetSelectedStatuses() =>
        StatusFilterItems[0].IsChecked
            ? new HashSet<int> { 0, 1, 2, 3, 4, 5, 6, 7, 10 }
            : StatusFilterItems.Skip(1)
                .Where(i => i.IsChecked && i.StatusValue.HasValue)
                .Select(i => i.StatusValue!.Value)
                .ToHashSet();

    public async Task LoadDataAsync()
    {
        if (_mainVM.CurrentUser == null) return;
        IsLoading = true;
        try
        {
            var projects = await _mainVM.SupabaseService.GetProjectsAsync();

            Projects.Clear();
            Projects.Add(new NMKProject { Id = "", Name = "All Projects" });
            foreach (var p in projects) Projects.Add(p);

            DateTimeOffset start, end;
            if (IsToday)
            {
                start = new DateTimeOffset(DateTime.Today);
                end = start.AddDays(1).AddTicks(-1);
            }
            else
            {
                var year  = SelectedYear  > 0 ? SelectedYear  : DateTime.Now.Year;
                var month = SelectedMonth is >= 1 and <= 12 ? SelectedMonth : DateTime.Now.Month;
                start = new DateTimeOffset(new DateTime(year, month, 1));
                end = start.AddMonths(1).AddTicks(-1);
            }

            var tasks = await _mainVM.SupabaseService.GetTasksByDateRangeAsync(
                _mainVM.CurrentUser.Id, start, end, IsAssignedByMe);

            var selectedStatuses = GetSelectedStatuses();
            tasks = tasks.Where(t => selectedStatuses.Contains(t.Status ?? 0)).ToList();

            if (!string.IsNullOrEmpty(SelectedProjectFilter))
                tasks = tasks.Where(t => t.ProjectId == SelectedProjectFilter).ToList();

            ProjectCards.Clear();
            _taskDict.Clear();

            var projectIds = tasks
                .Select(t => t.ProjectId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            var relevantProjects = projects.Where(p => projectIds.Contains(p.Id)).ToList();
            int colorIdx = 0;

            foreach (var project in relevantProjects)
            {
                var projectColor = !string.IsNullOrEmpty(project.Color)
                    ? project.Color
                    : ProjectColorPalette[colorIdx % ProjectColorPalette.Length];
                colorIdx++;

                var card = new ProjectCardViewModel
                {
                    Id           = project.Id,
                    Name         = project.Name ?? "",
                    CreatedAt    = project.CreatedAt?.LocalDateTime.ToString("dd/MM/yyyy") ?? "",
                    RevitVersion = project.RevitVersion ?? "",
                    Description  = project.Description ?? "",
                    Avatar       = project.Avatar ?? "",
                    Color        = projectColor
                };

                foreach (var task in tasks.Where(t => t.ProjectId == project.Id))
                {
                    var tc = CreateTaskCard(task, project.Name ?? "");
                    card.Tasks.Add(tc);
                    _taskDict[tc.Id] = tc;
                }
                ProjectCards.Add(card);
            }

            var orphanTasks = tasks
                .Where(t => string.IsNullOrEmpty(t.ProjectId) || !projectIds.Contains(t.ProjectId))
                .ToList();
            if (orphanTasks.Count > 0)
            {
                var noProj = new ProjectCardViewModel { Id = "", Name = "No Project", Color = "#546E7A" };
                foreach (var task in orphanTasks)
                {
                    var tc = CreateTaskCard(task, "No Project");
                    noProj.Tasks.Add(tc);
                    _taskDict[tc.Id] = tc;
                }
                ProjectCards.Add(noProj);
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

    private static TaskCardViewModel CreateTaskCard(NMKTask task, string projectName)
    {
        var status = task.Status ?? 0;
        return new TaskCardViewModel
        {
            Id            = task.Id,
            Name          = task.Name ?? "",
            Status        = status,
            StatusText    = GetStatusText(status),
            StatusColor   = GetStatusColor(status),
            ProjectId     = task.ProjectId ?? "",
            ProjectName   = projectName,
            DateStart     = task.DateStart,
            DateEnd       = task.DateEnd,
            AssigneeEmail = task.AssigneeEmail ?? "",
            AssignerEmail = task.AssigneeByEmail ?? ""
        };
    }

    public static string GetStatusText(int status) => status switch
    {
        0  => "Complete",
        1  => "New",
        2  => "Edit",
        3  => "New",
        4  => "Checked",
        5  => "ReChecked",
        6  => "Start",
        7  => "Accepted",
        10 => "Interrupted",
        _  => "Unknown"
    };

    public static string GetStatusColor(int status) => status switch
    {
        0  => "#00897B",
        1  => "#9E9E9E",
        2  => "#78909C",
        3  => "#1E88E5",
        4  => "#E64A19",
        5  => "#FB8C00",
        6  => "#8E24AA",
        7  => "#F57F17",
        10 => "#E53935",
        _  => "#9E9E9E"
    };

    partial void OnIsTodayChanged(bool value)          => _ = LoadDataAsync();
    partial void OnSelectedMonthChanged(int value)     => _ = LoadDataAsync();
    partial void OnSelectedYearChanged(int value)      => _ = LoadDataAsync();
    partial void OnSelectedProjectFilterChanged(string? value) => _ = LoadDataAsync();

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
    private async Task ChangeProjectAvatar(string projectId)
    {
        if (!IsAdmin) return;
        var dlg = new OpenFileDialog
        {
            Title  = "Select Project Avatar",
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            IsLoading = true;
            var url = await _mainVM.SupabaseService.UpdateProjectAvatarAsync(projectId, dlg.FileName);
            if (url != null)
            {
                var card = ProjectCards.FirstOrDefault(p => p.Id == projectId);
                if (card != null) card.Avatar = url;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Change avatar error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
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
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Accept task error: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task StartTask(string taskId)
    {
        if (_mainVM.CurrentUser == null) return;
        try
        {
            await _mainVM.SupabaseService.UpdateTaskStatusAsync(taskId, (int)Models.TaskStatus.Start);
            await LoadDataAsync();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Start task error: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task CompleteTask(string taskId)
    {
        if (_mainVM.CurrentUser == null) return;
        try
        {
            await _mainVM.SupabaseService.UpdateTaskStatusAsync(taskId, (int)Models.TaskStatus.Complete);

            if (_taskDict.TryGetValue(taskId, out var taskCard) && !string.IsNullOrEmpty(taskCard.AssignerEmail))
            {
                try
                {
                    _mainVM.OutlookService.SendTaskCompletedEmail(
                        taskCard.AssignerEmail, taskCard.Name,
                        taskCard.ProjectName, _mainVM.CurrentUserEmail);
                }
                catch (Exception emailEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Email error: {emailEx.Message}");
                }

                await _mainVM.SupabaseService.CreateNotificationAsync(new NMKNotify
                {
                    TaskId        = taskId,
                    ProjectId     = taskCard.ProjectId,
                    ReceiverId    = "",
                    ReceiverEmail = taskCard.AssignerEmail,
                    ActorId       = _mainVM.CurrentUser.Id,
                    ActorEmail    = _mainVM.CurrentUserEmail,
                    NotifyType    = (int)NotifyType.Task,
                    Title         = $"Task Completed: {taskCard.Name}",
                    Content       = $"{_mainVM.CurrentUserEmail} completed task '{taskCard.Name}'",
                    EntityType    = (int)EntityType.Task,
                    EventType     = (int)EventType.StatusChanged,
                    EntityId      = taskId
                });
            }
            await LoadDataAsync();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Complete task error: {ex.Message}"); }
    }
}
