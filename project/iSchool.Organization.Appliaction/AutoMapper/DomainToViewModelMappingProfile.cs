using AutoMapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.EvaluationComments;
using iSchool.Organization.Appliaction.ResponseModels.Evaluations;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.AutoMapper
{
    /// <summary>
    /// Domain mapper ViewModel 的配置文件
    /// </summary>
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile()
        {
            //CreateMap<,>():
            /* CreateMap<,>()
                .ForMember(t => t., option => option.MapFrom((s, t) => s.)); */

            CreateMap<Evaluation, EvaluationItemDto>()
                .AfterMap((s, t) =>
                {
                    t.Id = s.Id;
                    t.Id_s = Infrastructure.Extensions.UrlShortIdUtil.Long2Base32(s.No);
                    t.Title = s.Title;
                    t.Stick = s.Stick;
                    t.IsPlaintext = s.IsPlaintext;
                    t.Cover = s.Cover;
                    t.AuthorId = s.Userid;
                    t.CreateTime = s.CreateTime;
                    t.CollectionCount = s.Collectioncount;
                    t.CommentCount = s.Commentcount;
                    t.LikeCount = s.Likes;
                    t.ViewCount = s.Viewcount;
                });

            CreateMap<Evaluation, EvltQueryResult>()
               .AfterMap((s, t) =>
               {
                   t.Id = s.Id;
                   t.Id_s = Infrastructure.Extensions.UrlShortIdUtil.Long2Base32(s.No);
                   t.Title = s.Title;
                   t.Stick = s.Stick;
                   t.IsPlaintext = s.IsPlaintext;
                   t.Cover = s.Cover;
                   t.AuthorId = s.Userid;
                   t.CreateTime = s.CreateTime;                  
               });

            CreateMap<EvaluationComment, MyEvltCommentDto>()
               .AfterMap((s, t) =>
               {
                   t.Id = s.Id;
                   t.Comment = s.Comment;
                   t.UserId = s.Userid;
                   t.Username = s.Username;
                   t.CreateTime = s.CreateTime;
                   t.IsValid = s.IsValid;


               });
            CreateMap<Evaluation, EvaluationSimpleDto>()
                .ForMember(t => t.ShortId, option => option.MapFrom((s, t) => UrlShortIdUtil.Long2Base32(s.No)));

            CreateMap<Special, SpecialResEntity>();
            CreateMap<Special, SpecialResEntity2>()
                .AfterMap((s, t) =>
                {
                    t.SpecialType = s.Type;
                });

            CreateMap<EvaluationItem, EvaluationContentDto>()
                .ForMember(t => t.Pictures, option => option.MapFrom((s, t) => s.Pictures.ToObject<string[]>() ?? new string[0]))
                .ForMember(t => t.Thumbnails, option => option.MapFrom((s, t) => s.Thumbnails.ToObject<string[]>() ?? new string[0]))
                .ForMember(t => t.VideoUrl, option => option.MapFrom((s, t) => s.Video))
                .ForMember(t => t.VideoCoverUrl, option => option.MapFrom((s, t) => s.VideoCover))
                ;

            CreateMap<EvltBaseInfoDto, EvltDetailDto>()
                .ForMember(t => t.Id_s, option => option.MapFrom((s, t) => UrlShortIdUtil.Long2Base32(s.No)))
                .ForMember(t => t.SpecialId_s, option => option.MapFrom((s, t) => s.SpecialNo == null ? null : UrlShortIdUtil.Long2Base32(s.SpecialNo.Value)))
                ;
            CreateMap<EvltBaseInfoDto, MiniEvltDetailDto>()
                .ForMember(t => t.Id_s, option => option.MapFrom((s, t) => UrlShortIdUtil.Long2Base32(s.No)))                
                ;

            CreateMap<EvltDetailDto, PcEvltDetailDto>()
                .ForMember(t => t.Comments, option => option.MapFrom((s, t) => t.Comments = null));

            CreateMap<EvaluationCommentDto, PcEvaluationCommentDto>()
                .ForMember(t => t.SubComments, option => option.MapFrom((s, t) => t.SubComments = null));

            CreateMap<Domain.Organization, PcOrgItemDto>()
                .AfterMap((s, t) =>
                {
                    t.Id_s = UrlShortIdUtil.Long2Base32(s.No);
                });

            CreateMap<Domain.Course, PcCourseDetailDto>()
                .ForMember(t => t.Subjects, option => option.MapFrom((s, t) => default(int[])))
                .AfterMap((s, t) =>
                {
                    t.Id_s = UrlShortIdUtil.Long2Base32(s.No);
                    t.Banner = s.Banner.ToObject<List<string>>();
                    t.CName = s.Name;
                    t.SubjectDesc = s.Subject == null ? null : EnumUtil.GetDesc((SubjectEnum)s.Subject.Value);
                    t.EvaluationInfo = null;
                    //t.Logo = null;
                    t.Subjects = s.Subjects?.ToObject<int[]>() is int[] subjs && subjs.Length > 0 ? subjs : new[] { SubjectEnum.Other.ToInt() };
                    t.SubjectDescs = t.Subjects.Select(subj => EnumUtil.GetDesc((SubjectEnum)subj)).ToArray();
                });

            CreateMap<Domain.Course, CourseOrderProdItemDto>()
                .AfterMap((s, t) =>
                {
                    t.ProdType = s.Type;
                    t.Id_s = UrlShortIdUtil.Long2Base32(s.No);
                    t.Banner = (s.Banner_s ?? s.Banner).ToObject<string[]>() ?? Array.Empty<string>();
                    //t.SubjectDesc = s.Subject == null ? null : EnumUtil.GetDesc((SubjectEnum)s.Subject.Value);
                    t.Stock = s.Stock ?? 0;
                });
            CreateMap<Domain.Organization, CourseOrderProdItem_OrgItemDto>()
                .AfterMap((s, t) =>
                {
                    t.Id_s = UrlShortIdUtil.Long2Base32(s.No);
                });
            CreateMap<CourseGoodsOrderCtnDto, CourseOrderProdItemDto>()
                .ForMember(t => t.Banner, option => option.MapFrom((s, t) => t.Banner = s.Banner.IsNullOrEmpty() ? new string[0] : new[] { s.Banner }))
                .AfterMap((s, t) =>
                {
                    t.Id_s = UrlShortIdUtil.Long2Base32(s.No);
                    //t.SubjectDesc = s.Subject == null ? null : ((SubjectEnum)s.Subject.Value).GetDesc();
                    t.ProdType = s.ProdType;
                    t.OrgInfo = new CourseOrderProdItem_OrgItemDto();
                    t.OrgInfo.Id = s.OrgId;
                    t.OrgInfo.Id_s = UrlShortIdUtil.Long2Base32(s.OrgNo);
                    t.OrgInfo.Name = s.OrgName;
                    t.OrgInfo.Authentication = s.Authentication;
                    t.OrgInfo.Logo = s.OrgLogo;
                    t.OrgInfo.Desc = s.OrgDesc;
                    t.OrgInfo.Subdesc = s.OrgSubdesc;
                    t.NewUserExclusive = s.IsNewUserExclusive;
                });

            CreateMap<CourseOrderProdItemDto, CourseShoppingCartProdItemDto>();
        }
    }
}
