using Shutool.Models;
using Shutool.Services;
using Shutool.Views.Auth;
using Shutool.Views.Shared;

namespace Shutool.Views.Rider;

public partial class RiderMainPage : ContentPage
{
    private readonly SupabaseService _supabaseService;
    private UserModel? _currentUser;
    private bool _isHistoryTab = false;
    private const int DailyLimit = 3;

    public RiderMainPage(SupabaseService supabaseService)
    {
        InitializeComponent();
        _supabaseService = supabaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUserAsync();
        await LoadShuttlesAsync();
        UpdateLimitLabel();
    }

    private async Task LoadUserAsync()
    {
        try
        {
            var userId = _supabaseService.CurrentUserId;
            _currentUser = await _supabaseService.Client
                .From<UserModel>()
                .Where(u => u.Id == userId)
                .Single();
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Could not load user data.", "OK");
        }
    }

    private async Task LoadShuttlesAsync()
    {
        try
        {
            var drivers = await _supabaseService.Client
                .From<UserModel>()
                .Where(u => u.Role == "driver")
                .Get();

            ShuttlePicker.Items.Clear();

            if (drivers?.Models != null)
            {
                foreach (var driver in drivers.Models)
                {
                    if (!string.IsNullOrEmpty(driver.ShuttleNumber))
                        ShuttlePicker.Items.Add(driver.ShuttleNumber);
                }
            }
        }
        catch (Exception)
        {
            // silently fail — picker just stays empty
        }
    }

    private void UpdateLimitLabel()
    {
        if (_currentUser == null) return;

        // Reset count if last request was not today
        var isToday = _currentUser.LastRequestDate?.Date == DateTime.UtcNow.Date;
        var usedToday = isToday ? _currentUser.DailyRequestCount : 0;
        var remaining = DailyLimit - usedToday;

        LimitLabel.Text = $"Requests remaining today: {remaining}/{DailyLimit}";
        LimitLabel.TextColor = remaining == 0
            ? Color.FromArgb("#FF4444")
            : Color.FromArgb("#3D5A2A");
    }

    private void OnPriorityTabClicked(object sender, EventArgs e)
    {
        _isHistoryTab = false;
        PriorityPanel.IsVisible = true;
        HistoryPanel.IsVisible = false;
        ActionButton.Text = "Send";
        ActionButton.BackgroundColor = Color.FromArgb("#6ABF4B");
    }

    private async void OnHistoryTabClicked(object sender, EventArgs e)
    {
        _isHistoryTab = true;
        PriorityPanel.IsVisible = false;
        HistoryPanel.IsVisible = true;
        ActionButton.Text = "Refresh";
        ActionButton.BackgroundColor = Color.FromArgb("#4BBEF5");
        await LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        try
        {
            var userId = _supabaseService.CurrentUserId;
            var response = await _supabaseService.Client
                .From<PriorityRequestModel>()
                .Where(r => r.RiderId == userId)
                .Order(r => r.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            var items = response?.Models ?? new List<PriorityRequestModel>();

            // Add display property
            foreach (var item in items)
                item.StatusText = item.Handled ? "Handled" : "Pending";

            HistoryCollection.ItemsSource = items;
            EmptyHistoryLabel.IsVisible = items.Count == 0;
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Could not load history.", "OK");
        }
    }

    private async void OnActionButtonClicked(object sender, EventArgs e)
    {
        if (_isHistoryTab)
        {
            await LoadHistoryAsync();
            return;
        }

        await SendPriorityRequestAsync();
    }

    private async Task SendPriorityRequestAsync()
    {
        ErrorLabel.IsVisible = false;

        if (_currentUser == null) return;

        // Check daily limit
        var isToday = _currentUser.LastRequestDate?.Date == DateTime.UtcNow.Date;
        var usedToday = isToday ? _currentUser.DailyRequestCount : 0;

        if (usedToday >= DailyLimit)
        {
            ErrorLabel.Text = $"You've reached your daily limit of {DailyLimit} requests.";
            ErrorLabel.IsVisible = true;
            return;
        }

        // Validate shuttle
        var shuttleNumber = ShuttlePicker.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(shuttleNumber))
        {
            ErrorLabel.Text = "Please select a shuttle number.";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            // Insert request
            var request = new PriorityRequestModel
            {
                RiderId = _currentUser.Id,
                RiderEmail = _currentUser.Email,
                RiderName = _currentUser.Name,
                ShuttleNumber = shuttleNumber,
                Note = NoteEditor.Text?.Trim(),
                Handled = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _supabaseService.Client
                .From<PriorityRequestModel>()
                .Insert(request);

            // Update daily count
            var newCount = isToday ? usedToday + 1 : 1;
            await _supabaseService.Client
                .From<UserModel>()
                .Where(u => u.Id == _currentUser.Id)
                .Set(u => u.DailyRequestCount, newCount)
                .Set(u => u.LastRequestDate, DateTimeOffset.UtcNow)
                .Update();

            _currentUser.DailyRequestCount = newCount;
            _currentUser.LastRequestDate = DateTimeOffset.UtcNow;

            NoteEditor.Text = string.Empty;
            UpdateLimitLabel();

            await DisplayAlert("Sent!", "Your priority request has been sent.", "OK");
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = "Failed to send request. Try again.";
            ErrorLabel.IsVisible = true;
        }
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