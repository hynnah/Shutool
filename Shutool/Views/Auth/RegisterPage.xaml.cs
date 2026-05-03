using Shutool.Services;

namespace Shutool.Views.Auth;

public partial class RegisterPage : ContentPage
{
    private readonly SupabaseService _supabaseService;

    public RegisterPage(SupabaseService supabaseService)
    {
        InitializeComponent();
        _supabaseService = supabaseService;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        RegisterErrorLabel.IsVisible = false;

        var name = NameEntry.Text?.Trim();
        var email = RegisterEmailEntry.Text?.Trim();
        var password = RegisterPasswordEntry.Text;
        var confirm = ConfirmPasswordEntry.Text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
        {
            RegisterErrorLabel.Text = "Please fill in all fields.";
            RegisterErrorLabel.IsVisible = true;
            return;
        }

        if (password != confirm)
        {
            RegisterErrorLabel.Text = "Passwords do not match.";
            RegisterErrorLabel.IsVisible = true;
            return;
        }

        if (password.Length < 6)
        {
            RegisterErrorLabel.Text = "Password must be at least 6 characters.";
            RegisterErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            var options = new Supabase.Gotrue.SignUpOptions
            {
                Data = new Dictionary<string, object>
                {
                    { "name", name }
                }
            };

            var session = await _supabaseService.Client.Auth.SignUp(
                email, password, options);

            if (session?.User == null)
            {
                RegisterErrorLabel.Text = "Registration failed. Try again.";
                RegisterErrorLabel.IsVisible = true;
                return;
            }

            await DisplayAlert("Success!", "Account created. Please log in.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            RegisterErrorLabel.Text = ex.Message.Contains("already")
                ? "Email already registered."
                : "Registration failed. Try again.";
            RegisterErrorLabel.IsVisible = true;
        }
    }
}