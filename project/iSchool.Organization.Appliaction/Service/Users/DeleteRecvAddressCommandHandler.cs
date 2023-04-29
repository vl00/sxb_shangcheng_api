using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class DeleteRecvAddressCommandHandler : IRequestHandler<DeleteRecvAddressCommand, bool>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;        
        IMapper mapper;
        IConfiguration config;

        public DeleteRecvAddressCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config,
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;            
            this.mapper = mapper;
            this.config = config;
        }

        public async Task<bool> Handle(DeleteRecvAddressCommand cmd, CancellationToken cancellation)
        {
            
            return true;
        }

    }
}
