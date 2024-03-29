﻿using AutoMapper;
using Grpc.Core;
using PlatformService.Data;
using PlatformService.Protos;

namespace PlatformService.SyncDataServices.Grpc
{
    public class GrpcPlatformService : GrpcPlatform.GrpcPlatformBase
    {
        private readonly IPlatformRepository _repository;
        private readonly IMapper _mapper;

        public GrpcPlatformService(IPlatformRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public override Task<PlatformResponse> GetAllPlatforms(GetAllRequests requests, ServerCallContext context)
        {
            var response = new PlatformResponse();
            var platforms = _repository.GetAllPlatforms();
            foreach (var platform in platforms)
            {
                response.Platform.Add(_mapper.Map<GrpcPlatformModel>(platform));
            }
            return Task.FromResult(response);
        }
    }
}
