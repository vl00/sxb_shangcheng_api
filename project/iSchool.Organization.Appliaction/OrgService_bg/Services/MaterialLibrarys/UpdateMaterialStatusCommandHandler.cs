using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg
{
    public class UpdateMaterialStatusCommandHandler : IRequestHandler<UpdateMaterialStatusCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;

        public UpdateMaterialStatusCommandHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<bool> Handle(UpdateMaterialStatusCommand cmd, CancellationToken cancellationToken)
        {
            var sql = "update MaterialLibrary set [Status]=@Status,ModifyDateTime=getdate(),Modifier=@Userid where Id=@Id ";
            var i = await _orgUnitOfWork.ExecuteAsync(sql, new { cmd.Status, cmd.Id, cmd.Userid });

            return i > 0;
        }
    }
}
