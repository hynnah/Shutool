using Shutool.Models;
using Shutool.Services;
using Shutool.Views.Driver;
using Shutool.Views.Rider;

namespace Shutool.Views.Auth;

public partial class LoginPage : ContentPage
{
    private readonly SupabaseService _supabaseService;

    public LoginPage(SupabaseService supabaseService)
    {
        InitializeComponent();
        _supabaseService = supabaseService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Please fill in all fields.";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            var session = await _supabaseService.Client.Auth.SignIn(email, password);

            if (session?.User == null)
            {
                ErrorLabel.Text = "Invalid email or password.";
                ErrorLabel.IsVisible = true;
                return;
            }

            var userId = session.User.Id;

            var userRecord = await _supabaseService.Client
                .From<UserModel>()
                .Where(u => u.Id == userId)
                .Single();

            if (userRecord == null)
            {
                ErrorLabel.Text = "Account not found.";
                ErrorLabel.IsVisible = true;
                return;
            }

            Page nextPage = userRecord.Role switch
            {
                "driver" => new DriverMainPage(_supabaseService),
                "rider" => new RiderMainPage(_supabaseService),
                _ => new LoginPage(_supabaseService)
            };

            Application.Current!.Windows[0].Page = new NavigationPage(nextPage);
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message.Contains("Invalid")
                ? "Invalid email or password."
                : "Login failed. Try again.";
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(_supabaseService));
    }
}