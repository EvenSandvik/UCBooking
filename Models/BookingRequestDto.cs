using System;
using System.Text.Json.Serialization;

namespace UCBookingAPI.Models;

public class BookingRequestDto
{
    [JsonPropertyName("roomEmail")]
    public string RoomEmail { get; set; } = null!;
    
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = "Meeting";
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = "Scheduled meeting";
    
    [JsonPropertyName("start")]
    public string Start { get; set; } = null!;
    
    [JsonPropertyName("end")]
    public string End { get; set; } = null!;
    
    [JsonPropertyName("timeZone")]
    public string TimeZone { get; set; } = "W. Europe Standard Time";
    
    [JsonPropertyName("userName")]
    public string? UserName { get; set; }
}
