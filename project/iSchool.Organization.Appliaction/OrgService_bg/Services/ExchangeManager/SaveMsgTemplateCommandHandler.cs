using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;

namespace iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager
{
    /// <summary>
    /// 保存课程短信模板
    /// </summary>
    public class SaveMsgTemplateCommandHandler : IRequestHandler<SaveMsgTemplateCommand, ResponseResult>
    {


        OrgUnitOfWork _orgUnitOfWork;

        public SaveMsgTemplateCommandHandler(IOrgUnitOfWork unitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public async Task<ResponseResult> Handle(SaveMsgTemplateCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            try
            {
                string sql = "";
                var dy = new DynamicParameters();
                dy.Set("Variable1", request.Variable1);
                dy.Set("Variable2", request.Variable2);
                dy.Set("Url", request.Url);
                dy.Set("Msg", request.Msg);
                dy.Set("CourseId", request.CourseId);
                dy.Set("GoodId", request.GoodId);
                dy.Set("IsAuto", request.IsAuto);
                dy.Set("IsRedirect", request.IsRedirect);
                dy.Set("MsgTemplateId", request.Id);
                dy.Set("Content", request.Content);
                dy.Set("Code", request.Code);

                if (request.Id != null && request.Id != default)//更新
                {
                    sql += $@"
update dbo.MsgTemplate set Variable1=@Variable1, Variable2=@Variable2, [Url]=@Url, Msg=@Msg, 
IsAuto=@IsAuto, IsRedirect=@IsRedirect,Content=@Content,Code=@Code where id=@MsgTemplateId

;";
                    
                }
                else//新增
                {
                    sql += $@"
Insert into dbo.MsgTemplate(Id, Variable1, Variable2, [Url], Msg, CourseId, GoodId, IsAuto, IsRedirect,Content,Code)
values(NEWID(), @Variable1, @Variable2, @Url, @Msg, @CourseId, @GoodId, @IsAuto, @IsRedirect,@Content,@Code)
;";
                }
                var response = _orgUnitOfWork.ExecuteScalar<int>(sql, dy);
                return ResponseResult.Success(response);

            }
            catch (Exception ex)
            {
                return ResponseResult.Failed($"系统错误：{ex.Message}");
            }
        }
    }



}
