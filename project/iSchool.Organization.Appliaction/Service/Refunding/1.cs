using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    /* public class CourseMultiGoodsSettleInfosQueryHandler : IRequestHandler<CourseMultiGoodsSettleInfosQuery, CourseMultiGoodsSettleInfosQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;        
        IConfiguration _config;

        public CourseMultiGoodsSettleInfosQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;            
            this._config = config;
        }

        public async Task<CourseMultiGoodsSettleInfosQryResult> Handle(CourseMultiGoodsSettleInfosQuery query, CancellationToken cancellation)
        {
            var result = new CourseMultiGoodsSettleInfosQryResult();
            await default(ValueTask);

            
            return result;
        }

    } */
}
