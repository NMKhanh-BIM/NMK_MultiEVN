using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NMKApp.Models;
using System.Collections.ObjectModel;

namespace NMKApp.ViewModels;

public partial class NotifyViewModel : ObservableObject
{
    private readonly MainViewModel _mainVM;

    [ObservableProperty] private bool isLoading;

    public ObservableCollection<NMKNotify> Notifications { get; } = new();

    public NotifyViewModel(MainViewModel mainVM)
    {
        _mainVM = mainVM;
    }

    public async Task LoadDataAsync()
    {
        if (_mainVM.CurrentUser == null) return;
        IsLoading = true;
        try
        {
            var notifications = await _mainVM.SupabaseService.GetNotificationsAsync(_mainVM.CurrentUser.Id);
            Notifications.Clear();
            foreach (var n in notifications)
                Notifications.Add(n);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task MarkAsRead(string notifyId)
    {
        await _mainVM.SupabaseService.MarkNotificationReadAsync(notifyId);
        var item = Notifications.FirstOrDefault(n => n.Id == notifyId);
        if (item != null)
        {
            item.IsRead = true;
            item.ReadAt = DateTimeOffset.Now;
        }
        _mainVM.UnreadNotifyCount = await _mainVM.SupabaseService.GetUnreadNotificationCountAsync(_mainVM.CurrentUser!.Id);
    }

    [RelayCommand]
    private async Task MarkAllAsRead()
    {
        foreach (var n in Notifications.Where(n => !n.IsRead))
        {
            await _mainVM.SupabaseService.MarkNotificationReadAsync(n.Id);
        }
        await LoadDataAsync();
        _mainVM.UnreadNotifyCount = 0;
    }
}
