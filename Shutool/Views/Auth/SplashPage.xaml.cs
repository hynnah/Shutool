using Shutool.Models;
using Shutool.Services;
using Shutool.Views.Driver;
using Shutool.Views.Rider;

namespace Shutool.Views.Auth;

public partial class SplashPage : ContentPage
{
    private readonly SupabaseService _supabaseService;

    public SplashPage(SupabaseService supabaseService)
    {
        InitializeComponent();
        _supabaseService = supabaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await AnimateBus();
        await InitializeApp();
    }

    private async Task AnimateBus()
    {
        // Slide bus in from left
        BusImage.TranslationX = -300;
        await BusImage.TranslateTo(0, 0, 800, Easing.CubicOut);
        await Task.Delay(600);
    }

    private async Task InitializeApp()
    {
        try
        {
            await _supabaseService.InitializeAsync();

            if (_supabaseService.IsLoggedIn)
            {
                await NavigateByRole();
            }
            else
            {
                await NavigateToLogin();
            }
        }
        catch (Exception)
        {
            await NavigateToLogin();
        }
    }

    private async Task NavigateByRole()
    {
        try
        {
            var userId = _supabaseService.CurrentUserId;
            if (userId == null)
            {
                await NavigateToLogin();
                return;
            }

            var response = await _supabaseService.Client
                .From<UserModel>()
                .Where(u => u.Id == userId)
                .Single();

            if (response == null)
            {
                await NavigateToLogin();
                return;
            }

            Page nextPage = response.Role switch
            {
                "driver" => new DriverMainPage(_supabaseService),
                "rider" => new RiderMainPage(_supabaseService),
                _ => new LoginPage(_supabaseService)
            };

            Application.Current!.Windows[0].Page = new NavigationPage(nextPage);
        }
        catch (Exception)
        {
            await NavigateToLogin();
        }
    }

    private Task NavigateToLogin()
    {
        Application.Current!.Windows[0].Page =
            new NavigationPage(new LoginPage(_supabaseService));
        return Task.CompletedTask;
    }
}