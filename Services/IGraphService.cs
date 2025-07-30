using Microsoft.Graph;
using Microsoft.Graph.Models;
using UCBookingAPI.Models;

namespace UCBookingAPI.Services;

public interface IGraphService
{
    Task<Event> CreateEventAsync(BookingRequest request);
}
