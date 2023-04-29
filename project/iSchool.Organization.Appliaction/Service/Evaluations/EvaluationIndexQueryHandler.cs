using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class EvaluationIndexQueryHandler : IRequestHandler<EvaluationIndexQuery, EvaluationIndexQueryResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IConfiguration config;
       // IKeyValueRepository _kevValueRepo;
        public EvaluationIndexQueryHandler( IOrgUnitOfWork unitOfWork, IMediator mediator, IConfiguration config)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.config = config;
         
        }

        public async Task<EvaluationIndexQueryResult> Handle(EvaluationIndexQuery req, CancellationToken cancellation)
        {
            var result = new EvaluationIndexQueryResult();            

            var pg = await mediator.Send(new EvaluationLoadMoreQuery 
            {
                Age=req.Age,
                Subj = req.Subj,
                PageIndex = 1, 
                Stick=req.Stick
            });
            result.Evaluations = pg.CurrItems;
            result.TotalPageCount = pg.TotalPageCount;

            //result.Subjs = EnumUtil.GetDescs<SubjectEnum>().Select(x => (x.Desc, (int)x.Value)).ToArray();
            // 固定显示
            result.Subjs = GetSubjs();

            return result;
        }

        IEnumerable<(string, string)> GetSubjs()
        {
            foreach (var c1 in config.GetSection("AppSettings:elvtMainPage_subjSide").GetChildren())
            {
                yield return (c1["item1"],c1["item2"]);
            }
        }
    }
}
