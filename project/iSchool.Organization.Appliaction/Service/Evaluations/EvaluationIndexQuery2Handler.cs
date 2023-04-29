using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class EvaluationIndexQuery2Handler : IRequestHandler<EvaluationIndexQuery2, EvaluationIndexQueryResult2>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IConfiguration config;

        public EvaluationIndexQuery2Handler(IOrgUnitOfWork unitOfWork, IMediator mediator, IConfiguration config)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.config = config;
        }

        public async Task<EvaluationIndexQueryResult2> Handle(EvaluationIndexQuery2 req, CancellationToken cancellation)
        {
            var result = new EvaluationIndexQueryResult2();            

            var pg = await mediator.Send(new EvaluationLoadMoreQuery 
            {
                Stick = req.Stick,
                Subj = req.Subj ?? "",
                Age = req.Age ?? "",
                PageIndex = req.PageIndex,                
            });
            result.PageInfo = pg;
            
            // 固定显示
            result.Subjs = GetSubjs().ToArray();

            return result;
        }

        IEnumerable<(string, string)> GetSubjs()
        {
            foreach (var c1 in config.GetSection("AppSettings:elvtMainPage_subjSide").GetChildren())
            {
                yield return (c1["item1"], c1["item2"]);
            }
        }
    }
}
