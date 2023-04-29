using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Domain.Enum;
using System.Linq;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 新增/编辑专题
    /// </summary>
    public class AddOrUpdateSpecialCommandHandler:IRequestHandler<AddOrUpdateSpecialCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public AddOrUpdateSpecialCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public Task<ResponseResult> Handle(AddOrUpdateSpecialCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if(!Enum.IsDefined(typeof(SpecialTypeEnum), request.SpecialType))
                    return Task.FromResult(ResponseResult.Failed("非法专题类型！"));
                var oldcount = _orgUnitOfWork.DbConnection.Query<int>($" SELECT count(1) FROM [Organization].[dbo].[Special] where IsValid=1 and status={(int)SpecialStatusEnum.Ok} and title='{request.Title}'  and id<>'{request.Id}' ").FirstOrDefault();
                if (oldcount > 0)
                {
                    return Task.FromResult(ResponseResult.Failed("专题名称重复，请重新编辑！"));
                }

                string opSql = "";
                string delSql = "";
                StringBuilder intSql = new StringBuilder();
                List<string> delKeys = new List<string>();
                if (request.IsAdd)//新增评测
                {
                    opSql += $@"insert into [dbo].[Special]([id], [title], [subtitle], [sharetitle], [sharesubtitle], [status], [banner], [CreateTime], [Creator], [IsValid],[type])
                            values(@id, @title, @subtitle, @sharetitle, @sharesubtitle, @status, @banner, @Time, @userId, @IsValid,@type);";
                }                    
                else//编辑评测
                {
                    delKeys.AddRange(new List<string>() {
                        "org:spcl:*"//专题  
                        ,CacheKeys.Rdk_big_spclLs.FormatWith(request.Id,"*","*")
                        ,CacheKeys.Rdk_big_spclLsTotal.FormatWith(request.Id,"*","*")
                        ,CacheKeys.Rdk_Big_spcl.FormatWith("*")
                        ,"org:evlt:info:*"//评测详情有机构信息、课程信息、专题
                    }); ;
                    if(request.SpecialType == SpecialTypeEnum.BigSpecial.ToInt())//大专题，编辑需删除历史绑定
                    {
                        delSql = "  delete [dbo].[SpecialSeries] where bigspecial=@bigspecial ";
                    }
                    
                    opSql += $@"update [dbo].[Special] set [title]=@title, [subtitle]=@subtitle, [sharetitle]=@sharetitle,
                            [sharesubtitle] = @sharesubtitle, [banner]= @banner,[ModifyDateTime]= @Time,[Modifier]= @userId where id = @id; ";
                }

                if (request.SmallSpecialIds?.Any()==true)//大专题绑定小专题 && request.SpecialType == SpecialTypeEnum.BigSpecial.ToInt()
                {
                    intSql.Append("  INSERT INTO [dbo].[SpecialSeries]([id], [bigspecial], [smallspecial], [sort], [IsValid]) VALUES ");
                    var list = request.SmallSpecialIds.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        intSql.AppendFormat($" {",".If(i>0)} (NEWID(), '{request.Id}', '{list[i]}', {i}, 1) ");
                    }
                }

                _orgUnitOfWork.BeginTransaction();

                if (!string.IsNullOrEmpty(delSql))
                    _orgUnitOfWork.DbConnection.Execute(delSql, new DynamicParameters().Set("bigspecial", request.Id),_orgUnitOfWork.DbTransaction);

                if (intSql.Length > 0) 
                    _orgUnitOfWork.DbConnection.Execute(intSql.ToString(),null, _orgUnitOfWork.DbTransaction);

                _orgUnitOfWork.DbConnection.Execute(opSql, new DynamicParameters()
                   .Set("id", request.Id)
                   .Set("title", request.Title)
                   .Set("subtitle", request.SubTitle)
                   .Set("sharetitle", request.ShareTitle)
                   .Set("sharesubtitle", request.ShareSubTitle)
                   .Set("status", (int)SpecialStatusEnum.Ok)
                   .Set("banner", request.Banner)
                   .Set("Time", DateTime.Now)
                   .Set("userId", request.UserId)
                   .Set("IsValid", true)
                   .Set("type",request.SpecialType)
                    ,_orgUnitOfWork.DbTransaction);
                _orgUnitOfWork.CommitChanges();
                #region 清除API那边相关的缓存
                delKeys.AddRange(new List<string>()
                    {
                        "org:special:simple"//专题列表
                    });
                _redisClient.BatchDelAsync(delKeys, 10);
                #endregion
                return Task.FromResult(ResponseResult.Success("操作成功"));
            }
            catch(Exception ex)
            {
                _orgUnitOfWork.Rollback();
                if(ex.Message.Contains("PRIMARY KEY"))
                {
                    return Task.FromResult(ResponseResult.Failed("专题主键重复，请【刷新页面】重新操作"));
                }
                else
                {
                    return Task.FromResult(ResponseResult.Failed($"系统错误：【{ex.Message}】"));
                } 
            }
        
        }
    }
}
