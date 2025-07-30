using System.IO;
using System.Net;
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options")] HttpRequestData req,
        FunctionContext executionContext)
    {
        _logger.LogInformation("BookRoom function processed a request.");

        // Handle CORS preflight
        if (req.Method == "OPTIONS")
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Access-Control-Allow-Origin", new[] { "http://localhost:3000", "http://localhost:5173" });
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
            return response;
        }

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
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.Headers.Add("Access-Control-Allow-Origin", new[] { "http://localhost:3000", "http://localhost:5173" });
                await errorResponse.WriteAsJsonAsync(new { error = "Invalid request body", message = ex.Message });
                return errorResponse;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(bookingRequest.RoomEmail))
            {
                _logger.LogWarning("Room email is required");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.Headers.Add("Access-Control-Allow-Origin", new[] { "http://localhost:3000", "http://localhost:5173" });
                await errorResponse.WriteAsJsonAsync(new { error = "Room email is required" });
                return errorResponse;
            }

            // Create the event
            var createdEvent = await _graphService.CreateEventAsync(bookingRequest);
            
            // Return success response
            var response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Access-Control-Allow-Origin", new[] { "http://localhost:3000", "http://localhost:5173" });
            await response.WriteAsJsonAsync(new { 
                id = createdEvent.Id,
                message = "Room booked successfully" 
            });
            return response;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized access");
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            errorResponse.Headers.Add("Access-Control-Allow-Origin", new[] { "http://localhost:3000", "http://localhost:5173" });
            await errorResponse.WriteAsJsonAsync(new { error = "Unauthorized", message = "Authentication failed. Please sign in again." });
            return errorResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking room");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Access-Control-Allow-Origin", new[] { "http://localhost:3000", "http://localhost:5173" });
            await errorResponse.WriteAsJsonAsync(new { error = "Internal Server Error", message = "An error occurred while processing your request." });
            return errorResponse;
        }
    }
}
