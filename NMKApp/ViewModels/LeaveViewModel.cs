using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMKApp.Models;
using System.Collections.ObjectModel;

namespace NMKApp.ViewModels;

public partial class LeaveItemViewModel : ObservableObject
{
    [ObservableProperty] private string id = string.Empty;
    [ObservableProperty] private string requesterEmail = string.Empty;
    [ObservableProperty] private string approverEmail = string.Empty;
    [ObservableProperty] private int leaveType = 1;
    [ObservableProperty] private int status;
    [ObservableProperty] private string reason = string.Empty;
    [ObservableProperty] private string note = string.Empty;
    [ObservableProperty] private DateTimeOffset dateFrom;
    [ObservableProperty] private DateTimeOffset dateTo;
    [ObservableProperty] private decimal? totalDays;
    [ObservableProperty] private bool isHalfDay;
    [ObservableProperty] private string requesterId = string.Empty;
    [ObservableProperty] private string approverId = string.Empty;
    [ObservableProperty] private string projectId = string.Empty;
}

public partial class LeaveViewModel : ObservableObject
{
    private readonly MainViewModel _mainVM;

    [ObservableProperty] private bool isMyRequests = true;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isCreatingLeave;

    // New leave fields
    [ObservableProperty] private int newLeaveType = 1;
    [ObservableProperty] private DateTime newDateFrom = DateTime.Today;
    [ObservableProperty] private DateTime newDateTo = DateTime.Today;
    [ObservableProperty] private string newReason = string.Empty;
    [ObservableProperty] private bool newIsHalfDay;
    [ObservableProperty] private int newHalfDaySlot = 1;
    [ObservableProperty] private string? selectedApproverId;
    [ObservableProperty] private string? selectedApproverEmail;
    [ObservableProperty] private string? selectedProjectId;

    public ObservableCollection<LeaveItemViewModel> LeaveItems { get; } = new();
    public ObservableCollection<NMKUser> Approvers { get; } = new();
    public ObservableCollection<NMKProject> Projects { get; } = new();

    public ObservableCollection<KeyValuePair<int, string>> LeaveTypes { get; } = new()
    {
        new(1, "Annual Leave"),
        new(2, "Sick Leave"),
        new(3, "Personal Leave"),
        new(4, "Unpaid Leave"),
        new(5, "Other")
    };

    public LeaveViewModel(MainViewModel mainVM)
    {
        _mainVM = mainVM;
    }

    public async Task LoadDataAsync()
    {
        if (_mainVM.CurrentUser == null) return;
        IsLoading = true;
        try
        {
            var leaves = IsMyRequests
                ? await _mainVM.SupabaseService.GetLeavesByRequesterAsync(_mainVM.CurrentUser.Id)
                : await _mainVM.SupabaseService.GetLeavesByApproverAsync(_mainVM.CurrentUser.Id);

            LeaveItems.Clear();
            foreach (var leave in leaves)
            {
                LeaveItems.Add(new LeaveItemViewModel
                {
                    Id = leave.Id,
                    RequesterEmail = leave.RequesterEmail ?? "",
                    ApproverEmail = leave.ApproverEmail ?? "",
                    LeaveType = leave.LeaveType,
                    Status = leave.Status,
                    Reason = leave.Reason ?? "",
                    Note = leave.Note ?? "",
                    DateFrom = leave.DateFrom,
                    DateTo = leave.DateTo,
                    TotalDays = leave.TotalDays,
                    IsHalfDay = leave.IsHalfDay,
                    RequesterId = leave.RequesterId,
                    ApproverId = leave.ApproverId ?? "",
                    ProjectId = leave.ProjectId ?? ""
                });
            }

            // Load users for approver selection
            var users = await _mainVM.SupabaseService.GetUsersAsync();
            Approvers.Clear();
            foreach (var u in users.Where(u => u.Id != _mainVM.CurrentUser.Id))
                Approvers.Add(u);

            // Load projects
            var projects = await _mainVM.SupabaseService.GetProjectsAsync();
            Projects.Clear();
            foreach (var p in projects) Projects.Add(p);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Leave load error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleMyRequests()
    {
        IsMyRequests = true;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task TogglePendingApprovals()
    {
        IsMyRequests = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private void ShowCreateLeave()
    {
        IsCreatingLeave = true;
        NewLeaveType = 1;
        NewDateFrom = DateTime.Today.AddDays(1);
        NewDateTo = DateTime.Today.AddDays(1);
        NewReason = string.Empty;
        NewIsHalfDay = false;
    }

    [RelayCommand]
    private void CancelCreateLeave()
    {
        IsCreatingLeave = false;
    }

    [RelayCommand]
    private async Task SubmitLeave()
    {
        if (_mainVM.CurrentUser == null || string.IsNullOrEmpty(SelectedApproverId)) return;

        try
        {
            var totalDays = NewIsHalfDay ? 0.5m : (decimal)(NewDateTo - NewDateFrom).TotalDays + 1;

            var leave = new NMKLeave
            {
                RequesterId = _mainVM.CurrentUser.Id,
                RequesterEmail = _mainVM.CurrentUserEmail,
                ApproverId = SelectedApproverId,
                ApproverEmail = SelectedApproverEmail,
                ProjectId = SelectedProjectId,
                LeaveType = NewLeaveType,
                Status = (int)LeaveStatus.Pending,
                Reason = NewReason,
                DateFrom = new DateTimeOffset(NewDateFrom),
                DateTo = new DateTimeOffset(NewDateTo),
                TotalDays = totalDays,
                IsHalfDay = NewIsHalfDay,
                HalfDaySlot = NewIsHalfDay ? NewHalfDaySlot : null
            };

            await _mainVM.SupabaseService.CreateLeaveAsync(leave);

            // Send email to approver
            if (!string.IsNullOrEmpty(SelectedApproverEmail))
            {
                var leaveTypeName = LeaveTypes.FirstOrDefault(lt => lt.Key == NewLeaveType).Value ?? "Leave";
                try
                {
                    _mainVM.OutlookService.SendLeaveRequestEmail(
                        SelectedApproverEmail,
                        _mainVM.CurrentUserEmail,
                        leaveTypeName,
                        leave.DateFrom,
                        leave.DateTo,
                        totalDays,
                        NewReason);
                }
                catch (Exception emailEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Email error: {emailEx.Message}");
                }
            }

            // Create notification
            await _mainVM.SupabaseService.CreateNotificationAsync(new NMKNotify
            {
                LeaveId = leave.Id,
                ReceiverId = SelectedApproverId!,
                ReceiverEmail = SelectedApproverEmail,
                ActorId = _mainVM.CurrentUser.Id,
                ActorEmail = _mainVM.CurrentUserEmail,
                NotifyType = (int)Models.NotifyType.Leave,
                Title = $"Leave Request from {_mainVM.CurrentUserEmail}",
                Content = $"Leave request: {NewDateFrom:dd/MM/yyyy} - {NewDateTo:dd/MM/yyyy}",
                EntityType = (int)Models.EntityType.Leave,
                EventType = (int)Models.EventType.Created,
                EntityId = leave.Id
            });

            IsCreatingLeave = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Submit leave error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ApproveLeave(string leaveId)
    {
        if (_mainVM.CurrentUser == null) return;
        try
        {
            await _mainVM.SupabaseService.ApproveLeaveAsync(leaveId);

            var leaveItem = LeaveItems.FirstOrDefault(l => l.Id == leaveId);
            if (leaveItem != null && !string.IsNullOrEmpty(leaveItem.RequesterEmail))
            {
                var leaveTypeName = LeaveTypes.FirstOrDefault(lt => lt.Key == leaveItem.LeaveType).Value ?? "Leave";
                try
                {
                    _mainVM.OutlookService.SendLeaveApprovedEmail(
                        leaveItem.RequesterEmail,
                        _mainVM.CurrentUserEmail,
                        leaveTypeName,
                        leaveItem.DateFrom,
                        leaveItem.DateTo);
                }
                catch (Exception emailEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Email error: {emailEx.Message}");
                }

                await _mainVM.SupabaseService.CreateNotificationAsync(new NMKNotify
                {
                    LeaveId = leaveId,
                    ReceiverId = leaveItem.RequesterId,
                    ReceiverEmail = leaveItem.RequesterEmail,
                    ActorId = _mainVM.CurrentUser.Id,
                    ActorEmail = _mainVM.CurrentUserEmail,
                    NotifyType = (int)Models.NotifyType.Leave,
                    Title = "Leave Approved",
                    Content = $"Your leave request has been approved by {_mainVM.CurrentUserEmail}",
                    EntityType = (int)Models.EntityType.Leave,
                    EventType = (int)Models.EventType.StatusChanged,
                    EntityId = leaveId
                });
            }

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Approve leave error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RejectLeave(string leaveId)
    {
        if (_mainVM.CurrentUser == null) return;
        try
        {
            await _mainVM.SupabaseService.RejectLeaveAsync(leaveId);

            var leaveItem = LeaveItems.FirstOrDefault(l => l.Id == leaveId);
            if (leaveItem != null && !string.IsNullOrEmpty(leaveItem.RequesterEmail))
            {
                var leaveTypeName = LeaveTypes.FirstOrDefault(lt => lt.Key == leaveItem.LeaveType).Value ?? "Leave";
                try
                {
                    _mainVM.OutlookService.SendLeaveRejectedEmail(
                        leaveItem.RequesterEmail,
                        _mainVM.CurrentUserEmail,
                        leaveTypeName,
                        leaveItem.DateFrom,
                        leaveItem.DateTo,
                        leaveItem.Note);
                }
                catch (Exception emailEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Email error: {emailEx.Message}");
                }

                await _mainVM.SupabaseService.CreateNotificationAsync(new NMKNotify
                {
                    LeaveId = leaveId,
                    ReceiverId = leaveItem.RequesterId,
                    ReceiverEmail = leaveItem.RequesterEmail,
                    ActorId = _mainVM.CurrentUser.Id,
                    ActorEmail = _mainVM.CurrentUserEmail,
                    NotifyType = (int)Models.NotifyType.Leave,
                    Title = "Leave Rejected",
                    Content = $"Your leave request has been rejected by {_mainVM.CurrentUserEmail}",
                    EntityType = (int)Models.EntityType.Leave,
                    EventType = (int)Models.EventType.StatusChanged,
                    EntityId = leaveId
                });
            }

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Reject leave error: {ex.Message}");
        }
    }
}
