using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Shutool.Models;

[Table("users")]
public class UserModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("role")]
    public string Role { get; set; } = string.Empty;

    [Column("shuttle_number")]
    public string? ShuttleNumber { get; set; }

    [Column("daily_request_count")]
    public int DailyRequestCount { get; set; }

    [Column("last_request_date")]
    public DateTimeOffset? LastRequestDate { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}