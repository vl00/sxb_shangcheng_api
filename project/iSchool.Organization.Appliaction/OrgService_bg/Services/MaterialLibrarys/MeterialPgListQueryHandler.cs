using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Material
{
    public class MeterialPgListQueryHandler : IRequestHandler<MeterialPgListQuery, MeterialPgListQueryResponse>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;

        public MeterialPgListQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<MeterialPgListQueryResponse> Handle(MeterialPgListQuery query, CancellationToken cancellation)
        {
            var sql = $@"
SELECT COUNT(1)  FROM dbo.MaterialLibrary AS m  LEFT JOIN dbo.Course AS c ON m.CourseId=c.id
WHERE c.IsValid=1 {"and m.CreateTime>=@StartTime".If(query.StartTime != null)} {"and m.CreateTime<@EndTime".If(query.EndTime != null)}
{"and c.title like @CourseName".If(!query.CourseName.IsNullOrEmpty())} {"and m.title like @MeterialName".If(!query.MeterialName.IsNullOrEmpty())} 
---
SELECT m.id,m.Title,m.DownloadTime,m.CreateTime,m.Status,m.CourseId,c.title as CourseName,m.thumbnails,m.videoCover
from MaterialLibrary as m left join dbo.Course as c on m.CourseId=c.id
where c.IsValid=1 {"and m.CreateTime>=@StartTime".If(query.StartTime != null)} {"and m.CreateTime<@EndTime".If(query.EndTime != null)}
{"and c.title like @CourseName".If(!query.CourseName.IsNullOrEmpty())} {"and m.title like @MeterialName".If(!query.MeterialName.IsNullOrEmpty())} 
order by m.createtime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";

            var endTime = query.EndTime == null ? (DateTime?)null : query.EndTime.Value.AddDays(1).Date;

            var dys = new DynamicParameters(query)
                .Set("CourseName", $"%{query.CourseName}%")
                .Set("MeterialName", $"%{query.MeterialName}%")
                .Set("EndTime", endTime);

            var gr = await _orgUnitOfWork.DbConnection.QueryMultipleAsync(sql, dys);
            var pg = new PagedList<MeterialItemDto> { CurrentPageIndex = query.PageIndex, PageSize = query.PageSize };
            pg.TotalItemCount = await gr.ReadFirstAsync<int>();
            pg.CurrentPageItems = gr.Read<MeterialItemDto, (string, string), MeterialItemDto>(
                splitOn: "thumbnails",
                func: (dto, x) =>
                {
                    var (thumbnails, videoCover) = x;
                    dto.Cover = !videoCover.IsNullOrEmpty() ? videoCover : 
                        thumbnails.IsNullOrEmpty() ? null : thumbnails.ToObject<string[]>().FirstOrDefault();
                    return dto;
                });
            return new MeterialPgListQueryResponse { PageInfo = pg };
        }
    }
}
