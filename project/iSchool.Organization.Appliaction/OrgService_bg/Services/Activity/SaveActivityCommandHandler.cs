using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Domain.Enum;
using System.Linq;
using iSchool.Organization.Appliaction.Service.Organization;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    public class SaveActivityCommandHandler : IRequestHandler<SaveActivityCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IMediator _mediator;
              
        public SaveActivityCommandHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient,IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            _mediator = mediator;
        }

        public Task<ResponseResult> Handle(SaveActivityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var oldCount= _orgUnitOfWork.DbConnection.Query<int>($" SELECT count(1) FROM [Organization].[dbo].[Activity] where IsValid=1 and status={ActivityStatus.Ok.ToInt()}  and title='{request.Title}'  and id<>'{request.Id}'").FirstOrDefault();
                if (oldCount > 0) return Task.FromResult(ResponseResult.Failed("活动名称重复，请重新填写！"));

                var listSpecialIds = request.ListSpecials?.Split(',')?.SelectMany(_ => new List<Guid>() { new Guid(_) });//绑定专题集合
                var dy = new DynamicParameters()
                    .Set("id", request.Id)
                    .Set("Time", DateTime.Now)
                    .Set("UserId", request.UserId)
                    .Set("id", request.Id)
                    .Set("title", request.Title)
                    .Set("logo", request.Logo)
                    .Set("starttime", request.StartTime)
                    .Set("endtime", request.EndTime)
                    .Set("limit", request.Limit)
                    .Set("budget", request.Budget);

                var delCacheKeys = new List<string>() { "org:special:simple*" };

                string actSql = "";//活动表   
                string actExtSql = "", delActExtSql = "";//活动专题关系映射
                string actRuleSql = "", delActRuleSql = "";//活动规则
               
                #region 新活动专题映射关系
                if (listSpecialIds?.Any() == true)
                {
                    actExtSql = @$" Insert into [dbo].[ActivityExtend]([id], [activityid], [contentid], [type], [sort] ,[IsValid]) values  ";
                    StringBuilder strsql = new StringBuilder();
                    int index = 1;
                    foreach (var item in listSpecialIds)
                    {
                        strsql.AppendFormat($" {",".If(index > 1)} (NEWID() ,'{request.Id}', '{item}', {ActivityExtendType.Special.ToInt()}, {index},1) ");
                        ++index;
                    }

                    actExtSql += strsql.ToString();

                }
                #endregion

                #region 新活动规则
                
                StringBuilder bsql = new StringBuilder("  Insert into [dbo].[ActivityRule]([id], [activityid], [type], [number], [price], [CreateTime], [Creator],[IsValid]) Values  ");

                //1、停止/继续活动
                bsql.AppendFormat($" (NEWID(),'{request.Id}',{ActivityRuleType.StopOrKeepActivity.ToInt()},{request.StopOrKeepActivity},null,'{DateTime.Now}','{request.UserId}',1) ");

                //2、审核通过N天内不能修改
                
                bsql.AppendFormat($" , (NEWID(),'{request.Id}',{ActivityRuleType.OperationNotAllowed.ToInt()},@NDaysNotAllowChange,null,'{DateTime.Now}','{request.UserId}',1) ");

                //3、第N篇额外奖金
                if (request.NExtraBonusNum?.Any()==true)
                {
                    for (int i = 0; i < request.NExtraBonusNum.Count; i++)
                    {
                        bsql.AppendFormat($" ,(NEWID(),'{request.Id}',{ActivityRuleType.ExtraBonus.ToInt()},{request.NExtraBonusNum[i]},{request.NExtraBonusPrice[i]},'{DateTime.Now}','{request.UserId}',1)  ");
                    }
                }

                //4、单篇奖金
                bsql.AppendFormat($" , (NEWID(),'{request.Id}',{ActivityRuleType.SingleBonus.ToInt()},null,{request.Price},'{DateTime.Now}','{request.UserId}',1) ");


                actRuleSql += bsql.ToString();
                #endregion

                #region 新增
                if (request.IsAdd)
                {
                    actSql = $@" Insert Into [dbo].[Activity]([id], [title], [logo], [starttime], [endtime], [type], [limit], 
                              [budget], [Creator], [CreateTime], [IsValid], [acode], [status],[ModifyDateTime],[Modifier])
                              values(@id, @title, @logo, @starttime, @endtime, @type, @limit, 
                              @budget, @UserId, @Time, @IsValid, @acode, @status,@Time,@UserId)
                         ;";
                    dy.Set("type", ActivityType.Hd2.ToInt())                                             
                      .Set("IsValid", true)
                      .Set("acode", GetActivityCode())
                      .Set("status", ActivityStatus.Ok.ToInt());
                }
                #endregion

                #region 修改
                else//编辑
                {
                    delCacheKeys.AddRange(new  List<string>{
                         CacheKeys.Acd_id.FormatWith(request.Id)
                        ,CacheKeys.ActivitySimpleInfo.FormatWith(request.Id)           
                        ,CacheKeys.Hd_spcl_acti.FormatWith("*")
                    });


                    actSql = $@"Update [dbo].[Activity] set [title]=@title, [logo]=@logo, [starttime]=@starttime, [endtime]=@endtime
                                ,[limit]=@limit, [budget]=@budget,[ModifyDateTime]=@Time,[Modifier]=@UserId where id=@id and IsValid=1
                        ;";

                    //解绑活动专题关系
                    delActExtSql += $" Delete [dbo].[ActivityExtend] where type={ActivityExtendType.Special.ToInt()} and activityid='{request.Id}' ; ";

                    //删除旧活动规则
                    delActRuleSql += $" update [dbo].[ActivityRule] set IsValid=0, Modifier='{request.UserId}',ModifyDateTime=GETDATE() where activityid='{request.Id}' ;";

                }
                #endregion
               
                _orgUnitOfWork.BeginTransaction();
                
                _orgUnitOfWork.DbConnection.Execute(actSql, dy,_orgUnitOfWork.DbTransaction);//活动表

                if(!string.IsNullOrEmpty(delActExtSql))//活动扩展表--活动专题解绑
                    _orgUnitOfWork.DbConnection.Execute(delActExtSql, null, _orgUnitOfWork.DbTransaction);

                if (!string.IsNullOrEmpty(actExtSql))//活动扩展表--活动专题绑定
                    _orgUnitOfWork.DbConnection.Execute(actExtSql, null, _orgUnitOfWork.DbTransaction);

                if(!string.IsNullOrEmpty(delActRuleSql))//删除旧活动规则
                    _orgUnitOfWork.DbConnection.Execute(delActRuleSql, null, _orgUnitOfWork.DbTransaction);

                if (!string.IsNullOrEmpty(actRuleSql))//保存新活动规则
                    _orgUnitOfWork.DbConnection.Execute(actRuleSql, new {  request.NDaysNotAllowChange }, _orgUnitOfWork.DbTransaction);

                _orgUnitOfWork.CommitChanges();

                //新增修改操作，入库到[ActivityDataHistory]中
                var ruleIds= _orgUnitOfWork.DbConnection.Query<Guid>($" SELECT id from dbo.ActivityRule where  activityid='{request.Id}' and IsValid=1 ");
                string ruleIdsJson = JsonSerializationHelper.Serialize(ruleIds);
                string hisSql = @$"
                                Insert into [dbo].[ActivityDataHistory]([id], [activityid], [title], [logo], [starttime], [endtime], [type], [desc], [limit], [budget], [acode], [status], [rules], [Modifier], [ModifyDateTime])
                                select NEWID() as Id,act.id as activityid,act.title,act.logo,act.starttime,act.endtime,act.[type],act.[desc],act.limit,act.budget,act.acode,act.[status],'{ruleIdsJson}',act.Modifier,act.ModifyDateTime 
                                from Activity as act  where act.id='{request.Id}'
                                ";
                _orgUnitOfWork.DbConnection.Execute(hisSql);


                #region 清除API那边相关的缓存
                _redisClient.BatchDelAsync(delCacheKeys, 10);
                #endregion
                return Task.FromResult(ResponseResult.Success("操作成功"));
            }
            catch(Exception ex)
            {
                _orgUnitOfWork.Rollback();
                return Task.FromResult(ResponseResult.Failed($"系统错误：【{ex.Message}】"));
            }
           
        }

        /// <summary>
        /// 临时使用，与全名营销对接后，重写
        /// </summary>
        /// <returns></returns>
        private string GetActivityCode()
        {
            //活动码共五位
            string _zimu = "abcdefghijklmnopqrstuvwxyz";//要随机的字母
            string _shuzi = "abcdef123ghijklm456nopqrst7890uvwxyz";//要随机的字母+数字
            Random _rand = new Random(); 
            string _result = "";
            while (true)
            {
                //首位字母
                _result +=_zimu[_rand.Next(26)];
                for (int i = 0; i < 3; i++) //循环3次，生成3位数字+字母
                {
                    _result += _shuzi[_rand.Next(36)]; //通过索引下标随机

                }
                //尾位字母
                _result += _zimu[_rand.Next(26)];

                var count= _orgUnitOfWork.DbConnection.Query<int>($" select count(1) from [dbo].[Activity] where IsValid=1 and acode='{_result}' ").FirstOrDefault();
                if (count <= 0)
                    break;
            }            
            return _result;
        }
    }
}
