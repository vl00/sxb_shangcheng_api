using iSchool.Organization.Domain.Modles;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain.Repository.Interfaces.Organization
{
    public interface IStatisticsQueries : IDependency
    {
        public List<PVUV4Wechat> GetPvUvForWebChat(DateTime startTime, DateTime endTime);
    }
}
