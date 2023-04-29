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
    public class RecvAddressPglistQueryHandler : IRequestHandler<RecvAddressPglistQuery, RecvAddressPglistQueryResult>
    {
        OrgUnitOfWork _unitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;        
        IMapper _mapper;
        IConfiguration _config;

        public RecvAddressPglistQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config,
            IMapper mapper)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;            
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<RecvAddressPglistQueryResult> Handle(RecvAddressPglistQuery query, CancellationToken cancellation)
        {
            var result = new RecvAddressPglistQueryResult();
            await default(ValueTask);

            var rr = await _mediator.Send(new SwaggerSampleDataQuery(nameof(RecvAddressPglistQueryResult)));
            result = rr.GetData<RecvAddressPglistQueryResult>();
            result.PageInfo.CurrentPageIndex = query.PageIndex;
            result.PageInfo.PageSize = query.PageSize;

            return result;
        }

    }
}
