using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Infrastructure.Repositories.Organization
{
    /// <summary>
    /// 备注delete还差修改MidifyDateTime跟Modifier
    /// 这里的查询，只有单表的。多表查询需要自己写
    /// 这里实现基础仓储的接口，提供访问数据库的方法
    /// </summary>
    /// <typeparam name="Tentiy"></typeparam>
    public class OrgBaseRepository<Tentiy> : BaseRepository<Tentiy>, IBaseRepository<Tentiy> where Tentiy : class
    {


        public OrgBaseRepository(IOrgUnitOfWork IUnitOfWork) : base(IUnitOfWork)
        {

        }
    }
}
