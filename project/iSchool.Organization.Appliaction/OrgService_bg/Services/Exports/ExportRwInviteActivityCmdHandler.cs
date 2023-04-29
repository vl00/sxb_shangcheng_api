using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Domain;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Exports
{
    public class ExportRwInviteActivityCmdHandler : IRequestHandler<ExportRwInviteActivityCmd, string>
    {
        IMediator _mediator;
        IMapper _mapper;
        IConfiguration _config;
        OrgUnitOfWork _orgUnitOfWork;
        UnitOfWork _unitOfWork;
        CSRedisClient _redis;

        public ExportRwInviteActivityCmdHandler(IMediator mediator, IMapper mapper, CSRedisClient redis,
            IOrgUnitOfWork orgUnitOfWork, IUnitOfWork unitOfWork,
            IConfiguration config)
        {
            this._mediator = mediator;
            this._mapper = mapper;
            this._config = config;
            this._orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
            this._unitOfWork = unitOfWork as UnitOfWork;
            this._redis = redis;
        }

        public async Task<string> Handle(ExportRwInviteActivityCmd cmd, CancellationToken cancellation)
        {            
            IEnumerable<(string r1c, Func<int, int, DataModel, object> wr)> Wcell()
            {
                yield return ("昵称(微信)", (row, col, data) => data.un_nickname);
                yield return ("unionID", (row, col, data) => data.unionID);
                yield return ("昵称(上学帮账号)", (row, col, data) => data.u_nickname);
                yield return ("userid", (row, col, data) => data.userid);
                yield return ("下单数", (row, col, data) => data.c);
            }

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Sheet1");
            int row = 1, col = 1;
            foreach (var (r1c, _) in Wcell())
            {
                sheet.Cells[row, col++].Value = r1c;
            }

            var items = await GetDatas(cmd);
            foreach (var item in items)
            {
                row++; col = 1;
                foreach (var (_, wr) in Wcell())
                {
                    sheet.Cells[row, col].Value = wr(row, col, item)?.ToString();
                    col++;
                }
            }

            var id = Guid.NewGuid().ToString("n");
            package.SaveAs(new FileInfo(Path.Combine(AppContext.BaseDirectory, _config["AppSettings:XlsxDir"], $"{id}.xlsx")));
            return id;
        }

        async Task<IEnumerable<DataModel>> GetDatas(ExportRwInviteActivityCmd cmd)
        {
            var courseIds = await _redis.SMembersAsync<Guid>(CacheKeys.RwInviteActivity_InvisibleOnlineCourses);
            if ((courseIds?.Length ?? 0) < 1) return new DataModel[0];

            var sql = $@"
select o.userid,min(u.nickname) as u_nickname,o.unionID,min(un.nickname) as un_nickname,count(1) as c 
from (
select o.createtime,o.code,o.userid,o.type,json_value(p.ctn,'$._RwInviteActivity.unionID')as unionID,
json_value(p.ctn,'$._RwInviteActivity.courseExchange.type')as courseExchange_type,
try_convert(float,json_value(p.ctn,'$._RwInviteActivity.consumedScores'))as consumed_scores,
p.courseid,p.ctn
from [order] o join OrderDetial p on o.id=p.orderid
where o.IsValid=1 and o.type>=2 
and p.courseid in @courseIds
)o left join [iSchoolUser].dbo.userinfo u on u.id=o.userid
left join [iSchoolUser].dbo.unionid_weixin un on un.valid=1 and un.userid=o.userid
where o.unionID is not null {"and o.courseExchange_type=@CourseExchange_type".If(cmd.CourseExchange_type != null)}
group by o.userid,o.unionID
";
            var datas = (await _orgUnitOfWork.DbConnection.QueryAsync<DataModel>(sql, new 
            {
                courseIds,
                cmd.CourseExchange_type,
            })).AsArray();

            return datas;
        }


        class DataModel
        {
            public string un_nickname { get; set; }
            public string unionID { get; set; }
            public string u_nickname { get; set; }
            public Guid userid { get; set; }
            public int c { get; set; }
        }


    }
}
