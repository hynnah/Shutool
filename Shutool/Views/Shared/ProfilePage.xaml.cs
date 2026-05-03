using Shutool.Models;
using Shutool.Services;
using Shutool.Views.Auth;

namespace Shutool.Views.Shared;

public partial class ProfilePage : ContentPage
{
    private readonly SupabaseService _supabaseService;
    private UserModel? _currentUser;

    public ProfilePage(SupabaseService supabaseService)
    {
        InitializeComponent();
        _supabaseService = supabaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            var userId = _supabaseService.CurrentUserId;
            _currentUser = await _supabaseService.Client
                .From<UserModel>()
                .Where(u => u.Id == userId)
                .Single();

            NameEntry.Text = _currentUser.Name;
            EmailEntry.Text = _currentUser.Email;
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Could not load profile.", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        FeedbackLabel.IsVisible = false;

        if (_currentUser == null) return;

        var newName = NameEntry.Text?.Trim();
        var newPassword = NewPasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        if (string.IsNullOrEmpty(newName))
        {
            FeedbackLabel.Text = "Name cannot be empty.";
            FeedbackLabel.TextColor = Colors.Red;
            FeedbackLabel.IsVisible = true;
            return;
        }

        // Validate password if provided
        if (!string.IsNullOrEmpty(newPassword))
        {
            if (newPassword != confirmPassword)
            {
                FeedbackLabel.Text = "Passwords do not match.";
                FeedbackLabel.TextColor = Colors.Red;
                FeedbackLabel.IsVisible = true;
                return;
            }

            if (newPassword.Length < 6)
            {
                FeedbackLabel.Text = "Password must be at least 6 characters.";
                FeedbackLabel.TextColor = Colors.Red;
                FeedbackLabel.IsVisible = true;
                return;
            }
        }

        try
        {
            // Update name in users table
            await _supabaseService.Client
                .From<UserModel>()
                .Where(u => u.Id == _currentUser.Id)
                .Set(u => u.Name, newName)
                .Update();

            // Update password in Supabase Auth if provided
            if (!string.IsNullOrEmpty(newPassword))
            {
                await _supabaseService.Client.Auth.Update(
                    new Supabase.Gotrue.UserAttributes { Password = newPassword }
                );
            }

            _currentUser.Name = newName;
            NewPasswordEntry.Text = string.Empty;
            ConfirmPasswordEntry.Text = string.Empty;

            FeedbackLabel.Text = "Profile updated successfully!";
            FeedbackLabel.TextColor = Color.FromArgb("#6ABF4B");
            FeedbackLabel.IsVisible = true;
        }
        catch (Exception)
        {
            FeedbackLabel.Text = "Failed to update profile.";
            FeedbackLabel.TextColor = Colors.Red;
            FeedbackLabel.IsVisible = true;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
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