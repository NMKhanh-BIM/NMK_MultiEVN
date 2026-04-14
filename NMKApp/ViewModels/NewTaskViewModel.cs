using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMKApp.Models;
using System.Collections.ObjectModel;

namespace NMKApp.ViewModels;

public partial class NewTaskViewModel : ObservableObject
{
    private readonly MainViewModel _mainVM;

    [ObservableProperty] private string taskName = string.Empty;
    [ObservableProperty] private string? selectedProjectId;
    [ObservableProperty] private string? selectedAssigneeId;
    [ObservableProperty] private string? selectedAssigneeEmail;
    [ObservableProperty] private DateTime dateStart = DateTime.Now;
    [ObservableProperty] private DateTime dateEnd = DateTime.Now.AddHours(3);
    [ObservableProperty] private string detail = string.Empty;
    [ObservableProperty] private string folder = string.Empty;
    [ObservableProperty] private bool isVisible;

    public ObservableCollection<NMKProject> Projects { get; } = new();
    public ObservableCollection<NMKUser> Users { get; } = new();

    public NewTaskViewModel(MainViewModel mainVM)
    {
        _mainVM = mainVM;
    }

    public async Task LoadDataAsync()
    {
        var projects = await _mainVM.SupabaseService.GetProjectsAsync();
        Projects.Clear();
        foreach (var p in projects) Projects.Add(p);

        var users = await _mainVM.SupabaseService.GetUsersAsync();
        Users.Clear();
        foreach (var u in users) Users.Add(u);
    }

    [RelayCommand]
    private void Show()
    {
        IsVisible = true;
        TaskName = string.Empty;
        Detail = string.Empty;
        DateStart = DateTime.Now;
        DateEnd = DateTime.Now.AddHours(3);
    }

    [RelayCommand]
    private void Cancel()
    {
        IsVisible = false;
    }

    [RelayCommand]
    private async Task Create()
    {
        if (string.IsNullOrWhiteSpace(TaskName) || _mainVM.CurrentUser == null) return;

        try
        {
            var selectedProject = Projects.FirstOrDefault(p => p.Id == SelectedProjectId);
            var task = new NMKTask
            {
                Name = TaskName,
                ProjectId = SelectedProjectId,
                AssigneeTo = SelectedAssigneeId,
                AssigneeEmail = SelectedAssigneeEmail,
                AssigneeBy = _mainVM.CurrentUser.Id,
                AssigneeByEmail = _mainVM.CurrentUserEmail,
                DateStart = new DateTimeOffset(DateStart),
                DateEnd = new DateTimeOffset(DateEnd),
                Detail = Detail,
                Folder = Folder,
                Status = (int)Models.TaskStatus.New,
                Color = "#4CAF50"
            };

            var created = await _mainVM.SupabaseService.CreateTaskAsync(task);

            // Send email notification
            if (!string.IsNullOrEmpty(SelectedAssigneeEmail))
            {
                try
                {
                    _mainVM.OutlookService.SendTaskAssignedEmail(
                        SelectedAssigneeEmail,
                        TaskName,
                        selectedProject?.Name ?? "No Project",
                        task.DateStart!.Value,
                        task.DateEnd!.Value,
                        _mainVM.CurrentUserEmail);
                }
                catch (Exception emailEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Email error: {emailEx.Message}");
                }

                // Create notification
                if (!string.IsNullOrEmpty(SelectedAssigneeId))
                {
                    await _mainVM.SupabaseService.CreateNotificationAsync(new NMKNotify
                    {
                        TaskId = created.Id,
                        ProjectId = SelectedProjectId,
                        ReceiverId = SelectedAssigneeId,
                        ReceiverEmail = SelectedAssigneeEmail,
                        ActorId = _mainVM.CurrentUser.Id,
                        ActorEmail = _mainVM.CurrentUserEmail,
                        NotifyType = (int)Models.NotifyType.Task,
                        Title = $"New Task: {TaskName}",
                        Content = $"{_mainVM.CurrentUserEmail} assigned you task '{TaskName}'",
                        EntityType = (int)Models.EntityType.Task,
                        EventType = (int)Models.EventType.Assigned,
                        EntityId = created.Id
                    });
                }
            }

            IsVisible = false;
            await _mainVM.DashboardVM.LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Create task error: {ex.Message}");
        }
    }
}
