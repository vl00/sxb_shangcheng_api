using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 根据机构下的课程变更，实时更新机构的科目
    /// </summary>
    public class UpdateOrgSubjectByCourseCommandHandler : IRequestHandler<UpdateOrgSubjectByCourseCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public UpdateOrgSubjectByCourseCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public async Task<ResponseResult> Handle(UpdateOrgSubjectByCourseCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var delCacheKeys = new List<string>()
                        {
                             CacheKeys.Del_Organizations.FormatWith("*")
                            ,CacheKeys.OrgDetails.FormatWith(request.OrgId)                                     
                            ,$"org:organization:orgz:{request.OrgId}:*"
                        };
            string sql = " select [subjects] from [dbo].[Organization] where id=@OrgId and IsValid=1";
            var dp = new DynamicParameters().Set("OrgId", request.OrgId);
            string oldSubjects =_orgUnitOfWork.DbConnection.Query<string>(sql, dp).FirstOrDefault();
            var listSubjects = new List<int>();
            if(!string.IsNullOrEmpty(oldSubjects) && oldSubjects != "[]")
            {
                listSubjects=JsonSerializationHelper.JSONToObject<List<int>>(oldSubjects);
            }
            switch (request.OperationType)
            {
                case 1://新增、上架
                    {
                        if (request.NewSubject!=null && !listSubjects.Contains((int)request.NewSubject))
                            listSubjects.Add((int)request.NewSubject);
                    }
                    break;
                case 2://编辑--课程编辑涉及更换机构(新旧)
                    {
                        
                        if (request.OldOrgId!=request.OrgId)//切换了机构
                        {
                            //1、旧机构旧科目是否需要删除,
                            if (request.OldOrgId!=null &&  request.OldSubject != null && !ISExitSubject(request.CourseId,(Guid)request.OldOrgId,(int)request.OldSubject))
                            {
                                listSubjects.Remove((int)request.OldSubject);
                            }
                            //2、新机构是否需要增加科目
                            if (request.OrgId != null && request.NewSubject != null && !ISExitSubject(request.CourseId, (Guid)request.OrgId, (int)request.NewSubject))
                            {
                                listSubjects.Add((int)request.NewSubject);
                            }

                        }
                        else//没切换机构
                        {
                            //1、切换了科目
                            if (request.OldSubject != request.NewSubject)
                            {
                                //1、(新旧机构相等)机构的旧科目是否需要删除,
                                if (request.OldOrgId != null && request.OldSubject != null && !ISExitSubject(request.CourseId, (Guid)request.OldOrgId, (int)request.OldSubject))
                                {
                                    listSubjects.Remove((int)request.OldSubject);
                                }
                                //2、机构是否需要增加新科目
                                if (request.OrgId != null && request.NewSubject != null && !ISExitSubject(request.CourseId, (Guid)request.OrgId, (int)request.NewSubject))
                                {
                                    listSubjects.Add((int)request.NewSubject);
                                }
                            }
                        }
                        delCacheKeys.AddRange(new List<string>() {
                            CacheKeys.OrgDetails.FormatWith("*")                                 
                            ,$"org:organization:orgz:*:info"
                        });
                    }
                    break;
                case 3://下架
                    {
                        if (request.NewSubject != null && request.OrgId!=null && !ISExitSubject(request.CourseId,(Guid)request.OrgId,(int)request.NewSubject))
                            listSubjects.Remove((int)request.NewSubject);
                    }
                    break;
            }

            dp.Set("subjects", JsonSerializationHelper.Serialize(listSubjects));
            string update_sql = "update [dbo].[Organization] set subjects=@subjects where id=@OrgId  and IsValid=1";
            try
            {
                _orgUnitOfWork.DbConnection.Execute(update_sql, dp);
                _ = _redisClient.BatchDelAsync(delCacheKeys, 10);
                return ResponseResult.Success("操作成功");
            }
            catch(Exception ex)
            {
                return ResponseResult.Failed($"系统错误：{ex.Message}");
            }          
           
        }

        /// <summary>
        /// 判断某个机构下是否存在除了正在变更的课程外【其他课程(科目相等)】
        /// （true:存在；false:不存在）
        /// </summary>
        /// <param name="id">课程Id</param>
        /// <param name="orgid">机构Id</param>
        /// <param name="subject">科目Id</param>
        /// <returns></returns>
        private bool ISExitSubject(Guid id,Guid orgid,int? subject)
        {           
            string sql = " select count(1)  from[dbo].[Course] where  id<>@id and orgid=@orgid  and IsValid = 1 and subject=@subject";
            return _orgUnitOfWork.DbConnection.Query<int>(sql, 
                new DynamicParameters()
                .Set("orgid", orgid)
                .Set("subject", subject)
                .Set("id",id)).FirstOrDefault()>0;
        }

    }
}
