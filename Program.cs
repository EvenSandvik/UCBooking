using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UCBookingAPI.Services;
using Microsoft.Azure.Functions.Worker.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Net;
using System.Threading.Tasks;
using System.Linq;


// Create and start the host
var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.UseMiddleware<CorsMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        config
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddScoped<IGraphService, GraphService>();
        
        // Configure CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    })
    .ConfigureLogging(logging =>
    {
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            var loggerProvider = options.Rules.FirstOrDefault(rule => 
                rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            
            if (loggerProvider is not null)
            {
                options.Rules.Remove(loggerProvider);
            }
        });
    })
    .Build();

// Start the host
await host.RunAsync();

// CORS Middleware to handle CORS headers
public class CorsMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        // Get the HTTP request data
        var httpRequestData = await context.GetHttpRequestDataAsync();
        if (httpRequestData == null)
        {
            await next(context);
            return;
        }

        // Create the response
        var response = httpRequestData.CreateResponse();
        
        // Add CORS headers to all responses
        response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:5173,http://localhost:3000");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        
        // Handle preflight
        if (httpRequestData.Method == "OPTIONS")
        {
            response.StatusCode = HttpStatusCode.NoContent;
            context.GetInvocationResult().Value = response;
            return;
        }
        
        // For non-OPTIONS requests, process normally and then add CORS headers
        await next(context);
        
        // Get the response that was set by the function
        var functionResponse = context.GetHttpResponseData();
        if (functionResponse != null)
        {
            // Add CORS headers to the function's response
            foreach (var header in response.Headers)
            {
                if (!functionResponse.Headers.Contains(header))
                {
                    functionResponse.Headers.Add(header.Key, header.Value);
                }
            }
        }
    }
}
