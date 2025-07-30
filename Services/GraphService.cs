using System.Net;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using UCBookingAPI.Models;

namespace UCBookingAPI.Services;

public class GraphService : IGraphService
{
    private readonly ILogger<GraphService> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _tenantId;
    private readonly string[] _scopes = new[] { "https://graph.microsoft.com/.default" };

    public GraphService(IConfiguration configuration, ILogger<GraphService> logger)
    {
        _logger = logger;
        _clientId = configuration["ClientId"] ?? throw new ArgumentNullException("ClientId is not configured");
        _clientSecret = configuration["ClientSecret"] ?? throw new ArgumentNullException("ClientSecret is not configured");
        _tenantId = configuration["TenantId"] ?? throw new ArgumentNullException("TenantId is not configured");
    }

    private GraphServiceClient GetAuthenticatedClient()
    {
        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        var clientSecretCredential = new ClientSecretCredential(
            _tenantId, 
            _clientId, 
            _clientSecret, 
            options);

        return new GraphServiceClient(clientSecretCredential, _scopes);
    }

    public async Task<Event> CreateEventAsync(BookingRequest request)
    {
        try
        {
            var graphClient = GetAuthenticatedClient();

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
            var createdEvent = await graphClient.Users[request.RoomEmail].Events
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
