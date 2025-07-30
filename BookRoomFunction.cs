using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using UCBookingAPI.Models;
using UCBookingAPI.Services;

namespace UCBookingAPI;

public class BookRoomFunction
{
    private readonly IGraphService _graphService;
    private readonly ILogger<BookRoomFunction> _logger;

    public BookRoomFunction(IGraphService graphService, ILogger<BookRoomFunction> logger)
    {
        _graphService = graphService;
        _logger = logger;
    }

    [Function("BookRoom")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("BookRoom function processed a request.");

        try
        {
            // Parse the request body
            BookingRequest bookingRequest;
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                bookingRequest = System.Text.Json.JsonSerializer.Deserialize<BookingRequest>(requestBody, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new InvalidOperationException("Failed to deserialize request body");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid request body");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync("Invalid request body: " + ex.Message);
                return errorResponse;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(bookingRequest.RoomEmail))
            {
                _logger.LogWarning("Room email is required");
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }

            // Create the event
            var createdEvent = await _graphService.CreateEventAsync(bookingRequest);
            
            // Return success response
            var response = req.CreateResponse(System.Net.HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new
            {
                success = true,
                eventId = createdEvent.Id,
                webLink = createdEvent.WebLink,
                message = "Meeting room booked successfully"
            });

            return response;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Authentication failed");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            await errorResponse.WriteStringAsync("Authentication failed. Please check your Azure AD application permissions.");
            return errorResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"An error occurred: {ex.Message}");
            return errorResponse;
        }
    }
}
