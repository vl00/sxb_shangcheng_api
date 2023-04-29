using AutoMapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.AutoMapper
{
    /// <summary>
    /// viewmodel mapper domain 的配置文件
    /// </summary>
    public class ViewModelToDomainMappingProfile : Profile
    {
        public ViewModelToDomainMappingProfile()
        {
            //CreateMap<,>():
            /* CreateMap<,>()
                .ForMember(t => t., option => option.MapFrom((s, t) => s.)); */

            CreateMap<AddEvaluationCommand_CourseEntity, EvaluationBind>()
                .ForMember(t => t.Mode, 
                    option => option.MapFrom((s, t) => s.Mode?.ToJsonString()));

            CreateMap<OrgService_bg.Activitys.ExportAuditLsCommand, OrgService_bg.Activitys.AuditLsPagerQuery>();

            CreateMap<CourseGoodsSimpleInfoDto, ApiCourseGoodsSimpleInfoDto>();
        }
    }
}
