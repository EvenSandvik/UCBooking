using Microsoft.Graph.Models;

namespace UCBookingAPI.Models;

public class BookingRequest
{
    public string Subject { get; set; } = "Team Sync";
    public string Content { get; set; } = "Team meeting to discuss project updates";
    public DateTime Start { get; set; } = DateTime.UtcNow.AddHours(1);
    public DateTime End { get; set; } = DateTime.UtcNow.AddHours(2);
    public string TimeZone { get; set; } = "W. Europe Standard Time";
    public string RoomEmail { get; set; } = null!;
    public List<Attendee>? Attendees { get; set; }
}

