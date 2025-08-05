using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
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

        // Handles CORS preflight
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
                errorResponse.Headers.Add("Access-Control-Allow-Credentials", "true");
                var errorObj = new { error = "Invalid request body", message = ex.Message };
                var errorJson = System.Text.Json.JsonSerializer.Serialize(errorObj);
                errorResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await errorResponse.WriteStringAsync(errorJson);
                return errorResponse;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(bookingRequest.RoomEmail))
            {
                _logger.LogWarning("Room email is required");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.Headers.Add("Access-Control-Allow-Origin", new[] { "http://localhost:3000", "http://localhost:5173" });
                errorResponse.Headers.Add("Access-Control-Allow-Credentials", "true");
                var errorObj = new { error = "Room email is required" };
                var errorJson = System.Text.Json.JsonSerializer.Serialize(errorObj);
                errorResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await errorResponse.WriteStringAsync(errorJson);
                return errorResponse;
            }

            // Create the event
            var createdEvent = await _graphService.CreateEventAsync(bookingRequest);
            
            // Create response object
            var responseObj = new 
            { 
                id = createdEvent.Id,
                message = "Room booked successfully" 
            };
            
            // Create response with proper headers
            var response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000, http://localhost:5173");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            
            // Set content type and write response
            await response.WriteAsJsonAsync(responseObj);
            return response;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized access");
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            errorResponse.Headers.Add("Access-Control-Allow-Origin", new[] { "http://localhost:3000", "http://localhost:5173" });
            errorResponse.Headers.Add("Access-Control-Allow-Credentials", "true");
            var errorObj = new { error = "Unauthorized", message = "Authentication failed. Please sign in again." };
            var errorJson = System.Text.Json.JsonSerializer.Serialize(errorObj);
            errorResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await errorResponse.WriteStringAsync(errorJson);
            return errorResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking room: {Message}", ex.Message);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Access-Control-Allow-Origin", new[] { "http://localhost:3000", "http://localhost:5173" });
            errorResponse.Headers.Add("Access-Control-Allow-Credentials", "true");
            var errorObj = new { error = "Internal Server Error", message = ex.Message };
            var errorJson = System.Text.Json.JsonSerializer.Serialize(errorObj);
            errorResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await errorResponse.WriteStringAsync(errorJson);
            return errorResponse;
        }
    }
    
    [Function("GetAvailableRooms")]
    public async Task<HttpResponseData> GetAvailableRooms([HttpTrigger(AuthorizationLevel.Function, "get", Route = "rooms")] HttpRequestData req)
    {
        var availableRooms = await _graphService.GetAvailableRooms();
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(availableRooms);
        return response;
    }
}
