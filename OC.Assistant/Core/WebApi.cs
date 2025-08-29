using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OC.Assistant.Sdk;

namespace OC.Assistant.Core;

/// <summary>
/// Represents an HTTP-based API.
/// </summary>
[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public static class WebApi
{
    private static readonly ConcurrentQueueCapped<Message> MessageQueue = new(1000);
    private static bool _isRunning;
    
    public static event Action<XElement>? ConfigReceived;
    
    public static void BuildAndRun()
    {
        if (_isRunning)
        {
            throw new InvalidOperationException("Server is already running.");
        }
        
        _isRunning = true;

        Task.Run(async () =>
        {
            Logger.Info += Info;
            Logger.Warning += Warning;
            Logger.Error += Error;
            
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddControllers().AddXmlSerializerFormatters();
        
            var app = builder.Build();
         
            app.MapPost("/api/config", async (HttpRequest request) 
                    => await HandleConfig(request))
                .Accepts<XElement>("application/xml");
         
            app.MapPost("/api/timescaling", ([FromBody] double timeScaling) 
                => HandleTimeScaling(timeScaling));
         
            app.MapGet("/api/messages", HandleMessages);
        
            await app.RunAsync();
        });
    }
    
    private record Message(object? Sender, string Content, string Type)
    {
        public DateTime TimeStamp { get; } = DateTime.Now;
    }

    private static void Info(object sender, string content)
        => MessageQueue.Enqueue(new Message(sender.ToString()?.Split(':')[0], content, nameof(Info)));
    
    private static void Warning(object sender, string content)
        => MessageQueue.Enqueue(new Message(sender.ToString()?.Split(':')[0], content, nameof(Warning)));
    
    private static void Error(object sender, string content)
        => MessageQueue.Enqueue(new Message(sender.ToString()?.Split(':')[0], content, nameof(Error)));
    

    private static IResult HandleMessages(bool reset = false)
    {
        return Results.Ok(reset ? MessageQueue.DequeueAll() : MessageQueue);
    }
      
    private static IResult HandleTimeScaling(double timeScaling)
    {
        try
        {
            ApiLocal.Interface.TimeScaling = timeScaling;
            return Results.Accepted();
        }
        catch (Exception e)
        {
            return Results.Problem(detail: e.Message, statusCode: 400, title: "Invalid data");
        }
    }
    
    private static async Task<IResult> HandleConfig(HttpRequest request)
    {
        try
        {
            if (BusyState.IsSet || ProjectState.IsRunning)
            {
                return Results.Problem(detail: "Assistant is busy or running.", statusCode: 400, title: "Busy");
            }
            var config = await XElement.LoadAsync(request.Body, LoadOptions.PreserveWhitespace, CancellationToken.None);

            await Task.Run(() =>
            {
                ConfigReceived?.Invoke(config);
            });
            
            return Results.Accepted();
        }
        catch (Exception e)
        {
            return Results.Problem(detail: e.Message, statusCode: 400, title: "Invalid data");
        }
    }
}