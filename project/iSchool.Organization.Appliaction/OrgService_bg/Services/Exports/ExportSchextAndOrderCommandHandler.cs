using AutoMapper;
using Dapper;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces.Organization;
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
    public class ExportSchextAndOrderCommandHandler : IRequestHandler<ExportSchextAndOrderCommand, string>
    {
        IMediator _mediator;
        IMapper _mapper;
        IConfiguration _config;
        OrgUnitOfWork _orgUnitOfWork;
        UnitOfWork _unitOfWork;

        private readonly IStatisticsQueries _statisticsQueries;


        public ExportSchextAndOrderCommandHandler(IMediator mediator, IMapper mapper,
            IOrgUnitOfWork orgUnitOfWork, IUnitOfWork unitOfWork,
            IConfiguration config, IStatisticsQueries statisticsQueries)
        {
            this._mediator = mediator;
            this._mapper = mapper;
            this._config = config;
            this._orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
            this._unitOfWork = unitOfWork as UnitOfWork;
            this._statisticsQueries = statisticsQueries;
        }

        public async Task<string> Handle(ExportSchextAndOrderCommand cmd, CancellationToken cancellation)
        {
            IEnumerable<(string r1c, Func<int, int, dynamic, object> wr)> Wcell()
            {
                yield return ("日期", (row, col, data) => data.date);
                yield return ("源头学校+学部名称", (row, col, data) => data.schext_fullname);
                yield return ("链接", (row, col, data) => data.surl);
                yield return ("str_eid", (row, col, data) => data.str_eid);
                yield return ("学部id", (row, col, data) => data.eid == default(Guid) ? "" : data.str_eid);
                yield return ("课程短id", (row, col, data) => UrlShortIdUtil.Long2Base32(Convert.ToInt64(data.course_no)));
                yield return ("课程名称", (row, col, data) => data.course_title);
                yield return ("机构短id", (row, col, data) => UrlShortIdUtil.Long2Base32(Convert.ToInt64(data.org_no)));
                yield return ("机构名称", (row, col, data) => data.org_name);
                yield return ("UV", (row, col, data) => data.uv);
                yield return ("PV", (row, col, data) => data.pv);
                yield return ("支付成功单数(含退款,退款也算曾经支付成功过)", (row, col, data) => data.c1);
                yield return ("支付成功单数(不含退款)", (row, col, data) => data.c2);
                yield return ("支付成功人数(含退款,退款也算曾经支付成功过)", (row, col, data) => data.uc1);
                yield return ("支付成功人数(不含退款)", (row, col, data) => data.uc2);
            }

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Sheet1");
            int row = 1, col = 1;
            foreach (var (r1c, _) in Wcell())
            {
                sheet.Cells[row, col++].Value = r1c;
            }

            var items = await GetDatas3(cmd);
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
            var name = $"微信院校库转化数据统计"
                + (cmd.StartTime != null && cmd.EndTime != null ? $"({cmd.StartTime:yyyyMMdd}-{cmd.EndTime:yyyyMMdd})"
                    : cmd.EndTime == null ? $"({cmd.StartTime:yyyyMMdd}-{DateTime.Now:yyyyMMdd})"
                    : cmd.StartTime == null ? $"({DateTime.Now:yyyyMMdd}-{cmd.EndTime:yyyyMMdd})"
                    : "")
                + "." + id[..8];
            package.SaveAs(new FileInfo(Path.Combine(AppContext.BaseDirectory, _config["AppSettings:XlsxDir"], $"{name}.xlsx")));
            return name;
        }

        #region old codes
        async Task<IEnumerable<DataModel>> GetDatas(ExportSchextAndOrderCommand cmd)
        {
            var sql = $@"
select [date],eid,surl,courseid,course_no,course_title,--count(code) as c0,count(DISTINCT userid) as uc0,
sum(case when status>=103 then 1 else 0 end)as c1,
sum(case when status=103 or status>300 then 1 else 0 end)as c2,
count(DISTINCT case when status>=103 then userid else null end)as uc1,
count(DISTINCT case when status=103 or status>300 then userid else null end)as uc2,
org_no,org_name
from(
select convert(varchar(12),o.createtime,112)as [date],o.code,o.status,o.userid,o.isvalid,p.productid,
p.courseid,json_value(p.ctn,'$.no') as course_no,json_value(p.ctn,'$.title') as course_title,
json_value(p.ctn,'$.orgId') as orgid,json_value(p.ctn,'$.orgNo') as org_no,json_value(p.ctn,'$.orgName') as org_name,
json_value(p.SourceExtend,'$.surl') as surl,lower(json_value(p.SourceExtend,'$.eid'))as str_eid,isnull(try_convert(uniqueidentifier,json_value(p.SourceExtend,'$.eid')),'00000000-0000-0000-0000-000000000000') as eid
from [order] o join OrderDetial p on o.id=p.orderid
where o.IsValid=1 and o.type>=2
{"and o.CreateTime>=@StartTime".If(cmd.StartTime != null)} {"and o.CreateTime<@EndTime".If(cmd.EndTime != null)}
)T where 1=1 --and eid is not null
group by [date],eid,surl,courseid,course_no,course_title,org_no,org_name
order by [date] desc,eid
";
            var datas = (await _orgUnitOfWork.DbConnection.QueryAsync<DataModel>(sql, new
            {
                StartTime = cmd.StartTime != null ? cmd.StartTime.Value.Date : (DateTime?)null,
                EndTime = cmd.EndTime != null ? (cmd.EndTime.Value.AddDays(1).Date) : (DateTime?)null,
            })).AsArray();

            foreach (var sch in SplitArr(datas, 50))
            {
                var eids = sch.Select(_ => _.eid).Where(_ => _ != default);
                sql = @"
select e.id as eid,(s.name+'-'+e.name) as fullname 
from school s join SchoolExtension e on s.id=e.sid
where s.isvalid=1 and e.isvalid=1 and e.id in @eids
";
                var ls = await _unitOfWork.QueryAsync<(Guid, string)>(sql, new { eids });
                foreach (var d in sch)
                {
                    if (!ls.TryGetOne(out var x, _ => _.Item1 == d.eid)) continue;
                    d.schext_fullname = x.Item2;
                }
            }

            if (datas.Length < 1) return datas;

            var st = fmtToTime(datas.Min(_ => _.date));
            var et = fmtToTime(datas.Max(_ => _.date)).AddDays(1).AddMilliseconds(-1);
            var pvuvs = _statisticsQueries.GetPvUvForWebChat(st, et);

            foreach (var data in datas)
            {
                var b = pvuvs.TryGetOne(out var v, _ => Guid.TryParse(_._id.courseid, out var cid) && cid == data.courseid
                    && Guid.TryParse(_._id.eid, out var eid) && eid == data.eid
                    && data.date == _._id.day?.Replace("-", "").Replace("/", "")
                );
                if (!b) continue;

                data.pv = v.pv;
                data.uv = v.uv;
            }

            return datas;
        }
        #endregion

        async Task<IEnumerable<DataModel>> GetDatas2(ExportSchextAndOrderCommand cmd)
        {
            var sql = $@"
select [date],str_eid,surl,courseid,course_no,course_title,--count(code) as c0,count(DISTINCT userid) as uc0,
sum(case when status>=103 then 1 else 0 end)as c1,
sum(case when status=103 or status>300 then 1 else 0 end)as c2,
count(DISTINCT case when status>=103 then userid else null end)as uc1,
count(DISTINCT case when status=103 or status>300 then userid else null end)as uc2,
org_no,org_name
from(
select convert(varchar(12),o.createtime,112)as [date],o.code,o.status,o.userid,o.isvalid,p.productid,
p.courseid,json_value(p.ctn,'$.no') as course_no,json_value(p.ctn,'$.title') as course_title,
json_value(p.ctn,'$.orgId') as orgid,json_value(p.ctn,'$.orgNo') as org_no,json_value(p.ctn,'$.orgName') as org_name,
json_value(p.SourceExtend,'$.surl') as surl,lower(json_value(p.SourceExtend,'$.eid'))as str_eid,isnull(try_convert(uniqueidentifier,json_value(p.SourceExtend,'$.eid')),'00000000-0000-0000-0000-000000000000') as eid
from [order] o join OrderDetial p on o.id=p.orderid
where o.IsValid=1 and o.type>=2
{"and o.CreateTime>=@StartTime".If(cmd.StartTime != null)} {"and o.CreateTime<@EndTime".If(cmd.EndTime != null)}
)T where 1=1 and isnull(surl,'')<>''
group by [date],str_eid,surl,courseid,course_no,course_title,org_no,org_name
order by [date] desc,str_eid
";
            var order_datas = (await _orgUnitOfWork.DbConnection.QueryAsync<DataModel>(sql, new
            {
                StartTime = cmd.StartTime != null ? cmd.StartTime.Value.Date : (DateTime?)null,
                EndTime = cmd.EndTime != null ? (cmd.EndTime.Value.AddDays(1).Date) : (DateTime?)null,
            })).AsArray();

            var startTime = cmd.StartTime ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, DateTimeKind.Local);
            var endTime = cmd.EndTime ?? DateTime.Now;

            var st = startTime.Date;
            var et = endTime.AddDays(1).AddMilliseconds(-1);
            var pvuvs = _statisticsQueries.GetPvUvForWebChat(st, et);

            var datas = pvuvs.Select(pvuv =>
            {
                var isgid = Guid.TryParse(pvuv._id.courseid, out var courseid);
                var cno = isgid ? 0 : UrlShortIdUtil.Base322Long(pvuv._id.courseid);

                return new DataModel
                {
                    date = pvuv._id.day.Replace("-", "").Replace("/", ""),
                    str_eid = pvuv._id.eid?.ToLower(),
                    eid = pvuv._id.EidUUid,
#if DEBUG
                    surl = $"https://m3.sxkid.com/school_detail_wechat/eid={pvuv._id.eid.ToLower()}",
#else
                    surl = $"https://m.sxkid.com/school_detail_wechat/eid={pvuv._id.eid.ToLower()}",
#endif
                    courseid = isgid ? courseid : default,
                    course_no = cno,
                    pv = pvuv.pv,
                    uv = pvuv.uv,
                };
            }).OrderByDescending(_ => _.date).ToList();
            //if (datas.Count < 1) return order_datas;

            foreach (var sch in SplitArr(datas, 50))
            {
                var eids = sch.Where(_ => _ != default).Select(_ => _.eid).Distinct();
                sql = @"
select e.id as eid,(s.name+'-'+e.name) as fullname 
from school s join SchoolExtension e on s.id=e.sid
where s.isvalid=1 and e.isvalid=1 and e.id in @eids
";
                var ls = await _unitOfWork.QueryAsync<(Guid, string)>(sql, new { eids });
                foreach (var d in sch)
                {
                    if (!ls.TryGetOne(out var x, _ => _.Item1 == d.eid)) continue;
                    d.schext_fullname = x.Item2;
                }

                var cids = sch.Select(_ => _.courseid).Where(_ => _ != default).Distinct();
                var cnos = sch.Select(_ => _.course_no).Where(_ => _ != default).Distinct();
                sql = $@"
select c.id,c.no as course_no,c.title as course_title,o.no as org_no,o.name as org_name
from Course c left join Organization o on c.orgid=o.id
where 1=1 {"and c.id in @cids".If(cids.Any())} {"and c.no in @cnos".If(cnos.Any())}
";
                var ls2 = await _orgUnitOfWork.QueryAsync<(Guid id, long course_no, string course_title, long org_no, string org_name)>(sql, new { cids, cnos });
                foreach (var d in sch)
                {
                    if (!ls2.TryGetOne(out var x, _ => (d.courseid != default ? d.courseid == _.id : d.course_no == _.course_no))) continue;
                    d.courseid = x.id;
                    d.course_no = x.course_no;
                    d.course_title = x.course_title;
                    d.org_no = x.org_no;
                    d.org_name = x.org_name;
                }
            }

            foreach (var data in order_datas)
            {
                if (!datas.TryGetOne(out var x, _ => _.courseid == data.courseid && _.str_eid == data.str_eid && _.date == data.date && _.surl == data.surl)) continue;
                data.schext_fullname = x.schext_fullname;
                data.uv = x.uv;
                data.pv = x.pv;
            }

            return datas;
        }

        async Task<IEnumerable<DataModel>> GetDatas3(ExportSchextAndOrderCommand cmd)
        {
            var sql = $@"
select [date],str_eid,surl,courseid,course_no,course_title,--count(code) as c0,count(DISTINCT userid) as uc0,
sum(case when status>=103 then 1 else 0 end)as c1,
sum(case when status=103 or status>300 then 1 else 0 end)as c2,
count(DISTINCT case when status>=103 then userid else null end)as uc1,
count(DISTINCT case when status=103 or status>300 then userid else null end)as uc2,
org_no,org_name
from(
select convert(varchar(12),o.createtime,112)as [date],o.code,o.status,o.userid,o.isvalid,p.productid,
p.courseid,json_value(p.ctn,'$.no') as course_no,json_value(p.ctn,'$.title') as course_title,
json_value(p.ctn,'$.orgId') as orgid,json_value(p.ctn,'$.orgNo') as org_no,json_value(p.ctn,'$.orgName') as org_name,
json_value(p.SourceExtend,'$.surl') as surl,lower(json_value(p.SourceExtend,'$.eid'))as str_eid,isnull(try_convert(uniqueidentifier,json_value(p.SourceExtend,'$.eid')),'00000000-0000-0000-0000-000000000000') as eid
from [order] o join OrderDetial p on o.id=p.orderid
where o.IsValid=1 and o.type>=2
{"and o.CreateTime>=@StartTime".If(cmd.StartTime != null)} {"and o.CreateTime<@EndTime".If(cmd.EndTime != null)}
)T where 1=1 and isnull(surl,'')<>''
group by [date],str_eid,surl,courseid,course_no,course_title,org_no,org_name
order by [date] desc,str_eid
";
            var order_datas = (await _orgUnitOfWork.DbConnection.QueryAsync<DataModel>(sql, new
            {
                StartTime = cmd.StartTime != null ? cmd.StartTime.Value.Date : (DateTime?)null,
                EndTime = cmd.EndTime != null ? (cmd.EndTime.Value.AddDays(1).Date) : (DateTime?)null,
            })).AsArray();

            var startTime = cmd.StartTime ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0, DateTimeKind.Local);
            var endTime = cmd.EndTime ?? DateTime.Now;

            var st = startTime.Date;
            var et = endTime.AddDays(1).AddMilliseconds(-1);
            var pvuvs = _statisticsQueries.GetPvUvForWebChat(st, et);

            var datas = pvuvs.Select(pvuv =>
            {
                var isgid = Guid.TryParse(pvuv._id.courseid, out var courseid);
                var cno = isgid ? 0 : UrlShortIdUtil.Base322Long(pvuv._id.courseid);

                return new DataModel
                {
                    date = pvuv._id.day.Replace("-", "").Replace("/", ""),
                    str_eid = pvuv._id.eid?.ToLower(),
                    eid = pvuv._id.EidUUid,
                    surl = pvuv._id.surl,
                    courseid = isgid ? courseid : default,
                    course_no = cno,
                    pv = pvuv.pv,
                    uv = pvuv.uv,
                };
            }).OrderByDescending(_ => _.date).ToList();
            if (datas.Count < 1) return datas;

            foreach (var sch in SplitArr(datas, 50))
            {
                var eids = sch.Where(_ => _.eid != default).Select(_ => _.eid).Distinct();
                sql = @"
select e.id as eid,(s.name+'-'+e.name) as fullname 
from school s join SchoolExtension e on s.id=e.sid
where s.isvalid=1 and e.isvalid=1 and e.id in @eids
";
                var ls = await _unitOfWork.QueryAsync<(Guid, string)>(sql, new { eids });
                foreach (var d in sch)
                {
                    if (!ls.TryGetOne(out var x, _ => _.Item1 == d.eid)) continue;
                    d.schext_fullname = x.Item2;
                }

                var cids = sch.Select(_ => _.courseid).Where(_ => _ != default).Distinct();
                var cnos = sch.Select(_ => _.course_no).Where(_ => _ != default).Distinct();
                sql = $@"
select c.id,c.no as course_no,c.title as course_title,o.no as org_no,o.name as org_name
from Course c left join Organization o on c.orgid=o.id
where 1=1 {"and c.id in @cids".If(cids.Any())} {"and c.no in @cnos".If(cnos.Any())}
";
                var ls2 = await _orgUnitOfWork.QueryAsync<(Guid id, long course_no, string course_title, long org_no, string org_name)>(sql, new { cids, cnos });
                foreach (var d in sch)
                {
                    if (!ls2.TryGetOne(out var x, _ => (d.courseid != default ? d.courseid == _.id : d.course_no == _.course_no))) continue;
                    d.courseid = x.id;
                    d.course_no = x.course_no;
                    d.course_title = x.course_title;
                    d.org_no = x.org_no;
                    d.org_name = x.org_name;
                }
            }

            foreach (var data in datas)
            {
                if (!order_datas.TryGetOne(out var x, _ => _.courseid == data.courseid && _.str_eid == data.str_eid && _.date == data.date && _.surl == data.surl)) continue;
                data.c1 = x.c1;
                data.c2 = x.c2;
                data.uc1 = x.uc1;
                data.uc2 = x.uc2;
            }

            return datas;
        }

        static IEnumerable<T[]> SplitArr<T>(IEnumerable<T> collection, int c /* c > 0 */)
        {
            for (var arr = collection; arr.Any();)
            {
                yield return arr.Take(c).ToArray();
                arr = arr.Skip(c);
            }
        }

        class DataModel
        {
            public string date { get; set; }
            public string str_eid { get; set; }
            public Guid eid { get; set; }
            public string schext_fullname { get; set; }
            public string surl { get; set; }
            public Guid courseid { get; set; }
            public long course_no { get; set; }
            public string course_title { get; set; }
            public long org_no { get; set; }
            public string org_name { get; set; }

            public int c1 { get; set; }
            public int c2 { get; set; }
            public int uc1 { get; set; }
            public int uc2 { get; set; }

            public int pv { get; set; }
            public int uv { get; set; }
        }

        static DateTime fmtToTime(ReadOnlySpan<char> s)
        {
            // 20210809
            return DateTime.Parse($"{new string(s[..4])}-{new string(s[4..6])}-{new string(s[6..])}");
        }
    }
}
