using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMKApp.Models;
using System.Collections.ObjectModel;

namespace NMKApp.ViewModels;

public partial class ProjectsViewModel : ObservableObject
{
    private readonly MainViewModel _mainVM;
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<NMKProject> Projects { get; } = new();

    public ProjectsViewModel(MainViewModel mainVM) => _mainVM = mainVM;

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var projects = await _mainVM.SupabaseService.GetProjectsAsync();
            Projects.Clear();
            foreach (var p in projects) Projects.Add(p);
        }
        finally { IsLoading = false; }
    }
}

public partial class UsersViewModel : ObservableObject
{
    private readonly MainViewModel _mainVM;
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<NMKUser> Users { get; } = new();

    public UsersViewModel(MainViewModel mainVM) => _mainVM = mainVM;

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var users = await _mainVM.SupabaseService.GetUsersAsync();
            Users.Clear();
            foreach (var u in users) Users.Add(u);
        }
        finally { IsLoading = false; }
    }
}

public partial class TimeLineViewModel : ObservableObject
{
    private readonly MainViewModel _mainVM;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private int selectedMonth = DateTime.Now.Month;
    [ObservableProperty] private int selectedYear = DateTime.Now.Year;

    public ObservableCollection<NMKTask> Tasks { get; } = new();

    public TimeLineViewModel(MainViewModel mainVM) => _mainVM = mainVM;

    public async Task LoadDataAsync()
    {
        if (_mainVM.CurrentUser == null) return;
        IsLoading = true;
        try
        {
            var start = new DateTimeOffset(new DateTime(SelectedYear, SelectedMonth, 1));
            var end = start.AddMonths(1).AddTicks(-1);
            var tasks = await _mainVM.SupabaseService.GetTasksByDateRangeAsync(_mainVM.CurrentUser.Id, start, end, false);
            Tasks.Clear();
            foreach (var t in tasks) Tasks.Add(t);
        }
        finally { IsLoading = false; }
    }
}

public partial class RequestViewModel : ObservableObject
{
    private readonly MainViewModel _mainVM;
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<NMKLeave> PendingLeaves { get; } = new();

    public RequestViewModel(MainViewModel mainVM) => _mainVM = mainVM;

    public async Task LoadDataAsync()
    {
        if (_mainVM.CurrentUser == null) return;
        IsLoading = true;
        try
        {
            var leaves = await _mainVM.SupabaseService.GetLeavesByApproverAsync(_mainVM.CurrentUser.Id);
            PendingLeaves.Clear();
            foreach (var l in leaves.Where(l => l.Status == (int)LeaveStatus.Pending))
                PendingLeaves.Add(l);
        }
        finally { IsLoading = false; }
    }
}

public partial class ScheduleViewModel : ObservableObject
{
    private readonly MainViewModel _mainVM;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private int selectedMonth = DateTime.Now.Month;
    [ObservableProperty] private int selectedYear = DateTime.Now.Year;

    public ObservableCollection<NMKUserAttendance> Attendances { get; } = new();

    public ScheduleViewModel(MainViewModel mainVM) => _mainVM = mainVM;

    public async Task LoadDataAsync()
    {
        if (_mainVM.CurrentUser == null) return;
        IsLoading = true;
        try
        {
            var from = new DateTime(SelectedYear, SelectedMonth, 1);
            var to = from.AddMonths(1).AddDays(-1);
            var att = await _mainVM.SupabaseService.GetAttendanceAsync(_mainVM.CurrentUser.Id, from, to);
            Attendances.Clear();
            foreach (var a in att) Attendances.Add(a);
        }
        finally { IsLoading = false; }
    }
}

public partial class SettingsViewModel : ObservableObject
{
    private readonly MainViewModel _mainVM;
    [ObservableProperty] private string supabaseUrl = string.Empty;
    [ObservableProperty] private string supabaseKey = string.Empty;

    public SettingsViewModel(MainViewModel mainVM) => _mainVM = mainVM;
}
