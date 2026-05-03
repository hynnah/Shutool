using Supabase;

namespace Shutool.Services;

public class SupabaseService
{
    private Client? _client;

    private const string SupabaseUrl = "https://fmqpddlcqehawmdjsfem.supabase.co";
    private const string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImZtcXBkZGxjcWVoYXdtZGpzZmVtIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Nzc3OTIxOTcsImV4cCI6MjA5MzM2ODE5N30.GF8YnfYVNVPraWdOGnHYgwLWmrR-grBrvYhfpBajAMc";

    public Client Client => _client ?? throw new InvalidOperationException("Supabase not initialized");

    public async Task InitializeAsync()
    {
        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true,
        };

        _client = new Client(SupabaseUrl, SupabaseKey, options);
        await _client.InitializeAsync();
    }

    public bool IsLoggedIn => _client?.Auth.CurrentUser != null;

    public string? CurrentUserId => _client?.Auth.CurrentUser?.Id;

    public string? CurrentUserEmail => _client?.Auth.CurrentUser?.Email;
}