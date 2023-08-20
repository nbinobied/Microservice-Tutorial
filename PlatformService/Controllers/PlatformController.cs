using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformController : ControllerBase
    {
        private readonly IPlatformRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformController(IPlatformRepository repository, IMapper mapper,
                                    ICommandDataClient commandDataClient,
                                    IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _commandDataClient = commandDataClient;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformDto>> GetPlatforms()
        {
            Console.WriteLine("--> Getting Platforms...");
            var platforms = _repository.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformDto>>(platforms));
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformDto> GetPlatformById(int id)
        {
            Console.WriteLine("--> Getting Platform...");
            var platform = _repository.GetPlatformById(id);
            if (platform != null)
            {
                return Ok(_mapper.Map<PlatformDto>(platform));
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformDto>> CreatePlatform(PlatformCreateDto createPlatformDto)
        {
            Console.WriteLine("--> Creating Platform...");
            var platform = _mapper.Map<Platform>(createPlatformDto);
            _repository.CreatePlatform(platform);
            _repository.SaveChanges();
            var platfromDto = _mapper.Map<PlatformDto>(platform);

            //Send Sync Message
            try
            {
                await _commandDataClient.SendPlatformToCommand(platfromDto);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            //Send Async Message
            try
            {
                var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platfromDto);
                platformPublishedDto.Event = "Platform_Published";
                _messageBusClient.PublishNewPlatform(platformPublishedDto);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cloudn't Send Async - Error: {e.Message}");
            }

            return CreatedAtRoute(nameof(GetPlatformById), new { Id = platfromDto.Id }, platfromDto);
        }
    }
}
