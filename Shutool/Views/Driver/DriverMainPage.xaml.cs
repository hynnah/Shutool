using Shutool.Models;
using Shutool.Services;
using Shutool.Views.Auth;
using Shutool.Views.Shared;

namespace Shutool.Views.Driver;

public partial class DriverMainPage : ContentPage
{
    private readonly SupabaseService _supabaseService;
    private UserModel? _currentDriver;
    private bool _isHistoryTab = false;

    public DriverMainPage(SupabaseService supabaseService)
    {
        InitializeComponent();
        _supabaseService = supabaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDriverAsync();
        await LoadRequestsAsync();
    }

    private async Task LoadDriverAsync()
    {
        try
        {
            var userId = _supabaseService.CurrentUserId;
            _currentDriver = await _supabaseService.Client
                .From<UserModel>()
                .Where(u => u.Id == userId)
                .Single();

            DriverNameLabel.Text = _currentDriver.Name;
            ShuttleNumberLabel.Text = $"Shuttle #{_currentDriver.ShuttleNumber ?? "--"}";
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Could not load driver data.", "OK");
        }
    }

    private async Task LoadRequestsAsync()
    {
        try
        {
            if (_currentDriver?.ShuttleNumber == null) return;

            if (_isHistoryTab)
            {
                var response = await _supabaseService.Client
                    .From<PriorityRequestModel>()
                    .Where(r => r.ShuttleNumber == _currentDriver.ShuttleNumber)
                    .Where(r => r.Handled == true)
                    .Order(r => r.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                var items = response?.Models ?? new List<PriorityRequestModel>();
                HistoryCollection.ItemsSource = items;
                EmptyLabel.IsVisible = items.Count == 0;
                PendingCollection.IsVisible = false;
                HistoryCollection.IsVisible = items.Count > 0;
            }
            else
            {
                var response = await _supabaseService.Client
                    .From<PriorityRequestModel>()
                    .Where(r => r.ShuttleNumber == _currentDriver.ShuttleNumber)
                    .Where(r => r.Handled == false)
                    .Order(r => r.CreatedAt, Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                var items = response?.Models ?? new List<PriorityRequestModel>();
                PendingCollection.ItemsSource = items;
                EmptyLabel.IsVisible = items.Count == 0;
                PendingCollection.IsVisible = items.Count > 0;
                HistoryCollection.IsVisible = false;
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Could not load requests.", "OK");
        }
    }

    private void OnPendingTabClicked(object sender, EventArgs e)
    {
        _isHistoryTab = false;
        PanelTitleLabel.Text = "Pending Request";
        PanelTitleLabel.TextColor = Color.FromArgb("#F5C518");
        MarkAllBtn.IsVisible = true;
        _ = LoadRequestsAsync();
    }

    private void OnHistoryTabClicked(object sender, EventArgs e)
    {
        _isHistoryTab = true;
        PanelTitleLabel.Text = "Request History";
        PanelTitleLabel.TextColor = Color.FromArgb("#6ABF4B");
        MarkAllBtn.IsVisible = false;
        _ = LoadRequestsAsync();
    }

    private async void OnMarkAllCompleteClicked(object sender, EventArgs e)
    {
        if (_currentDriver?.ShuttleNumber == null) return;

        bool confirm = await DisplayAlert(
            "Mark All Complete",
            "Mark all pending requests as handled?",
            "Yes", "No");

        if (!confirm) return;

        try
        {
            await _supabaseService.Client
                .From<PriorityRequestModel>()
                .Where(r => r.ShuttleNumber == _currentDriver.ShuttleNumber)
                .Where(r => r.Handled == false)
                .Set(r => r.Handled, true)
                .Set(r => r.HandledAt, DateTimeOffset.UtcNow)
                .Update();

            await LoadRequestsAsync();
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Could not update requests.", "OK");
        }
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadRequestsAsync();
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilePage(_supabaseService));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Log Out", "Are you sure you want to log out?", "Yes", "No");
        if (!confirm) return;

        await _supabaseService.Client.Auth.SignOut();
        Application.Current!.Windows[0].Page =
            new NavigationPage(new LoginPage(_supabaseService));
    }
}