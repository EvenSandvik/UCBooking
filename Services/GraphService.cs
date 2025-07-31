using System.Net;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using UCBookingAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UCBookingAPI.Services;

public class GraphService : IGraphService
{
    private readonly ILogger<GraphService> _logger;
    private readonly GraphServiceClient _graphClient;
    private const string ClientId = "14d82eec-204b-4c2f-b7e8-296a70dab67e"; // Microsoft Graph Explorer client ID

    public GraphService(IConfiguration configuration, ILogger<GraphService> logger)
    {
        _logger = logger;
        
        try
        {
            var options = new InteractiveBrowserCredentialOptions
            {
                ClientId = ClientId,
                TenantId = "common",
                RedirectUri = new Uri("http://localhost"),
                TokenCachePersistenceOptions = new TokenCachePersistenceOptions
                {
                    Name = "UCBookingTokenCache",
                    UnsafeAllowUnencryptedStorage = true
                }
            };

            var credential = new InteractiveBrowserCredential(options);
            _graphClient = new GraphServiceClient(credential, new[] { "Calendars.ReadWrite" });
            
            _logger.LogInformation("GraphService initialized with interactive authentication");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize GraphService");
            throw;
        }
    }

    public async Task<Event> CreateEventAsync(BookingRequest request)
    {
        try
        {
            // First, get the current user's email
            var me = await _graphClient.Me.GetAsync();
            var userEmail = me.Mail ?? me.UserPrincipalName;
            
            if (string.IsNullOrEmpty(userEmail))
            {
                throw new ApplicationException("Could not determine current user's email address");
            }

            // Create the event in the current user's calendar
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
                Attendees = new List<Attendee>
                {
                    // Add the room as a required attendee
                    new Attendee
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = request.RoomEmail,
                            Name = request.RoomEmail
                        },
                        Type = AttendeeType.Required
                    }
                }
            };

            // Create the event in the current user's calendar
            var createdEvent = await _graphClient.Me.Events
                .PostAsync(@event);

            _logger.LogInformation("Successfully created event {EventId} in user's calendar with room {RoomEmail} as attendee",
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
