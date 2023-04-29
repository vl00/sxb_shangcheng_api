using Autofac;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Repositories;
using iSchool.Infrastructure.Repositories.Organization;
using iSchool.Organization.Appliaction.Queries;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;

namespace iSchool.Organization.Api.Modules.AutofacModule
{
    public class InfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        private readonly string _dbReadConnnectionString;

        public InfrastructureModule(string databaseConnectionString, string dbReadConnnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
            this._dbReadConnnectionString = dbReadConnnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(OrgBaseRepository<>))
                .As(typeof(IBaseRepository<>))
                .Named("OrgBaseRepository", typeof(IBaseRepository<>))
                .InstancePerLifetimeScope();

            //main repository
            builder.RegisterGeneric(typeof(OrgBaseRepository<>))
             .As(typeof(iSchool.Domain.Repository.Interfaces.IRepository<>))
             .InstancePerLifetimeScope();

            //优惠券部分仓储基础设施
            builder.RegisterType(typeof(GoodsQueries))
             .As(typeof(IGoodsQueries))
             .InstancePerLifetimeScope();
            builder.RegisterType(typeof(CouponQueries))
             .As(typeof(ICouponQueries))
             .InstancePerLifetimeScope();
            builder.RegisterType(typeof(CouponReceiveRepository))
             .As(typeof(ICouponReceiveRepository))
             .InstancePerLifetimeScope();
            builder.RegisterType(typeof(CouponInfoRepository))
             .As(typeof(ICouponInfoRepository))
             .InstancePerLifetimeScope();


            builder.RegisterType(typeof(OrderQueries))
             .As(typeof(IOrderQueries))
             .InstancePerLifetimeScope();

            if (!string.IsNullOrEmpty(_dbReadConnnectionString))
            {
                builder.RegisterType<OrgUnitOfWork>()
                   .As<IOrgUnitOfWork>()
                   .WithParameter("connectionString", _databaseConnectionString)
                   .WithParameter("readConnnectionString", _dbReadConnnectionString)
                   .InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<OrgUnitOfWork>()
                   .As<IOrgUnitOfWork>()
                   .WithParameter("connectionString", _databaseConnectionString)
                   .InstancePerLifetimeScope();
            }
        }
    }


    #region WX
    public class WXInfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        public WXInfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {              
            builder.RegisterGeneric(typeof(WXBaseRepository<>))
                .As(typeof(IBaseRepository<>))
                .Named("WXBaseRepository", typeof(IBaseRepository<>))
                .InstancePerLifetimeScope();

            builder.RegisterType<WXOrgUnitOfWork>()
               .As<IWXUnitOfWork>()
               .WithParameter("connectionString", _databaseConnectionString)
               .InstancePerLifetimeScope();
        }
    }
    #endregion

    #region Openid_WX
    public class Openid_WXInfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        public Openid_WXInfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {            
            builder.RegisterGeneric(typeof(Openid_WXBaseRepository<>))
                .As(typeof(IBaseRepository<>))
                .Named("Openid_WXBaseRepository", typeof(IBaseRepository<>))
                .InstancePerLifetimeScope();

            builder.RegisterType<Openid_WXOrgUnitOfWork>()
               .As<IOpenid_WXUnitOfWork>()
               .WithParameter("connectionString", _databaseConnectionString)
               .InstancePerLifetimeScope();
        }
    }
    #endregion

    #region 虎叔叔用户中心
    public class UserInfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        public UserInfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterGeneric(typeof(UserBaseRepository<>)).As(typeof(IBaseRepository<>))
            //    .Named("UserBaseRepository", typeof(IBaseRepository<>))
            //    .InstancePerLifetimeScope();

            builder.RegisterType<UserUnitOfWork>().As(typeof(IUserUnitOfWork))
                .WithParameter("connectionString", _databaseConnectionString)
                .InstancePerLifetimeScope();
        }
    }
    #endregion
}
