using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;
using System.Text.Json;

namespace CommandService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _mapper = mapper;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);
            switch (eventType)
            {
                case EventType.PlatformPublished:
                    AddPlatform(message);
                    break;
                default:
                    break;
            }
        }

        private void AddPlatform(string platformPublishedMessage)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();
                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                try
                {
                    var platform = _mapper.Map<Platform>(platformPublishedDto);
                    if (!repository.ExternalPlatformExists(platform.Id))
                    {
                        repository.CreatePlatform(platform);
                        repository.SaveChanges();
                        Console.WriteLine($"--> Platform added.");
                    }
                    else
                    {
                        Console.WriteLine($"--> Platform already exists...");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"--> Couldn't add Platform to DB: {e.Message}");
                }

            }
        }

        private EventType DetermineEvent(string message)
        {
            Console.WriteLine("--> Determining Event.");
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(message);
            switch (eventType.Event)
            {
                case "Platform_Published":
                    Console.WriteLine($"--> Platform Published Event Detected.");
                    return EventType.PlatformPublished;
                default:
                    Console.WriteLine($"--> Couldn't determine the event type");
                    return EventType.Undetermined;
            }
        }
    }

    public enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}
