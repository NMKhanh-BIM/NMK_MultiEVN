using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMKApp.Services;
using NMKApp.Models;
using System.Collections.ObjectModel;

namespace NMKApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SupabaseService _supabaseService;
    private readonly OutlookService _outlookService;

    [ObservableProperty]
    private string currentView = "Dashboard";

    [ObservableProperty]
    private string windowTitle = "RincovitchApp";

    [ObservableProperty]
    private string currentUserEmail = string.Empty;

    [ObservableProperty]
    private NMKUser? currentUser;

    [ObservableProperty]
    private int unreadNotifyCount;

    [ObservableProperty]
    private object? currentViewModel;

    [ObservableProperty]
    private bool isLoading;

    // Child ViewModels
    public DashboardViewModel DashboardVM { get; }
    public TimeLineViewModel TimeLineVM { get; }
    public RequestViewModel RequestVM { get; }
    public UsersViewModel UsersVM { get; }
    public ProjectsViewModel ProjectsVM { get; }
    public NotifyViewModel NotifyVM { get; }
    public LeaveViewModel LeaveVM { get; }
    public ScheduleViewModel ScheduleVM { get; }
    public SettingsViewModel SettingsVM { get; }
    public NewTaskViewModel NewTaskVM { get; }

    public SupabaseService SupabaseService => _supabaseService;
    public OutlookService OutlookService => _outlookService;

    public MainViewModel(SupabaseService supabaseService, OutlookService outlookService)
    {
        _supabaseService = supabaseService;
        _outlookService = outlookService;

        DashboardVM = new DashboardViewModel(this);
        TimeLineVM = new TimeLineViewModel(this);
        RequestVM = new RequestViewModel(this);
        UsersVM = new UsersViewModel(this);
        ProjectsVM = new ProjectsViewModel(this);
        NotifyVM = new NotifyViewModel(this);
        LeaveVM = new LeaveViewModel(this);
        ScheduleVM = new ScheduleViewModel(this);
        SettingsVM = new SettingsViewModel(this);
        NewTaskVM = new NewTaskViewModel(this);

        CurrentViewModel = DashboardVM;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            await _supabaseService.InitializeAsync();

            // Try to get user from Outlook email
            if (_outlookService.CurrentUserEmail != null)
            {
                CurrentUserEmail = _outlookService.CurrentUserEmail;
                CurrentUser = await _supabaseService.GetUserByEmailAsync(CurrentUserEmail);
            }

            WindowTitle = $"RincovitchApp - Logged in as {CurrentUserEmail}";

            // Load notification count
            if (CurrentUser != null)
            {
                UnreadNotifyCount = await _supabaseService.GetUnreadNotificationCountAsync(CurrentUser.Id);
            }

            await NewTaskVM.LoadDataAsync();
            await DashboardVM.LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Init error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateTo(string view)
    {
        CurrentView = view;
        CurrentViewModel = view switch
        {
            "Dashboard" => DashboardVM,
            "TimeLine" => TimeLineVM,
            "Request" => RequestVM,
            "Users" => UsersVM,
            "Projects" => ProjectsVM,
            "Notify" => NotifyVM,
            "Leave" => LeaveVM,
            "Schedule" => ScheduleVM,
            "Settings" => SettingsVM,
            _ => DashboardVM
        };

        // Load data for the target view
        try
        {
            IsLoading = true;
            switch (view)
            {
                case "Dashboard": await DashboardVM.LoadDataAsync(); break;
                case "TimeLine": await TimeLineVM.LoadDataAsync(); break;
                case "Request": await RequestVM.LoadDataAsync(); break;
                case "Users": await UsersVM.LoadDataAsync(); break;
                case "Projects": await ProjectsVM.LoadDataAsync(); break;
                case "Notify": await NotifyVM.LoadDataAsync(); break;
                case "Leave": await LeaveVM.LoadDataAsync(); break;
                case "Schedule": await ScheduleVM.LoadDataAsync(); break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation load error [{view}]: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            await NavigateTo(CurrentView);
            if (CurrentUser != null)
                UnreadNotifyCount = await _supabaseService.GetUnreadNotificationCountAsync(CurrentUser.Id);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Refresh error: {ex.Message}");
        }
    }
}
