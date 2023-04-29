using iSchool.Infrastructure.Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class MeterialPgListQueryResponse
    {
        public PagedList<MeterialItemDto> PageInfo { get; set; } = default!;

    }


    public class MeterialItemDto
    {
        public int Row { get; set; }
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        public int DownloadTime { get; set; }

        public DateTime? CreateTime { get; set; }

        public byte Status { get; set; }

        public Guid CourseId { get; set; }

        public string CourseName { get; set; }
    }
}
