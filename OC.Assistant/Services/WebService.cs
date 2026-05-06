using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OC.Assistant.Sdk;

namespace OC.Assistant.Services;

/// <summary>
/// Represents an HTTP-based API.
/// </summary>
[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class WebService
{
    private readonly ConcurrentQueueCapped<Message> _messageQueue = new(1000);
    private readonly AppService _appService;
    private readonly PluginService _pluginService;

    public WebService(AppService appService, PluginService pluginService)
    {
        _appService = appService;
        _pluginService = pluginService;

        Task.Run(async () =>
        {
            Logger.Info += Info;
            Logger.Warning += Warning;
            Logger.Error += Error;
            
            var builder = WebApplication.CreateBuilder();
            
            builder.WebHost.
                UseUrls($"http://{App.Settings.IpAddress}:{App.Settings.WebApiPort}");
            
            builder.Services
                .AddEndpointsApiExplorer()
                .AddControllers()
                .AddXmlSerializerFormatters();
            
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new WebApiLogger());
        
            var app = builder.Build();
            
            app.MapGet("/api/messages", HandleGetMessages);
            app.MapGet("/api/plugins", HandleGetPlugins);
            app.MapPost("/api/start", HandleApplicationStart);
            app.MapPost("/api/stop", HandleApplicationStop);
            
            app.MapPost("/api/data", async (HttpRequest request) 
                    => await HandleData(request))
                .Accepts<XElement>("application/xml");
            
            app.MapPost("/api/request", async (HttpRequest request) 
                    => await HandleData(request))
                .Accepts<XElement>("application/xml");
            
            Logger.LogInfo(this, $"WebApi listening on {app.Configuration["urls"]}");
            await app.RunAsync();
        });
    }

    private record Message(object? Sender, string Content, string Type)
    {
        public DateTime TimeStamp { get; } = DateTime.Now;
    }

    private void Info(object sender, string content)
        => _messageQueue.Enqueue(new Message(sender.ToString()?.Split(':')[0], content, nameof(Info)));
    
    private void Warning(object sender, string content)
        => _messageQueue.Enqueue(new Message(sender.ToString()?.Split(':')[0], content, nameof(Warning)));
    
    private void Error(object sender, string content)
        => _messageQueue.Enqueue(new Message(sender.ToString()?.Split(':')[0], content, nameof(Error)));

    private IResult HandleGetMessages(bool reset = false)
        => Results.Ok(reset ? _messageQueue.DequeueAll() : _messageQueue);
    
    private static IResult HandleGetPlugins(string? name = null)
    {
        if (XmlFile.Instance.Path is null)
        {
            return Results.Problem(
                detail: "Assistant is not connected.",
                statusCode: 400,
                title: "NotConnected");
        }

        var plugins = name == null ? 
            XmlFile.Instance.Plugins :
            XmlFile.Instance.Plugins
                .Elements("Plugin")
                .FirstOrDefault(x => x.Attribute("Name")?.Value == name);
        
        if (plugins is null)
        {
            return Results.Problem(
                detail: $"Plugin {name} not found.",
                statusCode: 400,
                title: "Not found");
        }
        
        return Results.Content(plugins.ToString(), "application/xml");
    }
    
    private IResult HandleApplicationStart(string? channelType = null)
    {
        try
        {
            if (!_appService.IsConnected) throw new Exception("Assistant is not connected.");
            if (BusyState.IsSet) throw new Exception("Assistant is busy.");
            _pluginService.StartPlugins(channelType is null ? null : Type.GetType(channelType));
            return Results.Accepted();
        }
        catch (Exception e)
        {
            return Results.Problem(detail: e.Message, statusCode: 400, title: "Command not executed");
        }
    }
    
    private IResult HandleApplicationStop(string? channelType = null)
    {
        try
        {
            if (!_appService.IsConnected) throw new Exception("Assistant is not connected.");
            if (BusyState.IsSet) throw new Exception("Assistant is busy.");
            _pluginService.StopPlugins(channelType is null ? null : Type.GetType(channelType));
            return Results.Accepted();
        }
        catch (Exception e)
        {
            return Results.Problem(detail: e.Message, statusCode: 400, title: "Command not executed");
        }
    }
    
    private static async Task<IResult> HandleData(HttpRequest request)
    {
        try
        {
            var message = await XElement.LoadAsync(request.Body, LoadOptions.PreserveWhitespace, CancellationToken.None);
            if (message.Element("Identifier")?.Value is not {} identifier) throw new Exception("Identifier is missing.");
            if (message.Element("Payload") is not {} payload) throw new Exception("Payload is missing.");
            
            await Task.Run(() =>
            {
                EventSystem.InvokeApiEvent($"data/{identifier}", payload);
            });
            
            return Results.Accepted();
        }
        catch (Exception e)
        {
            return Results.Problem(detail: e.Message, statusCode: 400, title: "Invalid data");
        }
    }
    
    private static async Task<IResult> HandleRequest(HttpRequest request)
    {
        try
        {
            var message = await XElement.LoadAsync(request.Body, LoadOptions.PreserveWhitespace, CancellationToken.None);
            if (message.Element("Identifier")?.Value is not {} identifier) throw new Exception("Identifier is missing.");
            if (message.Element("Payload") is not {} payload) throw new Exception("Payload is missing.");
            
            var results = await Task.Run(() 
                => EventSystem.InvokeApiRequest($"request/{identifier}", payload));
            
            return Results.Content(new XElement("Response", results).ToString(), "application/xml");
        }
        catch (Exception e)
        {
            return Results.Problem(detail: e.Message, statusCode: 400, title: "Invalid data");
        }
    }
}