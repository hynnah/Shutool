using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Shutool.Models;

[Table("priority_requests")]
public class PriorityRequestModel : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("rider_id")]
    public string RiderId { get; set; } = string.Empty;

    [Column("rider_email")]
    public string RiderEmail { get; set; } = string.Empty;

    [Column("rider_name")]
    public string RiderName { get; set; } = string.Empty;

    [Column("shuttle_number")]
    public string ShuttleNumber { get; set; } = string.Empty;

    [Column("note")]
    public string? Note { get; set; }

    [Column("handled")]
    public bool Handled { get; set; }

    [Column("handled_at")]
    public DateTimeOffset? HandledAt { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    // Display helper — not a DB column
    [Newtonsoft.Json.JsonIgnore]
    public string StatusText { get; set; } = string.Empty;
}