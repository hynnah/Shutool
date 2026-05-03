using Shutool.Services;
using Shutool.Views.Auth;

namespace Shutool;

public partial class App : Application
{
    private readonly SupabaseService _supabaseService;

    public App(SupabaseService supabaseService)
    {
        InitializeComponent();
        _supabaseService = supabaseService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new SplashPage(_supabaseService));
    }
}