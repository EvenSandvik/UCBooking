using System.Net;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using UCBookingAPI.Models;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace UCBookingAPI.Services;

public class GraphService : IGraphService
{
    private readonly ILogger<GraphService> _logger;
    private readonly string[] _scopes = new[] { "Calendars.ReadWrite", "User.Read", "offline_access" };
    private const string ClientId = "14d82eec-204b-4c2f-b7e8-296a70dab67e"; // Microsoft Graph Explorer client ID (safe for local dev)
    private const string TokenCachePath = "token_cache.bin";
    private GraphServiceClient _graphClient;
    private static readonly object TokenCacheLock = new object();

    public GraphService(IConfiguration configuration, ILogger<GraphService> logger)
    {
        _logger = logger;
        _graphClient = GetAuthenticatedClientAsync().GetAwaiter().GetResult();
    }

    private async Task<GraphServiceClient> GetAuthenticatedClientAsync()
    {
        var options = new InteractiveBrowserCredentialOptions
        {
            ClientId = ClientId,
            TenantId = "common",
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions
            {
                Name = "UCBookingTokenCache",
                UnsafeAllowUnencryptedStorage = true
            },
            RedirectUri = new Uri("http://localhost"),
        };

        var credential = new InteractiveBrowserCredential(options);
        
        var graphClient = new GraphServiceClient(credential, _scopes);
        
        // Test the connection
        try 
        {
            await graphClient.Me.GetAsync();
            _logger.LogInformation("Successfully authenticated with Microsoft Graph");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate with Microsoft Graph");
            throw;
        }
        
        return graphClient;
    }

    public async Task<Event> CreateEventAsync(BookingRequest request)
    {
        try
        {
var @event = new Event
            {
                Subject = request.Subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = request.Content
                },
                Start = new DateTimeTimeZone
                {
                    DateTime = request.Start.ToString("yyyy-MM-ddTHH:mm:ss"),
                    TimeZone = request.TimeZone
                },
                End = new DateTimeTimeZone
                {
                    DateTime = request.End.ToString("yyyy-MM-ddTHH:mm:ss"),
                    TimeZone = request.TimeZone
                },
                Location = new Location
                {
                    DisplayName = request.RoomEmail
                },
                Attendees = new List<Attendee>(
                    (request.Attendees ?? new List<Attendee>())
                    .Select(a => new Attendee
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = "placeholder",
                            Name = "placeholder"
                        },
                        Type = a.Type
                    })
                    .Append(new Attendee // Add room as required attendee
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = request.RoomEmail,
                            Name = request.RoomEmail
                        },
                        Type = null
                    }))
            };

            // Create the event in the room's calendar
            var createdEvent = await _graphClient.Users[request.RoomEmail].Calendar.Events
                .PostAsync(@event);

            _logger.LogInformation("Successfully created event {EventId} in room {RoomEmail}",
                createdEvent.Id, request.RoomEmail);

            return createdEvent;
        }
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Error creating event in Microsoft Graph: {Message}", ex.Message);
            throw new ApplicationException($"Error creating event: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating event");
            throw;
        }
    }
}
